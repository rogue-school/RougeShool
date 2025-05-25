using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    public class ForceNextSlotEffectCommand : ICardEffectCommand
    {
        private readonly CombatSlotPosition _forcedSlot;

        public ForceNextSlotEffectCommand(CombatSlotPosition forcedSlot)
        {
            _forcedSlot = forcedSlot;
        }

        public void Execute(ICardExecutionContext context, ITurnStateController controller)
        {
            if (context.Card == null || context.Target == null)
            {
                Debug.LogWarning("[ForceNextSlotEffectCommand] 카드 또는 대상 정보가 누락되었습니다.");
                return;
            }

            if (context.Card.IsFromPlayer())
            {
                Debug.LogWarning("[ForceNextSlotEffectCommand] 플레이어 카드에는 슬롯 지정 효과를 사용할 수 없습니다.");
                return;
            }

            if (controller is ITurnCardRegistry registry)
            {
                registry.ReserveNextEnemySlot(_forcedSlot);
                Debug.Log($"[ForceNextSlotEffectCommand] 다음 적 카드 슬롯 강제 설정: {_forcedSlot}");
            }
            else
            {
                Debug.LogError("[ForceNextSlotEffectCommand] 컨트롤러가 ITurnCardRegistry를 구현하지 않음");
            }
        }
    }
}
