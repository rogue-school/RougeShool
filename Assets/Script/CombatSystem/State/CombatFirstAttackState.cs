using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
using Game.CombatSystem.State;
using Game.Utility;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 선공 턴 상태입니다. 선공 슬롯의 스킬카드를 실행합니다.
    /// </summary>
    public class CombatFirstAttackState : ICombatTurnState
    {
        private readonly ITurnStateController controller;

        public CombatFirstAttackState(ITurnStateController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            Debug.Log("[CombatFirstAttackState] 선공 턴 시작");

            var slot = CombatSlotManager.Instance.GetFirstSlot(CombatSlotPosition.FIRST);
            if (slot == null)
            {
                Debug.LogWarning("[CombatFirstAttackState] 선공 슬롯이 비어있습니다.");
                controller.RequestStateChange(new CombatSecondAttackState(controller));
                return;
            }

            ISkillCard card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[CombatFirstAttackState] 선공 카드가 없습니다.");
                controller.RequestStateChange(new CombatSecondAttackState(controller));
                return;
            }

            slot.ExecuteCardAutomatically();

            // 사망 체크
            if (PlayerManager.Instance.GetPlayer()?.IsDead() == true)
            {
                Debug.Log("[CombatFirstAttackState] 플레이어 사망 - 게임 오버");
                GameOverManager.Instance?.ShowGameOver();
                return;
            }

            if (EnemyManager.Instance.GetCurrentEnemy()?.IsDead() == true)
            {
                Debug.Log("[CombatFirstAttackState] 적 사망 감지");
            }

            controller.RequestStateChange(new CombatSecondAttackState(controller));
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatFirstAttackState] 종료");
        }
    }
}
