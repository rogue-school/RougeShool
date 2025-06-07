namespace Game.CombatSystem.Context
{
    public class TurnContext
    {
        public bool WasEnemyDefeated { get; private set; }

        public void MarkEnemyDefeated() => WasEnemyDefeated = true;

        public void Reset() => WasEnemyDefeated = false;
    }
}
