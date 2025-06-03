using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Service
{
    public class CardDropService
    {
        private readonly ICardDropValidator validator;
        private readonly ICardRegistrar registrar;
        private readonly ITurnCardRegistry registry;
        private readonly ICombatTurnManager turnManager;
        private readonly ICardReplacementHandler replacementHandler;

        public CardDropService(
            ICardDropValidator validator,
            ICardRegistrar registrar,
            ITurnCardRegistry registry,
            ICombatTurnManager turnManager,
            ICardReplacementHandler replacementHandler)
        {
            this.validator = validator;
            this.registrar = registrar;
            this.registry = registry;
            this.turnManager = turnManager;
            this.replacementHandler = replacementHandler;
        }

        public bool TryDropCard(ISkillCard card, SkillCardUI ui, ICombatCardSlot slot, out string message)
        {
            if (!validator.IsValidDrop(card, slot, out message))
            {
                Debug.LogWarning($"[CardDropService] 드롭 유효성 실패: {message}");
                return false;
            }

            //  교체 처리 (핸드 복귀 포함)
            replacementHandler.ReplaceSlotCard(slot, card, ui);

            //  등록기 갱신
            var execSlot = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            registry.RegisterPlayerCard(execSlot, card);
            turnManager?.RegisterPlayerCard(execSlot, card);

            Debug.Log("[CardDropService] 드롭 처리 완료");
            return true;
        }
    }
}
