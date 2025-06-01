using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.IManager
{
    public interface IPlayerManager
    {
        void SetPlayer(IPlayerCharacter player);
        IPlayerCharacter GetPlayer();
        IPlayerHandManager GetPlayerHandManager();

        void CreateAndRegisterPlayer();

        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);
        void Reset();
    }
}
