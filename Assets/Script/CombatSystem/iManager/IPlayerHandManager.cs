using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

public interface IPlayerHandManager
{
    void Inject(IPlayerCharacter owner, IHandSlotRegistry slotRegistry, ISkillCardFactory cardFactory);
    void Initialize();
    void GenerateInitialHand();

    ISkillCard GetCardInSlot(SkillCardSlotPosition pos);
    ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);

    void RestoreCardToHand(ISkillCard card);
    void LogPlayerHandSlotStates();

    // 누락된 메서드 추가
    void EnableInput(bool isEnabled);
    void ClearAll();
}
