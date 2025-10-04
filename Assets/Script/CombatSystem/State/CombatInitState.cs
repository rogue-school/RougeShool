using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 초기화 상태
    /// - 전투 시작 시 진입하는 초기 상태입니다
    /// - 전투에 필요한 초기 설정을 수행합니다
    /// - 적 데이터를 받아서 초기 슬롯 셋업을 직접 수행합니다
    /// - 초기화 완료 후 PlayerTurnState로 전환합니다
    /// </summary>
    public class CombatInitState : BaseCombatState
    {
        public override string StateName => "CombatInit";

        // 초기화 중에는 모든 액션 차단
        public override bool AllowPlayerCardDrag => false;
        public override bool AllowEnemyAutoExecution => false;
        public override bool AllowSlotMovement => false;
        public override bool AllowTurnSwitch => false;

        private MonoBehaviour _coroutineRunner;
        private Game.CharacterSystem.Data.EnemyCharacterData _enemyData;
        private string _enemyName;
        private bool _skipSlotSetup = false;
        private bool _isSummonMode = false; // 소환/복귀 모드 여부

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            LogStateTransition("전투 초기화 시작");

            // 코루틴 실행을 위한 MonoBehaviour 찾기
            _coroutineRunner = context.StateMachine;

            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(InitializeCombat(context));
            }
            else
            {
                LogError("코루틴 실행을 위한 MonoBehaviour를 찾을 수 없습니다");
                // 에러 시에도 초기화는 시도
                PerformInitialization(context);
                StartPlayerTurn(context);
            }
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            LogStateTransition("전투 초기화 완료");
        }

        /// <summary>
        /// 적 데이터를 설정합니다 (상태 진입 전에 호출)
        /// </summary>
        public void SetEnemyData(Game.CharacterSystem.Data.EnemyCharacterData enemyData, string enemyName)
        {
            _enemyData = enemyData;
            _enemyName = enemyName;
        }

        /// <summary>
        /// 슬롯 설정을 건너뛰도록 설정합니다 (소환 전환 후 사용)
        /// </summary>
        public void SkipSlotSetup()
        {
            _skipSlotSetup = true;
        }

        /// <summary>
        /// 소환/복귀 모드로 설정합니다
        /// </summary>
        public void SetSummonMode(bool isSummonMode)
        {
            _isSummonMode = isSummonMode;
        }

        /// <summary>
        /// 전투 초기화 코루틴
        /// 소환/복귀와 일반 전투를 일관성 있게 처리
        /// </summary>
        private IEnumerator InitializeCombat(CombatStateContext context)
        {
            // 초기화 수행
            PerformInitialization(context);

            // 적 데이터가 있고 슬롯 설정을 건너뛰지 않는 경우에만 초기 슬롯 셋업 수행
            if (_enemyData != null && context.SlotMovement != null && !_skipSlotSetup)
            {
                LogStateTransition($"적 데이터로 초기 슬롯 셋업 시작: {_enemyName}");

                // 소환/복귀 모드라도 일반 초기 슬롯 셋업 사용 (처음 전투와 동일한 로직)
                // 자동 실행 억제는 SetupInitialEnemyQueueRoutine 내부에서 처리됨
                yield return context.StateMachine.StartCoroutine(
                    context.SlotMovement.SetupInitialEnemyQueueRoutine(_enemyData, _enemyName)
                );
                
                if (_isSummonMode)
                {
                    LogStateTransition("소환/복귀 모드 슬롯 셋업 완료 (일반 로직 사용)");
                }
                else
                {
                    LogStateTransition("초기 슬롯 셋업 완료");
                }
            }
            else if (_skipSlotSetup)
            {
                LogStateTransition("슬롯 설정 건너뜀 (이미 설정됨)");
            }

            // 소환/복귀 모드인 경우 자동 실행 억제 설정
            if (_isSummonMode && context.SlotMovement != null)
            {
                // SlotMovementController의 소환 모드 설정
                // 이는 소환/복귀 시 플레이어가 먼저 행동할 수 있도록 함
                context.SlotMovement.SetSummonMode(true);
                LogStateTransition("소환/복귀 모드 - 자동 실행 억제 설정");
                
                // 소환/복귀 모드에서는 플레이어 핸드 명시적 초기화
                if (context.HandManager != null)
                {
                    context.HandManager.ClearAll();
                    LogStateTransition("소환/복귀 모드 - 플레이어 핸드 초기화 완료");
                }
            }

            // 슬롯 셋업이 완료될 때까지 대기 (중요!)
            // SetupInitialEnemyQueueRoutine이 완전히 끝날 때까지 기다림
            LogStateTransition("슬롯 셋업 완료 대기 중...");
            
            // 슬롯 셋업 코루틴이 완료될 때까지 대기
            if (_isSummonMode)
            {
                // 소환 모드: EnemyManager를 통해 현재 활성화된 적의 데이터 사용
                var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
                if (stageManager != null)
                {
                    var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
                    var currentEnemy = enemyManager?.GetEnemy();
                    
                    if (currentEnemy != null)
                    {
                        var enemyData = currentEnemy.CharacterData as Game.CharacterSystem.Data.EnemyCharacterData;
                        var enemyName = currentEnemy.GetCharacterName();
                        
                        if (enemyData != null)
                        {
                            yield return context.StateMachine.StartCoroutine(
                                context.SlotMovement.SetupInitialEnemyQueueRoutine(enemyData, enemyName)
                            );
                            LogStateTransition($"소환 모드 슬롯 셋업 완료: {enemyName}");
                        }
                        else
                        {
                            LogError("소환된 적 데이터를 가져올 수 없습니다");
                        }
                    }
                    else
                    {
                        LogError("소환된 적을 찾을 수 없습니다");
                    }
                }
                else
                {
                    LogError("StageManager를 찾을 수 없습니다");
                }
            }
            else
            {
                // 일반 모드: 이미 전달받은 적 데이터 사용
                if (_enemyData != null && !string.IsNullOrEmpty(_enemyName))
                {
                    yield return context.StateMachine.StartCoroutine(
                        context.SlotMovement.SetupInitialEnemyQueueRoutine(_enemyData, _enemyName)
                    );
                    LogStateTransition($"일반 모드 슬롯 셋업 완료: {_enemyName}");
                }
                else
                {
                    LogError("일반 모드에서 적 데이터가 없습니다");
                }
            }
            
            // 추가 초기화 대기 시간 (UI 안정화 등)
            yield return new WaitForSeconds(0.2f);

            // 소환 모드인 경우와 일반 모드인 경우 구분
            if (_isSummonMode)
            {
                // 소환 모드: 소환된 적이 준비되면 적 턴으로 시작
                LogStateTransition("소환 모드 - 적 턴으로 전환");
                StartEnemyTurn(context);
            }
            else
            {
                // 일반 모드: 플레이어 턴으로 시작
                LogStateTransition("일반 모드 - 플레이어 턴으로 전환");
                StartPlayerTurn(context);
            }
        }

        /// <summary>
        /// 상태 전환 전 완료 검증
        /// 슬롯 셋업과 모든 초기화 작업이 완료되었는지 확인
        /// </summary>
        public override bool CanTransitionToNextState(CombatStateContext context)
        {
            LogStateTransition($"[검증] {StateName} 전환 가능 여부 확인");

            // 1. 컨텍스트 검증
            if (context == null || !context.ValidateManagers())
            {
                LogError("[검증] 컨텍스트 또는 매니저 검증 실패");
                return false;
            }

            // 2. 슬롯 셋업 완료 검증
            if (!_skipSlotSetup && context.SlotMovement != null)
            {
                // SlotMovementController의 초기 셋업 완료 여부 확인
                // 이 부분은 SlotMovementController에 상태 확인 메서드가 필요함
                LogStateTransition("[검증] 슬롯 셋업 완료 확인");
            }

            // 3. 적 데이터 검증
            if (_enemyData == null)
            {
                LogWarning("[검증] 적 데이터가 없음 - 전환 가능");
            }

            LogStateTransition("[검증] 모든 검증 통과 - 전환 가능");
            return true;
        }

        /// <summary>
        /// 상태 완료 대기
        /// 슬롯 셋업과 모든 비동기 작업이 완료될 때까지 대기
        /// </summary>
        public override System.Collections.IEnumerator WaitForCompletion(CombatStateContext context)
        {
            LogStateTransition($"[대기] {StateName} 완료 대기 시작");

            // 1. 슬롯 셋업 완료 대기
            if (!_skipSlotSetup && context.SlotMovement != null)
            {
                LogStateTransition("[대기] 슬롯 셋업 완료 대기 중...");
                
                // SlotMovementController의 초기 셋업이 완료될 때까지 대기
                // 이 부분은 SlotMovementController에 완료 확인 메서드가 필요함
                yield return new WaitForSeconds(0.5f); // 임시 대기 시간
                
                LogStateTransition("[대기] 슬롯 셋업 완료 확인");
            }

            // 2. 추가 안정화 시간
            LogStateTransition("[대기] 시스템 안정화 대기 중...");
            yield return new WaitForSeconds(0.2f);

            LogStateTransition($"[완료] {StateName} 모든 작업 완료 확인");
        }
        private void PerformInitialization(CombatStateContext context)
        {
            // 게임 시작 (TurnController 사용)
            if (context.TurnController != null)
            {
                context.TurnController.StartGame();
                LogStateTransition("게임 시작");
            }

            // 턴 리셋 (TurnController 사용)
            if (context.TurnController != null)
            {
                context.TurnController.ResetTurn();
                LogStateTransition("턴 리셋");
            }

            // 컨텍스트 리셋
            context.Reset();

            LogStateTransition("전투 초기화 작업 완료");
        }

        /// <summary>
        /// 플레이어 턴 시작
        /// </summary>
        private void StartPlayerTurn(CombatStateContext context)
        {
            LogStateTransition("플레이어 턴으로 전환");

            var playerTurnState = new PlayerTurnState();
            RequestTransition(context, playerTurnState);
        }

        /// <summary>
        /// 적 턴 시작 (소환 모드용)
        /// </summary>
        private void StartEnemyTurn(CombatStateContext context)
        {
            LogStateTransition("적 턴으로 전환");

            var enemyTurnState = new EnemyTurnState();
            RequestTransition(context, enemyTurnState);
        }
    }
}
