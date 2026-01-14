using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시공의 폭풍 효과를 적용하는 커맨드.
    /// 플레이어에게 목표 데미지를 입혀야 하는 기믹 디버프를 적용합니다.
    /// </summary>
    public class StormOfSpaceTimeEffectCommand : ICardEffectCommand
    {
        private readonly int targetDamage;
        private readonly int duration;
        private readonly Sprite icon;

        public StormOfSpaceTimeEffectCommand(int targetDamage = 30, int duration = 3, Sprite icon = null)
        {
            this.targetDamage = targetDamage;
            this.duration = duration;
            this.icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source is not ICharacter source)
            {
                GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] 소스가 캐릭터가 아니거나 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 적(본인)에게만 적용
            if (source.IsPlayerControlled())
            {
                GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] 플레이어에게 시공의 폭풍을 적용하려고 시도했습니다. 적에게만 적용 가능합니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 적 캐릭터 찾기 (실행 횟수 확인 및 증가용)
            var enemyCharacter = source as Game.CharacterSystem.Core.EnemyCharacter;
            
            // 실행 횟수 확인 및 증가 (리플렉션으로 private 필드 접근)
            int executionCount = 0;
            if (enemyCharacter != null)
            {
                var field = typeof(Game.CharacterSystem.Core.EnemyCharacter).GetField(
                    "stormOfSpaceTimeCardExecutionCount",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    executionCount = (int)field.GetValue(enemyCharacter);
                    // 실행 횟수 증가 (여기서 직접 증가시킴)
                    executionCount++;
                    field.SetValue(enemyCharacter, executionCount);
                    GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 시공의 폭풍 카드 실행 횟수 증가: {executionCount}번째", GameLogger.LogCategory.SkillCard);
                }
            }

            // 실행 횟수에 따라 처리
            // 1번째 카드 실행 시: executionCount = 1 (방금 증가함)
            // 2번째 카드 실행 시: executionCount = 2 (방금 증가함)
            // 3번째 카드 실행 시: executionCount = 3 (방금 증가함)
            int currentExecutionNumber = executionCount;
            
            // 기존 디버프 상태 확인 (디버깅용)
            StormOfSpaceTimeDebuff debugExistingDebuff = null;
            if (source is Game.CharacterSystem.Core.CharacterBase debugCharacterBase)
            {
                debugExistingDebuff = debugCharacterBase.GetEffect<StormOfSpaceTimeDebuff>();
            }
            
            GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 시공의 폭풍 카드 실행: {currentExecutionNumber}번째 (기존 디버프 존재: {debugExistingDebuff != null}, 기존 디버프 남은 턴: {(debugExistingDebuff != null ? debugExistingDebuff.RemainingTurns.ToString() : "N/A")})", GameLogger.LogCategory.SkillCard);

            // 1번째 카드: 버프 적용 및 강제 생성 모드 활성화
            if (currentExecutionNumber == 1)
            {
                // 기존 시공의 폭풍 디버프 확인
                StormOfSpaceTimeDebuff existingDebuff = null;
                if (source is Game.CharacterSystem.Core.CharacterBase characterBaseCheck)
                {
                    existingDebuff = characterBaseCheck.GetEffect<StormOfSpaceTimeDebuff>();
                }

                // 기존 디버프가 있으면 재사용 (무한 반복 방지)
                if (existingDebuff != null)
                {
                    GameLogger.LogWarning($"[StormOfSpaceTimeEffectCommand] 1번째 카드: 기존 시공의 폭풍 디버프가 이미 존재합니다. 재사용합니다. (남은 턴: {existingDebuff.RemainingTurns}, 누적 데미지: {existingDebuff.AccumulatedDamage}/{existingDebuff.TargetDamage})", GameLogger.LogCategory.SkillCard);
                    
                    // CharacterBase의 stormHP도 동기화 (기존 디버프의 StormHP 유지)
                    if (source is Game.CharacterSystem.Core.CharacterBase characterBaseExisting)
                    {
                        characterBaseExisting.SetStormHP(existingDebuff.StormHP);
                    }
                    
                    GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 시공의 폭풍 현재 남은 턴수: {existingDebuff.RemainingTurns}턴", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    // SkillCardDefinition의 커스텀 설정이 있으면 우선 사용
                    int finalTargetDamage = targetDamage;
                    int finalDuration = duration;
                    Sprite finalIcon = icon;

                    if (context.Card?.CardDefinition != null)
                    {
                        var cfg = context.Card.CardDefinition.configuration;
                        if (cfg != null && cfg.hasEffects)
                        {
                            foreach (var eff in cfg.effects)
                            {
                                if (eff?.effectSO is StormOfSpaceTimeEffectSO && eff.useCustomSettings)
                                {
                                    finalTargetDamage = eff.customSettings.stormOfSpaceTimeTargetDamage;
                                    finalDuration = eff.customSettings.stormOfSpaceTimeDuration;
                                    GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 커스텀 설정 사용: targetDamage={finalTargetDamage}, duration={finalDuration}", GameLogger.LogCategory.SkillCard);
                                    break;
                                }
                            }
                        }
                    }

                    // 적(본인)에게 버프 적용
                    var debuff = new StormOfSpaceTimeDebuff(finalTargetDamage, finalDuration, finalIcon);
                    source.RegisterPerTurnEffect(debuff);

                    // CharacterBase의 stormHP도 동기화
                    if (source is Game.CharacterSystem.Core.CharacterBase characterBase)
                    {
                        characterBase.SetStormHP(finalTargetDamage);
                    }

                    GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] {source.GetCharacterName()}에게 시공의 폭풍 버프 적용 (목표: {finalTargetDamage} 데미지, 추가 체력: {finalTargetDamage}, {finalDuration}턴)", GameLogger.LogCategory.SkillCard);
                    GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 시공의 폭풍 현재 남은 턴수: {debuff.RemainingTurns}턴", GameLogger.LogCategory.SkillCard);
                    existingDebuff = debuff;
                }

                // 강제 생성 모드 활성화 (리플렉션으로 설정)
                if (enemyCharacter != null)
                {
                    var shouldForceField = typeof(Game.CharacterSystem.Core.EnemyCharacter).GetField(
                        "shouldForceStormOfSpaceTimeCard",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (shouldForceField != null)
                    {
                        shouldForceField.SetValue(enemyCharacter, true);
                        // GetRandomCardEntry에서 버프 존재 여부를 확인하여 강제 생성 여부를 결정
                        GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 시공의 폭풍 카드 강제 생성 모드 활성화 (버프가 끝날 때까지 유지)", GameLogger.LogCategory.SkillCard);
                    }

                    // 남은 턴수만큼 시공의 폭풍 카드 생성
                    // context.Card.CardDefinition을 사용하여 시공의 폭풍 카드 정의 전달
                    if (context.Card?.CardDefinition != null)
                    {
                        var generateMethod = typeof(Game.CharacterSystem.Core.EnemyCharacter).GetMethod(
                            "GenerateStormOfSpaceTimeCardsForRemainingTurnsCoroutine",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        
                        if (generateMethod != null)
                        {
                            var coroutine = generateMethod.Invoke(enemyCharacter, new object[] { context.Card.CardDefinition }) as System.Collections.IEnumerator;
                            if (coroutine != null && enemyCharacter is MonoBehaviour enemyMono)
                            {
                                enemyMono.StartCoroutine(coroutine);
                                int remainingTurnsForLog = existingDebuff != null ? existingDebuff.RemainingTurns : duration;
                                GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 남은 턴수({remainingTurnsForLog}턴)만큼 시공의 폭풍 카드 생성 시작", GameLogger.LogCategory.SkillCard);
                            }
                        }
                        else
                        {
                            GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] GenerateStormOfSpaceTimeCardsForRemainingTurnsCoroutine 메서드를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                        }
                    }
                    else
                    {
                        GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] context.Card.CardDefinition이 null입니다. 카드 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                    }
                }
            }
            // 2번째 카드: 체크만 수행 (턴 감소는 전투 슬롯에 카드가 배치될 때 자동으로 처리됨)
            else if (currentExecutionNumber == 2)
            {
                // 적(본인)의 시공의 폭풍 버프 확인
                if (source is Game.CharacterSystem.Core.CharacterBase characterBase)
                {
                    var existingDebuff = characterBase.GetEffect<StormOfSpaceTimeDebuff>();
                    if (existingDebuff != null)
                    {
                        GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 2번째 카드: 목표 달성 여부 체크 - {existingDebuff.AccumulatedDamage}/{existingDebuff.TargetDamage} (목표 달성: {existingDebuff.IsTargetAchieved}, 남은 턴: {existingDebuff.RemainingTurns})", GameLogger.LogCategory.SkillCard);
                    }
                    else
                    {
                        GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] 2번째 카드: 시공의 폭풍 버프를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                    }
                }
            }
            // 3번째 카드: 체크 + 페널티 (턴이 0 이하일 때만)
            // 3번째 카드 실행 시 턴이 남아있으면 페널티 없이 계속 진행
            // 턴이 0 이하일 때만 목표 달성 여부를 체크하고 페널티 적용
            else if (currentExecutionNumber == 3)
            {
                // 적(본인)의 시공의 폭풍 버프 확인 및 페널티 적용
                // 주의: 시공의 폭풍은 적에게 버프가 적용되지만, 목표 데미지는 플레이어가 적에게 입혀야 함
                if (source is Game.CharacterSystem.Core.CharacterBase characterBase)
                {
                    var existingDebuff = characterBase.GetEffect<StormOfSpaceTimeDebuff>();
                    if (existingDebuff != null)
                    {
                        GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 3번째 카드: 목표 달성 여부 체크 - {existingDebuff.AccumulatedDamage}/{existingDebuff.TargetDamage} (목표 달성: {existingDebuff.IsTargetAchieved}, 남은 턴: {existingDebuff.RemainingTurns})", GameLogger.LogCategory.SkillCard);
                        
                        // 턴이 0 이하일 때만 페널티 적용 및 버프 만료
                        if (existingDebuff.RemainingTurns <= 0)
                        {
                            // 목표 미달성 시 페널티 적용 (플레이어에게 데미지 - 가드 및 모든 방어 효과 무시)
                            if (!existingDebuff.IsTargetAchieved)
                            {
                                int remainingDamage = existingDebuff.TargetDamage - existingDebuff.AccumulatedDamage;
                                if (remainingDamage > 0)
                                {
                                    // 플레이어 찾기
                                    var playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                                    var playerCharacter = playerManager?.GetCharacter();
                                    if (playerCharacter != null)
                                    {
                                        GameLogger.LogWarning($"[StormOfSpaceTimeEffectCommand] 3번째 카드: 목표 미달성! 플레이어에게 남은 데미지 {remainingDamage} 적용 (가드 무시)", GameLogger.LogCategory.SkillCard);
                                        // TakeDamageIgnoreGuard를 사용하여 가드 및 모든 방어 효과를 무시하고 데미지 적용
                                        playerCharacter.TakeDamageIgnoreGuard(remainingDamage);
                                    }
                                    else
                                    {
                                        GameLogger.LogError("[StormOfSpaceTimeEffectCommand] 3번째 카드: 플레이어 캐릭터를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                                    }
                                }
                                else
                                {
                                    GameLogger.LogInfo("[StormOfSpaceTimeEffectCommand] 3번째 카드: 목표 미달성이지만 남은 데미지가 0입니다.", GameLogger.LogCategory.SkillCard);
                                }
                            }
                            else
                            {
                                GameLogger.LogInfo("[StormOfSpaceTimeEffectCommand] 3번째 카드: 목표 달성! 페널티 없음", GameLogger.LogCategory.SkillCard);
                            }
                            
                            // 버프 만료 (턴이 0 이하일 때만)
                            existingDebuff.Expire();
                            GameLogger.LogInfo("[StormOfSpaceTimeEffectCommand] 3번째 카드: 시공의 폭풍 버프 만료 (턴 소진)", GameLogger.LogCategory.SkillCard);
                        }
                        else
                        {
                            // 턴이 남아있으면 페널티 없이 계속 진행 (실행 횟수만 리셋)
                            GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 3번째 카드: 턴이 남아있어 페널티 적용 안 함 (남은 턴: {existingDebuff.RemainingTurns})", GameLogger.LogCategory.SkillCard);
                        }
                    }
                    else
                    {
                        GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] 3번째 카드: 시공의 폭풍 버프를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                    }
                }

                // 버프가 만료되었을 때만 강제 생성 모드 종료 및 리셋 (리플렉션으로 설정)
                if (enemyCharacter != null)
                {
                    var shouldForceField = typeof(Game.CharacterSystem.Core.EnemyCharacter).GetField(
                        "shouldForceStormOfSpaceTimeCard",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var countField = typeof(Game.CharacterSystem.Core.EnemyCharacter).GetField(
                        "stormOfSpaceTimeCardExecutionCount",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (shouldForceField != null && countField != null)
                    {
                        // 버프가 만료되었는지 확인
                        if (source is Game.CharacterSystem.Core.CharacterBase characterBaseForCheck)
                        {
                            var debuffCheck = characterBaseForCheck.GetEffect<StormOfSpaceTimeDebuff>();
                            if (debuffCheck == null || debuffCheck.RemainingTurns <= 0)
                            {
                                // 버프가 만료되었으므로 강제 생성 모드 종료
                                shouldForceField.SetValue(enemyCharacter, false);
                                countField.SetValue(enemyCharacter, 0);
                                GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 시공의 폭풍 버프 만료 - 강제 생성 모드 종료 및 리셋", GameLogger.LogCategory.SkillCard);
                            }
                            else
                            {
                                // 버프가 아직 남아있으므로 강제 생성 모드 유지 (실행 횟수만 리셋)
                                countField.SetValue(enemyCharacter, 0);
                                GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 시공의 폭풍 버프 남음 (남은 턴: {debuffCheck.RemainingTurns}) - 강제 생성 모드 유지, 실행 횟수만 리셋", GameLogger.LogCategory.SkillCard);
                            }
                        }
                    }
                }
            }
        }
    }
}


