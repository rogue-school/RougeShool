using Zenject;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// CombatPlayerInputState의 인스턴스를 생성하는 Zenject 팩토리입니다.
    /// </summary>
    public class CombatPlayerInputStateFactory : IFactory<CombatPlayerInputState>
    {
        #region 필드

        private readonly DiContainer container;

        #endregion

        #region 생성자

        /// <summary>
        /// 팩토리 생성자. DiContainer를 주입받습니다.
        /// </summary>
        /// <param name="container">Zenject 의존성 주입 컨테이너</param>
        public CombatPlayerInputStateFactory(DiContainer container)
        {
            this.container = container;
        }

        #endregion

        #region 메서드

        /// <summary>
        /// CombatPlayerInputState 인스턴스를 생성합니다.
        /// </summary>
        public CombatPlayerInputState Create()
        {
            return container.Instantiate<CombatPlayerInputState>();
        }

        #endregion
    }
}
