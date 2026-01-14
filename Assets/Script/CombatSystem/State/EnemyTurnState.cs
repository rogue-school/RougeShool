using UnityEngine;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 적 턴 상태
    /// - 적 카드가 자동으로 실행됩니다
    /// - 배틀 슬롯에 적 카드가 도달하면 자동 실행됩니다
    /// - 플레이어는 카드를 드래그할 수 없습니다
    /// </summary>
    public class EnemyTurnState : BaseCombatState
    {
        public override string StateName => "EnemyTurn";

        // 적 턴에서는 적 카드 자동 실행과 슬롯 이동 허용
        public override bool AllowPlayerCardDrag => false;
        public override bool AllowEnemyAutoExecution => true;
        public override bool AllowSlotMovement => true;
        public override bool AllowTurnSwitch => false;

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            LogStateTransition("적 턴 시작");

            // 적 턴 설정 (TurnController 사용)
            if (context.TurnController != null)
            {
                context.TurnController.SetTurnAndIncrement(Interface.TurnType.Enemy);
            }

            // 턴별 효과 처리 및 출혈 이펙트 완료 대기 후 다음 동작 진행
            if (context.StateMachine != null && context.StateMachine is MonoBehaviour stateMachineMono)
            {
                stateMachineMono.StartCoroutine(ProcessTurnEffectsAndContinue(context));
            }
            else
            {
                // Fallback: 코루틴을 시작할 수 없으면 즉시 처리
                ProcessTurnEffects(context);
                ContinueAfterTurnEffects(context);
            }
        }

        /// <summary>
        /// 턴별 효과 처리 및 출혈 이펙트 완료 후 다음 동작 진행
        /// </summary>
        private System.Collections.IEnumerator ProcessTurnEffectsAndContinue(CombatStateContext context)
        {
            // 턴별 효과 처리 (출혈 이펙트 완료 대기)
            yield return ProcessTurnEffectsCoroutine(context);

            // 출혈 이펙트 완료 후 다음 동작 진행
            ContinueAfterTurnEffects(context);
        }

        /// <summary>
        /// 턴별 효과 처리 후 다음 동작 진행
        /// </summary>
        private void ContinueAfterTurnEffects(CombatStateContext context)
        {
            // 턴별 효과 처리 후 적이 사망했는지 확인
            if (context.EnemyManager != null)
            {
                var currentEnemy = context.EnemyManager.GetCharacter();
                if (currentEnemy == null || currentEnemy.IsDead())
                {
                    LogStateTransition("턴별 효과 처리 후 적 사망 감지 - 카드 실행 건너뜀");
                    return;
                }

                // 페이즈 전환 조건 체크 (적 카드 실행 전)
                if (currentEnemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
                {
                    if (enemyChar.ShouldTransitionPhase())
                    {
                        LogStateTransition("페이즈 전환 조건 만족 - 적 턴 종료 및 안전한 상태로 전환");
                        // 페이즈 전환은 EnemyCharacter에서 자동으로 시작됨
                        // 적 턴을 종료하고 SlotMovingState로 전환하여 페이즈 전환이 완료될 때까지 안전하게 대기
                        var slotMovingState = new Game.CombatSystem.State.SlotMovingState();
                        RequestTransition(context, slotMovingState);
                        return;
                    }
                }
            }

            // 소환 트리거 확인 (턴 효과 처리 중에 소환이 트리거되었을 수 있음)
            if (context.StageManager != null && context.EnemyManager != null)
            {
                var currentEnemy = context.EnemyManager.GetCharacter();
                if (currentEnemy != null && !currentEnemy.IsDead())
                {
                    bool isSummonActive = context.StageManager.IsSummonedEnemyActive();
                    if (isSummonActive)
                    {
                        LogStateTransition("턴 효과 처리 중 소환 트리거 감지 - 즉시 SummonState로 전환");
                        
                        var summonData = context.StageManager.GetSummonTarget();
                        var originalHP = context.StageManager.GetOriginalEnemyHP();
                        
                        if (summonData != null)
                        {
                            var summonState = new SummonState(summonData, originalHP);
                            RequestTransition(context, summonState);
                            return; // 소환 상태로 전환했으므로 여기서 종료
                        }
                        else
                        {
                            Game.CoreSystem.Utility.GameLogger.LogError(
                                "[EnemyTurnState] 소환 대상 데이터가 없습니다",
                                Game.CoreSystem.Utility.GameLogger.LogCategory.Error);
                        }
                    }
                }
            }

            // 플레이어 손패 정리 (적 턴에는 플레이어가 카드를 낼 수 없음)
            if (context.HandManager != null)
            {
                context.HandManager.ClearAll();
            }

            // 배틀 슬롯의 적 카드를 즉시 실행
            CheckAndExecuteEnemyCard(context);
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            LogStateTransition("적 턴 종료");
        }

        /// <summary>
        /// 턴별 효과를 처리합니다 (가드, 출혈, 반격, 기절 등)
        /// 출혈 이펙트 완료를 기다립니다.
        /// </summary>
        private void ProcessTurnEffects(CombatStateContext context)
        {
            if (context?.TurnController == null)
            {
                LogWarning("TurnController가 null - 턴별 효과 처리 건너뜀");
                return;
            }

            // 즉시 처리 (코루틴 없이)
            context.TurnController.ProcessAllCharacterTurnEffects();
            LogStateTransition("턴별 효과 처리 완료 (가드, 출혈, 반격, 기절 등)");
        }

        /// <summary>
        /// 턴별 효과를 처리하는 코루틴 (출혈 이펙트 완료 대기)
        /// 플레이어와 적의 출혈 효과를 동시에 처리합니다.
        /// </summary>
        private System.Collections.IEnumerator ProcessTurnEffectsCoroutine(CombatStateContext context)
        {
            if (context?.TurnController == null || context.PlayerManager == null || context.EnemyManager == null)
            {
                LogWarning("필수 매니저가 null - 턴별 효과 처리 건너뜀");
                yield break;
            }

            // 플레이어와 적 캐릭터 가져오기
            var player = context.PlayerManager.GetCharacter();
            var enemy = context.EnemyManager.GetCharacter();

            // 모든 캐릭터의 출혈 효과 개수 카운트
            int totalBleedEffectCount = 0;
            if (player is Game.CharacterSystem.Core.CharacterBase playerBase)
            {
                var playerBuffs = playerBase.GetBuffs();
                foreach (var buff in playerBuffs)
                {
                    if (buff is Game.SkillCardSystem.Effect.BleedEffect)
                    {
                        totalBleedEffectCount++;
                    }
                }
            }

            if (enemy is Game.CharacterSystem.Core.CharacterBase enemyBase)
            {
                var enemyBuffs = enemyBase.GetBuffs();
                foreach (var buff in enemyBuffs)
                {
                    if (buff is Game.SkillCardSystem.Effect.BleedEffect)
                    {
                        totalBleedEffectCount++;
                    }
                }
            }

            // 출혈 효과가 없으면 즉시 처리
            if (totalBleedEffectCount == 0)
            {
                context.TurnController.ProcessAllCharacterTurnEffects();
                yield break;
            }

            // 출혈 효과가 있으면 동시에 처리
            int completedBleedEffects = 0;
            System.Action onBleedComplete = () => completedBleedEffects++;

            // 이벤트 구독 (ProcessTurnEffects 호출 전에 구독해야 함)
            Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete += onBleedComplete;

            // 모든 캐릭터의 턴 효과를 동시에 처리 (출혈 이펙트 재생 시작)
            context.TurnController.ProcessAllCharacterTurnEffects();

            // 모든 출혈 이펙트 완료 대기 (타임아웃: 최대 1.5초)
            float timeout = 1.5f;
            float elapsed = 0f;
            
            while (completedBleedEffects < totalBleedEffectCount && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 이벤트 구독 해제
            Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete -= onBleedComplete;

            if (completedBleedEffects >= totalBleedEffectCount)
            {
                LogStateTransition($"모든 출혈 이펙트 완료 ({completedBleedEffects}/{totalBleedEffectCount})");
            }
            else
            {
                LogWarning($"출혈 이펙트 완료 타임아웃 ({completedBleedEffects}/{totalBleedEffectCount}, {elapsed:F2}초 경과)");
            }
        }

        /// <summary>
        /// 배틀 슬롯의 적 카드를 확인하고 실행합니다
        /// 새로운 로직: 적 스킬카드만 실행, 플레이어 턴 마커는 무시
        /// </summary>
        private void CheckAndExecuteEnemyCard(CombatStateContext context)
        {
            if (context?.SlotRegistry == null)
            {
                LogError("SlotRegistry가 null입니다");
                return;
            }

            // 카드가 실행 중인지 확인 (시공의 폭풍 등이 턴 효과 처리 중에 실행되었을 수 있음)
            if (context.ExecutionManager != null && context.ExecutionManager.IsExecuting)
            {
                LogStateTransition("카드가 실행 중입니다 - 적 턴 종료 대기");
                // 카드 실행이 완료되면 자동으로 적 턴이 종료되므로 여기서는 아무것도 하지 않음
                return;
            }

            var battleCard = context.SlotRegistry.GetCardInSlot(
                Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);

            // 배틀 슬롯 상태 확인
            if (battleCard == null)
            {
                // 배틀 슬롯이 비어있지만 카드가 실행 중이 아니면 정상적으로 적 턴 종료
                // (시공의 폭풍 등이 턴 효과 처리 중에 실행되어 완료된 경우)
                LogStateTransition("배틀 슬롯이 비어있음 - 적 턴 종료");
                // 빈 슬롯 → 플레이어 턴으로 복귀
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
                return;
            }

            // 플레이어 턴 마커인 경우 → 플레이어 턴으로 전환
            if (IsPlayerTurnMarker(battleCard))
            {
                LogStateTransition($"플레이어 턴 마커 발견: {battleCard.GetCardName()} → 플레이어 턴으로 전환");
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
                return;
            }

            // 적 스킬카드인 경우 → 실행
            if (!battleCard.IsFromPlayer())
            {
                OnEnemyCardReady(context, battleCard);
                return;
            }

            // 예상치 못한 상황 (플레이어 스킬카드 등)
            LogWarning($"예상치 못한 카드 타입: {battleCard.GetCardName()} → 플레이어 턴으로 복귀");
            var fallbackPlayerTurnState = new PlayerTurnState();
            RequestTransition(context, fallbackPlayerTurnState);
        }

        /// <summary>
        /// 카드가 플레이어 턴 마커인지 확인합니다
        /// </summary>
        private bool IsPlayerTurnMarker(Game.SkillCardSystem.Interface.ISkillCard card)
        {
            if (card?.CardDefinition == null) return false;
            
            // PLAYER_MARKER ID이면서 플레이어 소유인 경우
            return card.CardDefinition.cardId == "PLAYER_MARKER" && card.IsFromPlayer();
        }

        /// <summary>
        /// 적 카드가 배틀 슬롯에 준비되었을 때 호출
        /// CardExecutionState로 전환합니다
        /// </summary>
        public void OnEnemyCardReady(CombatStateContext context,
            Game.SkillCardSystem.Interface.ISkillCard card)
        {
            if (context?.StateMachine == null)
            {
                LogError("StateMachine이 null입니다");
                return;
            }

            if (card == null || card.IsFromPlayer())
            {
                LogWarning("유효하지 않은 적 카드입니다");
                return;
            }

            // 시공의 폭풍 카드가 실행될 때만 턴 감소
            // 시공의 폭풍 카드는 특수 기믹이므로 실행될 때만 턴이 감소해야 함
            if (card.CardDefinition != null && card.CardDefinition.IsStormOfSpaceTimeCard())
            {
                DecrementStormOfSpaceTimeTurn(context);
            }

            // 실행 컨텍스트 설정
            context.CurrentExecutingCard = card;
            context.CurrentExecutingSlot = Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT;

            // 카드 실행 상태로 전환
            var executionState = new CardExecutionState();
            RequestTransition(context, executionState);
        }

        /// <summary>
        /// 시공의 폭풍 버프의 턴을 감소시킵니다.
        /// 시공의 폭풍 카드가 실행될 때만 호출됩니다.
        /// </summary>
        private void DecrementStormOfSpaceTimeTurn(CombatStateContext context)
        {
            if (context?.EnemyManager == null)
            {
                return;
            }

            var enemyCharacter = context.EnemyManager.GetCharacter();
            if (enemyCharacter == null)
            {
                return;
            }

            // 시공의 폭풍 버프 확인
            if (enemyCharacter is Game.CharacterSystem.Core.CharacterBase characterBase)
            {
                var stormDebuff = characterBase.GetEffect<Game.SkillCardSystem.Effect.StormOfSpaceTimeDebuff>();
                if (stormDebuff != null && stormDebuff.RemainingTurns > 0)
                {
                    // 턴 감소 (DecrementTurn 메서드 호출)
                    stormDebuff.DecrementTurn(enemyCharacter);
                    Game.CoreSystem.Utility.GameLogger.LogInfo(
                        $"[EnemyTurnState] 시공의 폭풍 카드 실행 - 시공의 폭풍 버프 턴 감소 (남은 턴: {stormDebuff.RemainingTurns})",
                        Game.CoreSystem.Utility.GameLogger.LogCategory.Combat);
                }
            }
        }

        /// <summary>
        /// 적의 턴 중에 시공의 폭풍 카드가 배틀 슬롯에 배치되었을 때 호출됩니다.
        /// 외부에서 호출 가능하도록 public으로 선언합니다.
        /// </summary>
        /// <param name="context">전투 상태 컨텍스트</param>
        public void CheckForStormOfSpaceTimeCard(CombatStateContext context)
        {
            if (context == null)
            {
                LogError("컨텍스트가 null입니다");
                return;
            }

            // 카드 실행을 다시 체크
            CheckAndExecuteEnemyCard(context);
        }
    }
}
