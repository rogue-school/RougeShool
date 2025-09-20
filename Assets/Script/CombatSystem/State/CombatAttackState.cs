using UnityEngine;
using Game.CombatSystem.Manager;
using Game.CharacterSystem.Manager;

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
        private readonly CombatSlotManager slotManager;
        private readonly PlayerManager playerManager;
        private readonly EnemyManager enemyManager;

        #endregion

        #region 생성자

        /// <summary>
        /// 공격 상태를 초기화합니다.
        /// </summary>
        public CombatAttackState(
            TurnManager turnManager,
            CombatSlotManager slotManager,
            PlayerManager playerManager,
            EnemyManager enemyManager
        )
        {
            this.turnManager = turnManager;
            this.slotManager = slotManager;
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
            Debug.Log("<color=cyan>[STATE] CombatAttackState 진입</color>");
            
            // 새로운 시스템에서는 SlotExecutionSystem을 통해 카드 실행
            var executionSystem = SlotExecutionSystem.Instance;
            if (executionSystem != null)
            {
                // 전투 시퀀스 실행 (비동기)
                _ = executionSystem.ExecuteCombatSequence();
                
                // 사망 체크
                bool enemyDead = enemyManager.GetEnemy()?.IsDead() ?? false;
                bool playerDead = playerManager.GetPlayer()?.IsDead() ?? false;

                if (enemyDead || playerDead)
                {
                    Debug.Log("<color=cyan>[STATE] CombatAttackState → CombatResultState 전이 (캐릭터 사망)</color>");
                    // TODO: 결과 상태로 전환 로직 구현
                }
                else
                {
                    Debug.Log("<color=cyan>[STATE] CombatAttackState → 다음 턴으로 전환</color>");
                    turnManager.NextTurn();
                }
            }
            else
            {
                Debug.LogError("[CombatAttackState] SlotExecutionSystem을 찾을 수 없습니다!");
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
            Debug.Log("<color=cyan>[STATE] CombatAttackState 종료</color>"); 
        }

        #endregion
    }
}
