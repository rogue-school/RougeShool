using Zenject;

namespace Game.Infrastructure.DI
{
    /// <summary>
    /// 저장소, 오디오, 외부 서비스 어댑터 등 인프라 레이어 의존성을 바인딩하기 위한 Installer입니다.
    /// 현재는 뼈대만 제공하며, 인프라 서비스 재작성 이후 구현을 추가합니다.
    /// </summary>
    public sealed class InfrastructureInstaller : MonoInstaller<InfrastructureInstaller>
    {
        /// <summary>
        /// 인프라 레이어 관련 의존성을 바인딩합니다.
        /// </summary>
        public override void InstallBindings()
        {
            // Phase 3에서 AudioManager 등 인프라 서비스를 재작성한 뒤
            // 이 메서드에서 구체 구현을 바인딩합니다.
        }
    }
}


