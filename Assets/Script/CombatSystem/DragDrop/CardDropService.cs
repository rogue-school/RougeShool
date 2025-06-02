using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using UnityEngine;

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
            Debug.Log("[CardDropService] TryDropCard 호출됨");

            if (!validator.IsValidDrop(card, slot, out message))
            {
                Debug.LogWarning($"[CardDropService] 드롭 유효성 실패: {message}");
                return false;
            }

            if (registrar == null)
            {
                Debug.LogError("[CardDropService] ❗ registrar가 null입니다.");
                message = "Card registrar가 없습니다.";
                return false;
            }

            if (registry == null)
            {
                Debug.LogError("[CardDropService] ❗ registry가 null입니다.");
                message = "Turn registry가 없습니다.";
                return false;
            }

            if (turnManager == null)
            {
                Debug.LogError("[CardDropService] ❗ turnManager가 null입니다.");
            }

            registrar.RegisterCard(slot, card, ui);

            var execSlot = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            registry.RegisterPlayerCard(execSlot, card);
            turnManager?.RegisterPlayerCard(execSlot, card);

            Debug.Log("[CardDropService] 드롭 처리 완료");

            return true;
        }
    }
}
