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
        /// 전투 초기화 코루틴
        /// </summary>
        private IEnumerator InitializeCombat(CombatStateContext context)
        {
            // 초기화 수행
            PerformInitialization(context);

            // 적 데이터가 있으면 초기 슬롯 셋업 수행
            if (_enemyData != null && context.TurnManager != null)
            {
                LogStateTransition($"적 데이터로 초기 슬롯 셋업 시작: {_enemyName}");

                // TurnManager를 통해 초기 슬롯 셋업
                yield return context.TurnManager.StartCoroutine(
                    context.TurnManager.SetupInitialEnemyQueueRoutine(_enemyData, _enemyName)
                );

                LogStateTransition("초기 슬롯 셋업 완료");
            }

            // 추가 초기화 대기 시간 (UI 안정화 등)
            yield return new WaitForSeconds(0.2f);

            // 플레이어 턴으로 시작
            StartPlayerTurn(context);
        }

        /// <summary>
        /// 전투 초기화 작업 수행
        /// </summary>
        private void PerformInitialization(CombatStateContext context)
        {
            // 게임 시작
            if (context.TurnManager != null)
            {
                context.TurnManager.StartGame();
                LogStateTransition("게임 시작");
            }

            // 턴 리셋
            if (context.TurnManager != null)
            {
                context.TurnManager.ResetTurn();
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
    }
}
