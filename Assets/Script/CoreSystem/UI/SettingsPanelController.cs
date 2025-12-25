using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Manager;
using Game.CombatSystem.State;
using Game.CombatSystem.Manager;
using Game.CombatSystem.UI;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.UI;
using Zenject;

namespace Game.CoreSystem.UI
{
    /// <summary>
    /// 시스템 기능 전용 설정창 UI 컨트롤러
    /// 이어하기, 진행도 초기화, 메인 메뉴, 게임 종료 기능 제공
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private Button goToMainButton;
        [SerializeField] private Button exitGameButton;
        
        // DI로 주입
        [Inject(Optional = true)] private ISceneTransitionManager sceneTransitionManager;
        [Inject(Optional = true)] private SettingsManager settingsManager;
        [Inject(Optional = true)] private Game.StageSystem.Interface.IStageManager stageManager;
        [Inject(Optional = true)] private Game.ItemSystem.Interface.IItemService itemService;
        [Inject(Optional = true)] private Game.CharacterSystem.Manager.PlayerManager playerManager;
        [Inject(Optional = true)] private Game.SkillCardSystem.Interface.IPlayerHandManager playerHandManager;
        [Inject(Optional = true)] private CombatStateMachine combatStateMachine;
        [Inject(Optional = true)] private TurnManager turnManager;
        [Inject(Optional = true)] private EnemyManager enemyManager;
        [Inject(Optional = true)] private GameOverUI gameOverUI;
        [Inject(Optional = true)] private VictoryUI victoryUI;
        // SaveSystem 및 Statistics 제거됨
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogging = false;
        
        private void Start()
        {
            // 매니저들 찾기
            FindManagers();
            InitializeUI();
        }
        
        /// <summary>
        /// 매니저들 찾기
        /// </summary>
        private void FindManagers()
        {
            // saveManager, sceneTransitionManager, settingsManager는 DI로 주입받음
            if (enableDebugLogging)
            {
            }
        }

        /// <summary>
        /// SceneTransitionManager가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsureSceneTransitionManagerInjected()
        {
            if (sceneTransitionManager != null) return;

            try
            {
                // 1. ProjectContext에서 SceneContextRegistry를 통해 찾기
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                        if (sceneContainer != null)
                        {
                            var resolvedManager = sceneContainer.TryResolve<ISceneTransitionManager>();
                            if (resolvedManager != null)
                            {
                                sceneTransitionManager = resolvedManager;
                                GameLogger.LogInfo("[SettingsPanelController] SceneTransitionManager를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.UI);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[SettingsPanelController] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Manager.SceneTransitionManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    sceneTransitionManager = foundManager;
                    GameLogger.LogInfo("[SettingsPanelController] SceneTransitionManager 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SettingsPanelController] SceneTransitionManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// SettingsManager가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsureSettingsManagerInjected()
        {
            if (settingsManager != null) return;

            try
            {
                // 1. ProjectContext에서 SceneContextRegistry를 통해 찾기
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                        if (sceneContainer != null)
                        {
                            var resolvedManager = sceneContainer.TryResolve<SettingsManager>();
                            if (resolvedManager != null)
                            {
                                settingsManager = resolvedManager;
                                GameLogger.LogInfo("[SettingsPanelController] SettingsManager를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.UI);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[SettingsPanelController] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                var foundManager = UnityEngine.Object.FindFirstObjectByType<SettingsManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    settingsManager = foundManager;
                    GameLogger.LogInfo("[SettingsPanelController] SettingsManager 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SettingsPanelController] SettingsManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// StageManager가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsureStageManagerInjected()
        {
            if (stageManager != null) return;

            try
            {
                // 1. ProjectContext에서 SceneContextRegistry를 통해 찾기
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                        if (sceneContainer != null)
                        {
                            var resolvedManager = sceneContainer.TryResolve<Game.StageSystem.Interface.IStageManager>();
                            if (resolvedManager != null)
                            {
                                stageManager = resolvedManager;
                                GameLogger.LogInfo("[SettingsPanelController] StageManager를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.UI);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[SettingsPanelController] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    stageManager = foundManager;
                    GameLogger.LogInfo("[SettingsPanelController] StageManager 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SettingsPanelController] StageManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// ItemService가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsureItemServiceInjected()
        {
            if (itemService != null) return;

            try
            {
                // 1. ProjectContext에서 SceneContextRegistry를 통해 찾기
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                        if (sceneContainer != null)
                        {
                            var resolvedService = sceneContainer.TryResolve<Game.ItemSystem.Interface.IItemService>();
                            if (resolvedService != null)
                            {
                                itemService = resolvedService;
                                GameLogger.LogInfo("[SettingsPanelController] ItemService를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.UI);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[SettingsPanelController] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                var foundService = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>(UnityEngine.FindObjectsInactive.Include);
                if (foundService != null)
                {
                    itemService = foundService;
                    GameLogger.LogInfo("[SettingsPanelController] ItemService 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SettingsPanelController] ItemService 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// PlayerManager가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsurePlayerManagerInjected()
        {
            if (playerManager != null) return;

            try
            {
                // 1. ProjectContext에서 SceneContextRegistry를 통해 찾기
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                        if (sceneContainer != null)
                        {
                            var resolvedManager = sceneContainer.TryResolve<Game.CharacterSystem.Manager.PlayerManager>();
                            if (resolvedManager != null)
                            {
                                playerManager = resolvedManager;
                                GameLogger.LogInfo("[SettingsPanelController] PlayerManager를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.UI);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[SettingsPanelController] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    playerManager = foundManager;
                    GameLogger.LogInfo("[SettingsPanelController] PlayerManager 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SettingsPanelController] PlayerManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// PlayerHandManager가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsurePlayerHandManagerInjected()
        {
            if (playerHandManager != null) return;

            try
            {
                // 1. ProjectContext에서 SceneContextRegistry를 통해 찾기
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                        if (sceneContainer != null)
                        {
                            var resolvedManager = sceneContainer.TryResolve<Game.SkillCardSystem.Interface.IPlayerHandManager>();
                            if (resolvedManager != null)
                            {
                                playerHandManager = resolvedManager;
                                GameLogger.LogInfo("[SettingsPanelController] PlayerHandManager를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.UI);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[SettingsPanelController] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerHandManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    playerHandManager = foundManager;
                    GameLogger.LogInfo("[SettingsPanelController] PlayerHandManager 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SettingsPanelController] PlayerHandManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
            
            if (resetProgressButton != null)
                resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            
            if (goToMainButton != null)
                goToMainButton.onClick.AddListener(OnGoToMainClicked);
            
            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(OnExitGameClicked);
            
            if (enableDebugLogging)
            {
            }
        }
        
        #region 이벤트 핸들러
        
        /// <summary>
        /// 설정창 닫기
        /// </summary>
        private void OnCloseButtonClicked()
        {
            EnsureSettingsManagerInjected();
            
            if (settingsManager != null)
            {
                settingsManager.CloseSettings();
            }
            else
            {
                GameLogger.LogWarning("[SettingsPanelController] SettingsManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 이어하기 (계속 진행하기) - 창 닫기
        /// </summary>
        private void OnContinueClicked()
        {
            try
            {
                EnsureSettingsManagerInjected();
                
                if (enableDebugLogging)
                {
                }
                // 설정창 닫기 (게임 계속 진행)
                if (settingsManager != null)
                {
                    settingsManager.CloseSettings();
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] SettingsManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"이어하기 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 다시하기 (스테이지 1로 다시 시작)
        /// 새 게임 시작과 동일한 완전한 초기화를 수행합니다.
        /// </summary>
        private void OnResetProgressClicked()
        {
            try
            {
                EnsureStageManagerInjected();
                EnsureItemServiceInjected();
                EnsurePlayerManagerInjected();
                EnsurePlayerHandManagerInjected();
                EnsureSettingsManagerInjected();
                
                // StageManager가 없으면 실패
                if (stageManager == null)
                {
                    GameLogger.LogError("[SettingsPanelController] StageManager를 찾을 수 없습니다. 다시하기를 수행할 수 없습니다.", GameLogger.LogCategory.Error);
                    return;
                }
                
                GameLogger.LogInfo("[SettingsPanelController] 다시하기 시작 - 완전 초기화 수행", GameLogger.LogCategory.UI);
                
                // 1. UI 초기화 (GameOverUI, VictoryUI 숨김)
                if (gameOverUI != null)
                {
                    gameOverUI.HideGameOver();
                    GameLogger.LogInfo("[SettingsPanelController] GameOverUI 숨김", GameLogger.LogCategory.UI);
                }
                
                if (victoryUI != null)
                {
                    victoryUI.Hide();
                    GameLogger.LogInfo("[SettingsPanelController] VictoryUI 숨김", GameLogger.LogCategory.UI);
                }
                
                // 2. 전투 상태 머신 리셋 (가장 먼저 수행)
                if (combatStateMachine != null)
                {
                    var currentState = combatStateMachine.GetCurrentState();
                    if (currentState != null)
                    {
                        GameLogger.LogInfo($"[SettingsPanelController] 전투 상태 리셋: {currentState.StateName} → None", GameLogger.LogCategory.Combat);
                        combatStateMachine.ResetCombatState();
                    }
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] CombatStateMachine을 찾을 수 없습니다. 전투 상태 리셋을 건너뜁니다.", GameLogger.LogCategory.UI);
                }
                
                // 3. TurnManager 완전 리셋
                if (turnManager != null)
                {
                    turnManager.ClearAllSlots();
                    turnManager.ClearEnemyCache();
                    turnManager.ResetSlotStates();
                    turnManager.ResetTurn();
                    GameLogger.LogInfo("[SettingsPanelController] TurnManager 완전 리셋 완료", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] TurnManager를 찾을 수 없습니다. 턴 관리자 리셋을 건너뜁니다.", GameLogger.LogCategory.UI);
                }
                
                // 4. 소환 데이터 초기화 (가장 먼저 수행 - 소환 판정이 남아있을 수 있음)
                var concreteStageManager = stageManager as Game.StageSystem.Manager.StageManager;
                if (concreteStageManager != null)
                {
                    concreteStageManager.ClearSummonData();
                    GameLogger.LogInfo("[SettingsPanelController] 소환 데이터 초기화 완료", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] StageManager를 구체 타입으로 캐스팅할 수 없습니다. 소환 데이터 초기화를 건너뜁니다.", GameLogger.LogCategory.UI);
                }
                
                // 5. 적 정리 (EnemyManager에서 등록 해제 및 GameObject 파괴)
                if (enemyManager != null)
                {
                    var currentEnemy = enemyManager.GetEnemy();
                    if (currentEnemy != null)
                    {
                        GameLogger.LogInfo($"[SettingsPanelController] 기존 적 정리 시작: {currentEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                        
                        // EnemyManager에서 등록 해제
                        enemyManager.UnregisterEnemy();
                        
                        // 적 GameObject 파괴
                        if (currentEnemy is EnemyCharacter enemyChar)
                        {
                            UnityEngine.Object.Destroy(enemyChar.gameObject);
                            GameLogger.LogInfo("[SettingsPanelController] 기존 적 GameObject 파괴 완료", GameLogger.LogCategory.Combat);
                        }
                    }
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] EnemyManager를 찾을 수 없습니다. 적 정리를 건너뜁니다.", GameLogger.LogCategory.UI);
                }
                
                // 6. 플레이어 핸드 초기화
                if (playerHandManager != null)
                {
                    playerHandManager.ClearAll();
                    GameLogger.LogInfo("[SettingsPanelController] 플레이어 핸드 초기화 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] PlayerHandManager를 찾을 수 없습니다. 핸드 초기화를 건너뜁니다.", GameLogger.LogCategory.UI);
                }
                
                // 7. 플레이어 버프/디버프 초기화 (플레이어 제거 전에)
                if (playerManager != null)
                {
                    var currentPlayer = playerManager.GetCharacter();
                    if (currentPlayer != null)
                    {
                        // 버프/디버프 아이콘 제거
                        if (currentPlayer is PlayerCharacter playerChar)
                        {
                            playerChar.ClearAllBuffDebuffIcons();
                        }
                    }
                }
                
                // 8. 인벤토리 완전 초기화 (ItemService는 DontDestroyOnLoad이므로 명시적으로 초기화 필요)
                if (itemService != null)
                {
                    itemService.ResetInventoryForNewGame();
                    GameLogger.LogInfo("[SettingsPanelController] 인벤토리 초기화 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] ItemService를 찾을 수 없습니다. 인벤토리 초기화를 건너뜁니다.", GameLogger.LogCategory.UI);
                }
                
                // 9. 플레이어 완전히 다시 생성 (버프/디버프, 리소스 등 모든 상태 초기화)
                if (playerManager != null)
                {
                    // 기존 플레이어 제거
                    var currentPlayer = playerManager.GetCharacter();
                    if (currentPlayer != null)
                    {
                        // 버프/디버프 아이콘 제거
                        if (currentPlayer is PlayerCharacter playerChar)
                        {
                            playerChar.ClearAllBuffDebuffIcons();
                        }
                        
                        // 플레이어 GameObject 제거
                        if (currentPlayer is MonoBehaviour playerMono)
                        {
                            UnityEngine.Object.Destroy(playerMono.gameObject);
                            GameLogger.LogInfo("[SettingsPanelController] 기존 플레이어 캐릭터 제거 완료", GameLogger.LogCategory.UI);
                        }
                    }
                    
                    // 플레이어 등록 해제
                    playerManager.UnregisterCharacter();
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] PlayerManager를 찾을 수 없습니다. 플레이어 재생성을 건너뜁니다.", GameLogger.LogCategory.UI);
                }
                
                // 10. NEW_GAME_REQUESTED 플래그 설정 (StageManager.Start()에서 인벤토리 초기화를 다시 수행하도록)
                PlayerPrefs.SetInt("NEW_GAME_REQUESTED", 1);
                PlayerPrefs.Save();
                
                // 11. 플레이어 재생성 완료 대기 후 스테이지 시작 (코루틴으로 처리)
                StartCoroutine(ResetProgressCoroutine());
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"다시하기 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 다시하기 코루틴 (플레이어 재생성 후 스테이지 시작)
        /// OnPlayerCharacterReady 이벤트를 기다려 완전한 초기화를 보장합니다.
        /// </summary>
        private System.Collections.IEnumerator ResetProgressCoroutine()
        {
            // GameObject 파괴 완료 대기 (다음 프레임까지)
            yield return new WaitForEndOfFrame();
            yield return null;
            
            // 플레이어 재생성
            bool playerReady = false;
            System.Action<Game.CharacterSystem.Interface.ICharacter> onReady = (player) => 
            { 
                playerReady = true;
                GameLogger.LogInfo($"[SettingsPanelController] 플레이어 재생성 완료 이벤트 수신: {player.GetCharacterName()}", GameLogger.LogCategory.UI);
            };
            
            if (playerManager != null)
            {
                // 이벤트 구독
                playerManager.OnPlayerCharacterReady += onReady;
                
                // 플레이어 다시 생성 (CreateAndRegisterCharacter 사용 - UI 연결, 핸드 매니저 초기화 포함)
                // 이 메서드는 currentCharacter가 null일 때만 새로 생성하므로 UnregisterCharacter() 후 호출
                try
                {
                    playerManager.CreateAndRegisterCharacter();
                    GameLogger.LogInfo("[SettingsPanelController] 플레이어 캐릭터 재생성 요청 완료", GameLogger.LogCategory.UI);
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError($"[SettingsPanelController] 플레이어 재생성 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
                    playerManager.OnPlayerCharacterReady -= onReady;
                    yield break;
                }
                
                // OnPlayerCharacterReady 이벤트 발생 대기 (최대 5초)
                float timeout = 5f;
                float elapsed = 0f;
                while (!playerReady && elapsed < timeout)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                    
                    // 폴백: 이벤트 없이도 플레이어가 생성되었는지 확인
                    if (playerManager.GetCharacter() != null && !playerReady)
                    {
                        GameLogger.LogInfo("[SettingsPanelController] 플레이어 생성 확인 (이벤트 없이)", GameLogger.LogCategory.UI);
                        playerReady = true;
                    }
                }
                
                // 이벤트 구독 해제
                playerManager.OnPlayerCharacterReady -= onReady;
                
                if (!playerReady)
                {
                    GameLogger.LogError("[SettingsPanelController] 플레이어 재생성 실패 - 게임을 계속할 수 없습니다", GameLogger.LogCategory.Error);
                    yield break;
                }
                
                // 추가 안정화 대기 (입장 애니메이션 완료 후 UI 연결 시간)
                yield return new WaitForSeconds(0.3f);
                
                // 플레이어 재생성 완료 후 버프/디버프 아이콘 명시적 초기화
                // OnBuffsChanged 이벤트가 발생하지 않을 수 있으므로 직접 UI를 초기화
                var playerUIController = UnityEngine.Object.FindFirstObjectByType<PlayerCharacterUIController>(UnityEngine.FindObjectsInactive.Include);
                if (playerUIController != null)
                {
                    playerUIController.ClearAllBuffDebuffIcons();
                    GameLogger.LogInfo("[SettingsPanelController] 버프/디버프 아이콘 명시적 초기화 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[SettingsPanelController] PlayerCharacterUIController를 찾을 수 없습니다. 버프/디버프 아이콘 초기화를 건너뜁니다.", GameLogger.LogCategory.UI);
                }
            }
            else
            {
                GameLogger.LogError("[SettingsPanelController] PlayerManager를 찾을 수 없습니다. 게임을 계속할 수 없습니다.", GameLogger.LogCategory.Error);
                yield break;
            }
            
            // StageManager를 통해 스테이지 1로 리셋
            var concreteStageManager = stageManager as Game.StageSystem.Manager.StageManager;
            if (concreteStageManager != null)
            {
                // 스테이지 1 로드
                if (concreteStageManager.LoadStage(1))
                {
                    GameLogger.LogInfo("[SettingsPanelController] 스테이지 1로 다시 시작", GameLogger.LogCategory.UI);
                    
                    // 스테이지 시작 (플레이어가 재생성되었으므로 정상 작동)
                    concreteStageManager.StartStage();
                }
                else
                {
                    GameLogger.LogError("[SettingsPanelController] 스테이지 1 로드 실패", GameLogger.LogCategory.Error);
                }
            }
            else
            {
                GameLogger.LogError("[SettingsPanelController] StageManager를 구체 타입으로 캐스팅할 수 없습니다", GameLogger.LogCategory.Error);
            }
            
            // 설정창 닫기
            if (settingsManager != null)
            {
                settingsManager.CloseSettings();
            }
        }
        
        /// <summary>
        /// 메인 메뉴로 돌아가기
        /// </summary>
        private async void OnGoToMainClicked()
        {
            try
            {
                EnsureSceneTransitionManagerInjected();
                EnsureSettingsManagerInjected();
                
                if (sceneTransitionManager != null)
                {
                    await sceneTransitionManager.TransitionToMainScene();
                }
                else
                {
                    GameLogger.LogError("[SettingsPanelController] SceneTransitionManager를 찾을 수 없습니다", GameLogger.LogCategory.Error);
                    return;
                }
                
                if (settingsManager != null)
                {
                    settingsManager.CloseSettings();
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"메인 메뉴 이동 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        private void OnExitGameClicked()
        {
            try
            {
                // 게임 종료
                Application.Quit();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"게임 종료 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        #endregion
    }
}
