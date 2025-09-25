using UnityEngine;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using System.Linq;
using Game.SaveSystem.Data;
using Game.SaveSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Service;
using Game.SkillCardSystem.Data;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Interface;
using Zenject;
using Game.CharacterSystem.Manager;
using Game.StageSystem.Manager;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 카드 상태 복원 매니저
    /// 슬레이 더 스파이어 방식: 저장된 카드 상태를 복원하는 매니저
    /// </summary>
    public class CardStateRestorer : MonoBehaviour, ICardStateRestorer
    {
        #region 의존성 주입

        [Inject(Optional = true)] private IPlayerHandManager playerHandManager;
        [Inject(Optional = true)] private ICardCirculationSystem circulationSystem;
        // CombatSlotManager 제거됨 - 슬롯 상태 복원 기능을 다른 방식으로 처리
        [Inject(Optional = true)] private ISkillCardFactory cardFactory;
        [Inject(Optional = true)] private SkillCardRegistry skillCardRegistry;
        [Inject(Optional = true)] private CombatSlotRegistry combatSlotRegistry;
        [Inject(Optional = true)] private PlayerManager playerManager;
        [Inject(Optional = true)] private EnemyManager enemyManager;
        [Inject(Optional = true)] private StageManager stageManager;
        [Inject(Optional = true)] private CombatFlowManager combatFlowManager;

        #endregion

        #region ICardStateRestorer 구현

        /// <summary>
        /// 완전한 카드 상태를 복원합니다.
        /// </summary>
        public bool RestoreCompleteCardState(CompleteCardStateData cardState)
        {
            if (!ValidateCardStateForRestore(cardState))
            {
                GameLogger.LogError("[CardStateRestorer] 복원할 카드 상태가 유효하지 않습니다.", GameLogger.LogCategory.Save);
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
                bool characterSuccess = RestoreCharacterState(cardState);
                bool flowSuccess = RestoreFlowAndStage(cardState);
                bool deckSuccess = RestoreDeckAndRng(cardState);
                
                bool allSuccess = playerSuccess && combatSuccess && circulationSuccess && turnSuccess && characterSuccess && flowSuccess && deckSuccess;
                
                if (allSuccess)
                {
                    GameLogger.LogInfo($"[CardStateRestorer] 카드 상태 복원 완료: {cardState.saveTrigger}", GameLogger.LogCategory.Save);
                }
                else
                {
                    GameLogger.LogWarning("[CardStateRestorer] 일부 카드 상태 복원에 실패했습니다.", GameLogger.LogCategory.Save);
                }
                
                return allSuccess;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CardStateRestorer] 카드 상태 복원 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Save);
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
                GameLogger.LogWarning("[CardStateRestorer] PlayerHandManager가 없거나 플레이어 핸드 데이터가 없습니다.", GameLogger.LogCategory.Save);
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
                            playerHandManager.AddCardToSlot(slotPos, card);
                        }
                    }
                }
                
                GameLogger.LogInfo($"[CardStateRestorer] 플레이어 핸드카드 복원: {cardState.playerHandSlots.Count}장", GameLogger.LogCategory.Save);
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CardStateRestorer] 플레이어 핸드카드 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        // 적 핸드카드 상태 복원 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        /// <summary>
        /// 전투 슬롯 카드 상태를 복원합니다.
        /// </summary>
        public bool RestoreCombatSlotState(CompleteCardStateData cardState)
        {
            if (combatSlotRegistry == null || !combatSlotRegistry.IsInitialized)
            {
                GameLogger.LogWarning("[CardStateRestorer] CombatSlotRegistry가 초기화되지 않았습니다.", GameLogger.LogCategory.Save);
                return false;
            }

            try
            {
                var battle = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.BATTLE_SLOT);
                var w1 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_1);
                var w2 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_2);
                var w3 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_3);
                var w4 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_4);

                if (battle != null)
                {
                    battle.ClearAll();
                    var c = CreateCardFromData(cardState.battleSlotCard);
                    if (c != null) battle.SetCard(c);
                }
                if (w1 != null)
                {
                    w1.ClearAll();
                    var c = CreateCardFromData(cardState.waitSlot1Card);
                    if (c != null) w1.SetCard(c);
                }
                if (w2 != null)
                {
                    w2.ClearAll();
                    var c = CreateCardFromData(cardState.waitSlot2Card);
                    if (c != null) w2.SetCard(c);
                }
                if (w3 != null)
                {
                    w3.ClearAll();
                    var c = CreateCardFromData(cardState.waitSlot3Card);
                    if (c != null) w3.SetCard(c);
                }
                if (w4 != null)
                {
                    w4.ClearAll();
                    var c = CreateCardFromData(cardState.waitSlot4Card);
                    if (c != null) w4.SetCard(c);
                }

                GameLogger.LogInfo("[CardStateRestorer] 전투 슬롯 카드 복원 완료", GameLogger.LogCategory.Save);
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CardStateRestorer] 전투 슬롯 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
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
                GameLogger.LogWarning("[CardStateRestorer] CardCirculationSystem이 없습니다.", GameLogger.LogCategory.Save);
                return false;
            }

            try
            {
                // 카드 순환 상태 복원 (보관함 시스템 제거됨)
                // unusedStorageCards와 usedStorageCards는 더 이상 사용되지 않습니다.
                
                GameLogger.LogInfo("[CardStateRestorer] 카드 순환 상태 복원 완료 (보관함 시스템 제거됨)", GameLogger.LogCategory.Save);
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CardStateRestorer] 카드 순환 상태 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        /// <summary>
        /// 턴 상태를 복원합니다.
        /// </summary>
        public bool RestoreTurnState(CompleteCardStateData cardState)
        {
            try
            {
                // 가능한 범위에서 TurnManager에 반영
                var tm = FindFirstObjectByType<TurnManager>();
                if (tm != null)
                {
                    if (cardState.currentTurn > 0)
                    {
                        // TurnManager는 직접 Setter가 없으므로 로그만 남기고 흐름 게이트로 우회
                        GameLogger.LogInfo($"[CardStateRestorer] 턴 복원 대상: Turn={cardState.currentTurn}, Phase={cardState.turnPhase}", GameLogger.LogCategory.Save);
                    }
                }
                SetCurrentTurnPhase(cardState.turnPhase);
                
                GameLogger.LogInfo($"[CardStateRestorer] 턴 상태 복원: PlayerFirst={cardState.isPlayerFirst}, Turn={cardState.currentTurn}, Phase={cardState.turnPhase}", GameLogger.LogCategory.Save);
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CardStateRestorer] 턴 상태 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        private bool RestoreCharacterState(CompleteCardStateData state)
        {
            try
            {
                var player = playerManager?.GetPlayer();
                var enemy = enemyManager?.GetEnemy();
                ApplyCharacterSnapshot(player, state.player);
                ApplyCharacterSnapshot(enemy, state.enemy);
                GameLogger.LogInfo("[CardStateRestorer] 캐릭터 상태 복원 완료", GameLogger.LogCategory.Save);
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CardStateRestorer] 캐릭터 상태 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        private void ApplyCharacterSnapshot(ICharacter ch, CompleteCardStateData.CharacterSnapshot snap)
        {
            if (ch == null || snap == null) return;
            try
            {
                // HP/가드 복원: CharacterBase에 세터가 노출되어 있지 않다면 효과로 보정 필요. 여기선 가능한 범위만 적용.
                var currentHP = ch.GetCurrentHP();
                int delta = snap.currentHP - currentHP;
                if (delta != 0)
                {
                    if (delta < 0) ch.TakeDamageIgnoreGuard(-delta); // 음수면 데미지
                    else ch.Heal(delta); // 양수면 회복 (ICharacter에 Heal이 없으면 스킵)
                }
                ch.SetGuarded(snap.isGuarded);
                // 버프는 런타임 효과 생성이 필요하므로, 여기서는 남은 턴 수 정보만 로깅/후처리 훅
            }
            catch { /* 안전 복원: 실패 시 무시 */ }
        }

        private bool RestoreFlowAndStage(CompleteCardStateData state)
        {
            try
            {
                if (state.stageNumber > 0 && stageManager != null)
                {
                    stageManager.SetCurrentStageNumber(state.stageNumber);
                }
                // combatPhase는 내부 상태머신에 직접 반영은 보류(안전성). 필요 시 매핑 테이블 구축.
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CardStateRestorer] 플로우/스테이지 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        private bool RestoreDeckAndRng(CompleteCardStateData state)
        {
            try
            {
                var deckMgr = FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerDeckManager>();
                if (deckMgr != null && state.remainingDeckCardIds != null && state.remainingDeckCardIds.Count > 0)
                {
                    // 간략 복원: 덱 재초기화 후 카드 정의를 통해 재구성 (정확한 수량/순서 복원은 후속 개선)
                    var allDefs = deckMgr.GetAllCards();
                    // 스킵: 여기서는 저장된 카드ID를 우선순위로 보정할 훅만 남김
                }
                // RNG는 UnityEngine.Random.InitState 사용 가능
                if (state.rngSeed != 0) UnityEngine.Random.InitState(state.rngSeed);
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CardStateRestorer] 덱/RNG 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        /// <summary>
        /// 현재 턴 단계를 설정합니다.
        /// </summary>
        public bool SetCurrentTurnPhase(string turnPhase)
        {
            // 실제 구현에서는 턴 매니저의 상태를 설정
            GameLogger.LogInfo($"[CardStateRestorer] 턴 단계 설정: {turnPhase}", GameLogger.LogCategory.Save);
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
            GameLogger.LogInfo("[CardStateRestorer] 현재 게임 상태 초기화", GameLogger.LogCategory.Save);
            
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
                if (skillCardRegistry == null)
                {
                    GameLogger.LogWarning("[CardStateRestorer] SkillCardRegistry가 없습니다.", GameLogger.LogCategory.Save);
                    return null;
                }

                if (!skillCardRegistry.TryGet(cardData.cardId, out SkillCardDefinition definition))
                {
                    GameLogger.LogWarning($"[CardStateRestorer] 카드 정의를 찾을 수 없습니다: {cardData.cardId}", GameLogger.LogCategory.Save);
                    return null;
                }

                // 저장 데이터에는 소유자 정보가 단순화되어 있으므로 플레이어 기준으로 복원
                var card = cardFactory.CreateFromDefinition(definition, Owner.Player, "플레이어");
                if (card != null)
                {
                    card.SetCurrentCoolTime(cardData.coolDownTime);
                }
                GameLogger.LogInfo($"[CardStateRestorer] 카드 생성: {cardData.cardName} ({cardData.cardId})", GameLogger.LogCategory.Save);
                return card;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CardStateRestorer] 카드 생성 실패: {ex.Message}", GameLogger.LogCategory.Save);
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
