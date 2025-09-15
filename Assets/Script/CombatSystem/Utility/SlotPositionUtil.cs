using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using UnityEngine;

namespace Game.CombatSystem.Utility
{
    public static class SlotPositionUtil
    {
        // [Obsolete] 4-슬롯 표준 전환: CombatFieldSlotPosition 경로는 제거 예정
        [System.Obsolete("4-슬롯 표준: CombatFieldSlotPosition 대신 CombatSlotPosition 사용" )]
        public static CombatSlotPosition ToExecutionSlot(CombatFieldSlotPosition fieldPosition) =>
            fieldPosition switch
            {
                CombatFieldSlotPosition.FIELD_LEFT => CombatSlotPosition.SLOT_1,
                CombatFieldSlotPosition.FIELD_RIGHT => CombatSlotPosition.SLOT_2,
                _ => CombatSlotPosition.NONE
            };

        // [Obsolete]
        [System.Obsolete("4-슬롯 표준: CombatFieldSlotPosition 대신 CombatSlotPosition 사용" )]
        public static CombatFieldSlotPosition ToFieldSlot(CombatSlotPosition position) =>
            position switch
            {
                CombatSlotPosition.SLOT_1 => CombatFieldSlotPosition.FIELD_LEFT,
                CombatSlotPosition.SLOT_2 => CombatFieldSlotPosition.FIELD_RIGHT,
                _ => CombatFieldSlotPosition.NONE
            };

        // [Obsolete]
        [System.Obsolete("4-슬롯 표준: 적 핸드 슬롯 변환은 사용하지 않음" )]
        public static SkillCardSlotPosition ToEnemyHandSlot(CombatFieldSlotPosition position)
        {
            return position switch
            {
                CombatFieldSlotPosition.FIELD_LEFT => SkillCardSlotPosition.ENEMY_SLOT_1,
                CombatFieldSlotPosition.FIELD_RIGHT => SkillCardSlotPosition.ENEMY_SLOT_2,
                _ => SkillCardSlotPosition.ENEMY_SLOT_1
            };
        }
        public static CombatSlotPosition ToCombatSlot(SkillCardSlotPosition slot)
        {
            return slot switch
            {
                SkillCardSlotPosition.PLAYER_SLOT_1 => CombatSlotPosition.SLOT_1,
                SkillCardSlotPosition.PLAYER_SLOT_2 => CombatSlotPosition.SLOT_2,
                SkillCardSlotPosition.PLAYER_SLOT_3 => CombatSlotPosition.NONE,
                _ => CombatSlotPosition.NONE
            };
        }
    }
}
