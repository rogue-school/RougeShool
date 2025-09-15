using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.SaveSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Interface;
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
        [Inject] private IEnemyHandManager enemyHandManager;
        [Inject] private ITurnCardRegistry turnCardRegistry;
        [Inject] private ICardCirculationSystem circulationSystem;
        [Inject] private ICombatTurnManager combatTurnManager;
        [Inject] private ICombatFlowCoordinator combatFlowCoordinator;

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
            CollectEnemyHandState(cardState);
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

        /// <summary>
        /// 적 핸드카드 상태를 수집합니다.
        /// </summary>
        public CompleteCardStateData CollectEnemyHandState()
        {
            var cardState = new CompleteCardStateData();
            CollectEnemyHandState(cardState);
            return cardState;
        }

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
            // 턴 단계를 결정하는 로직
            if (combatTurnManager == null)
                return "Unknown";
            
            // 실제 구현에서는 턴 매니저의 상태를 확인
            return "TurnPhase"; // 임시 구현
        }

        /// <summary>
        /// 카드 상태 수집이 가능한지 확인합니다.
        /// </summary>
        public bool CanCollectCardState()
        {
            return playerHandManager != null && enemyHandManager != null;
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

        /// <summary>
        /// 적 핸드카드 상태를 수집합니다.
        /// </summary>
        private void CollectEnemyHandState(CompleteCardStateData cardState)
        {
            if (enemyHandManager == null)
            {
                Debug.LogWarning("[CardStateCollector] EnemyHandManager가 없습니다.");
                return;
            }

            cardState.enemyHandSlots = new List<CardSlotData>();
            
            // 적 핸드 슬롯들 수집 (순서 중요!)
            var enemySlots = new[] { 
                SkillCardSlotPosition.ENEMY_SLOT_1, 
                SkillCardSlotPosition.ENEMY_SLOT_2, 
                SkillCardSlotPosition.ENEMY_SLOT_3 
            };
            
            foreach (var slotPos in enemySlots)
            {
                var card = enemyHandManager.GetCardInSlot(slotPos);
                var cardData = CreateCardSlotData(card, slotPos, "ENEMY");
                cardState.enemyHandSlots.Add(cardData);
            }
            
            Debug.Log($"[CardStateCollector] 적 핸드카드 수집: {cardState.enemyHandSlots.Count}장");
        }

        /// <summary>
        /// 전투 슬롯 카드 상태를 수집합니다.
        /// </summary>
        private void CollectCombatSlotState(CompleteCardStateData cardState)
        {
            if (turnCardRegistry == null)
            {
                Debug.LogWarning("[CardStateCollector] TurnCardRegistry가 없습니다.");
                return;
            }

            // 첫 번째 슬롯 카드 수집
            var firstCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.SLOT_1);
            cardState.firstSlotCard = CreateCardSlotData(firstCard, CombatSlotPosition.SLOT_1, "COMBAT");
            
            // 두 번째 슬롯 카드 수집
            var secondCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.SLOT_2);
            cardState.secondSlotCard = CreateCardSlotData(secondCard, CombatSlotPosition.SLOT_2, "COMBAT");
            
            Debug.Log($"[CardStateCollector] 전투 슬롯 카드 수집: FIRST={cardState.firstSlotCard?.cardName ?? "Empty"}, SECOND={cardState.secondSlotCard?.cardName ?? "Empty"}");
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

            // 미사용 카드들 수집
            cardState.unusedStorageCards = ConvertCardsToIds(circulationSystem.GetUnusedCards());
            
            // 사용된 카드들 수집
            cardState.usedStorageCards = ConvertCardsToIds(circulationSystem.GetUsedCards());
            
            Debug.Log($"[CardStateCollector] 카드 순환 상태 수집: Unused={cardState.unusedStorageCards.Count}장, Used={cardState.usedStorageCards.Count}장");
        }

        /// <summary>
        /// 턴 상태를 수집합니다.
        /// </summary>
        private void CollectTurnState(CompleteCardStateData cardState)
        {
            if (combatFlowCoordinator == null || combatTurnManager == null)
            {
                Debug.LogWarning("[CardStateCollector] 턴 관련 매니저가 없습니다.");
                return;
            }

            // 턴 상태 수집
            cardState.isPlayerFirst = !combatFlowCoordinator.IsEnemyFirst;
            cardState.currentTurn = combatTurnManager.GetCurrentTurn();
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
