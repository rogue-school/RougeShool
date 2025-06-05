using UnityEngine;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    [CreateAssetMenu(fileName = "ForceNextSlotEffect", menuName = "SkillEffects/ForceNextSlotEffect")]
    public class ForceNextSlotEffectSO : SkillCardEffectSO
    {
        [SerializeField] private CombatSlotPosition forcedSlot = CombatSlotPosition.FIRST;

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new ForceNextSlotEffectCommand(forcedSlot);
        }

        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            if (turnManager == null)
            {
                Debug.LogWarning("[ForceNextSlotEffectSO] CombatTurnManager가 null입니다.");
                return;
            }

            var slot = (CombatSlotPosition)value;
            turnManager.ReserveNextEnemySlot(slot);
            Debug.Log($"[ForceNextSlotEffectSO] 다음 적 행동 슬롯 강제 예약: {slot}");
        }
    }
}
