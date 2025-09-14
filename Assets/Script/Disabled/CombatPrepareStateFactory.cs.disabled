using Zenject;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// CombatPrepareState를 Zenject 컨테이너를 통해 생성하는 팩토리 클래스입니다.
    /// </summary>
    public class CombatPrepareStateFactory : IFactory<CombatPrepareState>
    {
        #region Fields

        private readonly DiContainer container;

        #endregion

        #region Constructor

        /// <summary>
        /// CombatPrepareStateFactory 생성자
        /// </summary>
        /// <param name="container">의존성 주입 컨테이너</param>
        public CombatPrepareStateFactory(DiContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Factory Method

        /// <summary>
        /// CombatPrepareState 인스턴스를 생성합니다.
        /// </summary>
        public CombatPrepareState Create()
        {
            return container.Instantiate<CombatPrepareState>();
        }

        #endregion
    }
}
