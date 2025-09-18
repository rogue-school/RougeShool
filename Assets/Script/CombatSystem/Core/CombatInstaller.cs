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
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Validator;
using Game.SkillCardSystem.Manager;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Initialization;
using Game.StageSystem.Manager;
using Game.StageSystem.Interface;
using Game.CoreSystem.Manager;
using Game.UtilitySystem.GameFlow;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.State;
using Game.CombatSystem.Factory;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem.DragDrop;
using Game.SkillCardSystem.Runtime;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Save;
 

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
        BindCardCirculationSystem();
        BindDeckManagementSystem();
    }

    #region 상태머신

    /// <summary>
    /// 전투 상태별 StateFactory 바인딩
    /// </summary>
    private void BindStateFactories()
    {
        Container.Bind<IFactory<CombatPrepareState>>().To<CombatPrepareStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatPlayerInputState>>().To<CombatPlayerInputStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatAttackState>>().To<CombatAttackStateFactory>().AsTransient();
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
        // Victory/GameOver UI는 현재 미사용이므로 바인딩 중지
        // BindMono<IVictoryManager, VictoryManager>();
        // BindMono<IGameOverManager, GameOverManager>();
        BindMono<IEnemySpawnerManager, EnemySpawnerManager>();
        BindMono<IStageManager, StageManager>();
        BindMono<ICharacterDeathListener, CharacterDeathHandler>();
        
        // CoroutineRunner는 CoreSystemInstaller에서 바인딩됨
        
        // PlayerCharacterSelectionManager는 CoreSystemInstaller에서 바인딩됨
        
        BindMonoInterfaces<CombatTurnManager>();

        // SaveManager는 CoreSystemInstaller에서 바인딩됨
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
        // SlotRegistry는 Zenject DI로 자동 바인딩됨
        var slotRegistry = Container.Resolve<SlotRegistry>();
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

        // SlotInitializer는 Zenject DI로 자동 바인딩됨
    }

    #endregion

    #region 초기화 단계

    /// <summary>
    /// 전투 준비 단계 초기화 요소 바인딩
    /// </summary>
    private void BindInitializerSteps()
    {
        // 필수 초기화 스텝들 보장: 없으면 자동 생성 후 바인딩
        EnsureAndBindInitializer<Game.CombatSystem.Intialization.SlotInitializationStep>("SlotInitializationStep");
        EnsureAndBindInitializer<Game.CombatSystem.Intialization.FlowCoordinatorInitializationStep>("FlowCoordinatorInitializationStep");
        EnsureAndBindInitializer<PlayerCharacterInitializer>("PlayerCharacterInitializer");
        EnsureAndBindInitializer<PlayerSkillCardInitializer>("PlayerSkillCardInitializer");
        EnsureAndBindInitializer<EnemyCharacterInitializer>("EnemyCharacterInitializer");
        EnsureAndBindInitializer<EnemyHandInitializer>("EnemyHandInitializer");

        // CombatStartupManager는 Zenject DI로 자동 바인딩됨
    }

    #endregion

    #region 씬 로더

    private void BindSceneLoader()
    {
        // SceneTransitionManager는 CoreSystemInstaller에서 바인딩됨
        var transitionManager = Container.Resolve<ISceneLoader>();
        if (transitionManager == null)
        {
            Debug.LogError("[CombatInstaller] SceneTransitionManager를 찾을 수 없습니다.");
            return;
        }
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
            Debug.LogWarning("[CombatInstaller] startButtonHandler가 비어 있습니다. 즉발 규칙에서는 생략합니다.");
            return; // 선택 요소이므로 바인딩 생략
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

    #region 카드 순환 시스템

    private void BindCardCirculationSystem()
    {
        // ICardCirculationSystem, ITurnBasedCardManager를 씬 컴포넌트에서 찾거나 새로 생성해 단일 바인딩
        // CardCirculationSystem은 Zenject DI로 자동 바인딩됨

        // TurnBasedCardManager는 Zenject DI로 자동 바인딩됨
    }

    #endregion

    #region 덱 관리 시스템

    /// <summary>
    /// 덱 관리 관련 시스템 바인딩
    /// </summary>
    private void BindDeckManagementSystem()
    {
        // PlayerDeckManager 바인딩
        // PlayerDeckManager는 Zenject DI로 자동 바인딩됨

        // CardRewardManager는 Zenject DI로 자동 바인딩됨
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

    /// <summary>
    /// 씬에 존재하지 않으면 새 GameObject를 생성해 컴포넌트를 추가하고, 해당 인스턴스를 단일 바인딩합니다.
    /// </summary>
    private void EnsureAndBindInitializer<T>(string gameObjectName) where T : Component
    {
        // T는 Zenject DI로 자동 바인딩됨
    }

    #endregion
}
