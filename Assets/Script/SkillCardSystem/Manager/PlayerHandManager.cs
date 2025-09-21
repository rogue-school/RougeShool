using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 플레이어의 손패를 관리하는 매니저 클래스입니다.
    /// 카드 추가/제거와 슬롯 관리를 담당합니다.
    /// </summary>
    public class PlayerHandManager : BaseSkillCardManager<IPlayerHandManager>, IPlayerHandManager
    {
        #region 필드

        private IPlayerCharacter owner;
        private IHandSlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;

        private readonly Dictionary<SkillCardSlotPosition, ISkillCard> cards = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        #endregion

        #region 의존성 주입 및 초기화

        [Inject]
        public void Construct(
            ISlotRegistry slotRegistry,
            ISkillCardFactory cardFactory)
        {
            this.slotRegistry = slotRegistry.GetHandSlotRegistry();
            this.cardFactory = cardFactory;

            cards.Clear();
            cardUIs.Clear();
        }

        /// <summary>
        /// 핸드 소유자를 설정합니다.
        /// </summary>
        public void SetPlayer(IPlayerCharacter player)
        {
            this.owner = player;
        }

        #endregion

        #region 베이스 클래스 구현

        protected override System.Collections.IEnumerator OnInitialize()
        {
            // 카드 설정 검증
            if (!ValidateCardSettings())
            {
                yield break;
            }

            // 참조 검증
            ValidateReferences();

            // 카드 UI 연결
            ConnectCardUI();

            // 매니저 상태 로깅
            LogManagerState();

            yield return null;
        }

        public override void Reset()
        {
            // 카드 정리
            cards.Clear();
            cardUIs.Clear();

            // 소유자 초기화
            owner = null;

            if (managerSettings.enableDebugLogging)
            {
                GameLogger.LogInfo("PlayerHandManager 리셋 완료", GameLogger.LogCategory.SkillCard);
            }
        }

        #endregion

        #region 베이스 클래스 오버라이드

        /// <summary>
        /// PlayerHandManager는 핸드 컨테이너만 필요합니다.
        /// </summary>
        protected override bool RequiresHandContainer()
        {
            return true;
        }

        /// <summary>
        /// PlayerHandManager는 덱 컨테이너가 필요하지 않습니다.
        /// </summary>
        protected override bool RequiresDeckContainer()
        {
            return false;
        }

        #endregion

        #region 초기 핸드 구성

        /// <summary>
        /// 초기 핸드를 생성합니다.
        /// </summary>
        public void GenerateInitialHand()
        {
            var deck = owner?.CharacterData?.SkillDeck;
            if (deck == null) return;

            var allCards = deck.GetAllCards();
            if (allCards.Count == 0) return;

            // 랜덤하게 3장 선택
            for (int i = 0; i < 3 && i < allCards.Count; i++)
            {
                var randomIndex = Random.Range(0, allCards.Count);
                var cardDefinition = allCards[randomIndex];
                
                if (cardDefinition != null)
                {
                    var card = cardFactory.CreateFromDefinition(cardDefinition, Game.SkillCardSystem.Data.Owner.Player, owner?.CharacterData?.name);
                    AddCardToSlot((SkillCardSlotPosition)i, card);
                }
            }
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
            for (int i = 0; i < 3; i++)
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
            cards[slot] = card;
            
            var handSlot = slotRegistry.GetPlayerHandSlot(slot);
            if (handSlot != null)
            {
                var ui = handSlot.AttachCard(card, cardSettings.cardPrefab);
                if (ui != null) cardUIs[slot] = ui;
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
                    var slot = slotRegistry.GetPlayerHandSlot(kvp.Key);
                    slot?.DetachCard();
                    
                    cards[kvp.Key] = null;
                    cardUIs.Remove(kvp.Key);
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
                var slot = slotRegistry.GetPlayerHandSlot(pos);
                slot?.DetachCard();
            }
            cards.Clear();
            cardUIs.Clear();
        }

        /// <summary>
        /// 플레이어를 반환합니다.
        /// </summary>
        public IPlayerCharacter GetPlayer() => owner;

        #endregion
    }
}
