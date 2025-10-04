using UnityEngine;
using System.Collections;

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

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

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
        /// 정리 작업 후 다음 단계로 진행하는 코루틴
        /// </summary>
        private IEnumerator CleanupAndProceed(CombatStateContext context)
        {
            // 적 카드 정리
            PerformCleanup(context);

            // 정리 애니메이션/효과를 위한 대기
            yield return new WaitForSeconds(0.3f);

            // 다음 적 확인 및 진행
            CheckNextEnemy(context);
        }

        /// <summary>
        /// 적 관련 정리 작업 수행
        /// 새로운 로직: 플레이어 핸드만 정리 (슬롯 정리는 StageManager에서 처리됨)
        /// </summary>
        private void PerformCleanup(CombatStateContext context)
        {
            LogStateTransition("적 처치 - 플레이어 핸드 정리 시작");

            // 플레이어 핸드 완전 정리 (슬롯 정리는 StageManager에서 이미 처리됨)
            if (context.HandManager != null)
            {
                context.HandManager.ClearAll();
                LogStateTransition("플레이어 핸드 정리 완료");
            }
            else
            {
                LogWarning("HandManager가 null - 핸드 정리 건너뜀");
            }

            LogStateTransition("적 처치 - 정리 작업 완료");
        }

        /// <summary>
        /// 다음 적이 있는지 확인하고 적절한 상태로 전환
        /// 새로운 로직: 새로운 적 셋업 후 플레이어 턴으로 진입
        /// </summary>
        private void CheckNextEnemy(CombatStateContext context)
        {
            LogStateTransition("새로운 적 셋업 시작");

            // 현재 적의 데이터를 가져와서 CombatInitState에 전달
            var enemyManager = context.EnemyManager;
            if (enemyManager != null)
            {
                var currentEnemy = enemyManager.GetCharacter();
                if (currentEnemy != null && currentEnemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
                {
                    var enemyData = enemyChar.CharacterData;
                    if (enemyData != null)
                    {
                        LogStateTransition($"새로운 적 데이터로 초기화: {currentEnemy.GetCharacterName()}");
                        
                        // 새로운 적 셋업을 위한 초기화 상태로 전환
                        var initState = new CombatInitState();
                        initState.SetEnemyData(enemyData, currentEnemy.GetCharacterName());
                        RequestTransition(context, initState);
                        return;
                    }
                }
            }

            // 적 데이터를 가져올 수 없는 경우 기본 초기화
            LogStateTransition("적 데이터 없음 - 기본 초기화");
            var defaultInitState = new CombatInitState();
            RequestTransition(context, defaultInitState);
        }
    }
}
