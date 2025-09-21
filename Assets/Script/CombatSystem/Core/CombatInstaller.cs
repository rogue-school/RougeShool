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
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Validator;
using Game.SkillCardSystem.Manager;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Initialization;
using Game.CoreSystem.Utility;
using Game.UtilitySystem.GameFlow;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Interface;
using Game.StageSystem.Manager;
using Game.StageSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.State;
using Game.CombatSystem.Factory;
using Game.CombatSystem.DragDrop;
using Game.UtilitySystem;
using Game.CoreSystem.Save;
using Game.CoreSystem.Audio;
using Game.SkillCardSystem.Runtime;
 

/// <summary>
/// 전투 씬용 Zenject 설치자 (역할 명확화됨)
/// DI 바인딩만 담당하고 객체 생성은 다른 시스템에 위임합니다.
/// </summary>
public class CombatInstaller : MonoInstaller
{
    [Header("전투 씬 프리팹")]
    [SerializeField] private SkillCardUI cardUIPrefab;

    [Header("DI 바인딩 설정")]
#pragma warning disable CS0414 // 사용하지 않는 필드 경고 억제 (향후 사용 예정)
    [SerializeField] private bool enableLazyInitialization = true;
    [SerializeField] private bool enableCircularDependencyCheck = true;
    [SerializeField] private bool enableAutoObjectCreation = false; // 객체 자동 생성 비활성화
#pragma warning restore CS0414
    [SerializeField] private bool enablePerformanceLogging = false;

    public override void InstallBindings()
    {
        // 성능 측정 시작
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // 최적화된 바인딩 순서 (의존성 순서 고려)
        BindCoreServices();          // 핵심 서비스 (의존성 없음)
        BindFactories();            // 팩토리 (서비스 의존)
        BindSlotSystem();           // 슬롯 시스템 (팩토리 의존)
        BindIntegratedManagers();   // 통합 매니저 (모든 것 의존)
        BindUIPrefabs();            // UI 프리팹 (매니저 의존)
        
        stopwatch.Stop();
        if (enablePerformanceLogging)
        {
            GameLogger.LogInfo($"CombatInstaller 바인딩 완료: {stopwatch.ElapsedMilliseconds}ms", GameLogger.LogCategory.Combat);
        }
        
        GameLogger.LogInfo("최적화된 전투 시스템 바인딩 완료", GameLogger.LogCategory.Combat);
    }
    

    #region 통합 시스템 바인딩

    /// <summary>
    /// 새로운 통합 매니저들 바인딩
    /// </summary>
    private void BindIntegratedManagers()
    {
        // CombatFlowManager 바인딩
        var combatFlowManager = FindFirstObjectByType<CombatFlowManager>();
        if (combatFlowManager != null)
        {
            Container.Bind<ICombatFlowManager>().FromInstance(combatFlowManager).AsSingle();
            GameLogger.LogInfo(" CombatFlowManager 바인딩 완료");
        }

        // CombatExecutionManager 바인딩
        var combatExecutionManager = FindFirstObjectByType<CombatExecutionManager>();
        if (combatExecutionManager != null)
        {
            Container.Bind<ICombatExecutionManager>().FromInstance(combatExecutionManager).AsSingle();
            GameLogger.LogInfo(" CombatExecutionManager 바인딩 완료");
        }

        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합

        // TurnManager는 BindFactories()에서 바인딩됨
    }


    /// <summary>
    /// UI 프리팹 바인딩
    /// </summary>
    private void BindUIPrefabs()
    {
        if (cardUIPrefab != null)
        {
            Container.Bind<SkillCardUI>().FromInstance(cardUIPrefab).AsSingle();
            GameLogger.LogInfo(" SkillCardUI 프리팹 바인딩 완료");
        }
        else
        {
            GameLogger.LogWarning(" SkillCardUI 프리팹이 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 팩토리 및 서비스 바인딩
    /// </summary>
    private void BindFactories()
    {
        // PlayerManager 바인딩 - 먼저 바인딩 (PlayerDeckManager가 의존함)
        Container.BindInterfacesAndSelfTo<PlayerManager>()
            .FromComponentInHierarchy().AsSingle();
        GameLogger.LogInfo(" PlayerManager 바인딩 완료 (씬에서 찾기)");
        
        // SkillCardFactory 바인딩
        Container.Bind<ISkillCardFactory>().To<SkillCardFactory>().AsSingle();
        GameLogger.LogInfo(" SkillCardFactory 바인딩 완료");
        
        // CardCirculationSystem 바인딩
        Container.Bind<ICardCirculationSystem>().To<CardCirculationSystem>().AsSingle();
        GameLogger.LogInfo(" CardCirculationSystem 바인딩 완료");
        
        // TurnBasedCardManager 바인딩 (삭제됨 - CardCirculationSystem에 통합)
        // Container.Bind<ITurnBasedCardManager>().To<TurnBasedCardManager>().AsSingle();
        GameLogger.LogInfo(" TurnBasedCardManager 바인딩 완료");
        
        // PlayerHandManager 바인딩 - PlayerManager 이후에 바인딩
        Container.BindInterfacesAndSelfTo<PlayerHandManager>()
            .FromNewComponentOnNewGameObject().AsSingle();
        GameLogger.LogInfo(" PlayerHandManager 바인딩 완료");
        
        // PlayerDeckManager 바인딩 - PlayerManager 이후에 바인딩
        Container.BindInterfacesAndSelfTo<PlayerDeckManager>()
            .FromNewComponentOnNewGameObject().AsSingle();
        GameLogger.LogInfo(" PlayerDeckManager 바인딩 완료");
        
        // TurnContext 바인딩 (CharacterDeathHandler가 의존함)
        Container.Bind<TurnContext>().AsSingle();
        GameLogger.LogInfo(" TurnContext 바인딩 완료");
        
        // IStageManager 바인딩 - 씬에 있으면 사용, 없으면 자동 생성
        var stageManager = FindFirstObjectByType<StageManager>();
        if (stageManager != null)
        {
            Container.Bind<IStageManager>().FromInstance(stageManager).AsSingle();
            GameLogger.LogInfo(" IStageManager 바인딩 완료 (씬에서 찾기)");
        }
        else
        {
            // StageManager가 없으면 자동 생성
            var stageManagerGO = new GameObject("StageManager");
            stageManager = stageManagerGO.AddComponent<StageManager>();
            Container.Bind<IStageManager>().FromInstance(stageManager).AsSingle();
            GameLogger.LogInfo(" IStageManager 자동 생성 및 바인딩 완료");
        }
        
        // CardDropService 의존성 바인딩 (구조화된 배열에서 처리됨)
        // Container.Bind<ITurnCardRegistry>().To<TurnCardRegistry>().AsSingle(); // 삭제됨
        
        // TurnManager 바인딩 - 자동 생성으로 최적화
        Container.BindInterfacesAndSelfTo<TurnManager>()
            .FromNewComponentOnNewGameObject().AsSingle();
        GameLogger.LogInfo(" TurnManager 바인딩 완료");
        
        GameLogger.LogInfo(" CardDropService 및 의존성 바인딩 완료");
    }

    #endregion

    #region 상태머신 (레거시)

    /// <summary>
    /// 전투 상태별 StateFactory 바인딩 (레거시)
    /// </summary>
    private void BindStateFactories()
    {
        Container.Bind<IFactory<CombatPrepareState>>().To<CombatPrepareStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatPlayerInputState>>().To<CombatPlayerInputStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatAttackState>>().To<CombatAttackStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatResultState>>().To<CombatResultStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatVictoryState>>().To<CombatVictoryStateFactory>().AsTransient();
        Container.Bind<IFactory<CombatGameOverState>>().To<CombatGameOverStateFactory>().AsTransient();
        // Container.Bind<ICombatStateFactory>().To<CombatStateFactory>().AsSingle(); // 삭제됨
    }

    #endregion

    #region MonoBehaviour 및 인터페이스 바인딩

    /// <summary>
    /// 씬 내 컴포넌트에서 인터페이스 자동 바인딩
    /// </summary>
    private void BindMonoBehaviours()
    {
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합

        // TurnManager는 BindFactories()에서 바인딩됨

        // CombatExecutionManager 바인딩 (SlotExecutionSystem 대신 CombatExecutionManager 사용)
        var executionManager = FindFirstObjectByType<CombatExecutionManager>();
        if (executionManager != null)
        {
            Container.Bind<CombatExecutionManager>().FromInstance(executionManager).AsSingle();
            Container.Bind<ICombatExecutionManager>().FromInstance(executionManager).AsSingle();
            GameLogger.LogInfo(" CombatExecutionManager 바인딩 완료");
        }
        else
        {
            // CombatExecutionManager가 없으면 생성
            var executionManagerGO = new GameObject("CombatExecutionManager");
            executionManager = executionManagerGO.AddComponent<CombatExecutionManager>();
            Container.Bind<CombatExecutionManager>().FromInstance(executionManager).AsSingle();
            Container.Bind<ICombatExecutionManager>().FromInstance(executionManager).AsSingle();
            GameLogger.LogInfo(" CombatExecutionManager 생성 및 바인딩 완료");
        }

        // 기존 매니저들 바인딩
        // PlayerManager는 BindFactories()에서 바인딩됨 (씬에서 찾기)
        // PlayerHandManager는 BindFactories()에서 바인딩됨
        
        // EnemyManager 바인딩 - 씬에 있으면 사용, 없으면 자동 생성
        var enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            Container.BindInterfacesAndSelfTo<EnemyManager>().FromInstance(enemyManager).AsSingle();
            GameLogger.LogInfo(" EnemyManager 바인딩 완료 (씬에서 찾기)");
        }
        else
        {
            // EnemyManager가 없으면 자동 생성
            var enemyManagerGO = new GameObject("EnemyManager");
            enemyManager = enemyManagerGO.AddComponent<EnemyManager>();
            Container.BindInterfacesAndSelfTo<EnemyManager>().FromInstance(enemyManager).AsSingle();
            GameLogger.LogInfo(" EnemyManager 자동 생성 및 바인딩 완료");
        }
        
        // IStageManager는 BindFactories()에서 바인딩됨
        
        // CharacterDeathHandler 바인딩 - 자동 생성으로 최적화
        Container.BindInterfacesAndSelfTo<CharacterDeathHandler>()
            .FromNewComponentOnNewGameObject().AsSingle();
        
        // CoroutineRunner는 CoreSystemInstaller에서 바인딩됨
        // PlayerCharacterSelectionManager는 CoreSystemInstaller에서 바인딩됨
        
        // SaveManager 바인딩
        BindSaveManager();
    }

    #endregion

    #region 서비스 바인딩

    /// <summary>
    /// 핵심 서비스 클래스 바인딩 (중복 제거됨)
    /// </summary>
    private void BindCoreServices()
    {
        // 서비스들을 배열로 관리하여 반복문으로 처리
        var services = new (System.Type interfaceType, System.Type implementationType)[]
        {
            // (typeof(ICardPlacementService), typeof(CardPlacementService)), // 삭제됨
            // (typeof(ISlotSelector), typeof(SlotSelector)), // 삭제됨
            // (typeof(ICardExecutor), typeof(CardExecutor)), // 삭제됨
            // (typeof(ICardExecutionContextProvider), typeof(CardExecutionContextProvider)), // 삭제됨
            (typeof(ICardEffectCommandFactory), typeof(CardEffectCommandFactory)),
            (typeof(ICardValidator), typeof(DefaultCardExecutionValidator)), // ICardExecutionValidator -> ICardValidator로 변경
            // (typeof(IEnemySpawnValidator), typeof(DefaultEnemySpawnValidator)), // 삭제됨
            // (typeof(IPlayerInputController), typeof(PlayerInputController)), // 삭제됨
            // (typeof(ICardDropValidator), typeof(DefaultCardDropValidator)), // 삭제됨
            // (typeof(ICardRegistrar), typeof(DefaultCardRegistrar)), // 삭제됨
            // (typeof(ICardReplacementHandler), typeof(PlayerCardReplacementHandler)), // 삭제됨
            // (typeof(ITurnStartConditionChecker), typeof(DefaultTurnStartConditionChecker)) // 삭제됨
        };

        foreach (var (interfaceType, implementationType) in services)
        {
            Container.Bind(interfaceType).To(implementationType).AsSingle();
            
            if (enablePerformanceLogging)
            {
                GameLogger.LogInfo($"{interfaceType.Name} 바인딩 완료", GameLogger.LogCategory.Combat);
            }
        }

        // 특별한 경우들
        Container.Bind<CardDropService>().AsSingle();
        
        if (enablePerformanceLogging)
        {
            GameLogger.LogInfo("핵심 서비스 바인딩 완료", GameLogger.LogCategory.Combat);
        }
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
        // 슬롯 레지스트리들을 DI 컨테이너에서 자동 생성
        Container.Bind<HandSlotRegistry>().AsSingle();
        Container.Bind<CombatSlotRegistry>().AsSingle();
        Container.Bind<CharacterSlotRegistry>().AsSingle();
        
        // SlotRegistry는 MonoBehaviour이므로 씬에서 찾아서 바인딩
        var slotRegistry = FindFirstObjectByType<SlotRegistry>();
        if (slotRegistry != null)
        {
            Container.Bind<SlotRegistry>().FromInstance(slotRegistry).AsSingle();
            GameLogger.LogInfo(" SlotRegistry 바인딩 완료");
        }
        else
        {
            // SlotRegistry가 없으면 자동 생성
            Container.BindInterfacesAndSelfTo<SlotRegistry>()
                .FromNewComponentOnNewGameObject().AsSingle();
            GameLogger.LogInfo(" SlotRegistry 자동 생성 및 바인딩 완료");
        }

        // DefaultTurnStartConditionChecker 바인딩
        Container.Bind<DefaultTurnStartConditionChecker>().AsSingle();

        // CardDropRegistrar 바인딩
        Container.Bind<CardDropRegistrar>().AsSingle();

        GameLogger.LogInfo(" 슬롯 시스템 바인딩 완료");
    }

    #endregion

    #region 초기화 단계

    /// <summary>
    /// 전투 준비 단계 초기화 요소 바인딩
    /// </summary>
    private void BindInitializerSteps()
    {
        // Zenject DI가 자동으로 FromComponentInHierarchy()로 바인딩
        // Container.Bind<ICombatInitializerStep>().To<Game.CombatSystem.Intialization.SlotInitializationStep>().AsSingle(); // 삭제됨
        // Container.BindInterfacesAndSelfTo<PlayerCharacterInitializer>().FromComponentInHierarchy().AsSingle(); // 삭제됨
        Container.BindInterfacesAndSelfTo<PlayerSkillCardInitializer>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<EnemyCharacterInitializer>().FromComponentInHierarchy().AsSingle();
        
        GameLogger.LogInfo(" 초기화 스텝들 바인딩 완료");
    }

    #endregion

    #region 씬 로더

    private void BindSceneLoader()
    {
        // SceneTransitionManager는 MonoBehaviour이므로 씬에서 찾아야 함
        var transitionManager = FindFirstObjectByType<SceneTransitionManager>();
        if (transitionManager == null)
        {
            GameLogger.LogError(" SceneTransitionManager를 씬에서 찾을 수 없습니다.");
            return;
        }

        // ISceneLoader 인터페이스로 바인딩
        Container.Bind<ISceneLoader>().FromInstance(transitionManager).AsSingle();
        Container.Bind<ISceneTransitionManager>().FromInstance(transitionManager).AsSingle();
        
        GameLogger.LogInfo(" 씬 로더 바인딩 완료");
    }

    #endregion

    #region UI 바인딩


    /// <summary>
    /// UI 핸들러 바인딩 (즉시 실행 시스템에서는 스타트 버튼 불필요)
    /// </summary>
    private void BindUIHandlers()
    {
        // 새로운 즉시 실행 시스템에서는 스타트 버튼이 필요하지 않음
        // 필요시 다른 UI 핸들러 바인딩을 여기에 추가
    }

    #endregion

    #region 카드 순환 시스템

    private void BindCardCirculationSystem()
    {
        GameLogger.LogInfo(" BindCardCirculationSystem 호출 시작");
        
        // DI 서비스로 바인딩 (MonoBehaviour 제거됨)
        Container.Bind<ICardCirculationSystem>().To<CardCirculationSystem>().AsSingle();
        // Container.Bind<ITurnBasedCardManager>().To<TurnBasedCardManager>().AsSingle(); // 삭제됨
        
        GameLogger.LogInfo(" 카드 순환 시스템 바인딩 완료");
    }

    #endregion

    #region 애니메이션 시스템

    /// <summary>
    /// 애니메이션 시스템 바인딩
    /// </summary>
    private void BindAnimationFacade()
    {
        GameLogger.LogInfo(" BindAnimationFacade 호출 시작");
        
        // AnimationFacade를 씬에서 찾기
        // AnimationFacade 바인딩 제거 (AnimationSystem 제거로 인해 비활성화)
        GameLogger.LogInfo("AnimationFacade 바인딩을 건너뜁니다.", GameLogger.LogCategory.Combat);
    }

    /// <summary>
    /// 저장 시스템 바인딩
    /// </summary>
    private void BindSaveManager()
    {
        GameLogger.LogInfo(" BindSaveManager 호출 시작");
        
        // SaveManager를 씬에서 찾기
        var saveManager = FindFirstObjectByType<SaveManager>();
        
        if (saveManager == null)
        {
            GameLogger.LogWarning(" SaveManager를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // ISaveManager와 SaveManager를 null로 바인딩
            Container.Bind<ISaveManager>().FromInstance(null).AsSingle();
            Container.Bind<SaveManager>().FromInstance(null).AsSingle();
        }
        else
        {
            GameLogger.LogInfo($"[CombatInstaller] SaveManager 발견: {saveManager.name}", GameLogger.LogCategory.Combat);
            
            // ISaveManager 인터페이스로 바인딩
            Container.Bind<ISaveManager>().FromInstance(saveManager).AsSingle();
            
            // SaveManager 클래스로도 바인딩 (PlayerDeckManager가 직접 요구)
            Container.Bind<SaveManager>().FromInstance(saveManager).AsSingle();
            
            GameLogger.LogInfo(" 저장 시스템 바인딩 완료");
        }
    }

    /// <summary>
    /// 오디오 시스템 바인딩
    /// </summary>
    private void BindAudioManager()
    {
        GameLogger.LogInfo(" BindAudioManager 호출 시작");
        
        // AudioManager를 씬에서 찾기
        var audioManager = FindFirstObjectByType<AudioManager>();
        
        if (audioManager == null)
        {
            GameLogger.LogWarning(" AudioManager를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // IAudioManager를 null로 바인딩
            Container.Bind<IAudioManager>().FromInstance(null).AsSingle();
        }
        else
        {
            GameLogger.LogInfo($"[CombatInstaller] AudioManager 발견: {audioManager.name}", GameLogger.LogCategory.Combat);
            
            // IAudioManager 인터페이스로 바인딩
            Container.Bind<IAudioManager>().FromInstance(audioManager).AsSingle();
            
            GameLogger.LogInfo(" 오디오 시스템 바인딩 완료");
        }
    }

    /// <summary>
    /// 코루틴 러너 바인딩
    /// </summary>
    private void BindCoroutineRunner()
    {
        GameLogger.LogInfo(" BindCoroutineRunner 호출 시작");
        
        // CoroutineRunner를 씬에서 찾기
        var coroutineRunner = FindFirstObjectByType<CoroutineRunner>();
        
        if (coroutineRunner == null)
        {
            GameLogger.LogWarning(" CoroutineRunner를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // ICoroutineRunner를 null로 바인딩
            Container.Bind<ICoroutineRunner>().FromInstance(null).AsSingle();
        }
        else
        {
            GameLogger.LogInfo($"[CombatInstaller] CoroutineRunner 발견: {coroutineRunner.name}", GameLogger.LogCategory.Combat);
            
            // ICoroutineRunner 인터페이스로 바인딩
            Container.Bind<ICoroutineRunner>().FromInstance(coroutineRunner).AsSingle();
        }
        
        GameLogger.LogInfo(" 코루틴 러너 바인딩 완료");
    }

    /// <summary>
    /// 슬롯 초기화기 바인딩
    /// </summary>
    private void BindSlotInitializer()
    {
        GameLogger.LogInfo(" BindSlotInitializer 호출 시작");
        
        // SlotInitializer가 제거되었으므로 바인딩 건너뜀
        GameLogger.LogInfo(" SlotInitializer가 제거되어 바인딩을 건너뜁니다.");
        
        GameLogger.LogInfo(" 슬롯 초기화기 바인딩 완료");
    }

    #endregion

    #region 덱 관리 시스템

    /// <summary>
    /// 덱 관리 관련 시스템 바인딩
    /// </summary>
    private void BindDeckManagementSystem()
    {
        GameLogger.LogInfo(" 덱 관리 시스템 바인딩 시작");
        
        // PlayerDeckManager는 BindFactories()에서 자동 생성으로 바인딩됨
        // 여기서는 추가 설정만 수행
        
        GameLogger.LogInfo(" 덱 관리 시스템 바인딩 완료");
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
    /// 캐릭터 시스템 관련 컴포넌트들을 바인딩합니다.
    /// </summary>
    private void BindCharacterSystem()
    {
        GameLogger.LogInfo(" 캐릭터 시스템 바인딩 시작");
        
        // PlayerResourceManager 바인딩 (삭제됨 - PlayerManager에 통합)
        // var playerResourceManager = FindFirstObjectByType<PlayerResourceManager>();
        // if (playerResourceManager != null)
        // {
        //     Container.Bind<PlayerResourceManager>().FromInstance(playerResourceManager).AsSingle();
        //     GameLogger.LogInfo(" PlayerResourceManager 바인딩 완료");
        // }
        // else
        // {
        //     GameLogger.LogWarning(" PlayerResourceManager를 찾을 수 없습니다.");
        // }
        
        // GameStateManager 바인딩 (DontDestroyOnLoad로 유지되는 인스턴스)
        var gameStateManager = FindFirstObjectByType<GameStateManager>();
        if (gameStateManager != null)
        {
            Container.Bind<IGameStateManager>().FromInstance(gameStateManager).AsSingle();
            Container.Bind<GameStateManager>().FromInstance(gameStateManager).AsSingle();
            GameLogger.LogInfo(" GameStateManager 바인딩 완료");
        }
        else
        {
            GameLogger.LogWarning(" GameStateManager를 찾을 수 없습니다.");
        }
        
        GameLogger.LogInfo(" 캐릭터 시스템 바인딩 완료");
    }

    #endregion
}
