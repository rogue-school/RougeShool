using UnityEngine;
using System.Collections;
using Game.CombatSystem.Manager;
using Game.CharacterSystem.Manager;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 종료 상태. 게임 오버 루틴을 실행하고 UI를 표시합니다.
    /// 새로운 싱글톤 기반 아키텍처로 단순화되었습니다.
    /// </summary>
    public class CombatGameOverState
    {
        #region 필드

        private readonly TurnManager turnManager;
        private readonly CombatSlotManager slotManager;
        private readonly PlayerManager playerManager;
        private readonly EnemyManager enemyManager;
        private readonly ICoroutineRunner coroutineRunner;

        #endregion

        #region 생성자

        /// <summary>
        /// CombatGameOverState 생성자
        /// </summary>
        public CombatGameOverState(
            TurnManager turnManager,
            CombatSlotManager slotManager,
            PlayerManager playerManager,
            EnemyManager enemyManager,
            ICoroutineRunner coroutineRunner
        )
        {
            this.turnManager = turnManager;
            this.slotManager = slotManager;
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
            this.coroutineRunner = coroutineRunner;
        }

        #endregion

        #region 상태 메서드

        /// <summary>
        /// 상태 진입 시 게임 오버 루틴 실행
        /// </summary>
        public void EnterState()
        {
            Debug.Log("<color=cyan>[STATE] CombatGameOverState 진입</color>");
            coroutineRunner.RunCoroutine(GameOverRoutine());
        }

        /// <summary>
        /// 상태 실행 중 특별한 작업 없음
        /// </summary>
        public void ExecuteState() { }

        /// <summary>
        /// 상태 종료 시 특별한 작업 없음
        /// </summary>
        public void ExitState() 
        { 
            Debug.Log("<color=cyan>[STATE] CombatGameOverState 종료</color>"); 
        }

        #endregion

        #region 내부 루틴

        /// <summary>
        /// 게임 오버 연출과 UI를 실행하는 루틴
        /// </summary>
        private IEnumerator GameOverRoutine()
        {
            Debug.Log("[CombatGameOverState] 게임 오버 처리 중...");
            
            // TODO: 게임 오버 애니메이션 및 UI 표시
            yield return new WaitForSeconds(1.0f);

            if (CheckPlayerDeath())
            {
                // TODO: 설정 창으로 메인 화면 복귀 로직 구현 예정
                Debug.Log("[CombatGameOverState] 플레이어 사망 - 설정 창으로 메인 화면 복귀 예정");
            }
            else
            {
                Debug.Log("[CombatGameOverState] 적 사망 - 승리 처리");
            }
        }

        /// <summary>
        /// 플레이어가 사망했는지 확인
        /// </summary>
        private bool CheckPlayerDeath()
        {
            var player = playerManager.GetPlayer();
            return player == null || player.IsDead();
        }

        #endregion
    }
}
