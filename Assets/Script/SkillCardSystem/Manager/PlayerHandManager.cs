using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 단순화된 플레이어 핸드 매니저
    /// 핸드 카드 3장만 관리하는 최소한의 기능만 제공
    /// </summary>
    public class PlayerHandManager : MonoBehaviour, IPlayerHandManager
    {
        #region 최소한의 인스펙터 설정
        
        [Header("핸드 설정")]
        [Tooltip("최대 핸드 크기")]
        [SerializeField] private int maxHandSize = 3;
        
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true;
        
        #endregion
        
        #region 의존성 주입 (DI로 자동 해결)
        
        [Inject] private HandSlotRegistry slotRegistry;
        [Inject] private ISkillCardFactory cardFactory;
        
        #endregion
        
        #region 핵심 필드
        
        private ICharacter owner;
        private readonly Dictionary<SkillCardSlotPosition, ISkillCard> cards = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();
        
        #endregion

        #region 초기화
        
        private void Awake()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("PlayerHandManager 초기화", GameLogger.LogCategory.SkillCard);
            }
        }
        
        #endregion
        
        #region 소유자 설정
        
        /// <summary>
        /// 핸드 소유자를 설정합니다.
        /// </summary>
        public void SetPlayer(ICharacter player)
        {
            this.owner = player;
        }
        
        #endregion

        #region 초기 핸드 구성

        /// <summary>
        /// 초기 핸드를 생성합니다.
        /// </summary>
        public void GenerateInitialHand()
        {
            // TODO: CharacterData가 object 타입이므로 적절한 캐스팅 필요
            // var deck = owner?.CharacterData?.SkillDeck;
            // if (deck == null) return;

            // var allCards = deck.GetAllCards();
            // if (allCards.Count == 0) return;

            // 랜덤하게 3장 선택
            // for (int i = 0; i < maxHandSize && i < allCards.Count; i++)
            // {
            //     var randomIndex = Random.Range(0, allCards.Count);
            //     var cardDefinition = allCards[randomIndex];
            //     
            //     if (cardDefinition != null)
            //     {
            //         var card = cardFactory.CreateFromDefinition(cardDefinition, Game.SkillCardSystem.Data.Owner.Player, owner?.CharacterData?.name);
            //         AddCardToSlot((SkillCardSlotPosition)i, card);
            //     }
            // }
        }

        #endregion

        #region 카드 조회

        /// <inheritdoc/>
        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos) =>
            cards.TryGetValue(pos, out var c) ? c : null;

        /// <inheritdoc/>
        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos) =>
            cardUIs.TryGetValue(pos, out var ui) ? ui : null;

        #endregion

        #region 카드 관리

        /// <summary>
        /// 빈 슬롯에 카드를 추가합니다.
        /// </summary>
        public void AddCardToHand(ISkillCard card)
        {
            for (int i = 0; i < maxHandSize; i++)
            {
                var pos = (SkillCardSlotPosition)i;
                if (!cards.ContainsKey(pos) || cards[pos] == null)
                {
                    AddCardToSlot(pos, card);
                    return;
                }
            }
        }

        /// <summary>
        /// 특정 슬롯에 카드를 추가합니다.
        /// </summary>
        public void AddCardToSlot(SkillCardSlotPosition slot, ISkillCard card)
        {
            if (card == null) return;
            
            cards[slot] = card;
            
            // UI 연결은 슬롯 레지스트리가 자동 처리
            var handSlots = slotRegistry?.GetPlayerHandSlot();
            var handSlot = handSlots?.FirstOrDefault();
            if (handSlot != null)
            {
                var ui = handSlot.AttachCard(card, null); // 프리팹은 DI에서 자동 주입
                if (ui != null) 
                {
                    cardUIs[slot] = ui;
                }
            }
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 추가: {card.GetCardName()} -> {slot}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 카드를 제거합니다.
        /// </summary>
        public void RemoveCard(ISkillCard card)
        {
            foreach (var kvp in cards)
            {
                if (kvp.Value == card)
                {
                    var handSlots = slotRegistry?.GetPlayerHandSlot();
                    var slot = handSlots?.FirstOrDefault();
                    slot?.DetachCard();
                    
                    cards[kvp.Key] = null;
                    cardUIs.Remove(kvp.Key);
                    
                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo($"카드 제거: {card.GetCardName()} <- {kvp.Key}", GameLogger.LogCategory.SkillCard);
                    }
                    return;
                }
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 카드들의 드래그 가능 여부를 설정합니다.
        /// </summary>
        public void EnableInput(bool enable)
        {
            foreach (var ui in cardUIs.Values)
            {
                if (ui != null) ui.SetDraggable(enable);
            }
        }

        /// <summary>
        /// 모든 카드를 제거합니다.
        /// </summary>
        public void ClearAll()
        {
            foreach (var pos in cards.Keys)
            {
                var handSlots = slotRegistry?.GetPlayerHandSlot();
                var slot = handSlots?.FirstOrDefault();
                slot?.DetachCard();
            }
            cards.Clear();
            cardUIs.Clear();
        }

        /// <summary>
        /// 플레이어를 반환합니다.
        /// </summary>
        public ICharacter GetPlayer() => owner;

        #endregion
        
        #region 카드 교체 핸들러 (PlayerCardReplacementHandler 통합)
        
        /// <summary>
        /// 슬롯의 기존 카드를 핸드로 복귀시키고 새로운 카드를 슬롯에 배치합니다.
        /// </summary>
        /// <param name="slot">대상 슬롯</param>
        /// <param name="newCard">새로 등록할 카드</param>
        /// <param name="newCardUI">새 카드 UI</param>
        public void ReplaceSlotCard(ICombatCardSlot slot, ISkillCard newCard, SkillCardUI newCardUI)
        {
            var oldCard = slot.GetCard();
            var oldUI = slot.GetCardUI() as SkillCardUI;

            if (oldCard != null && oldUI != null)
            {
                // 기존 카드 슬롯에서 제거
                slot.ClearAll();

                // 기존 카드 핸드로 복귀
                var oldHandSlot = oldCard.GetHandSlot();
                if (oldHandSlot.HasValue)
                {
                    AddCardToSlot(oldHandSlot.Value, oldCard);
                }
                else
                {
                    GameLogger.LogWarning("핸드 슬롯 정보 없음 → 자동 복귀", GameLogger.LogCategory.SkillCard);
                    AddCardToHand(oldCard);
                }
            }

            // 새 카드 등록
            PlaceCardInSlot(newCard, newCardUI, slot);
        }
        
        #endregion
        
        #region 카드 배치 서비스 (CardPlacementService 통합)
        
        /// <summary>
        /// 지정된 슬롯에 카드와 UI를 배치합니다.
        /// </summary>
        /// <param name="card">배치할 스킬 카드</param>
        /// <param name="ui">해당 카드의 UI</param>
        /// <param name="slot">카드를 배치할 전투 슬롯</param>
        public void PlaceCardInSlot(ISkillCard card, ISkillCardUI ui, ICombatCardSlot slot)
        {
            if (card == null || ui == null || slot == null)
            {
                GameLogger.LogError("카드, UI, 슬롯 중 하나 이상이 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 카드 설정
            slot.SetCard(card);
            slot.SetCardUI(ui);

            // UI 오브젝트 위치 정렬
            if (ui is MonoBehaviour uiMb)
            {
                uiMb.transform.SetParent(((MonoBehaviour)slot).transform);
                uiMb.transform.localPosition = Vector3.zero;
                uiMb.transform.localScale = Vector3.one;
            }
            else
            {
                GameLogger.LogWarning("카드 UI가 MonoBehaviour가 아닙니다. Transform 설정을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
            }
        }
        
        #endregion
    }
}
