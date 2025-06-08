using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    /// <summary>
    /// CombatFirstAttackState 인스턴스를 생성하는 팩토리 클래스입니다.
    /// Zenject를 이용하여 의존성을 주입합니다.
    /// </summary>
    public class CombatFirstAttackStateFactory : IFactory<CombatFirstAttackState>
    {
        #region 필드

        private readonly DiContainer container;

        #endregion

        #region 생성자

        /// <summary>
        /// DI 컨테이너를 통해 팩토리를 초기화합니다.
        /// </summary>
        /// <param name="container">Zenject DI 컨테이너</param>
        public CombatFirstAttackStateFactory(DiContainer container)
        {
            this.container = container;
        }

        #endregion

        #region 팩토리 메서드

        /// <summary>
        /// CombatFirstAttackState 인스턴스를 생성합니다.
        /// </summary>
        /// <returns>새로운 CombatFirstAttackState 인스턴스</returns>
        public CombatFirstAttackState Create()
        {
            return container.Instantiate<CombatFirstAttackState>();
        }

        #endregion
    }
}
