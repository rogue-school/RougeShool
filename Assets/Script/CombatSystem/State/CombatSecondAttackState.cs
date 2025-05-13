using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.Utility;
using Game.CombatSystem.Enemy;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 후공 턴 상태입니다. 후공 슬롯의 스킬카드를 실행하며, 적이 사망했을 경우 무효 처리됩니다.
    /// </summary>
    public class CombatSecondAttackState : ICombatTurnState
    {
        private readonly ITurnStateController controller;

        public CombatSecondAttackState(ITurnStateController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            Debug.Log("[CombatSecondAttackState] 후공 턴 시작");

            var enemy = EnemyManager.Instance.GetCurrentEnemy();
            if (enemy == null || enemy.IsDead())
            {
                Debug.Log("[CombatSecondAttackState] 적이 이미 사망했습니다. 후공 카드 무효화");
                controller.RequestStateChange(new CombatResultState(controller));
                return;
            }

            var slot = CombatSlotManager.Instance.GetFirstSlot(CombatSlotPosition.SECOND);
            if (slot == null)
            {
                Debug.LogWarning("[CombatSecondAttackState] 후공 슬롯이 비어있습니다.");
                controller.RequestStateChange(new CombatResultState(controller));
                return;
            }

            ISkillCard card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[CombatSecondAttackState] 후공 카드가 없습니다.");
                controller.RequestStateChange(new CombatResultState(controller));
                return;
            }

            slot.ExecuteCardAutomatically();

            if (PlayerManager.Instance.GetPlayer()?.IsDead() == true)
            {
                Debug.Log("[CombatSecondAttackState] 플레이어 사망 - 게임 오버");
                GameOverManager.Instance?.ShowGameOver();
                return;
            }

            controller.RequestStateChange(new CombatResultState(controller));
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatSecondAttackState] 후공 턴 종료");
        }
    }

}
