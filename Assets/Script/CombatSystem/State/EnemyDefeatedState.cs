using UnityEngine;
using System.Collections;
using Game.StageSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 적 처치 상태
    /// - 적이 처치되었을 때의 정리 작업을 수행합니다
    /// - 적 카드를 모두 제거합니다
    /// - 다음 적이 있다면 전투 재개, 없다면 BattleEndState로 전환
    /// </summary>
    public class EnemyDefeatedState : BaseCombatState
    {
        public override string StateName => "EnemyDefeated";

        // 적 처치 중에는 모든 액션 차단
        public override bool AllowPlayerCardDrag => false;
        public override bool AllowEnemyAutoExecution => false;
        public override bool AllowSlotMovement => false;
        public override bool AllowTurnSwitch => false;

        private MonoBehaviour _coroutineRunner;
        private CombatStateContext _context;

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            // 컨텍스트 저장
            _context = context;

            LogStateTransition("적 처치 - 정리 시작");

            // 코루틴 실행을 위한 MonoBehaviour 찾기
            _coroutineRunner = context.StateMachine;

            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(CleanupAndProceed(context));
            }
            else
            {
                LogError("코루틴 실행을 위한 MonoBehaviour를 찾을 수 없습니다");
                // 에러 시에도 정리는 시도
                PerformCleanup(context);
                CheckNextEnemy(context);
            }
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            LogStateTransition("적 처치 정리 완료");
        }

        /// <summary>
        /// StageManager에서 다음 적 생성 완료를 알릴 때 호출되는 메서드
        /// </summary>
        public void OnNextEnemyReady()
        {
            LogStateTransition("다음 적 생성 완료 - 상태 전환 시작");
            
            // 대기 플래그 해제 (현재 사용하지 않음)
            
            // 컨텍스트가 유효한지 확인
            if (_context == null || !_context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패 - 상태 전환 건너뜀");
                return;
            }

            // 다음 적 확인 및 상태 전환
            CheckNextEnemy(_context);
        }

        /// <summary>
        /// 정리 작업 후 다음 단계로 진행하는 코루틴
        /// </summary>
        private IEnumerator CleanupAndProceed(CombatStateContext context)
        {
            LogStateTransition("적 처치 - 정리 시작");
            
            // 전투 정리 작업 수행
            PerformCleanup(context);
            
            // 정리 작업 완료 대기
            yield return new WaitForSeconds(0.3f);
            
            LogStateTransition("적 처치 - 정리 작업 완료");
            
                   // StageManager에 정리 완료 알림
                   var stageManager = Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager != null)
            {
                GameLogger.LogInfo("[EnemyDefeatedState] StageManager 발견 - 정리 완료 알림 전송", GameLogger.LogCategory.Combat);
                stageManager.OnEnemyDefeatedCleanupCompleted();
            }
            else
            {
                GameLogger.LogWarning("[EnemyDefeatedState] StageManager를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            }
            
            // 보상 처리가 완료될 때까지 대기 (StageManager에서 처리)
            // 이 상태는 보상창이 닫힐 때까지 유지됨
        }

        /// <summary>
        /// 보상 처리가 완료되었을 때 호출되는 메서드
        /// StageManager에서 보상 완료 시 호출됨
        /// </summary>
        public void OnRewardProcessCompleted()
        {
            LogStateTransition("보상 처리 완료 - 다음 상태로 전환");
            
            // 다음 상태로 전환 (IdleState)
            if (_context != null && _context.StateMachine != null)
            {
                // IdleState 대신 적절한 다음 상태로 전환
                // 현재는 보상 처리가 완료되었으므로 다음 적 대기 상태로 전환
                GameLogger.LogInfo("[EnemyDefeatedState] 보상 처리 완료 - 다음 상태로 전환", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 적 관련 정리 작업 수행
        /// 플레이어 핸드 정리 및 전투 슬롯 정리
        /// </summary>
        private void PerformCleanup(CombatStateContext context)
        {
            LogStateTransition("적 처치 - 플레이어 핸드 정리 시작");

            // 플레이어 핸드 완전 정리
            if (context.HandManager != null)
            {
                context.HandManager.ClearAll();
                LogStateTransition("플레이어 핸드 정리 완료");
            }
            else
            {
                LogWarning("HandManager가 null - 핸드 정리 건너뜀");
            }

            LogStateTransition("적 처치 - 전투/대기 슬롯 정리 시작");

                   // 전투 슬롯 정리
                   if (context.SlotRegistry != null)
                   {
                       // 모든 전투 슬롯과 대기 슬롯 정리
                       // ICardSlotRegistry에는 GetSlotsByType이 없으므로 직접 슬롯들을 정리
                       var battleSlot = Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT;
                       var waitSlots = new[]
                       {
                           Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_1,
                           Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_2,
                           Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_3,
                           Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_4
                       };
                       
                       // 전투 슬롯 정리
                       if (context.SlotRegistry.HasCardInSlot(battleSlot))
                       {
                           context.SlotRegistry.ClearSlot(battleSlot);
                       }
                       
                       // 대기 슬롯 정리 (ClearWaitSlots 메서드 사용)
                       context.SlotRegistry.ClearWaitSlots();
                       
                       LogStateTransition("전투/대기 슬롯 정리 완료");
                   }
            else
            {
                LogWarning("SlotRegistry가 null - 슬롯 정리 건너뜀");
            }

            LogStateTransition("적 처치 - 정리 작업 완료");
        }

        /// <summary>
        /// 다음 적이 있는지 확인하고 적절한 상태로 전환
        /// 올바른 로직: 새로운 적 생성 후 CombatInitState로 전환하여 초기화 수행
        /// </summary>
        private void CheckNextEnemy(CombatStateContext context)
        {
            LogStateTransition("다음 적 확인 및 상태 전환");

            // StageManager가 다음 적을 생성했는지 확인
            var enemyManager = context.EnemyManager;
            if (enemyManager != null)
            {
                var currentEnemy = enemyManager.GetCharacter();
                if (currentEnemy != null && !currentEnemy.IsDead())
                {
                    // 새로운 적이 생성되었음 - CombatInitState로 전환하여 초기화 수행
                    LogStateTransition($"새로운 적 감지: {currentEnemy.GetCharacterName()} - 전투 초기화로 전환");
                    
                    var initState = new CombatInitState();
                    
                    // 적 데이터를 CombatInitState에 전달
                    if (currentEnemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
                    {
                        var enemyData = enemyChar.CharacterData;
                        if (enemyData != null)
                        {
                            initState.SetEnemyData(enemyData, currentEnemy.GetCharacterName());
                        }
                    }
                    
                    RequestTransition(context, initState);
                    return;
                }
            }

            // 새로운 적이 없는 경우 - 전투 종료 처리 (승리)
            LogStateTransition("새로운 적 없음 - 전투 승리!");
            var battleEndState = new BattleEndState(true); // 승리
            RequestTransition(context, battleEndState);
        }
    }
}
