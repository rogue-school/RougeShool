using Zenject;

namespace Game.CombatSystem.State
{
    public class CombatPrepareStateFactory : IFactory<CombatPrepareState>
    {
        private readonly DiContainer container;

        public CombatPrepareStateFactory(DiContainer container)
        {
            this.container = container;
        }

        public CombatPrepareState Create()
        {
            return container.Instantiate<CombatPrepareState>();
        }
    }
}
