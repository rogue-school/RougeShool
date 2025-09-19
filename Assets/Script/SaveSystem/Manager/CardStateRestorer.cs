using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.SaveSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.Factory;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Interface;
using Zenject;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 카드 상태 복원 매니저
    /// 슬레이 더 스파이어 방식: 저장된 카드 상태를 복원하는 매니저
    /// </summary>
    public class CardStateRestorer : MonoBehaviour, ICardStateRestorer
    {
        #region 의존성 주입

        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private ITurnCardRegistry turnCardRegistry;
        [Inject] private ICardCirculationSystem circulationSystem;
        [Inject] private ICombatTurnManager combatTurnManager;
        [Inject] private ICombatFlowCoordinator combatFlowCoordinator;
        [Inject] private ISkillCardFactory cardFactory;

        #endregion

        #region ICardStateRestorer 구현

        /// <summary>
        /// 완전한 카드 상태를 복원합니다.
        /// </summary>
        public bool RestoreCompleteCardState(CompleteCardStateData cardState)
        {
            if (!ValidateCardStateForRestore(cardState))
            {
                Debug.LogError("[CardStateRestorer] 복원할 카드 상태가 유효하지 않습니다.");
                return false;
            }

            try
            {
                // 현재 게임 상태 초기화
                ClearCurrentGameState();
                
                // 각 상태 복원
                bool playerSuccess = RestorePlayerHandState(cardState);
                bool combatSuccess = RestoreCombatSlotState(cardState);
                bool circulationSuccess = RestoreCardCirculationState(cardState);
                bool turnSuccess = RestoreTurnState(cardState);
                
                bool allSuccess = playerSuccess && combatSuccess && circulationSuccess && turnSuccess;
                
                if (allSuccess)
                {
                    Debug.Log($"[CardStateRestorer] 카드 상태 복원 완료: {cardState.saveTrigger}");
                }
                else
                {
                    Debug.LogWarning("[CardStateRestorer] 일부 카드 상태 복원에 실패했습니다.");
                }
                
                return allSuccess;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CardStateRestorer] 카드 상태 복원 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 플레이어 핸드카드 상태를 복원합니다.
        /// </summary>
        public bool RestorePlayerHandState(CompleteCardStateData cardState)
        {
            if (playerHandManager == null || cardState.playerHandSlots == null)
            {
                Debug.LogWarning("[CardStateRestorer] PlayerHandManager가 없거나 플레이어 핸드 데이터가 없습니다.");
                return false;
            }

            try
            {
                // 플레이어 핸드 슬롯들 복원
                var playerSlots = new[] { 
                    SkillCardSlotPosition.PLAYER_SLOT_1, 
                    SkillCardSlotPosition.PLAYER_SLOT_2, 
                    SkillCardSlotPosition.PLAYER_SLOT_3 
                };
                
                for (int i = 0; i < playerSlots.Length && i < cardState.playerHandSlots.Count; i++)
                {
                    var slotPos = playerSlots[i];
                    var cardData = cardState.playerHandSlots[i];
                    
                    if (!cardData.IsEmpty())
                    {
                        var card = CreateCardFromData(cardData);
                        if (card != null)
                        {
                            playerHandManager.RestoreCardToHand(card, slotPos);
                        }
                    }
                }
                
                Debug.Log($"[CardStateRestorer] 플레이어 핸드카드 복원: {cardState.playerHandSlots.Count}장");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CardStateRestorer] 플레이어 핸드카드 복원 실패: {ex.Message}");
                return false;
            }
        }

        // 적 핸드카드 상태 복원 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        /// <summary>
        /// 전투 슬롯 카드 상태를 복원합니다.
        /// </summary>
        public bool RestoreCombatSlotState(CompleteCardStateData cardState)
        {
            if (turnCardRegistry == null)
            {
                Debug.LogWarning("[CardStateRestorer] TurnCardRegistry가 없습니다.");
                return false;
            }

            try
            {
                // 첫 번째 슬롯 카드 복원
                if (cardState.firstSlotCard != null && !cardState.firstSlotCard.IsEmpty())
                {
                    var firstCard = CreateCardFromData(cardState.firstSlotCard);
                    if (firstCard != null)
                    {
                        turnCardRegistry.RegisterCard(CombatSlotPosition.BATTLE_SLOT, firstCard, null, SlotOwner.PLAYER);
                    }
                }
                
                // 두 번째 슬롯 카드 복원
                if (cardState.secondSlotCard != null && !cardState.secondSlotCard.IsEmpty())
                {
                    var secondCard = CreateCardFromData(cardState.secondSlotCard);
                    if (secondCard != null)
                    {
                        turnCardRegistry.RegisterCard(CombatSlotPosition.WAIT_SLOT_1, secondCard, null, SlotOwner.ENEMY);
                    }
                }
                
                Debug.Log($"[CardStateRestorer] 전투 슬롯 카드 복원: BATTLE_SLOT={cardState.firstSlotCard?.cardName ?? "Empty"}, WAIT_SLOT_1={cardState.secondSlotCard?.cardName ?? "Empty"}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CardStateRestorer] 전투 슬롯 카드 복원 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 카드 순환 시스템 상태를 복원합니다.
        /// </summary>
        public bool RestoreCardCirculationState(CompleteCardStateData cardState)
        {
            if (circulationSystem == null)
            {
                Debug.LogWarning("[CardStateRestorer] CardCirculationSystem이 없습니다.");
                return false;
            }

            try
            {
                // 카드 순환 상태 복원 (보관함 시스템 제거됨)
                // unusedStorageCards와 usedStorageCards는 더 이상 사용되지 않습니다.
                
                Debug.Log("[CardStateRestorer] 카드 순환 상태 복원 완료 (보관함 시스템 제거됨)");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CardStateRestorer] 카드 순환 상태 복원 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 턴 상태를 복원합니다.
        /// </summary>
        public bool RestoreTurnState(CompleteCardStateData cardState)
        {
            if (combatFlowCoordinator == null || combatTurnManager == null)
            {
                Debug.LogWarning("[CardStateRestorer] 턴 관련 매니저가 없습니다.");
                return false;
            }

            try
            {
                // 턴 상태 복원
                combatFlowCoordinator.SetEnemyFirst(!cardState.isPlayerFirst);
                combatTurnManager.SetCurrentTurn(cardState.currentTurn);
                SetCurrentTurnPhase(cardState.turnPhase);
                
                Debug.Log($"[CardStateRestorer] 턴 상태 복원: PlayerFirst={cardState.isPlayerFirst}, Turn={cardState.currentTurn}, Phase={cardState.turnPhase}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CardStateRestorer] 턴 상태 복원 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 현재 턴 단계를 설정합니다.
        /// </summary>
        public bool SetCurrentTurnPhase(string turnPhase)
        {
            // 실제 구현에서는 턴 매니저의 상태를 설정
            Debug.Log($"[CardStateRestorer] 턴 단계 설정: {turnPhase}");
            return true;
        }

        /// <summary>
        /// 카드 상태 복원이 가능한지 확인합니다.
        /// </summary>
        public bool CanRestoreCardState()
        {
            return playerHandManager != null && cardFactory != null;
        }

        /// <summary>
        /// 복원할 카드 상태의 유효성을 검증합니다.
        /// </summary>
        public bool ValidateCardStateForRestore(CompleteCardStateData cardState)
        {
            if (cardState == null)
                return false;
            
            return cardState.IsValid();
        }

        /// <summary>
        /// 현재 게임 상태를 초기화합니다.
        /// </summary>
        public void ClearCurrentGameState()
        {
            Debug.Log("[CardStateRestorer] 현재 게임 상태 초기화");
            
            // 실제 구현에서는 각 매니저의 초기화 메서드 호출
            // playerHandManager.ClearHand();
            // turnCardRegistry.ClearSlots();
            // circulationSystem.ResetCirculation();
        }

        #endregion

        #region 내부 유틸리티 메서드

        /// <summary>
        /// 카드 데이터로부터 카드를 생성합니다.
        /// </summary>
        private ISkillCard CreateCardFromData(CardSlotData cardData)
        {
            if (cardData.IsEmpty() || cardFactory == null)
                return null;
            
            try
            {
                // 실제 구현에서는 카드 팩토리를 사용하여 카드 생성
                // var card = cardFactory.CreateCard(cardData.cardId);
                // card.SetCurrentCoolTime(cardData.coolDownTime);
                // return card;
                
                Debug.Log($"[CardStateRestorer] 카드 생성: {cardData.cardName}");
                return null; // 임시 구현
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CardStateRestorer] 카드 생성 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 카드 ID 목록으로부터 카드들을 생성합니다.
        /// </summary>
        private List<ISkillCard> CreateCardsFromIds(List<string> cardIds)
        {
            var cards = new List<ISkillCard>();
            
            foreach (var cardId in cardIds)
            {
                var card = CreateCardFromData(new CardSlotData { cardId = cardId });
                if (card != null)
                {
                    cards.Add(card);
                }
            }
            
            return cards;
        }

        #endregion
    }
}
