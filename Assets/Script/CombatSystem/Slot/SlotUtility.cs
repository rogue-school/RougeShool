using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 카드 슬롯 관련 유틸리티 기능을 제공합니다.
    /// </summary>
    public static class SlotUtility
    {
        /// <summary>
        /// 슬롯 배열의 카드들을 한 칸씩 앞으로 당기고 마지막 슬롯은 비웁니다.
        /// </summary>
        /// <param name="slots">카드 슬롯 배열</param>
        public static void AdvanceSlots(IHandCardSlot[] slots)
        {
            for (int i = 0; i < slots.Length - 1; i++)
            {
                ISkillCard nextCard = slots[i + 1].GetCard();
                slots[i].SetCard(nextCard);
            }

            slots[slots.Length - 1].Clear();
        }
    }
}
