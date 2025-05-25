using System.Collections.Generic;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Service
{
    public class TurnCardRegistry : ITurnCardRegistry
    {
        private readonly Dictionary<CombatSlotPosition, ISkillCard> playerCards = new();
        private ISkillCard enemyCard;
        private CombatSlotPosition? reservedEnemySlot = null;

        public void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card)
        {
            playerCards[slot] = card;
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            enemyCard = card;
        }

        public void ClearSlot(CombatSlotPosition slot)
        {
            if (playerCards.ContainsKey(slot))
                playerCards.Remove(slot);
        }

        public CombatSlotPosition? GetReservedEnemySlot()
        {
            return reservedEnemySlot;
        }

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
        }
    }
}
