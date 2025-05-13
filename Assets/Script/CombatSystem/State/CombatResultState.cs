using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
using Game.CombatSystem.Stage;
using Game.Utility;
using Game.CombatSystem.Enemy;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 후공까지 완료된 후 턴 정보를 정리하는 상태입니다.
    /// 죽은 적 제거 및 슬롯/카드 정리를 담당합니다.
    /// </summary>
    public class CombatResultState : ICombatTurnState
    {
        private readonly ITurnStateController controller;

        public CombatResultState(ITurnStateController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            Debug.Log("[CombatResultState] 전투 정리 시작");

            var enemy = EnemyManager.Instance.GetCurrentEnemy();
            if (enemy != null && enemy.IsDead())
            {
                Debug.Log("[CombatResultState] 적 사망 확인 - 제거 및 다음 적 대기");
                EnemyHandManager.Instance.ClearAllSlots();
                EnemyManager.Instance.RemoveEnemy();

                if (!StageManager.Instance.HasNextEnemy())
                {
                    Debug.Log("[CombatResultState] 스테이지의 모든 적 제거됨 → 승리 처리");
                    VictoryManager.Instance?.ShowVictory();
                    return;
                }

                StageManager.Instance.SpawnNextEnemy();
            }

            CombatSlotManager.Instance.GetFirstSlot(CombatSlotPosition.FIRST)?.Clear();
            CombatSlotManager.Instance.GetFirstSlot(CombatSlotPosition.SECOND)?.Clear();

            controller.RequestStateChange(new CombatPrepareState(controller));
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatResultState] 상태 종료");
        }
    }

}
