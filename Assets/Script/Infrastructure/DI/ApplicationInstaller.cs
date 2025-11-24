using Zenject;
using Game.Application.Battle;
using Game.Application.Card;
using Game.Application.Character;
using Game.Application.Services;

namespace Game.Infrastructure.DI
{
    /// <summary>
    /// Application 레이어 유스케이스와 공용 서비스를 Zenject로 바인딩하는 Installer입니다.
    /// </summary>
    public sealed class ApplicationInstaller : MonoInstaller<ApplicationInstaller>
    {
        /// <summary>
        /// Application 레이어 관련 의존성을 바인딩합니다.
        /// </summary>
        public override void InstallBindings()
        {
            BindBuses();
            BindBattleUseCases();
            BindCharacterUseCases();
            BindCardUseCases();
        }

        private void BindBuses()
        {
            Container.Bind<IEventBus>().To<EventBus>().AsSingle();
            Container.Bind<ICommandBus>().To<CommandBus>().AsSingle();
            Container.Bind<IQueryBus>().To<QueryBus>().AsSingle();
        }

        private void BindBattleUseCases()
        {
            Container.Bind<StartCombatUseCase>().AsSingle();
            Container.Bind<ExecuteCardUseCase>().AsSingle();
            Container.Bind<EndTurnUseCase>().AsSingle();
            Container.Bind<MoveSlotUseCase>().AsSingle();
        }

        private void BindCharacterUseCases()
        {
            Container.Bind<InitializeCharacterUseCase>().AsSingle();
            Container.Bind<TakeDamageUseCase>().AsSingle();
            Container.Bind<HealCharacterUseCase>().AsSingle();
            Container.Bind<ApplyEffectUseCase>().AsSingle();
        }

        private void BindCardUseCases()
        {
            Container.Bind<DrawCardUseCase>().AsSingle();
            Container.Bind<PlayCardUseCase>().AsSingle();
            Container.Bind<DiscardCardUseCase>().AsSingle();
            Container.Bind<ShuffleDeckUseCase>().AsSingle();
        }
    }
}


