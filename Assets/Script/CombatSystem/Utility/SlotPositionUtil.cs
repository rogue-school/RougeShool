using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.Utility
{
    public static class SlotPositionUtil
    {
        // 필드 슬롯 위치를 실행 슬롯으로 변환
        public static CombatSlotPosition ToExecutionSlot(CombatFieldSlotPosition fieldPosition)
        {
            return fieldPosition switch
            {
                CombatFieldSlotPosition.FIELD_LEFT => CombatSlotPosition.FIRST,
                CombatFieldSlotPosition.FIELD_RIGHT => CombatSlotPosition.SECOND,
                _ => CombatSlotPosition.NONE
            };
        }

        // 기존에 정의한 것들 함께 예시로 포함 가능
        public static CombatFieldSlotPosition ToFieldSlot(CombatSlotPosition position)
        {
            return position switch
            {
                CombatSlotPosition.FIRST => CombatFieldSlotPosition.FIELD_LEFT,
                CombatSlotPosition.SECOND => CombatFieldSlotPosition.FIELD_RIGHT,
                _ => CombatFieldSlotPosition.NONE
            };
        }

        public static SkillCardSlotPosition ToEnemyHandSlot(CombatFieldSlotPosition position)
        {
            return position switch
            {
                CombatFieldSlotPosition.FIELD_LEFT => SkillCardSlotPosition.ENEMY_SLOT_1,
                CombatFieldSlotPosition.FIELD_RIGHT => SkillCardSlotPosition.ENEMY_SLOT_2,
                _ => SkillCardSlotPosition.ENEMY_SLOT_1 // fallback
            };
        }
    }
}
