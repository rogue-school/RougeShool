using Zenject;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// CombatPrepareState 생성 전용 팩토리
    /// </summary>
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
