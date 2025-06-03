using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    public interface IPlayerHandManager
    {
        void SetPlayer(IPlayerCharacter player);
        void GenerateInitialHand();
        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);
        void RestoreCardToHand(ISkillCard card);
        void RestoreCardToHand(ISkillCard card, SkillCardSlotPosition slot);
        void LogPlayerHandSlotStates();
        void EnableInput(bool enable);
        void ClearAll();
    }
}
