using UnityEngine;
using Zenject;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Interface;
using Game.CoreSystem.UI;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Save;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Manager;
using Game.CharacterSystem.Manager;
using System.Linq;
using System.Collections.Generic;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Service;
using Game.ItemSystem.Service.Reward;
using Game.CoreSystem.Statistics;

namespace Game.CoreSystem
{
    /// <summary>
    /// CoreSystem용 Zenject 설치자 (최적화됨)
    /// 모든 코어 시스템 매니저와 서비스를 효율적으로 바인딩합니다.
    /// </summary>
    public class CoreSystemInstaller : MonoInstaller<CoreSystemInstaller>
    {
        [Header("CoreSystem 매니저들")]
        [SerializeField] private CoreSystemInitializer coreSystemInitializer;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private SceneTransitionManager sceneTransitionManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private SettingsManager settingsManager;
        [SerializeField] private CoroutineRunner coroutineRunner;
        [SerializeField] private PlayerCharacterSelectionManager playerCharacterSelectionManager;
        [SerializeField] private SkillCardTooltipManager skillCardTooltipManager;
        [SerializeField] private BuffDebuffTooltipManager buffDebuffTooltipManager;
        [SerializeField] private Game.ItemSystem.Manager.ItemTooltipManager itemTooltipManager;
        [SerializeField] private GameSessionStatistics gameSessionStatistics;
        [SerializeField] private StatisticsManager statisticsManager;
        [SerializeField] private LeaderboardManager leaderboardManager;
        
        [Header("CharacterSystem 매니저들")]
        [SerializeField] private PlayerManager playerManager;

        [Header("DI 최적화 설정")]
#pragma warning disable CS0414 // 사용하지 않는 필드 경고 억제 (향후 사용 예정)
        [SerializeField] private bool enableLazyInitialization = true;
        [SerializeField] private bool enableCircularDependencyCheck = true;
#pragma warning restore CS0414
        [SerializeField] private bool enablePerformanceLogging = false;

        public override void InstallBindings()
        {
            // 성능 측정 시작
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 최적화된 바인딩 순서
            BindCoreServices();      // 서비스 먼저 (의존성 없음)
            BindCoreUtilities();     // 유틸리티 (서비스 의존)
            BindCoreManagers();      // 매니저 (서비스/유틸리티 의존)
            BindCoreInterfaces();    // 인터페이스 (모든 것 의존)
            
            stopwatch.Stop();
            if (enablePerformanceLogging)
            {
                GameLogger.LogInfo($"CoreSystemInstaller 바인딩 완료: {stopwatch.ElapsedMilliseconds}ms", GameLogger.LogCategory.Core);
            }
        }

        #region 코어 매니저 바인딩 (최적화됨)

        /// <summary>
        /// 코어 매니저들을 최적화된 방식으로 바인딩합니다.
        /// </summary>
        private void BindCoreManagers()
        {
            // 매니저들을 배열로 관리하여 반복문으로 처리
            var managers = new (MonoBehaviour instance, string name, System.Type interfaceType)[]
            {
                (coreSystemInitializer, "CoreSystemInitializer", null),
                (sceneTransitionManager, "SceneTransitionManager", typeof(ISceneTransitionManager)),
                (gameStateManager, "GameStateManager", typeof(IGameStateManager)),
                (audioManager, "AudioManager", typeof(IAudioManager)),
                (saveManager, "SaveManager", typeof(ISaveManager)),
                (settingsManager, "SettingsManager", null),
                (playerCharacterSelectionManager, "PlayerCharacterSelectionManager", typeof(IPlayerCharacterSelectionManager)),
                (skillCardTooltipManager, "SkillCardTooltipManager", null),
                (buffDebuffTooltipManager, "BuffDebuffTooltipManager", null),
                (itemTooltipManager, "ItemTooltipManager", null),
                (playerManager, "PlayerManager", null),
                (gameSessionStatistics, "GameSessionStatistics", null),
                (statisticsManager, "StatisticsManager", typeof(IStatisticsManager)),
                (leaderboardManager, "LeaderboardManager", typeof(ILeaderboardManager))
            };

            foreach (var (instance, name, interfaceType) in managers)
            {
                MonoBehaviour finalInstance = instance;
                
                // Inspector에 할당되지 않은 경우 씬에서 찾기
                if (finalInstance == null)
                {
                    finalInstance = FindManagerInScene(name);
                }
                
                // 여전히 null이면 경고만 출력 (일부 매니저는 선택적일 수 있음)
                if (finalInstance == null)
                {
                    // PlayerManager는 CombatScene에서 생성되므로 CoreScene에서는 없어도 정상
                    if (name == "PlayerManager")
                    {
                        GameLogger.LogInfo($"{name}가 CoreScene에 없습니다. (CombatScene에서 생성됨 - 정상)", GameLogger.LogCategory.Core);
                    }
                    else
                    {
                        GameLogger.LogWarning($"{name}가 할당되지 않았습니다. (선택적 매니저일 수 있음)", GameLogger.LogCategory.Core);
                    }
                    continue;
                }
                
                // 기본 바인딩
                Container.Bind(finalInstance.GetType()).FromInstance(finalInstance).AsSingle();
                
                // 인터페이스 바인딩 (있는 경우)
                if (interfaceType != null)
                {
                    Container.Bind(interfaceType).FromInstance(finalInstance).AsSingle();
                }
                
                // 의존성 주입 예약
                Container.QueueForInject(finalInstance);
                
                if (enablePerformanceLogging)
                {
                    GameLogger.LogInfo($"{name} 바인딩 완료", GameLogger.LogCategory.Core);
                }
            }
        }

        #endregion

        #region 코어 서비스 바인딩

        /// <summary>
        /// 코어 서비스들을 바인딩합니다.
        /// </summary>
        private void BindCoreServices()
        {
            // AudioPoolManager는 AudioManager에 포함되어 있으므로 별도 바인딩 불필요
            // CardStateCollector, CardStateRestorer는 제거되었고 SaveManager에 StageProgressCollector가 포함됨

            // ItemSystem 전역 바인딩
            // IItemService: 전역 싱글톤으로 생성(없으면 새 GO 생성)
            Container.Bind<IItemService>()
                .To<ItemService>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            // IRewardGenerator: 순수 서비스 싱글톤
            Container.Bind<IRewardGenerator>()
                .To<RewardGenerator>()
                .AsSingle();

            // ICombatStatsProvider: 전투 통계 수집기 (StageScene에서 생성되지만 전역에서 찾기)
            Container.Bind<Game.CombatSystem.Manager.ICombatStatsProvider>()
                .FromMethod(_ =>
                {
                    return Object.FindFirstObjectByType<Game.CombatSystem.Manager.CombatStatsAggregator>(FindObjectsInactive.Include);
                })
                .AsSingle()
                .NonLazy();

            // AudioEventTrigger: 씬에 있는 컴포넌트를 찾아서 바인딩 (Optional)
            Container.Bind<AudioEventTrigger>()
                .FromComponentInHierarchy()
                .AsSingle()
                .NonLazy();

            // VictoryUI: 씬에 있는 컴포넌트를 찾아서 바인딩 (Optional)
            Container.Bind<Game.CombatSystem.UI.VictoryUI>()
                .FromComponentInHierarchy()
                .AsSingle()
                .NonLazy();
        }

        #endregion

        #region 코어 유틸리티 바인딩

        /// <summary>
        /// 코어 유틸리티들을 바인딩합니다.
        /// </summary>
        private void BindCoreUtilities()
        {
            // CoroutineRunner - 인터페이스와 함께 바인딩
            EnsureAndBindCoreManagerWithInterface<CoroutineRunner, ICoroutineRunner>(coroutineRunner, "CoroutineRunner");
            
            // UnityMainThreadDispatcher - 인터페이스와 함께 바인딩 (전역 시스템으로 추가)
            Container.BindInterfacesAndSelfTo<UnityMainThreadDispatcher>()
                .FromComponentInHierarchy().AsSingle();
            
            // GameLogger (정적 클래스이므로 인스턴스 바인딩 불필요)
            // GameLogger는 정적 메서드로 사용하므로 DI 바인딩하지 않음
        }

        #endregion

        #region 인터페이스 바인딩

        /// <summary>
        /// 코어 인터페이스들을 바인딩합니다.
        /// </summary>
        private void BindCoreInterfaces()
        {
            // ICoreSystemInitializable 구현체들을 자동으로 찾아서 바인딩
            var initializableComponents = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<ICoreSystemInitializable>();
            
            // 이미 BindCoreManagers에서 바인딩된 매니저들을 제외
            // BindCoreManagers()에서 바인딩하는 모든 매니저 타입을 포함해야 함
            var alreadyBoundTypes = new HashSet<System.Type>
            {
                typeof(CoreSystemInitializer),
                typeof(SceneTransitionManager),
                typeof(GameStateManager),
                typeof(AudioManager),
                typeof(SaveManager),
                typeof(SettingsManager),
                typeof(PlayerCharacterSelectionManager),
                typeof(SkillCardTooltipManager),
                typeof(BuffDebuffTooltipManager),
                typeof(Game.ItemSystem.Manager.ItemTooltipManager),
                typeof(PlayerManager),
                typeof(GameSessionStatistics),
                typeof(StatisticsManager),
                typeof(LeaderboardManager)
            };
            
            // 아직 바인딩되지 않은 ICoreSystemInitializable 컴포넌트만 바인딩
            foreach (var component in initializableComponents)
            {
                if (!alreadyBoundTypes.Contains(component.GetType()))
                {
                    Container.BindInterfacesAndSelfTo(component.GetType()).FromInstance(component).AsSingle();
                }
            }
            
            // ICoreSystemInitializable 리스트로 바인딩 (CoreSystemInitializer에서 사용)
            // CoreSystemInitializer는 자기 자신을 제외하고 바인딩
            // 하지만 이미 바인딩된 매니저들도 coreSystems에 포함시켜야 초기화가 진행됨
            Container.Bind<List<ICoreSystemInitializable>>().FromMethod(context =>
            {
                // 이미 바인딩된 매니저들도 포함 (초기화를 위해)
                var allSystems = initializableComponents
                    .Where(x => !ReferenceEquals(x, coreSystemInitializer))
                    .ToList();
                
                // 디버그 로그
                if (enablePerformanceLogging)
                {
                    GameLogger.LogInfo($"[CoreSystemInstaller] coreSystems에 {allSystems.Count}개 시스템 포함", GameLogger.LogCategory.Core);
                }
                
                return new List<ICoreSystemInitializable>(allSystems);
            }).AsSingle();
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 코어 매니저를 보장하고 바인딩합니다.
        /// </summary>
        private void EnsureAndBindCoreManager<T>(T instance, string gameObjectName) where T : MonoBehaviour
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    var go = new GameObject(gameObjectName);
                    instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                }
            }
            
            // 의존성 주입 예약 (바인딩은 BindCoreInterfaces에서 처리)
            Container.QueueForInject(instance);
        }

        /// <summary>
        /// 코어 매니저를 보장하고 인터페이스와 함께 바인딩합니다.
        /// </summary>
        private void EnsureAndBindCoreManagerWithInterface<TConcrete, TInterface>(TConcrete instance, string gameObjectName) 
            where TConcrete : MonoBehaviour, TInterface
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<TConcrete>();
                if (instance == null)
                {
                    var go = new GameObject(gameObjectName);
                    instance = go.AddComponent<TConcrete>();
                    DontDestroyOnLoad(go);
                }
            }
            
            // 의존성 주입 예약 (바인딩은 BindCoreInterfaces에서 처리)
            Container.QueueForInject(instance);
        }

        /// <summary>
        /// 매니저 이름으로 씬에서 매니저를 찾습니다.
        /// </summary>
        private MonoBehaviour FindManagerInScene(string name)
        {
            return name switch
            {
                "CoreSystemInitializer" => FindFirstObjectByType<CoreSystemInitializer>(FindObjectsInactive.Include),
                "SceneTransitionManager" => FindFirstObjectByType<SceneTransitionManager>(FindObjectsInactive.Include),
                "GameStateManager" => FindFirstObjectByType<GameStateManager>(FindObjectsInactive.Include),
                "AudioManager" => FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include),
                "SaveManager" => FindFirstObjectByType<SaveManager>(FindObjectsInactive.Include),
                "SettingsManager" => FindFirstObjectByType<SettingsManager>(FindObjectsInactive.Include),
                "PlayerCharacterSelectionManager" => FindFirstObjectByType<PlayerCharacterSelectionManager>(FindObjectsInactive.Include),
                "SkillCardTooltipManager" => FindFirstObjectByType<SkillCardTooltipManager>(FindObjectsInactive.Include),
                "BuffDebuffTooltipManager" => FindFirstObjectByType<BuffDebuffTooltipManager>(FindObjectsInactive.Include),
                "ItemTooltipManager" => FindFirstObjectByType<Game.ItemSystem.Manager.ItemTooltipManager>(FindObjectsInactive.Include),
                "PlayerManager" => FindFirstObjectByType<PlayerManager>(FindObjectsInactive.Include),
                "GameSessionStatistics" => FindFirstObjectByType<GameSessionStatistics>(FindObjectsInactive.Include),
                "StatisticsManager" => FindFirstObjectByType<StatisticsManager>(FindObjectsInactive.Include),
                "LeaderboardManager" => FindFirstObjectByType<LeaderboardManager>(FindObjectsInactive.Include),
                _ => null
            };
        }

        #endregion

    }
}
