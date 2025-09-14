using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    public class CombatSecondAttackStateFactory : IFactory<CombatSecondAttackState>
    {
        private readonly DiContainer container;

        public CombatSecondAttackStateFactory(DiContainer container)
        {
            this.container = container;
        }

        public CombatSecondAttackState Create()
        {
            return container.Instantiate<CombatSecondAttackState>();
        }
    }
}
