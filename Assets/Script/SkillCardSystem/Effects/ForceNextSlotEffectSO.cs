using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Effect
{
    [CreateAssetMenu(fileName = "ForceNextSlotEffect", menuName = "SkillEffects/ForceNextSlotEffect")]
    public class ForceNextSlotEffectSO : SkillCardEffectSO
    {
        [SerializeField] private CombatSlotPosition forcedSlot = CombatSlotPosition.FIRST;

        public override void ApplyEffect(ICardExecutionContext context, int power, ITurnStateController controller)
        {
            if (context.Target == null || context.Card == null)
            {
                Debug.LogWarning("[ForceNextSlotEffectSO] 대상 또는 카드 정보가 누락되었습니다.");
                return;
            }

            if (context.Card.IsFromPlayer())
            {
                Debug.LogWarning("[ForceNextSlotEffectSO] 플레이어 카드에는 강제 슬롯 적용 불가");
                return;
            }

            if (controller is ITurnCardRegistry registry)
            {
                registry.ReserveNextEnemySlot(forcedSlot);
                Debug.Log($"[ForceNextSlotEffectSO] 다음 적 카드 슬롯 강제 설정: {forcedSlot}");
            }
            else
            {
                Debug.LogError("[ForceNextSlotEffectSO] 컨트롤러가 ITurnCardRegistry를 구현하지 않음");
            }
        }
    }
}
