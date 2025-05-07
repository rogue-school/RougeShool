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
    }
}
