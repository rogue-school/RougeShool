using System.Collections;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

public interface IEnemyHandManager
{
    void Initialize(IEnemyCharacter enemy);
    void GenerateInitialHand();
    void FillEmptySlots();
    void AdvanceSlots();

    IEnumerator StepwiseFillSlotsFromBack(float delay = 0.5f);

    ISkillCard GetCardForCombat();
    ISkillCard GetSlotCard(SkillCardSlotPosition pos);
    ISkillCardUI GetCardUI(int index);

    void ClearHand();
    void LogHandSlotStates();

    SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos);
    (ISkillCard card, SkillCardUI ui) PopCardFromSlot(SkillCardSlotPosition pos);

    ISkillCard PickCardForSlot(SkillCardSlotPosition pos);
    void RegisterCardToSlot(SkillCardSlotPosition pos, ISkillCard card, SkillCardUI ui);

    // 누락된 메서드 추가
    (ISkillCard card, SkillCardUI ui) PopFirstAvailableCard();
}
