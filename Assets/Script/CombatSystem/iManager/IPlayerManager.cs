using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

public interface IPlayerManager
{
    void SetPlayer(IPlayerCharacter player);
    IPlayerCharacter GetPlayer();

    void SetPlayerHandManager(IPlayerHandManager manager);
    IPlayerHandManager GetPlayerHandManager();

    void CreateAndRegisterPlayer();

    ISkillCard GetCardInSlot(SkillCardSlotPosition pos);
    ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);

    void SetPlayerCharacterSelector(IPlayerCharacterSelector selector);

    void Reset();
}