using UnityEngine;
using System.Collections;
using Game.CombatSystem.Manager;
using Game.CharacterSystem.Manager;
using Game.CoreSystem.Utility;
using Game.CombatSystem.UI;

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
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합
        private readonly PlayerManager playerManager;
        private readonly EnemyManager enemyManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly Game.CombatSystem.UI.GameOverUI gameOverUI;

        #endregion

        #region 생성자

        /// <summary>
        /// CombatGameOverState 생성자
        /// </summary>
        public CombatGameOverState(
            TurnManager turnManager,
            PlayerManager playerManager,
            EnemyManager enemyManager,
            ICoroutineRunner coroutineRunner,
            Game.CombatSystem.UI.GameOverUI gameOverUI = null
        )
        {
            this.turnManager = turnManager;
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
            this.coroutineRunner = coroutineRunner;
            this.gameOverUI = gameOverUI;
        }

        #endregion

        #region 상태 메서드

        /// <summary>
        /// 상태 진입 시 게임 오버 루틴 실행
        /// </summary>
        public void EnterState()
        {
            GameLogger.LogError("CombatGameOverState 진입 - 게임 오버!", GameLogger.LogCategory.Combat);
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
            GameLogger.LogInfo("CombatGameOverState 종료", GameLogger.LogCategory.Combat); 
        }

        #endregion

        #region 내부 루틴

        /// <summary>
        /// 게임 오버 연출과 UI를 실행하는 루틴
        /// </summary>
        private IEnumerator GameOverRoutine()
        {
            GameLogger.LogError("게임 오버 처리 중...", GameLogger.LogCategory.Combat);
            
            // 게임 오버 애니메이션 대기
            yield return new WaitForSeconds(1.0f);

            if (CheckPlayerDeath())
            {
                // 게임 오버 UI 표시
                if (gameOverUI != null)
                {
                    gameOverUI.ShowGameOver();
                    GameLogger.LogInfo("게임 오버 UI 표시 완료", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning("GameOverUI를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogInfo("적 사망 - 승리 처리", GameLogger.LogCategory.Combat);
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
