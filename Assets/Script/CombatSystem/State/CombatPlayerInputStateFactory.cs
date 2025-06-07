using Zenject;

namespace Game.CombatSystem.State
{
    public class CombatPlayerInputStateFactory : IFactory<CombatPlayerInputState>
    {
        private readonly DiContainer container;

        public CombatPlayerInputStateFactory(DiContainer container)
        {
            this.container = container;
        }

        public CombatPlayerInputState Create()
        {
            return container.Instantiate<CombatPlayerInputState>();
        }
    }
}
