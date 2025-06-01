using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropService
    {
        private readonly ICardDropValidator validator;
        private readonly ICardRegistrar registrar;
        private readonly ITurnCardRegistry registry;
        private readonly ICombatTurnManager turnManager;

        public CardDropService(
            ICardDropValidator validator,
            ICardRegistrar registrar,
            ITurnCardRegistry registry,
            ICombatTurnManager turnManager)
        {
            this.validator = validator;
            this.registrar = registrar;
            this.registry = registry;
            this.turnManager = turnManager;
        }

        public bool TryDropCard(ISkillCard card, SkillCardUI ui, ICombatCardSlot slot, out string message)
        {
            if (!validator.IsValidDrop(card, slot, out message))
                return false;

            registrar.RegisterCard(slot, card, ui);

            var execSlot = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            registry.RegisterPlayerCard(execSlot, card);
            turnManager?.RegisterPlayerCard(execSlot, card);

            return true;
        }
    }
}
