using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    /// <summary>
    /// CombatGameOverState를 생성하는 팩토리 클래스입니다.
    /// Zenject DI 컨테이너를 통해 상태 객체를 생성합니다.
    /// </summary>
    public class CombatGameOverStateFactory : IFactory<CombatGameOverState>
    {
        #region 필드

        private readonly DiContainer container;

        #endregion

        #region 생성자

        /// <summary>
        /// 생성자: Zenject의 DI 컨테이너를 주입받습니다.
        /// </summary>
        /// <param name="container">의존성 주입 컨테이너</param>
        public CombatGameOverStateFactory(DiContainer container)
        {
            this.container = container;
        }

        #endregion

        #region 팩토리 메서드

        /// <summary>
        /// CombatGameOverState 인스턴스를 생성합니다.
        /// </summary>
        public CombatGameOverState Create()
        {
            return container.Instantiate<CombatGameOverState>();
        }

        #endregion
    }
}
