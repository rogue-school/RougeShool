using UnityEngine;
using UnityEngine.SceneManagement;
using Game.CoreSystem.Utility;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.SaveSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Zenject;
using Game.CharacterSystem.Manager;
using Game.StageSystem.Manager;
// duplicate using removed

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 카드 상태 수집 매니저
    /// 슬레이 더 스파이어 방식: 모든 카드 상태를 수집하는 매니저
    /// </summary>
    public class CardStateCollector : MonoBehaviour, ICardStateCollector
    {
        #region 의존성 주입

        [Inject(Optional = true)] private IPlayerHandManager playerHandManager;
        [Inject(Optional = true)] private ICardCirculationSystem circulationSystem;
        [Inject(Optional = true)] private CombatSlotRegistry combatSlotRegistry;
        [Inject(Optional = true)] private PlayerManager playerManager;
        [Inject(Optional = true)] private EnemyManager enemyManager;
        [Inject(Optional = true)] private StageManager stageManager;
        [Inject(Optional = true)] private CombatFlowManager combatFlowManager;
        [Inject(Optional = true)] private TurnManager turnManager;
        // CombatSlotManager 제거됨 - 슬롯 상태 수집 기능을 다른 방식으로 처리

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
            CollectCharacterState(cardState);
            CollectFlowAndStage(cardState);
            CollectDeckAndRng(cardState);
            
            GameLogger.LogInfo($"[CardStateCollector] 카드 상태 수집 완료: {saveTrigger}", GameLogger.LogCategory.Save);
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

        /// <summary>
        /// 런타임 저장 스냅샷을 안전하게 수집할 준비가 되었는지 확인합니다.
        /// </summary>
        public bool IsRuntimeReady()
        {
            if (playerHandManager == null) return false;
            if (circulationSystem == null) return false;
            if (combatSlotRegistry == null || !combatSlotRegistry.IsInitialized) return false;
            return true;
        }

        public void LogNotReadyReasons()
        {
            if (playerHandManager == null)
            {
                GameLogger.LogWarning("[CardStateCollector] 준비 미완료: PlayerHandManager 없음", GameLogger.LogCategory.Save);
            }
            if (circulationSystem == null)
            {
                GameLogger.LogWarning("[CardStateCollector] 준비 미완료: CardCirculationSystem 없음", GameLogger.LogCategory.Save);
            }
            if (combatSlotRegistry == null)
            {
                GameLogger.LogWarning("[CardStateCollector] 준비 미완료: CombatSlotRegistry 없음", GameLogger.LogCategory.Save);
            }
            else if (!combatSlotRegistry.IsInitialized)
            {
                GameLogger.LogWarning("[CardStateCollector] 준비 미완료: CombatSlotRegistry 초기화 전", GameLogger.LogCategory.Save);
            }
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
                GameLogger.LogWarning("[CardStateCollector] PlayerHandManager가 없습니다.", GameLogger.LogCategory.Save);
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
            
            GameLogger.LogInfo($"[CardStateCollector] 플레이어 핸드카드 수집: {cardState.playerHandSlots.Count}장", GameLogger.LogCategory.Save);
        }

        // 적 핸드카드 상태 수집 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        /// <summary>
        /// 전투 슬롯 카드 상태를 수집합니다.
        /// </summary>
        private void CollectCombatSlotState(CompleteCardStateData cardState)
        {
            if (combatSlotRegistry == null || !combatSlotRegistry.IsInitialized)
            {
                GameLogger.LogWarning("[CardStateCollector] CombatSlotRegistry가 초기화되지 않았습니다.", GameLogger.LogCategory.Save);
                return;
            }

            var battle = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.BATTLE_SLOT);
            var w1 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_1);
            var w2 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_2);
            var w3 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_3);
            var w4 = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.WAIT_SLOT_4);

            cardState.battleSlotCard = CreateCardSlotData(battle?.GetCard(), CombatSlotPosition.BATTLE_SLOT, GetOwner(battle));
            cardState.waitSlot1Card = CreateCardSlotData(w1?.GetCard(), CombatSlotPosition.WAIT_SLOT_1, GetOwner(w1));
            cardState.waitSlot2Card = CreateCardSlotData(w2?.GetCard(), CombatSlotPosition.WAIT_SLOT_2, GetOwner(w2));
            cardState.waitSlot3Card = CreateCardSlotData(w3?.GetCard(), CombatSlotPosition.WAIT_SLOT_3, GetOwner(w3));
            cardState.waitSlot4Card = CreateCardSlotData(w4?.GetCard(), CombatSlotPosition.WAIT_SLOT_4, GetOwner(w4));

            GameLogger.LogInfo("[CardStateCollector] 전투 슬롯 카드 수집 완료", GameLogger.LogCategory.Save);
        }

        private string GetOwner(ICombatCardSlot slot)
        {
            if (slot == null) return "";
            var pos = slot.Position.ToString();
            return pos.Contains("ENEMY") ? "ENEMY" : pos.Contains("WAIT") || pos.Contains("BATTLE") ? "ENEMY" : "PLAYER";
        }

        private void CollectCharacterState(CompleteCardStateData cardState)
        {
            try
            {
                var player = playerManager?.GetPlayer();
                var enemy = enemyManager?.GetEnemy();

                cardState.player = CreateCharacterSnapshot(player);
                cardState.enemy = CreateCharacterSnapshot(enemy);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CardStateCollector] 캐릭터 상태 수집 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
        }

        private CompleteCardStateData.CharacterSnapshot CreateCharacterSnapshot(ICharacter ch)
        {
            if (ch == null) return null;
            var snap = new CompleteCardStateData.CharacterSnapshot();
            snap.characterId = ch.GetCharacterName();
            snap.currentHP = ch.GetCurrentHP();
            // MaxHP가 노출되지 않는다면 current를 최대치로 캡
            try { snap.maxHP = ch.GetMaxHP(); } catch { snap.maxHP = snap.currentHP; }
            snap.isGuarded = ch.IsGuarded();
            var buffs = ch.GetBuffs();
            if (buffs != null)
            {
                foreach (var e in buffs)
                {
                    var b = new CompleteCardStateData.BuffSnapshot();
                    b.effectId = e.GetType().Name;
                    try { b.remainingTurns = e.RemainingTurns; } catch { b.remainingTurns = 1; }
                    b.isDebuff = false; // 효과 타입으로 구분 필요시 후속 개선
                    snap.buffs.Add(b);
                }
            }
            return snap;
        }

        private void CollectFlowAndStage(CompleteCardStateData cardState)
        {
            try
            {
                cardState.combatPhase = combatFlowManager != null ? combatFlowManager.GetType().Name : "";
                cardState.stageNumber = stageManager != null ? stageManager.GetCurrentStageNumber() : 0;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CardStateCollector] 플로우/스테이지 수집 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
        }

        private void CollectDeckAndRng(CompleteCardStateData cardState)
        {
            try
            {
                // 남은 덱은 PlayerDeckManager에서 조회, 여기서는 circulationSystem 사용이 제한적이므로 간략화
                var deckMgr = FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerDeckManager>();
                if (deckMgr != null)
                {
                    var allCards = deckMgr.GetAllCards();
                    if (allCards != null)
                    {
                        cardState.remainingDeckCardIds = allCards.ConvertAll(c => c != null ? c.CardId : "");
                    }
                }
                cardState.rngSeed = UnityEngine.Random.state.GetHashCode();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CardStateCollector] 덱/RNG 수집 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
        }

        /// <summary>
        /// 카드 순환 시스템 상태를 수집합니다.
        /// </summary>
        private void CollectCardCirculationState(CompleteCardStateData cardState)
        {
            if (circulationSystem == null)
            {
                GameLogger.LogWarning("[CardStateCollector] CardCirculationSystem이 없습니다.", GameLogger.LogCategory.Save);
                return;
            }

            // 카드 순환 상태 수집 (보관함 시스템 제거됨)
            // unusedStorageCards와 usedStorageCards는 더 이상 사용되지 않습니다.
            
            GameLogger.LogInfo("[CardStateCollector] 카드 순환 상태 수집 완료 (보관함 시스템 제거됨)", GameLogger.LogCategory.Save);
        }

        /// <summary>
        /// 턴 상태를 수집합니다.
        /// </summary>
        private void CollectTurnState(CompleteCardStateData cardState)
        {
            try
            {
                if (turnManager != null)
                {
                    cardState.isPlayerFirst = turnManager.GetCurrentTurnType() == TurnManager.TurnType.Player;
                    cardState.currentTurn = turnManager.GetTurnCount();
                    cardState.turnPhase = turnManager.IsPlayerTurn() ? "Player" : "Enemy";
                }
                else
                {
                    cardState.isPlayerFirst = true;
                    cardState.currentTurn = 1;
                    cardState.turnPhase = GetCurrentTurnPhase();
                }
            }
            catch
            {
                cardState.isPlayerFirst = true;
                cardState.currentTurn = 1;
                cardState.turnPhase = GetCurrentTurnPhase();
            }
            
            GameLogger.LogInfo($"[CardStateCollector] 턴 상태 수집: PlayerFirst={cardState.isPlayerFirst}, Turn={cardState.currentTurn}, Phase={cardState.turnPhase}", GameLogger.LogCategory.Save);
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
