using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using System.Linq;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 플레이어의 손패를 관리하는 매니저 클래스입니다.
    /// 카드 생성, 제거, 복구 및 드래그 가능 여부 설정 등의 기능을 담당합니다.
    /// 카드 순환 시스템과 연동하여 로그 스쿨 시스템을 지원합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour, IPlayerHandManager
    {
        #region 필드

        private IPlayerCharacter owner;
        private IHandSlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;
        private SkillCardUI cardUIPrefab;
        private ICardCirculationSystem circulationSystem;
        private ITurnBasedCardManager turnBasedCardManager;

        private readonly Dictionary<SkillCardSlotPosition, ISkillCard> cards = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        #endregion

        #region 의존성 주입 및 초기화

        [Inject]
        public void Construct(
            ISlotRegistry slotRegistry,
            ISkillCardFactory cardFactory,
            SkillCardUI cardUIPrefab,
            ICardCirculationSystem circulationSystem,
            ITurnBasedCardManager turnBasedCardManager)
        {
            this.slotRegistry = slotRegistry.GetHandSlotRegistry();
            this.cardFactory = cardFactory;
            this.cardUIPrefab = cardUIPrefab;
            this.circulationSystem = circulationSystem;
            this.turnBasedCardManager = turnBasedCardManager;

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

        #region 초기 핸드 구성

        /// <summary>
        /// 플레이어 덱을 기반으로 초기 핸드를 생성합니다.
        /// 덱에서 랜덤하게 3장을 선택하여 핸드에 배치합니다.
        /// </summary>
        public void GenerateInitialHand()
        {
            var deck = owner?.CharacterData?.SkillDeck;
            if (deck == null)
            {
                Debug.LogError("[PlayerHandManager] 플레이어 덱이 비어 있음");
                return;
            }

            var allCards = deck.GetAllCards();
            if (allCards.Count == 0)
            {
                Debug.LogWarning("[PlayerHandManager] 덱에 카드가 없습니다.");
                return;
            }

            // 덱에서 랜덤하게 3장 선택
            var selectedCards = new List<Game.SkillCardSystem.Data.SkillCardDefinition>();
            for (int i = 0; i < 3 && allCards.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, allCards.Count);
                selectedCards.Add(allCards[randomIndex]);
            }

            // 선택된 카드들을 슬롯에 배치
            for (int slotIndex = 0; slotIndex < selectedCards.Count; slotIndex++)
            {
                SkillCardSlotPosition pos = (SkillCardSlotPosition)slotIndex; // PLAYER_SLOT_1(0), PLAYER_SLOT_2(1), PLAYER_SLOT_3(2)
                var cardDefinition = selectedCards[slotIndex];
                ISkillCard card = null;
                
                if (cardDefinition != null)
                {
                    card = cardFactory.CreateFromDefinition(cardDefinition, Game.SkillCardSystem.Data.Owner.Player, owner?.CharacterData?.name);
                }
                
                cards[pos] = card;

                var slot = slotRegistry.GetPlayerHandSlot(pos);
                if (slot != null)
                {
                    var ui = slot.AttachCard(card, cardUIPrefab);
                    if (ui != null) cardUIs[pos] = ui;
                    
                    // 초기 핸드 생성 시에는 애니메이션 이벤트를 발생시키지 않음
                    // (PlayerSkillCardInitializer에서 일괄 처리)
                }
            }
            
            Debug.Log($"[PlayerHandManager] 초기 핸드 생성 완료: {selectedCards.Count}장 (랜덤 선택)");
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

        #region 카드 복구

        /// <summary>
        /// 사용 가능한 빈 슬롯에 카드를 복귀시킵니다.
        /// </summary>
        public void RestoreCardToHand(ISkillCard card)
        {
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                // 플레이어 전용 슬롯만 대상으로 복귀
                if (!IsPlayerHandSlot(pos)) continue;
                if (!cards.ContainsKey(pos) || cards[pos] == null)
                {
                    cards[pos] = card;
                    var slot = slotRegistry.GetPlayerHandSlot(pos);
                    if (slot != null)
                    {
                        var ui = slot.AttachCard(card, cardUIPrefab);
                        if (ui != null) cardUIs[pos] = ui;
                        else
                        {
                            Debug.LogWarning($"[PlayerHandManager] AttachCard 실패: {pos} 슬롯에 UI 생성 불가, 강제 복구 시도");
                            if (cardUIs.TryGetValue(pos, out var oldUI) && oldUI != null)
                                Game.CombatSystem.Utility.CardSlotHelper.AttachCardToHandSlot(oldUI, pos);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[PlayerHandManager] 핸드 슬롯 UI가 존재하지 않음: {pos} - 씬에 PlayerHandCardSlotUI가 반드시 존재해야 함");
                    }
                    LogHandSlotSyncState();
                    Debug.Log($"[HandRestore] 카드 {card.GetCardName()}를 슬롯 {pos}에 복귀");
                    return;
                }
            }
            Debug.LogWarning("[PlayerHandManager] 빈 슬롯을 찾을 수 없어 카드 복귀 실패");
            LogHandSlotSyncState();
        }

        /// <summary>
        /// 지정된 슬롯이 플레이어 핸드 슬롯인지 확인합니다.
        /// </summary>
        private static bool IsPlayerHandSlot(SkillCardSlotPosition pos)
        {
            return pos == SkillCardSlotPosition.PLAYER_SLOT_1
                || pos == SkillCardSlotPosition.PLAYER_SLOT_2
                || pos == SkillCardSlotPosition.PLAYER_SLOT_3;
        }

        /// <summary>
        /// 특정 슬롯에 카드를 복귀시킵니다.
        /// </summary>
        public void RestoreCardToHand(ISkillCard card, SkillCardSlotPosition slot)
        {
            cards[slot] = card;

            var handSlot = slotRegistry.GetPlayerHandSlot(slot);
            if (handSlot != null)
            {
                var ui = handSlot.AttachCard(card, cardUIPrefab);
                if (ui != null)
                    cardUIs[slot] = ui;
                else
                {
                    Debug.LogWarning($"[PlayerHandManager] AttachCard 실패: {slot} 슬롯에 UI 생성 불가, 강제 복구 시도");
                    if (cardUIs.TryGetValue(slot, out var oldUI) && oldUI != null)
                        Game.CombatSystem.Utility.CardSlotHelper.AttachCardToHandSlot(oldUI, slot);
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerHandManager] 지정된 슬롯이 존재하지 않습니다: {slot}");
            }
            LogHandSlotSyncState();
        }

        #endregion

        #region 카드 제거

        /// <summary>
        /// 손패에서 특정 카드를 제거합니다.
        /// 소멸 애니메이션 콜백 이외에는 카드UI/데이터를 파괴하지 말 것!
        /// </summary>
        public void RemoveCard(ISkillCard card)
        {
            Debug.LogWarning("[PlayerHandManager] RemoveCard는 반드시 AnimationFacade 소멸 애니메이션 콜백에서만 호출되어야 합니다. 직접 호출 금지!");
            foreach (var kvp in cards)
            {
                if (kvp.Value == card)
                {
                    var slot = slotRegistry.GetPlayerHandSlot(kvp.Key);
                    slot?.DetachCard();
                    if (cardUIs.TryGetValue(kvp.Key, out var ui))
                    {
                        if (ui != null && ui.gameObject != null)
                            CombatEvents.RaisePlayerCardUse(card.CardDefinition.displayName, ui.gameObject);
                    }
                    cards[kvp.Key] = null;
                    cardUIs.Remove(kvp.Key);
                    LogHandSlotSyncState();
                    return;
                }
            }
            Debug.LogWarning("[PlayerHandManager] 해당 카드를 찾을 수 없어 제거 실패");
            LogHandSlotSyncState();
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 각 손패 슬롯의 상태를 로그로 출력합니다.
        /// </summary>
        public void LogPlayerHandSlotStates()
        {
            // 디버깅용 로그 제거 - 필요시 개발자가 임시로 활성화
            // foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            // {
            //     var card = GetCardInSlot(pos);
            //     Debug.Log($"슬롯 {pos}: {(card != null ? card.CardDefinition.displayName : "비어 있음")}");
            // }
        }

        /// <summary>
        /// 손패 카드들의 드래그 가능 여부를 설정합니다.
        /// </summary>
        public void EnableInput(bool enable)
        {
            foreach (var pos in cardUIs.Keys.ToList())
            {
                var ui = cardUIs[pos];
                if (ui == null || ui.gameObject == null)
                {
                    cardUIs.Remove(pos);
                    if (cards.ContainsKey(pos)) cards.Remove(pos);
                    continue;
                }
                ui.SetDraggable(enable);
            }
        }


        /// <summary>
        /// 사망 등으로 인해 소멸 애니메이션이 끝난 후, 해당 슬롯의 카드UI와 참조를 완전히 해제/파괴합니다.
        /// 반드시 AnimationFacade 소멸 애니메이션 콜백에서만 호출되어야 함! 직접 호출 금지.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        public void RemoveCardUIAndReferences(SkillCardSlotPosition pos)
        {
            Debug.LogWarning("[PlayerHandManager] RemoveCardUIAndReferences는 반드시 AnimationFacade 소멸 애니메이션 콜백에서만 호출되어야 합니다. 직접 호출 금지!");
            var slot = slotRegistry.GetPlayerHandSlot(pos);
            slot?.Clear();
            if (cardUIs.TryGetValue(pos, out var ui))
            {
                if (ui != null && ui.gameObject != null)
                    GameObject.Destroy(ui.gameObject);
                cardUIs.Remove(pos);
            }
            if (cards.ContainsKey(pos))
                cards[pos] = null;
            LogHandSlotSyncState();
        }

        /// <summary>
        /// 모든 슬롯의 카드를 제거하고 초기화합니다.
        /// 소멸 애니메이션 콜백 이외에는 카드UI/데이터를 파괴하지 말 것!
        /// </summary>
        public void ClearAll()
        {
            Debug.LogWarning("[PlayerHandManager] ClearAll은 반드시 SafeClearHandAfterAllAnimations 등에서만 호출되어야 합니다. 직접 호출 금지!");
            foreach (var pos in cards.Keys)
                slotRegistry.GetPlayerHandSlot(pos)?.DetachCard();
            cards.Clear();
            cardUIs.Clear();
        }

        /// <summary>
        /// 현재 손패의 모든 카드와 UI를 반환합니다.
        /// </summary>
        public IEnumerable<(ISkillCard card, ISkillCardUI ui)> GetAllHandCards()
        {
            foreach (var kvp in cards)
            {
                var slot = kvp.Key;
                var card = kvp.Value;
                cardUIs.TryGetValue(slot, out var ui);
                yield return (card, ui);
            }
        }

        /// <summary>
        /// 핸드 슬롯 동기화 상태를 한 번에 출력합니다.
        /// </summary>
        public void LogHandSlotSyncState()
        {
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var card = cards.ContainsKey(pos) ? cards[pos] : null;
                var ui = cardUIs.ContainsKey(pos) ? cardUIs[pos] : null;
                string cardName = card != null ? card.CardDefinition.displayName : "null";
                string uiName = ui != null ? ui.name : "null";
                string sync = (card != null && ui != null) ? "정상"
                            : (card != null && ui == null) ? "⚠️ 카드만 존재"
                            : (card == null && ui != null) ? "⚠️ UI만 존재"
                            : "비어있음";
                Debug.Log($"[HandSync] 슬롯: {pos}, 카드: {cardName}, 카드UI: {uiName}, 상태: {sync}");
                if (sync != "정상" && sync != "비어있음")
                    Debug.LogWarning($"[HandSync][경고] 슬롯 {pos} 동기화 깨짐: {sync}");
            }
        }

        /// <summary>
        /// 카드UI가 슬롯에서 분리(부모 변경)될 때 동기화 보장용 함수
        /// </summary>
        public void OnCardUIDetachedFromSlot(SkillCardSlotPosition pos)
        {
            if (cardUIs.ContainsKey(pos)) cardUIs[pos] = null;
            if (cards.ContainsKey(pos)) cards[pos] = null;
            LogHandSlotSyncState();
        }

        // IPlayerHandManager.GetPlayer() 구현
        public IPlayerCharacter GetPlayer()
        {
            return owner;
        }

        // 카드 데이터와 UI 동기화 함수 추가
        public void SyncHandSlotUI()
        {
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var card = GetCardInSlot(pos);
                var ui = GetCardUIInSlot(pos);
                var slot = slotRegistry.GetPlayerHandSlot(pos);

                if (card != null && (ui == null || (ui is UnityEngine.Object unityObj && unityObj == null)))
                {
                    // UI가 없으면 강제로 생성
                    if (slot != null)
                    {
                        var newUI = slot.AttachCard(card, cardUIPrefab);
                        if (newUI != null)
                            cardUIs[pos] = newUI;
                        else
                            Debug.LogWarning($"[PlayerHandManager] SyncHandSlotUI: AttachCard 실패 - {pos}");
                    }
                    else
                    {
                        Debug.LogError($"[PlayerHandManager] SyncHandSlotUI: 핸드 슬롯 UI 없음 - {pos}");
                    }
                }
            }
        }

        #endregion

        #region 카드 순환 시스템 연동

        /// <summary>
        /// 카드 순환 시스템을 초기화합니다.
        /// </summary>
        public void InitializeCardCirculationSystem()
        {
            if (circulationSystem == null)
            {
                Debug.LogError("[PlayerHandManager] 카드 순환 시스템이 주입되지 않았습니다.");
                return;
            }

            // 현재 핸드의 카드들을 카드 순환 시스템에 초기화
            var initialCards = new List<ISkillCard>();
            foreach (var card in cards.Values)
            {
                if (card != null)
                    initialCards.Add(card);
            }

            circulationSystem.Initialize(initialCards);
            Debug.Log($"[PlayerHandManager] 카드 순환 시스템 초기화 완료: {initialCards.Count}장");
        }

        /// <summary>
        /// 턴 시작 시 카드를 드로우합니다.
        /// </summary>
        public void DrawCardsForTurn()
        {
            if (turnBasedCardManager == null)
            {
                Debug.LogError("[PlayerHandManager] 턴 기반 카드 매니저가 주입되지 않았습니다.");
                return;
            }

            turnBasedCardManager.StartTurn();
            var drawnCards = turnBasedCardManager.GetCurrentTurnCards();
            
            // 드로우된 카드들을 핸드 슬롯에 배치
            for (int i = 0; i < drawnCards.Count && i < 3; i++)
            {
                var card = drawnCards[i];
                var slotPosition = (SkillCardSlotPosition)(i + 1); // PLAYER_SLOT_1, PLAYER_SLOT_2, PLAYER_SLOT_3
                
                cards[slotPosition] = card;
                
                var slot = slotRegistry.GetPlayerHandSlot(slotPosition);
                if (slot != null)
                {
                    var ui = slot.AttachCard(card, cardUIPrefab);
                    if (ui != null) cardUIs[slotPosition] = ui;
                }
            }

            Debug.Log($"[PlayerHandManager] 턴 드로우 완료: {drawnCards.Count}장");
        }

        /// <summary>
        /// 턴 종료 시 사용하지 않은 카드들을 Used Storage로 이동시킵니다.
        /// </summary>
        public void EndTurn()
        {
            if (turnBasedCardManager == null)
            {
                Debug.LogError("[PlayerHandManager] 턴 기반 카드 매니저가 주입되지 않았습니다.");
                return;
            }

            turnBasedCardManager.EndTurn();
            
            // 핸드 슬롯 정리
            foreach (var pos in cards.Keys.ToList())
            {
                cards[pos] = null;
                if (cardUIs.TryGetValue(pos, out var ui) && ui != null)
                {
                    Destroy(ui.gameObject);
                    cardUIs[pos] = null;
                }
            }

            Debug.Log("[PlayerHandManager] 턴 종료 완료 (보관함 시스템 제거됨)");
        }

        /// <summary>
        /// 카드를 사용합니다. (보관함 시스템 제거됨)
        /// </summary>
        /// <param name="card">사용할 카드</param>
        public void UseCard(ISkillCard card)
        {
            if (turnBasedCardManager == null)
            {
                Debug.LogError("[PlayerHandManager] 턴 기반 카드 매니저가 주입되지 않았습니다.");
                return;
            }

            turnBasedCardManager.UseCard(card);
            
            // 핸드에서 카드 제거
            foreach (var kvp in cards.ToList())
            {
                if (kvp.Value == card)
                {
                    cards[kvp.Key] = null;
                    if (cardUIs.TryGetValue(kvp.Key, out var ui) && ui != null)
                    {
                        Destroy(ui.gameObject);
                        cardUIs[kvp.Key] = null;
                    }
                    break;
                }
            }

            Debug.Log($"[PlayerHandManager] 카드 사용: {card.CardDefinition?.displayName ?? "Unknown"}");
        }


        /// <summary>
        /// 핸드카드를 직접 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="cards">설정할 카드들</param>
        public void SetHandCards(List<ISkillCard> cards)
        {
            if (cards == null)
            {
                Debug.LogError("[PlayerHandManager] 설정할 카드 리스트가 null입니다.");
                return;
            }

            // 기존 핸드카드 모두 제거
            ClearHand();

            // 새 카드들 배치
            for (int i = 0; i < cards.Count && i < 3; i++)
            {
                var card = cards[i];
                var slotPosition = (SkillCardSlotPosition)(i + 1); // PLAYER_SLOT_1, PLAYER_SLOT_2, PLAYER_SLOT_3
                
                if (card != null)
                {
                    RestoreCardToHand(card, slotPosition);
                }
            }

            Debug.Log($"[PlayerHandManager] 핸드카드 설정 완료: {cards.Count}장");
        }

        /// <summary>
        /// 핸드카드를 모두 제거합니다. (내부용)
        /// </summary>
        private void ClearHand()
        {
            foreach (var kvp in cards.ToList())
            {
                if (kvp.Value != null)
                {
                    cards[kvp.Key] = null;
                    if (cardUIs.TryGetValue(kvp.Key, out var ui) && ui != null)
                    {
                        Destroy(ui.gameObject);
                        cardUIs[kvp.Key] = null;
                    }
                }
            }
        }

        #endregion
    }
}
