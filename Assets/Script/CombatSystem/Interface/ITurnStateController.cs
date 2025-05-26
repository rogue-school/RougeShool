namespace Game.CombatSystem.Interface
{
    public interface ITurnStateController
    {
        void RequestStateChange(ICombatTurnState nextState);
    }
}
