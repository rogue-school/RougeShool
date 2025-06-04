using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
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
using Game.CombatSystem.State;
using Game.CombatSystem.Factory;
using Game.Utility;
using Game.CombatSystem.DragDrop;
using Game.CombatSystem.CoolTime;

public class CombatInstaller : MonoInstaller
{
    [SerializeField] private SkillCardUI cardUIPrefab;
    [SerializeField] private TurnStartButtonHandler startButtonHandler; // 인스펙터 연결

    public override void InstallBindings()
    {
        BindStateFactories();
        BindMonoBehaviours();
        BindServices();
        BindExecutionContext();
        BindSlotSystem();
        BindInitializerSteps();
        BindSceneLoader();
        BindUIPrefabs();
        BindUIHandlers();
    }

    private void BindStateFactories()
    {
        Container.Bind<IFactory<CombatPrepareState>>().To<CombatPrepareStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatPlayerInputState>>().To<CombatPlayerInputStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatFirstAttackState>>().To<CombatFirstAttackStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatSecondAttackState>>().To<CombatSecondAttackStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatResultState>>().To<CombatResultStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatVictoryState>>().To<CombatVictoryStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatGameOverState>>().To<CombatGameOverStateFactory>().AsTransient();
        Container.Bind<ICombatStateFactory>().To<CombatStateFactory>().AsSingle();
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
        BindMono<ICoroutineRunner, CoroutineRunner>();
        BindMonoInterfaces<CombatTurnManager>();
    }

    private void BindServices()
    {
        Container.Bind<ICombatPreparationService>().To<CombatPreparationService>().AsSingle();
        Container.Bind<ICardPlacementService>().To<CardPlacementService>().AsSingle();
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
        Container.Bind<ICardDropValidator>().To<DefaultCardDropValidator>().AsSingle();
        Container.Bind<ICardRegistrar>().To<DefaultCardRegistrar>().AsSingle();
        Container.Bind<ICardReplacementHandler>().To<PlayerCardReplacementHandler>().AsSingle();
        Container.Bind<CardDropService>().AsSingle();

        Container.Bind<ITurnStartConditionChecker>().To<DefaultTurnStartConditionChecker>().AsSingle();

        Container.Bind<ICoolTimeHandler>().To<CoolTimeHandler>().AsSingle();
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
            Debug.LogError("[CombatInstaller] SlotRegistry를 찾을 수 없습니다.");
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
            Debug.LogError("[CombatInstaller] SceneLoader가 없습니다.");
            return;
        }

        Container.Bind<ISceneLoader>().FromInstance(loader).AsSingle();
    }

    private void BindUIPrefabs()
    {
        if (cardUIPrefab == null)
        {
            Debug.LogWarning("[CombatInstaller] cardUIPrefab이 할당되지 않았습니다.");
            return;
        }

        Container.Bind<SkillCardUI>().FromInstance(cardUIPrefab).AsSingle();
    }

    private void BindUIHandlers()
    {
        if (startButtonHandler == null)
        {
            Debug.LogError("[CombatInstaller] startButtonHandler가 인스펙터에 할당되지 않았습니다.");
            return;
        }

        Container.Bind<TurnStartButtonHandler>().FromInstance(startButtonHandler).AsSingle();
    }

    // 공통 바인딩 메서드
    private void BindMono<TInterface, TImpl>() where TImpl : Component, TInterface
    {
        Container.Bind<TInterface>().To<TImpl>().FromComponentInHierarchy().AsSingle();
    }

    private void BindMonoInterfaces<T>() where T : Component
    {
        Container.BindInterfacesAndSelfTo<T>().FromComponentInHierarchy().AsSingle();
    }
}
