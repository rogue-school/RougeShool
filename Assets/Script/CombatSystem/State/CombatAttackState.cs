using UnityEngine;
using Game.CombatSystem.Manager;
using Game.CharacterSystem.Manager;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 공격 상태를 나타내며, 공격 처리 후 결과 상태로 전환합니다.
    /// 새로운 싱글톤 기반 아키텍처로 단순화되었습니다.
    /// </summary>
    public class CombatAttackState
    {
        #region 필드

        private readonly TurnManager turnManager;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합
        private readonly PlayerManager playerManager;
        private readonly EnemyManager enemyManager;

        #endregion

        #region 생성자

        /// <summary>
        /// 공격 상태를 초기화합니다.
        /// </summary>
        public CombatAttackState(
            TurnManager turnManager,
            PlayerManager playerManager,
            EnemyManager enemyManager
        )
        {
            this.turnManager = turnManager;
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        #endregion

        #region 상태 메서드

        /// <summary>
        /// 공격 상태 진입 시 호출되며, 공격 처리를 수행한 후 결과 상태로 전이합니다.
        /// </summary>
        public void EnterState()
        {
            GameLogger.LogInfo("CombatAttackState 진입", GameLogger.LogCategory.Combat);
            
            // 새로운 시스템에서는 CombatExecutionManager를 통해 카드 실행
            // CombatExecutionManager는 DI로 주입되므로 별도 참조 불필요
            // 전투 시퀀스 실행 로직은 추후 구현 예정
            
            // 사망 체크
            bool enemyDead = enemyManager.GetEnemy()?.IsDead() ?? false;
            bool playerDead = playerManager.GetPlayer()?.IsDead() ?? false;

            if (enemyDead || playerDead)
            {
                GameLogger.LogInfo("CombatAttackState → CombatResultState 전이 (캐릭터 사망)", GameLogger.LogCategory.Combat);
                // TODO: 결과 상태로 전환 로직 구현
            }
            else
            {
                GameLogger.LogInfo("CombatAttackState → 다음 턴으로 전환", GameLogger.LogCategory.Combat);
                turnManager.NextTurn();
            }
        }

        /// <summary>
        /// 공격 상태 실행 중 반복적으로 호출됩니다. (현재는 비어 있음)
        /// </summary>
        public void ExecuteState() { }

        /// <summary>
        /// 상태 종료 시 호출됩니다. (현재는 비어 있음)
        /// </summary>
        public void ExitState() 
        { 
            GameLogger.LogInfo("CombatAttackState 종료", GameLogger.LogCategory.Combat); 
        }

        #endregion
    }
}
