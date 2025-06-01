using UnityEngine;
using Zenject;
using Game.CombatSystem.Core;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Service;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Context;
using Game.CombatSystem.Initialization;
using Game.CombatSystem.Manager;
using Game.SkillCardSystem.Executor;
using Game.SkillCardSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Executor;
using Game.CombatSystem.Utility;
using Game.Manager;
using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Validator;

public class CombatInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        BindMonoBehaviours();
        BindServices();
        BindExecutionContext();
        BindSlotSystem();
        BindInitializerSteps();
    }

    private void BindMonoBehaviours()
    {
        Container.BindInterfacesTo<CombatTurnManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ICombatFlowCoordinator>().To<CombatFlowCoordinator>().FromComponentInHierarchy().AsSingle();

        Container.Bind<IPlayerManager>().To<PlayerManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IEnemyManager>().To<EnemyManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IEnemyHandManager>().To<EnemyHandManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IPlayerHandManager>().To<PlayerHandManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ICombatSlotManager>().To<CombatSlotManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IVictoryManager>().To<VictoryManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IGameOverManager>().To<GameOverManager>().FromComponentInHierarchy().AsSingle();

        Container.Bind<IEnemySpawnerManager>().To<EnemySpawnerManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IStageManager>().To<StageManager>().FromComponentInHierarchy().AsSingle();

        Container.Bind<ICharacterDeathListener>().To<CharacterDeathHandler>().FromComponentInHierarchy().AsSingle();
    }


    private void BindServices()
    {
        Container.Bind<ICombatPreparationService>().To<CombatPreparationService>().AsSingle();
        Container.Bind<ICardPlacementService>().To<CardPlacementService>().AsSingle();

        Container.Bind<ICombatStateFactory>().To<CombatStateFactory>().AsSingle();
        Container.Bind<ITurnCardRegistry>().To<TurnCardRegistry>().AsSingle();
        Container.Bind<ISlotSelector>().To<SlotSelector>().AsSingle();

        Container.BindInterfacesTo<CombatExecutorService>().AsSingle();
        Container.Bind<ICardExecutor>().To<CardExecutor>().AsSingle();
        Container.Bind<ICardExecutionContextProvider>().To<CardExecutionContextProvider>().AsSingle();
        Container.Bind<ICardEffectCommandFactory>().To<CardEffectCommandFactory>().AsSingle();
        Container.Bind<ICardExecutionValidator>().To<DefaultCardExecutionValidator>().AsSingle();

        Container.Bind<IEnemySpawnValidator>().To<DefaultEnemySpawnValidator>().AsSingle();
        Container.Bind<IPlayerInputController>().To<PlayerInputController>().AsSingle();

        Container.Bind<ISkillCardFactory>().To<SkillCardFactory>().AsSingle();
    }



    private void BindExecutionContext()
    {
        var ctx = new DefaultCardExecutionContext(null, null, null);
        Container.Bind<ICardExecutionContext>().FromInstance(ctx).AsSingle();
    }

    private void BindSlotSystem()
    {
        var slotRegistry = Object.FindFirstObjectByType<SlotRegistry>();
        Container.Bind<SlotRegistry>().FromInstance(slotRegistry).AsSingle();
        Container.Bind<ISlotRegistry>().FromInstance(slotRegistry).AsSingle();
        Container.Bind<ICombatSlotRegistry>().FromInstance(slotRegistry.GetCombatSlotRegistry()).AsSingle();
        Container.Bind<IHandSlotRegistry>().FromInstance(slotRegistry.GetHandSlotRegistry()).AsSingle();
        Container.Bind<ICharacterSlotRegistry>().FromInstance(slotRegistry.GetCharacterSlotRegistry()).AsSingle();

        Container.BindInterfacesAndSelfTo<SlotInitializer>().FromComponentInHierarchy().AsSingle();
    }

    private void BindInitializerSteps()
    {
        Container.BindInterfacesAndSelfTo<SlotInitializationStep>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<FlowCoordinatorInitializationStep>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerCharacterInitializer>().FromComponentInHierarchy().AsSingle();
        Container.Bind<CombatStartupManager>().FromComponentInHierarchy().AsSingle();
    }
}
