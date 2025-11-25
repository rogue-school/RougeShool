using Game.CombatSystem.Manager;
using Game.CombatSystem.Slot;
using Game.Domain.Combat.Interfaces;
using Game.Infrastructure.Combat;
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
            BindCombatAdapters();
        }

        private void BindCombatAdapters()
        {
            // 도메인 슬롯 레지스트리 → Unity 전투 슬롯 레지스트리 어댑터
            Container.Bind<ISlotRegistry>()
                .To<SlotRegistryAdapter>()
                .AsSingle();

            // 도메인 전투 실행기 → Unity CombatExecutionManager 어댑터
            Container.Bind<ICombatExecutor>()
                .To<CombatExecutorAdapter>()
                .AsSingle();
        }
    }
}


