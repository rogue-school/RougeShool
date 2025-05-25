using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using UnityEngine;

namespace Game.CombatSystem.Utility
{
    public static class SlotPositionUtil
    {
        public static CombatSlotPosition ToExecutionSlot(CombatFieldSlotPosition fieldPosition) =>
            fieldPosition switch
            {
                CombatFieldSlotPosition.FIELD_LEFT => CombatSlotPosition.FIRST,
                CombatFieldSlotPosition.FIELD_RIGHT => CombatSlotPosition.SECOND,
                _ => CombatSlotPosition.NONE
            };

        public static CombatFieldSlotPosition ToFieldSlot(CombatSlotPosition position) =>
            position switch
            {
                CombatSlotPosition.FIRST => CombatFieldSlotPosition.FIELD_LEFT,
                CombatSlotPosition.SECOND => CombatFieldSlotPosition.FIELD_RIGHT,
                _ => CombatFieldSlotPosition.NONE
            };

        public static SkillCardSlotPosition ToEnemyHandSlot(CombatFieldSlotPosition position)
        {
            return position switch
            {
                CombatFieldSlotPosition.FIELD_LEFT => SkillCardSlotPosition.ENEMY_SLOT_1,
                CombatFieldSlotPosition.FIELD_RIGHT => SkillCardSlotPosition.ENEMY_SLOT_2,
                _ => FallbackWithWarning()
            };

            SkillCardSlotPosition FallbackWithWarning()
            {
                Debug.LogWarning("[SlotPositionUtil] 알 수 없는 필드 위치입니다. 기본값 ENEMY_SLOT_1 반환");
                return SkillCardSlotPosition.ENEMY_SLOT_1;
            }
        }
    }
}
