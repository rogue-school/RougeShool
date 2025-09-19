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
using Game.UtilitySystem.GameFlow;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Interface;
using Game.StageSystem.Manager;
using Game.StageSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.State;
using Game.CombatSystem.Factory;
using Game.UtilitySystem;
using Game.AnimationSystem.Manager;
using Game.AnimationSystem.Interface;
using Game.CoreSystem.Save;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Utility;
using Game.CombatSystem.DragDrop;
using Game.SkillCardSystem.Runtime;
 

/// <summary>
/// 전투 씬에서 사용하는 Zenject 설치자입니다.
/// 필요한 서비스, 매니저, 상태머신, 슬롯, UI 등을 바인딩합니다.
/// </summary>
public class CombatInstaller : MonoInstaller
{
    [Header("카드 UI 프리팹")]
    [SerializeField] private SkillCardUI cardUIPrefab;


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
        BindAnimationFacade();
        BindSaveManager();
        BindAudioManager();
        BindCoroutineRunner();
        BindSlotInitializer();
        BindCharacterSystem();
        
        // 전투 시작 로직 추가
        StartCoroutine(StartCombatAfterInitialization());
    }
    
    /// <summary>
    /// 초기화 완료 후 전투를 시작합니다.
    /// 이어하기 기능을 고려하여 조건부로 전투를 시작합니다.
    /// </summary>
    private System.Collections.IEnumerator StartCombatAfterInitialization()
    {
        // 모든 초기화가 완료될 때까지 대기
        yield return new WaitForEndOfFrame();
        
        // 이어하기 상태 확인
        bool isResumingGame = CheckIfResumingGame();
        
        if (isResumingGame)
        {
            Debug.Log("[CombatInstaller] 이어하기 모드: 저장된 전투 상태 복원");
            yield return ResumeCombatFromSave();
        }
        else
        {
            Debug.Log("[CombatInstaller] 새 전투 시작");
            yield return StartNewCombat();
        }
    }
    
    /// <summary>
    /// 이어하기 상태인지 확인합니다.
    /// </summary>
    private bool CheckIfResumingGame()
    {
        // SaveManager를 통해 이어하기 상태 확인
        var saveManager = FindFirstObjectByType<Game.CoreSystem.Save.SaveManager>();
        if (saveManager != null)
        {
            // 저장된 전투 데이터가 있는지 확인
            return saveManager.HasCombatSaveData();
        }
        
        // 기본값: 새 전투 시작
        return false;
    }
    
    /// <summary>
    /// 저장된 전투 상태를 복원합니다.
    /// </summary>
    private System.Collections.IEnumerator ResumeCombatFromSave()
    {
        var flowCoordinator = FindFirstObjectByType<Game.CombatSystem.Manager.CombatFlowCoordinator>();
        if (flowCoordinator != null)
        {
            // 저장된 데이터 로드 및 전투 상태 복원
            yield return flowCoordinator.ResumeCombatFromSave();
        }
        else
        {
            Debug.LogError("[CombatInstaller] CombatFlowCoordinator를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 새로운 전투를 시작합니다.
    /// </summary>
    private System.Collections.IEnumerator StartNewCombat()
    {
        var flowCoordinator = FindFirstObjectByType<Game.CombatSystem.Manager.CombatFlowCoordinator>();
        if (flowCoordinator != null)
        {
            Debug.Log("[CombatInstaller] 새 전투 시작");
            flowCoordinator.StartCombatFlow();
        }
        else
        {
            Debug.LogError("[CombatInstaller] CombatFlowCoordinator를 찾을 수 없습니다!");
        }
        
        yield return null;
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
        // 적 핸드 시스템 완전 비활성화: 더 이상 바인딩하지 않음
        BindMono<IPlayerHandManager, PlayerHandManager>();
        BindMono<ICombatSlotManager, CombatSlotManager>();
        Container.Bind<CombatStartupManager>().FromComponentInHierarchy().AsSingle();
        // Victory/GameOver UI는 현재 미사용이므로 바인딩 중지
        // BindMono<IVictoryManager, VictoryManager>();
        // BindMono<IGameOverManager, GameOverManager>();
        BindMono<IEnemySpawnerManager, EnemySpawnerManager>();
        BindMono<IStageManager, StageManager>();
        BindMono<ICharacterDeathListener, CharacterDeathHandler>();
        
        // CoroutineRunner는 CoreSystemInstaller에서 바인딩됨
        
        // PlayerCharacterSelectionManager는 CoreSystemInstaller에서 바인딩됨
        
        // CombatTurnManager 명시적 바인딩
        var turnManager = FindFirstObjectByType<CombatTurnManager>();
        if (turnManager != null)
        {
            Container.Bind<ICombatTurnManager>().FromInstance(turnManager).AsSingle();
            Container.Bind<CombatTurnManager>().FromInstance(turnManager).AsSingle();
            Debug.Log("[CombatInstaller] CombatTurnManager 바인딩 완료");
        }
        else
        {
            Debug.LogError("[CombatInstaller] CombatTurnManager를 찾을 수 없습니다.");
        }

        // SaveManager는 CoreSystemInstaller에서 바인딩됨
    }

    #endregion

    #region 서비스 바인딩

    /// <summary>
    /// 주요 서비스 클래스 바인딩
    /// </summary>
    private void BindServices()
    {
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
        // SlotRegistry는 MonoBehaviour이므로 씬에서 찾아야 함
        var slotRegistry = FindFirstObjectByType<SlotRegistry>();
        if (slotRegistry == null)
        {
            Debug.LogError("[CombatInstaller] SlotRegistry를 씬에서 찾을 수 없습니다.");
            return;
        }

        // SlotRegistry와 관련 인터페이스들을 DI 컨테이너에 바인딩
        Container.Bind<SlotRegistry>().FromInstance(slotRegistry).AsSingle();
        Container.Bind<ISlotRegistry>().FromInstance(slotRegistry).AsSingle();
        
        // 각 슬롯 레지스트리들을 개별적으로 바인딩
        var combatSlotRegistry = slotRegistry.GetCombatSlotRegistry();
        var handSlotRegistry = slotRegistry.GetHandSlotRegistry();
        var characterSlotRegistry = slotRegistry.GetCharacterSlotRegistry();
        
        if (combatSlotRegistry != null)
            Container.Bind<ICombatSlotRegistry>().FromInstance(combatSlotRegistry).AsSingle();
        
        if (handSlotRegistry != null)
            Container.Bind<IHandSlotRegistry>().FromInstance(handSlotRegistry).AsSingle();
        
        if (characterSlotRegistry != null)
            Container.Bind<ICharacterSlotRegistry>().FromInstance(characterSlotRegistry).AsSingle();

        Debug.Log("[CombatInstaller] 슬롯 시스템 바인딩 완료");
    }

    #endregion

    #region 초기화 단계

    /// <summary>
    /// 전투 준비 단계 초기화 요소 바인딩
    /// </summary>
    private void BindInitializerSteps()
    {
        // Zenject DI가 자동으로 FromComponentInHierarchy()로 바인딩
        Container.BindInterfacesAndSelfTo<Game.CombatSystem.Intialization.SlotInitializationStep>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<Game.CombatSystem.Intialization.FlowCoordinatorInitializationStep>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerCharacterInitializer>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerSkillCardInitializer>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<EnemyCharacterInitializer>().FromComponentInHierarchy().AsSingle();
        
        Debug.Log("[CombatInstaller] 초기화 스텝들 바인딩 완료");
    }

    #endregion

    #region 씬 로더

    private void BindSceneLoader()
    {
        // SceneTransitionManager는 MonoBehaviour이므로 씬에서 찾아야 함
        var transitionManager = FindFirstObjectByType<SceneTransitionManager>();
        if (transitionManager == null)
        {
            Debug.LogError("[CombatInstaller] SceneTransitionManager를 씬에서 찾을 수 없습니다.");
            return;
        }

        // ISceneLoader 인터페이스로 바인딩
        Container.Bind<ISceneLoader>().FromInstance(transitionManager).AsSingle();
        Container.Bind<ISceneTransitionManager>().FromInstance(transitionManager).AsSingle();
        
        Debug.Log("[CombatInstaller] 씬 로더 바인딩 완료");
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
    /// UI 핸들러 바인딩 (즉시 실행 시스템에서는 스타트 버튼 불필요)
    /// </summary>
    private void BindUIHandlers()
    {
        // 새로운 즉시 실행 시스템에서는 스타트 버튼이 필요하지 않음
        // 필요시 다른 UI 핸들러 바인딩을 여기에 추가
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
        // CardCirculationSystem은 MonoBehaviour이므로 씬에서 찾아야 함
        var circulationSystem = FindFirstObjectByType<CardCirculationSystem>();
        if (circulationSystem == null)
        {
            Debug.LogError("[CombatInstaller] CardCirculationSystem을 씬에서 찾을 수 없습니다.");
            return;
        }

        // ICardCirculationSystem 인터페이스로 바인딩
        Container.Bind<ICardCirculationSystem>().FromInstance(circulationSystem).AsSingle();
        
        // TurnBasedCardManager는 MonoBehaviour이므로 씬에서 찾아야 함
        var turnBasedCardManager = FindFirstObjectByType<TurnBasedCardManager>();
        if (turnBasedCardManager == null)
        {
            Debug.LogWarning("[CombatInstaller] TurnBasedCardManager를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // ITurnBasedCardManager를 null로 바인딩
            Container.Bind<ITurnBasedCardManager>().FromInstance(null).AsSingle();
        }
        else
        {
            // ITurnBasedCardManager 인터페이스로 바인딩
            Container.Bind<ITurnBasedCardManager>().FromInstance(turnBasedCardManager).AsSingle();
        }
        
        Debug.Log("[CombatInstaller] 카드 순환 시스템 바인딩 완료");
    }

    #endregion

    #region 애니메이션 시스템

    /// <summary>
    /// 애니메이션 시스템 바인딩
    /// </summary>
    private void BindAnimationFacade()
    {
        Debug.Log("[CombatInstaller] BindAnimationFacade 호출 시작");
        
        // AnimationFacade를 씬에서 찾기
        var animationFacade = FindFirstObjectByType<AnimationFacade>();
        
        if (animationFacade == null)
        {
            Debug.LogWarning("[CombatInstaller] AnimationFacade를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // IAnimationFacade를 null로 바인딩
            Container.Bind<IAnimationFacade>().FromInstance(null).AsSingle();
        }
        else
        {
            Debug.Log($"[CombatInstaller] AnimationFacade 발견: {animationFacade.name}");
            
            // IAnimationFacade 인터페이스로 바인딩
            Container.Bind<IAnimationFacade>().FromInstance(animationFacade).AsSingle();
            
            Debug.Log("[CombatInstaller] 애니메이션 시스템 바인딩 완료");
        }
    }

    /// <summary>
    /// 저장 시스템 바인딩
    /// </summary>
    private void BindSaveManager()
    {
        Debug.Log("[CombatInstaller] BindSaveManager 호출 시작");
        
        // SaveManager를 씬에서 찾기
        var saveManager = FindFirstObjectByType<SaveManager>();
        
        if (saveManager == null)
        {
            Debug.LogWarning("[CombatInstaller] SaveManager를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // ISaveManager와 SaveManager를 null로 바인딩
            Container.Bind<ISaveManager>().FromInstance(null).AsSingle();
            Container.Bind<SaveManager>().FromInstance(null).AsSingle();
        }
        else
        {
            Debug.Log($"[CombatInstaller] SaveManager 발견: {saveManager.name}");
            
            // ISaveManager 인터페이스로 바인딩
            Container.Bind<ISaveManager>().FromInstance(saveManager).AsSingle();
            
            // SaveManager 클래스로도 바인딩 (PlayerDeckManager가 직접 요구)
            Container.Bind<SaveManager>().FromInstance(saveManager).AsSingle();
            
            Debug.Log("[CombatInstaller] 저장 시스템 바인딩 완료");
        }
    }

    /// <summary>
    /// 오디오 시스템 바인딩
    /// </summary>
    private void BindAudioManager()
    {
        Debug.Log("[CombatInstaller] BindAudioManager 호출 시작");
        
        // AudioManager를 씬에서 찾기
        var audioManager = FindFirstObjectByType<AudioManager>();
        
        if (audioManager == null)
        {
            Debug.LogWarning("[CombatInstaller] AudioManager를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // IAudioManager를 null로 바인딩
            Container.Bind<IAudioManager>().FromInstance(null).AsSingle();
        }
        else
        {
            Debug.Log($"[CombatInstaller] AudioManager 발견: {audioManager.name}");
            
            // IAudioManager 인터페이스로 바인딩
            Container.Bind<IAudioManager>().FromInstance(audioManager).AsSingle();
            
            Debug.Log("[CombatInstaller] 오디오 시스템 바인딩 완료");
        }
    }

    /// <summary>
    /// 코루틴 러너 바인딩
    /// </summary>
    private void BindCoroutineRunner()
    {
        Debug.Log("[CombatInstaller] BindCoroutineRunner 호출 시작");
        
        // CoroutineRunner를 씬에서 찾기
        var coroutineRunner = FindFirstObjectByType<CoroutineRunner>();
        
        if (coroutineRunner == null)
        {
            Debug.LogWarning("[CombatInstaller] CoroutineRunner를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // ICoroutineRunner를 null로 바인딩
            Container.Bind<ICoroutineRunner>().FromInstance(null).AsSingle();
        }
        else
        {
            Debug.Log($"[CombatInstaller] CoroutineRunner 발견: {coroutineRunner.name}");
            
            // ICoroutineRunner 인터페이스로 바인딩
            Container.Bind<ICoroutineRunner>().FromInstance(coroutineRunner).AsSingle();
        }
        
        Debug.Log("[CombatInstaller] 코루틴 러너 바인딩 완료");
    }

    /// <summary>
    /// 슬롯 초기화기 바인딩
    /// </summary>
    private void BindSlotInitializer()
    {
        Debug.Log("[CombatInstaller] BindSlotInitializer 호출 시작");
        
        // SlotInitializer를 씬에서 찾기
        var slotInitializer = FindFirstObjectByType<SlotInitializer>();
        
        if (slotInitializer == null)
        {
            Debug.LogWarning("[CombatInstaller] SlotInitializer를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // SlotInitializer를 null로 바인딩
            Container.Bind<SlotInitializer>().FromInstance(null).AsSingle();
        }
        else
        {
            Debug.Log($"[CombatInstaller] SlotInitializer 발견: {slotInitializer.name}");
            
            // SlotInitializer 클래스로 바인딩
            Container.Bind<SlotInitializer>().FromInstance(slotInitializer).AsSingle();
        }
        
        Debug.Log("[CombatInstaller] 슬롯 초기화기 바인딩 완료");
    }

    #endregion

    #region 덱 관리 시스템

    /// <summary>
    /// 덱 관리 관련 시스템 바인딩
    /// </summary>
    private void BindDeckManagementSystem()
    {
        Debug.Log("[CombatInstaller] 덱 관리 시스템 바인딩 시작");
        
        // PlayerDeckManager를 씬에서 찾기
        var playerDeckManager = FindFirstObjectByType<PlayerDeckManager>();
        
        if (playerDeckManager == null)
        {
            Debug.LogWarning("[CombatInstaller] PlayerDeckManager를 씬에서 찾을 수 없습니다. null로 바인딩합니다.");
            // IPlayerDeckManager를 null로 바인딩
            Container.Bind<IPlayerDeckManager>().FromInstance(null).AsSingle();
        }
        else
        {
            Debug.Log($"[CombatInstaller] PlayerDeckManager 발견: {playerDeckManager.name}");
            
            // IPlayerDeckManager 인터페이스로 바인딩
            Container.Bind<IPlayerDeckManager>().FromInstance(playerDeckManager).AsSingle();
            
            Debug.Log("[CombatInstaller] 덱 관리 시스템 바인딩 완료");
        }
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
    /// Zenject DI가 자동으로 FromComponentInHierarchy()로 바인딩하므로 불필요한 메서드
    /// </summary>
    private void EnsureAndBindInitializer<T>(string gameObjectName) where T : Component
    {
        // 이 메서드는 더 이상 사용되지 않음 - Zenject DI가 자동으로 처리
        Debug.LogWarning($"[CombatInstaller] EnsureAndBindInitializer<{typeof(T).Name}>는 더 이상 사용되지 않습니다. Zenject DI가 자동으로 처리합니다.");
    }

    /// <summary>
    /// 캐릭터 시스템 관련 컴포넌트들을 바인딩합니다.
    /// </summary>
    private void BindCharacterSystem()
    {
        Debug.Log("[CombatInstaller] 캐릭터 시스템 바인딩 시작");
        
        // PlayerResourceManager 바인딩
        var playerResourceManager = FindFirstObjectByType<PlayerResourceManager>();
        if (playerResourceManager != null)
        {
            Container.Bind<PlayerResourceManager>().FromInstance(playerResourceManager).AsSingle();
            Debug.Log("[CombatInstaller] PlayerResourceManager 바인딩 완료");
        }
        else
        {
            Debug.LogWarning("[CombatInstaller] PlayerResourceManager를 찾을 수 없습니다.");
        }
        
        // GameStateManager 바인딩 (DontDestroyOnLoad로 유지되는 인스턴스)
        var gameStateManager = FindFirstObjectByType<GameStateManager>();
        if (gameStateManager != null)
        {
            Container.Bind<IGameStateManager>().FromInstance(gameStateManager).AsSingle();
            Container.Bind<GameStateManager>().FromInstance(gameStateManager).AsSingle();
            Debug.Log("[CombatInstaller] GameStateManager 바인딩 완료");
        }
        else
        {
            Debug.LogWarning("[CombatInstaller] GameStateManager를 찾을 수 없습니다.");
        }
        
        Debug.Log("[CombatInstaller] 캐릭터 시스템 바인딩 완료");
    }

    #endregion
}
