using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.Services;
using Zenject;

namespace Game.Infrastructure.DI
{
    /// <summary>
    /// 도메인 레이어 관련 DI 바인딩을 담당하는 Installer입니다.
    /// 현재는 별도의 도메인 서비스 구현이 없으므로 뼈대만 제공하며,
    /// 향후 도메인 서비스가 추가되면 이 Installer에서 바인딩합니다.
    /// </summary>
    public sealed class DomainInstaller : MonoInstaller<DomainInstaller>
    {
        /// <summary>
        /// 도메인 레이어 관련 의존성을 바인딩합니다.
        /// </summary>
        public override void InstallBindings()
        {
            BindTurnManager();
        }

        private void BindTurnManager()
        {
            Container.Bind<ITurnManager>()
                .To<DomainTurnManager>()
                .AsSingle();
        }
    }
}


