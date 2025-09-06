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
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Validator;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Manager;
using Game.UtilitySystem.GameFlow;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.State;
using Game.CombatSystem.Factory;
using Game.Utility;
using Game.CoreSystem.Utility;
using Game.CombatSystem.DragDrop;
using Game.CombatSystem.CoolTime;
using Game.SkillCardSystem.Runtime;
using Game.CoreSystem.Interface;

/// <summary>
/// 전투 씬에서 사용하는 Zenject 설치자입니다.
/// 필요한 서비스, 매니저, 상태머신, 슬롯, UI 등을 바인딩합니다.
/// </summary>
public class CombatInstaller : MonoInstaller
{
    [Header("카드 UI 프리팹")]
    [SerializeField] private SkillCardUI cardUIPrefab;

    [Header("턴 시작 버튼 핸들러")]
    [SerializeField] private TurnStartButtonHandler startButtonHandler;

    public override void InstallBindings()
    {
        BindStateFactories();
        BindMonoBehaviours();
        BindServices();

        Container.Bind<TurnContext>().AsSingle();

        BindExecutionContext();
        BindSlotSystem();
        BindInitializerSteps();
        BindSceneLoader();
        BindUIPrefabs();
        BindUIHandlers();
        BindCooldownSystem();
    }

    #region 상태머신

    /// <summary>
    /// 전투 상태별 StateFactory 바인딩
    /// </summary>
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

    #endregion

    #region MonoBehaviour 및 인터페이스 바인딩

    /// <summary>
    /// 씬 내 컴포넌트에서 인터페이스 자동 바인딩
    /// </summary>
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
        
        // CoroutineRunner 바인딩 - CoreScene에서 찾거나 새로 생성
        var coroutineRunner = FindFirstObjectByType<CoroutineRunner>();
        if (coroutineRunner == null)
        {
            Debug.LogWarning("[CombatInstaller] CoroutineRunner를 찾을 수 없습니다. 새로 생성합니다.");
            var runnerObj = new GameObject("CoroutineRunner");
            coroutineRunner = runnerObj.AddComponent<CoroutineRunner>();
            DontDestroyOnLoad(runnerObj);
        }
        Container.Bind<ICoroutineRunner>().FromInstance(coroutineRunner).AsSingle();
        
        // PlayerCharacterSelectionManager 바인딩
        var characterSelectionManager = FindFirstObjectByType<PlayerCharacterSelectionManager>();
        if (characterSelectionManager == null)
        {
            Debug.LogWarning("[CombatInstaller] PlayerCharacterSelectionManager를 찾을 수 없습니다. 새로 생성합니다.");
            var managerObj = new GameObject("PlayerCharacterSelectionManager");
            characterSelectionManager = managerObj.AddComponent<PlayerCharacterSelectionManager>();
            DontDestroyOnLoad(managerObj);
        }
        Container.Bind<IPlayerCharacterSelectionManager>().FromInstance(characterSelectionManager).AsSingle();
        
        BindMonoInterfaces<CombatTurnManager>();
    }

    #endregion

    #region 서비스 바인딩

    /// <summary>
    /// 주요 서비스 클래스 바인딩
    /// </summary>
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

    #endregion

    #region 컨텍스트

    /// <summary>
    /// (주의) 실행 컨텍스트를 초기화된 null 값으로 바인딩
    /// </summary>
    private void BindExecutionContext()
    {
        var ctx = new DefaultCardExecutionContext(null, null, null);
        Container.Bind<ICardExecutionContext>().FromInstance(ctx).AsSingle();
    }

    #endregion

    #region 슬롯 시스템

    /// <summary>
    /// 슬롯 레지스트리와 관련 시스템 바인딩
    /// </summary>
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

        // SlotInitializer 바인딩 (씬에서 자동으로 찾아서 바인딩)
        Container.BindInterfacesAndSelfTo<SlotInitializer>().FromComponentInHierarchy().AsSingle();
    }

    #endregion

    #region 초기화 단계

    /// <summary>
    /// 전투 준비 단계 초기화 요소 바인딩
    /// </summary>
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

    #endregion

    #region 씬 로더

    private void BindSceneLoader()
    {
        var transitionManager = SceneTransitionManager.Instance;
        if (transitionManager == null)
        {
            Debug.LogWarning("[CombatInstaller] SceneTransitionManager가 없습니다. CoreScene에서 초기화되지 않았을 수 있습니다.");
            
            // CoreScene에서 SceneTransitionManager를 찾아서 바인딩 시도
            var coreSceneTransitionManager = FindFirstObjectByType<SceneTransitionManager>();
            if (coreSceneTransitionManager != null)
            {
                Debug.Log("[CombatInstaller] CoreScene의 SceneTransitionManager를 찾았습니다.");
                Container.Bind<ISceneLoader>().FromInstance(coreSceneTransitionManager).AsSingle();
            }
            else
            {
                Debug.LogError("[CombatInstaller] SceneTransitionManager를 전혀 찾을 수 없습니다.");
            }
            return;
        }

        Container.Bind<ISceneLoader>().FromInstance(transitionManager).AsSingle();
    }

    #endregion

    #region UI 바인딩

    /// <summary>
    /// 카드 UI 프리팹 바인딩
    /// </summary>
    private void BindUIPrefabs()
    {
        if (cardUIPrefab == null)
        {
            Debug.LogWarning("[CombatInstaller] cardUIPrefab이 할당되지 않았습니다.");
            return;
        }

        Container.Bind<SkillCardUI>().FromInstance(cardUIPrefab).AsSingle();
    }

    /// <summary>
    /// UI 핸들러 (버튼 등) 바인딩
    /// </summary>
    private void BindUIHandlers()
    {
        if (startButtonHandler == null)
        {
            Debug.LogError("[CombatInstaller] startButtonHandler가 인스펙터에 할당되지 않았습니다.");
            return;
        }

        Container.Bind<TurnStartButtonHandler>().FromInstance(startButtonHandler).AsSingle();
    }

    #endregion

    #region 쿨타임 시스템

    private void BindCooldownSystem()
    {
        Container.Bind<SkillCardCooldownSystem>().AsSingle();
    }

    #endregion


    #region 유틸 바인딩 메서드

    /// <summary>
    /// 특정 인터페이스에 대한 MonoBehaviour 바인딩
    /// </summary>
    private void BindMono<TInterface, TImpl>() where TImpl : Component, TInterface
    {
        Container.Bind<TInterface>().To<TImpl>().FromComponentInHierarchy().AsSingle();
    }

    /// <summary>
    /// MonoBehaviour의 모든 인터페이스 및 자기 자신 바인딩
    /// </summary>
    private void BindMonoInterfaces<T>() where T : Component
    {
        Container.BindInterfacesAndSelfTo<T>().FromComponentInHierarchy().AsSingle();
    }

    #endregion
}
