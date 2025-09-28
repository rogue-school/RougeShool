using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.CoreSystem.Utility;
using Game.StageSystem.Manager;
using Game.CombatSystem.Manager;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Manager;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 스테이지 진행 상황 데이터 수집기
    /// </summary>
    public class StageProgressCollector : MonoBehaviour
    {
        #region 데이터 수집 메서드
        
        /// <summary>
        /// 현재 진행 상황 수집
        /// </summary>
        public StageProgressData CollectCurrentProgress()
        {
            var data = new StageProgressData();
            
            try
            {
                // 스테이지 정보 수집
                CollectStageInfo(data);
                
                // 전투 상태 수집
                CollectCombatState(data);
                
                // 턴 정보 수집
                CollectTurnInfo(data);
                
                // 캐릭터 상태 수집
                CollectCharacterStates(data);
                
                // 카드 상태 수집
                CollectCardStates(data);
                
                // 메타데이터 설정
                data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                
                GameLogger.LogInfo("[StageProgressCollector] 진행 상황 수집 완료", GameLogger.LogCategory.Save);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[StageProgressCollector] 진행 상황 수집 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
            
            return data;
        }
        
        /// <summary>
        /// 스테이지 정보 수집
        /// </summary>
        private void CollectStageInfo(StageProgressData data)
        {
            var stageManager = FindFirstObjectByType<StageManager>();
            if (stageManager != null)
            {
                data.currentStageNumber = stageManager.GetCurrentStageNumber();
                data.currentStageName = stageManager.GetCurrentStage()?.stageName ?? "Unknown";
                data.progressState = stageManager.ProgressState; // 실제 진행 상태 사용
                data.currentEnemyIndex = stageManager.GetCurrentEnemyIndex(); // 실제 적 인덱스 사용
                
                GameLogger.LogInfo($"[StageProgressCollector] 스테이지 정보 수집: {data.currentStageName} (진행상태: {data.progressState}, 적인덱스: {data.currentEnemyIndex})", GameLogger.LogCategory.Save);
            }
            else
            {
                GameLogger.LogWarning("[StageProgressCollector] StageManager를 찾을 수 없습니다", GameLogger.LogCategory.Save);
            }
        }
        
        /// <summary>
        /// 전투 상태 수집
        /// </summary>
        private void CollectCombatState(StageProgressData data)
        {
            var combatFlowManager = FindFirstObjectByType<CombatFlowManager>();
            if (combatFlowManager != null)
            {
                data.isCombatActive = combatFlowManager.IsCombatActive;
                data.combatFlowState = combatFlowManager.GetCurrentCombatState(); // 실제 플로우 상태 사용
                
                GameLogger.LogInfo($"[StageProgressCollector] 전투 상태 수집: 활성={data.isCombatActive}, 플로우={data.combatFlowState}", GameLogger.LogCategory.Save);
            }
            else
            {
                GameLogger.LogWarning("[StageProgressCollector] CombatFlowManager를 찾을 수 없습니다", GameLogger.LogCategory.Save);
            }
        }
        
        /// <summary>
        /// 턴 정보 수집
        /// </summary>
        private void CollectTurnInfo(StageProgressData data)
        {
            var turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                data.turnCount = turnManager.GetTurnCount();
                data.currentTurn = turnManager.GetCurrentTurnType().ToString();
                
                GameLogger.LogInfo($"[StageProgressCollector] 턴 정보 수집: {data.currentTurn} 턴 {data.turnCount}", GameLogger.LogCategory.Save);
            }
            else
            {
                GameLogger.LogWarning("[StageProgressCollector] TurnManager를 찾을 수 없습니다", GameLogger.LogCategory.Save);
            }
        }
        
        /// <summary>
        /// 캐릭터 상태 수집
        /// </summary>
        private void CollectCharacterStates(StageProgressData data)
        {
            // 플레이어 상태 수집
            data.playerState = CollectCharacterState("Player");
            
            // 적 상태 수집
            data.enemyState = CollectCharacterState("Enemy");
        }
        
        /// <summary>
        /// 특정 캐릭터 상태 수집
        /// </summary>
        private CharacterStateData CollectCharacterState(string characterType)
        {
            var characterData = new CharacterStateData();
            
            try
            {
                if (characterType == "Player")
                {
                    var playerManager = FindFirstObjectByType<PlayerManager>();
                    if (playerManager != null)
                    {
                        var player = playerManager.GetPlayer();
                        if (player != null)
                        {
                            characterData.characterId = player.GetCharacterName();
                            characterData.currentHP = player.GetCurrentHP();
                            characterData.maxHP = player.GetMaxHP();
                            characterData.isGuarded = player.IsGuarded();
                            
                            // 버프 정보 수집 (간소화)
                            characterData.buffs = new List<BuffData>();
                            
                            GameLogger.LogInfo($"[StageProgressCollector] 플레이어 상태 수집: HP {characterData.currentHP}/{characterData.maxHP}", GameLogger.LogCategory.Save);
                        }
                    }
                }
                else if (characterType == "Enemy")
                {
                    var enemyManager = FindFirstObjectByType<EnemyManager>();
                    if (enemyManager != null)
                    {
                        var enemy = enemyManager.GetCurrentEnemy();
                        if (enemy != null)
                        {
                            characterData.characterId = enemy.GetCharacterName();
                            characterData.currentHP = enemy.GetCurrentHP();
                            characterData.maxHP = enemy.GetMaxHP();
                            characterData.isGuarded = enemy.IsGuarded();
                            
                            // 버프 정보 수집 (간소화)
                            characterData.buffs = new List<BuffData>();
                            
                            GameLogger.LogInfo($"[StageProgressCollector] 적 상태 수집: HP {characterData.currentHP}/{characterData.maxHP}", GameLogger.LogCategory.Save);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[StageProgressCollector] {characterType} 상태 수집 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
            
            return characterData;
        }
        
        /// <summary>
        /// 카드 상태 수집
        /// </summary>
        private void CollectCardStates(StageProgressData data)
        {
            // 플레이어 핸드 카드 수집
            data.playerHandCardIds = CollectPlayerHandCards();
            
            // 전투 슬롯 카드 수집(슬롯/소유자/카드ID)
            data.combatSlots = CollectCombatSlotStates();
        }
        
        /// <summary>
        /// 플레이어 핸드 카드 수집
        /// </summary>
        private List<string> CollectPlayerHandCards()
        {
            var cardIds = new List<string>();
            
            try
            {
                // HandSlotRegistry를 통해 현재 플레이어 핸드 슬롯의 카드를 수집
                var slotRegistry = FindFirstObjectByType<Game.CombatSystem.Slot.SlotRegistry>();
                var handRegistry = slotRegistry != null ? slotRegistry.GetHandSlotRegistry() : null;
                if (handRegistry != null)
                {
                    foreach (var slot in handRegistry.GetPlayerHandSlot())
                    {
                        var card = slot.GetCard();
                        var def = card?.CardDefinition;
                        if (def != null && !string.IsNullOrEmpty(def.cardId))
                        {
                            cardIds.Add(def.cardId);
                        }
                    }
                }
                
                GameLogger.LogInfo($"[StageProgressCollector] 플레이어 핸드 카드 수집: {cardIds.Count}개", GameLogger.LogCategory.Save);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[StageProgressCollector] 플레이어 핸드 카드 수집 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
            
            return cardIds;
        }
        
        /// <summary>
        /// 전투 슬롯 카드 수집
        /// </summary>
        private List<SlotCardState> CollectCombatSlotStates()
        {
            var result = new List<SlotCardState>();
            try
            {
                var slotRegistry = FindFirstObjectByType<Game.CombatSystem.Slot.SlotRegistry>();
                var combatRegistry = slotRegistry != null ? slotRegistry.GetCombatSlotRegistry() : null;
                if (combatRegistry != null)
                {
                    foreach (Game.CombatSystem.Slot.CombatSlotPosition pos in System.Enum.GetValues(typeof(Game.CombatSystem.Slot.CombatSlotPosition)))
                    {
                        var slot = combatRegistry.GetSlotByPosition(pos);
                        var card = slot?.GetCard();
                        var def = card?.CardDefinition;
                        if (def != null && !string.IsNullOrEmpty(def.cardId))
                        {
                            result.Add(new SlotCardState
                            {
                                position = pos.ToString(),
                                owner = Game.CombatSystem.Utility.SlotValidator.GetSlotOwner(pos).ToString().Replace("PLAYER","Player").Replace("ENEMY","Enemy"),
                                cardId = def.cardId
                            });
                        }
                    }
                }
                GameLogger.LogInfo($"[StageProgressCollector] 전투 슬롯 상태 수집: {result.Count}개", GameLogger.LogCategory.Save);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[StageProgressCollector] 전투 슬롯 상태 수집 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
            return result;
        }
        
        #endregion
    }
}
