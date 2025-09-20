using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.SaveSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Interface;
using Zenject;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 카드 상태 수집 매니저
    /// 슬레이 더 스파이어 방식: 모든 카드 상태를 수집하는 매니저
    /// </summary>
    public class CardStateCollector : MonoBehaviour, ICardStateCollector
    {
        #region 의존성 주입

        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private ICardCirculationSystem circulationSystem;
        [Inject] private CombatSlotManager combatSlotManager;

        #endregion

        #region ICardStateCollector 구현

        /// <summary>
        /// 완전한 카드 상태를 수집합니다.
        /// </summary>
        public CompleteCardStateData CollectCompleteCardState(string saveTrigger)
        {
            var cardState = new CompleteCardStateData();
            
            // 메타데이터 설정
            cardState.saveTrigger = saveTrigger;
            cardState.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            cardState.sceneName = SceneManager.GetActiveScene().name;
            
            // 각 상태 수집
            CollectPlayerHandState(cardState);
            CollectCombatSlotState(cardState);
            CollectCardCirculationState(cardState);
            CollectTurnState(cardState);
            
            Debug.Log($"[CardStateCollector] 카드 상태 수집 완료: {saveTrigger}");
            return cardState;
        }

        /// <summary>
        /// 플레이어 핸드카드 상태를 수집합니다.
        /// </summary>
        public CompleteCardStateData CollectPlayerHandState()
        {
            var cardState = new CompleteCardStateData();
            CollectPlayerHandState(cardState);
            return cardState;
        }

        // 적 핸드카드 상태 수집 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        /// <summary>
        /// 전투 슬롯 카드 상태를 수집합니다.
        /// </summary>
        public CompleteCardStateData CollectCombatSlotState()
        {
            var cardState = new CompleteCardStateData();
            CollectCombatSlotState(cardState);
            return cardState;
        }

        /// <summary>
        /// 카드 순환 시스템 상태를 수집합니다.
        /// </summary>
        public CompleteCardStateData CollectCardCirculationState()
        {
            var cardState = new CompleteCardStateData();
            CollectCardCirculationState(cardState);
            return cardState;
        }

        /// <summary>
        /// 현재 턴 상태를 수집합니다.
        /// </summary>
        public CompleteCardStateData CollectTurnState()
        {
            var cardState = new CompleteCardStateData();
            CollectTurnState(cardState);
            return cardState;
        }

        /// <summary>
        /// 현재 턴 단계를 가져옵니다.
        /// </summary>
        public string GetCurrentTurnPhase()
        {
            // Note: Turn phase is now handled by the simplified TurnManager
            return "TurnPhase"; // 임시 구현
        }

        /// <summary>
        /// 카드 상태 수집이 가능한지 확인합니다.
        /// </summary>
        public bool CanCollectCardState()
        {
            return playerHandManager != null;
        }

        /// <summary>
        /// 수집된 카드 상태의 유효성을 검증합니다.
        /// </summary>
        public bool ValidateCardState(CompleteCardStateData cardState)
        {
            if (cardState == null)
                return false;
            
            return cardState.IsValid();
        }

        #endregion

        #region 내부 수집 메서드

        /// <summary>
        /// 플레이어 핸드카드 상태를 수집합니다.
        /// </summary>
        private void CollectPlayerHandState(CompleteCardStateData cardState)
        {
            if (playerHandManager == null)
            {
                Debug.LogWarning("[CardStateCollector] PlayerHandManager가 없습니다.");
                return;
            }

            cardState.playerHandSlots = new List<CardSlotData>();
            
            // 플레이어 핸드 슬롯들 수집
            var playerSlots = new[] { 
                SkillCardSlotPosition.PLAYER_SLOT_1, 
                SkillCardSlotPosition.PLAYER_SLOT_2, 
                SkillCardSlotPosition.PLAYER_SLOT_3 
            };
            
            foreach (var slotPos in playerSlots)
            {
                var card = playerHandManager.GetCardInSlot(slotPos);
                var cardData = CreateCardSlotData(card, slotPos, "PLAYER");
                cardState.playerHandSlots.Add(cardData);
            }
            
            Debug.Log($"[CardStateCollector] 플레이어 핸드카드 수집: {cardState.playerHandSlots.Count}장");
        }

        // 적 핸드카드 상태 수집 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        /// <summary>
        /// 전투 슬롯 카드 상태를 수집합니다.
        /// </summary>
        private void CollectCombatSlotState(CompleteCardStateData cardState)
        {
            if (combatSlotManager == null)
            {
                Debug.LogWarning("[CardStateCollector] CombatSlotManager가 없습니다.");
                return;
            }

            // 전투 슬롯 카드 수집
            var battleCard = combatSlotManager.GetCardInSlot(CombatSlotPosition.BATTLE_SLOT);
            cardState.firstSlotCard = CreateCardSlotData(battleCard, CombatSlotPosition.BATTLE_SLOT, "COMBAT");
            
            // 대기 슬롯 1 카드 수집
            var waitCard = combatSlotManager.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_1);
            cardState.secondSlotCard = CreateCardSlotData(waitCard, CombatSlotPosition.WAIT_SLOT_1, "COMBAT");
            
            Debug.Log($"[CardStateCollector] 전투 슬롯 카드 수집: BATTLE_SLOT={cardState.firstSlotCard?.cardName ?? "Empty"}, WAIT_SLOT_1={cardState.secondSlotCard?.cardName ?? "Empty"}");
        }

        /// <summary>
        /// 카드 순환 시스템 상태를 수집합니다.
        /// </summary>
        private void CollectCardCirculationState(CompleteCardStateData cardState)
        {
            if (circulationSystem == null)
            {
                Debug.LogWarning("[CardStateCollector] CardCirculationSystem이 없습니다.");
                return;
            }

            // 카드 순환 상태 수집 (보관함 시스템 제거됨)
            // unusedStorageCards와 usedStorageCards는 더 이상 사용되지 않습니다.
            
            Debug.Log("[CardStateCollector] 카드 순환 상태 수집 완료 (보관함 시스템 제거됨)");
        }

        /// <summary>
        /// 턴 상태를 수집합니다.
        /// </summary>
        private void CollectTurnState(CompleteCardStateData cardState)
        {
            // Note: Turn state is now handled by the simplified TurnManager
            cardState.isPlayerFirst = true; // 플레이어가 먼저 시작
            cardState.currentTurn = 1; // 임시 값
            cardState.turnPhase = GetCurrentTurnPhase();
            
            Debug.Log($"[CardStateCollector] 턴 상태 수집: PlayerFirst={cardState.isPlayerFirst}, Turn={cardState.currentTurn}, Phase={cardState.turnPhase}");
        }

        /// <summary>
        /// 카드 슬롯 데이터를 생성합니다.
        /// </summary>
        private CardSlotData CreateCardSlotData(ISkillCard card, SkillCardSlotPosition slotPos, string slotOwner)
        {
            if (card == null)
                return new CardSlotData(); // 빈 슬롯
            
            return new CardSlotData(
                card.CardDefinition?.CardId ?? "",
                card.CardDefinition?.CardName ?? "",
                card.CardDefinition?.Cost ?? 0,
                card.CardDefinition?.Description ?? "",
                card.CardDefinition?.CardType ?? ""
            )
            {
                slotPosition = (int)slotPos,
                slotOwner = slotOwner,
                slotName = slotPos.ToString(),
                isUsed = false,
                coolDownTime = card.GetCurrentCoolTime(),
                isActive = true
            };
        }

        /// <summary>
        /// 카드 슬롯 데이터를 생성합니다. (전투 슬롯용)
        /// </summary>
        private CardSlotData CreateCardSlotData(ISkillCard card, CombatSlotPosition slotPos, string slotOwner)
        {
            if (card == null)
                return new CardSlotData(); // 빈 슬롯
            
            return new CardSlotData(
                card.CardDefinition?.CardId ?? "",
                card.CardDefinition?.CardName ?? "",
                card.CardDefinition?.Cost ?? 0,
                card.CardDefinition?.Description ?? "",
                card.CardDefinition?.CardType ?? ""
            )
            {
                slotPosition = (int)slotPos,
                slotOwner = slotOwner,
                slotName = slotPos.ToString(),
                isUsed = false,
                coolDownTime = card.GetCurrentCoolTime(),
                isActive = true
            };
        }

        /// <summary>
        /// 카드 리스트를 카드 ID 리스트로 변환합니다.
        /// </summary>
        /// <param name="cards">변환할 카드 리스트</param>
        /// <returns>카드 ID 리스트</returns>
        private List<string> ConvertCardsToIds(List<ISkillCard> cards)
        {
            var cardIds = new List<string>();
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card?.CardDefinition?.CardId != null)
                    {
                        cardIds.Add(card.CardDefinition.CardId);
                    }
                }
            }
            return cardIds;
        }

        #endregion
    }
}
