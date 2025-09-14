using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.IManager;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 종료 상태. 게임 오버 루틴을 실행하고 UI를 표시합니다.
    /// </summary>
    public class CombatGameOverState : ICombatTurnState
    {
        #region 필드

        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatSlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IPlayerManager playerManager;

        #endregion

        #region 생성자

        /// <summary>
        /// CombatGameOverState 생성자
        /// </summary>
        public CombatGameOverState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatSlotRegistry slotRegistry,
            ICoroutineRunner coroutineRunner,
            IPlayerManager playerManager
        )
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.slotRegistry = slotRegistry;
            this.coroutineRunner = coroutineRunner;
            this.playerManager = playerManager;
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
            yield return flowCoordinator.PerformGameOverPhase();

            if (CheckPlayerDeath())
            {
                // TODO: 설정 창으로 메인 화면 복귀 로직 구현 예정
                Debug.Log("[CombatGameOverState] 플레이어 사망 - 설정 창으로 메인 화면 복귀 예정");
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
