using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    public class CombatResultStateFactory : IFactory<CombatResultState>
    {
        private readonly DiContainer container;

        public CombatResultStateFactory(DiContainer container)
        {
            this.container = container;
        }

        public CombatResultState Create()
        {
            return container.Instantiate<CombatResultState>();
        }
    }
}
