using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    public class CombatGameOverStateFactory : IFactory<CombatGameOverState>
    {
        private readonly DiContainer container;

        public CombatGameOverStateFactory(DiContainer container)
        {
            this.container = container;
        }

        public CombatGameOverState Create()
        {
            return container.Instantiate<CombatGameOverState>();
        }
    }
}
