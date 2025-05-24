using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ICombatCardSlot
    {
        CombatFieldSlotPosition GetCombatPosition();
        SlotOwner GetOwner();

        ISkillCard GetCard();
        void SetCard(ISkillCard card);

        SkillCardUI GetCardUI();
        void SetCardUI(SkillCardUI cardUI);

        void Clear();
        bool HasCard();
        bool IsEmpty();

        void ExecuteCardAutomatically();
        void ExecuteCardAutomatically(ICardExecutionContext ctx);
    }
}
