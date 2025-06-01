using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

public interface IPlayerCharacter : ICharacter
{
    void SetLastUsedCard(ISkillCard card);
    ISkillCard GetLastUsedCard();
    void RestoreCardToHand(ISkillCard card);
    PlayerCharacterData Data { get; }
    bool IsAlive();

    ISkillCard GetCardInHandSlot(SkillCardSlotPosition pos);
    ISkillCardUI GetCardUIInHandSlot(SkillCardSlotPosition pos);
    void InjectHandManager(IPlayerHandManager manager);

    // 누락된 메서드 추가
    void SetCharacterData(PlayerCharacterData data);
}
