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
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.Utility.GameFlow;
using Game.SkillCardSystem.UI;

public class CombatInstaller : MonoInstaller
{
    [SerializeField] private SkillCardUI cardUIPrefab;

    public override void InstallBindings()
    {
        BindMonoBehaviours();
        BindServices();
        BindExecutionContext();
        BindSlotSystem();
        BindInitializerSteps();
        BindSceneLoader();
        BindUIPrefabs();
    }

    private void BindMonoBehaviours()
    {
        BindMono<ICombatFlowCoordinator, CombatFlowCoordinator>();
        BindMono<IPlayerManager, PlayerManager>();
        BindMono<IEnemyManager, EnemyManager>();
        BindMono<IEnemyHandManager, EnemyHandManager>();
        BindMono<IPlayerHandManager, PlayerHandManager>();
        BindMono<ICombatSlotManager, CombatSlotManager>();
        BindMono<IVictoryManager, VictoryManager>();
        BindMono<IGameOverManager, GameOverManager>();
        BindMono<IEnemySpawnerManager, EnemySpawnerManager>();
        BindMono<IStageManager, StageManager>();
        BindMono<ICharacterDeathListener, CharacterDeathHandler>();
        BindMono<IPlayerCharacterSelector, PlayerCharacterSelector>();
        BindMonoInterfaces<CombatTurnManager>();
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
        if (slotRegistry == null)
        {
            Debug.LogError("[CombatInstaller] SlotRegistry가 씬에 없습니다!");
            return;
        }

        Container.Bind<SlotRegistry>().FromInstance(slotRegistry).AsSingle();
        Container.Bind<ISlotRegistry>().FromInstance(slotRegistry).AsSingle();
        Container.Bind<ICombatSlotRegistry>().FromInstance(slotRegistry.GetCombatSlotRegistry()).AsSingle();
        Container.Bind<IHandSlotRegistry>().FromInstance(slotRegistry.GetHandSlotRegistry()).AsSingle();
        Container.Bind<ICharacterSlotRegistry>().FromInstance(slotRegistry.GetCharacterSlotRegistry()).AsSingle();

        Container.BindInterfacesAndSelfTo<SlotInitializer>().FromComponentInHierarchy().AsSingle();
    }

    private void BindInitializerSteps()
    {
        BindMonoInterfaces<SlotInitializationStep>();
        BindMonoInterfaces<FlowCoordinatorInitializationStep>();
        BindMonoInterfaces<PlayerCharacterInitializer>();
        BindMonoInterfaces<PlayerSkillCardInitializer>();
        BindMonoInterfaces<EnemyCharacterInitializer>();
        BindMonoInterfaces<EnemyHandInitializer>();

        Container.Bind<CombatStartupManager>().FromComponentInHierarchy().AsSingle();
    }

    private void BindSceneLoader()
    {
        var loader = Object.FindFirstObjectByType<SceneLoader>();
        if (loader == null)
        {
            Debug.LogError("[CombatInstaller] SceneLoader가 씬에 존재하지 않습니다!");
            return;
        }

        Container.Bind<ISceneLoader>().FromInstance(loader).AsSingle();
    }

    private void BindUIPrefabs()
    {
        if (cardUIPrefab == null)
        {
            Debug.LogError("[CombatInstaller] SkillCardUI 프리팹이 설정되지 않았습니다!");
            return;
        }

        Container.Bind<SkillCardUI>().FromInstance(cardUIPrefab).AsSingle();
    }

    // 헬퍼 메서드: MonoBehaviour 바인딩 간소화
    private void BindMono<TInterface, TImpl>() where TImpl : Component, TInterface
    {
        Container.Bind<TInterface>().To<TImpl>().FromComponentInHierarchy().AsSingle();
    }

    private void BindMonoInterfaces<T>() where T : Component
    {
        Container.BindInterfacesAndSelfTo<T>().FromComponentInHierarchy().AsSingle();
    }
}
