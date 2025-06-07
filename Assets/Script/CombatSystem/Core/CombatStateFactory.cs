using Game.CombatSystem.Interface;
using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    public class CombatStateFactory : ICombatStateFactory
    {
        [Inject] private IFactory<CombatPrepareState> prepareFactory;
        [Inject] private IFactory<CombatPlayerInputState> inputFactory;
        [Inject] private IFactory<CombatFirstAttackState> firstAttackFactory;
        [Inject] private IFactory<CombatSecondAttackState> secondAttackFactory;
        [Inject] private IFactory<CombatResultState> resultFactory;
        [Inject] private IFactory<CombatVictoryState> victoryFactory;
        [Inject] private IFactory<CombatGameOverState> gameOverFactory;

        public ICombatTurnState CreatePrepareState() => prepareFactory.Create();
        public ICombatTurnState CreatePlayerInputState() => inputFactory.Create();
        public ICombatTurnState CreateFirstAttackState() => firstAttackFactory.Create();
        public ICombatTurnState CreateSecondAttackState() => secondAttackFactory.Create();
        public ICombatTurnState CreateResultState() => resultFactory.Create();
        public ICombatTurnState CreateVictoryState() => victoryFactory.Create();
        public ICombatTurnState CreateGameOverState() => gameOverFactory.Create();
    }
}
