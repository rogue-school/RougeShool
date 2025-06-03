namespace Game.CombatSystem.Interface
{
    using Game.SkillCardSystem.Interface;
    using Game.SkillCardSystem.UI;

    public interface ICardReplacementHandler
    {
        void ReplaceSlotCard(ICombatCardSlot slot, ISkillCard newCard, SkillCardUI newCardUI);
    }
}
