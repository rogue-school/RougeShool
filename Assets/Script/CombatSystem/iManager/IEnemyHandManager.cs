using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    public interface IEnemyHandManager
    {
        void Initialize(IEnemyCharacter enemy);
        void GenerateInitialHand();
        void FillEmptySlots();
        void AdvanceSlots();
        ISkillCard GetCardForCombat();
        ISkillCard GetSlotCard(SkillCardSlotPosition pos);
        ISkillCardUI GetCardUI(int index);
        void ClearHand();
        void LogHandSlotStates();
        SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos);
        (ISkillCard card, SkillCardUI ui) PopCardFromSlot(SkillCardSlotPosition pos);

    }
}
