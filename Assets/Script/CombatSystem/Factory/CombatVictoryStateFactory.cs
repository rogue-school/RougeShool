using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    public class CombatVictoryStateFactory : IFactory<CombatVictoryState>
    {
        private readonly DiContainer container;

        public CombatVictoryStateFactory(DiContainer container)
        {
            this.container = container;
        }

        public CombatVictoryState Create()
        {
            return container.Instantiate<CombatVictoryState>();
        }
    }
}
