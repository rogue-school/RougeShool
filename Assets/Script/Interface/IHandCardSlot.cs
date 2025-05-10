using Game.Slots;
using Game.Interface;

namespace Game.Interface
{
    public interface IHandCardSlot
    {
        void SetCard(ISkillCard card);
        void Clear();
        ISkillCard GetCard();

        SkillCardSlotPosition GetSlotPosition();
        SlotOwner GetOwner();

        /// <summary>
        /// 현재 슬롯에 카드가 있는지 여부
        /// </summary>
        bool HasCard();
    }
}
