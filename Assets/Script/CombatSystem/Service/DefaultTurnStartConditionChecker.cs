using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    public class DefaultTurnStartConditionChecker : ITurnStartConditionChecker
    {
        private readonly ITurnCardRegistry cardRegistry;

        public DefaultTurnStartConditionChecker(ITurnCardRegistry cardRegistry)
        {
            this.cardRegistry = cardRegistry;
        }

        public bool CanStartTurn()
        {
            bool result = cardRegistry.HasPlayerCard() && cardRegistry.HasEnemyCard();
            Debug.Log($"[ConditionChecker] CanStartTurn() => {result}");
            return result;
        }
    }
}
