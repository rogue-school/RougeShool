using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    public class CombatFirstAttackStateFactory : IFactory<CombatFirstAttackState>
    {
        private readonly DiContainer container;

        public CombatFirstAttackStateFactory(DiContainer container)
        {
            this.container = container;
        }

        public CombatFirstAttackState Create()
        {
            return container.Instantiate<CombatFirstAttackState>();
        }
    }
}
