using UnityEngine;
using Game.CombatSystem.Core;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Stage;
using Game.CharacterSystem.Interface;

namespace Game.Utility
{
    /// <summary>
    /// 전투 씬에서 필요한 모든 매니저와 컴포넌트를 찾아 의존성 주입을 수행합니다.
    /// </summary>
    public class CombatBootstrapInstaller : MonoBehaviour
    {
        [Header("전투 초기화 관리자")]
        [SerializeField] private CombatInitializerManager combatInitializer;

        private void Start()
        {
            Debug.Log("[CombatBootstrapInstaller] Start() 진입");

            if (combatInitializer == null)
            {
                Debug.LogError("[CombatBootstrapInstaller] CombatInitializerManager가 연결되지 않았습니다.");
                return;
            }

            InjectDependencies();
        }

        private void InjectDependencies()
        {
            //Debug.Log("[CombatBootstrapInstaller] 의존성 검색 시작");

            // Resolve all dependencies
            var slotInitializer = Resolve<ISlotInitializer>();
            var playerInitializer = Resolve<IPlayerCharacterInitializer>();
            var enemyInitializer = Resolve<IEnemyInitializer>();
            var playerHandManager = Resolve<IPlayerHandManager>();
            var enemyHandManager = Resolve<IEnemyHandManager>();
            var enemyManager = Resolve<IEnemyManager>();
            var turnManager = Resolve<ICombatTurnManager>();
            var stageManager = Resolve<IStageManager>();
            var stateFactory = Resolve<ICombatStateFactory>();
            var spawner = Resolve<IEnemySpawnerManager>();
            var slotRegistry = Resolve<ISlotRegistry>();
            var playerManager = Resolve<IPlayerManager>();

            if (playerManager == null)
            {
                //Debug.LogError("[CombatBootstrapInstaller] PlayerManager를 찾지 못했습니다. 초기화 중단");
                return;
            }

            // 1. PlayerCharacterInitializer에 PlayerManager 주입
            if (playerInitializer != null)
            {
                playerInitializer.Inject(playerManager);
                //Debug.Log("[CombatBootstrapInstaller] PlayerCharacterInitializer에 PlayerManager 주입 완료");
            }

            // 2. PlayerHandManager → PlayerManager
            if (playerHandManager != null)
            {
                playerManager.SetPlayerHandManager(playerHandManager);
                //Debug.Log("[CombatBootstrapInstaller] PlayerManager에 PlayerHandManager 등록 완료");
            }

            // 3. StageManager <-> SpawnerManager 간 상호 주입
            if (stageManager is StageManager concreteStage &&
                spawner is EnemySpawnerManager concreteSpawner &&
                enemyManager != null && enemyHandManager != null)
            {
                concreteStage.Inject(concreteSpawner, enemyManager, enemyHandManager); // StageManager에 주입
                concreteSpawner.InjectStageManager(concreteStage);                      // SpawnerManager에 StageManager 주입
                //Debug.Log("[CombatBootstrapInstaller] StageManager <-> SpawnerManager 의존성 주입 완료");
            }
            else
            {
                //Debug.LogWarning("[CombatBootstrapInstaller] StageManager 또는 SpawnerManager 주입 실패");
            }

            // 4. CombatInitializerManager에 모든 의존성 주입
            //Debug.Log("[CombatBootstrapInstaller] CombatInitializerManager.Inject() 수행 시작");
            combatInitializer.Inject(
                slotInitializer,
                playerInitializer,
                enemyInitializer,
                playerHandManager,
                enemyHandManager,
                enemyManager,
                turnManager,
                stageManager,
                stateFactory,
                slotRegistry,
                playerManager
            );
            //Debug.Log("[CombatBootstrapInstaller] CombatInitializerManager 의존성 주입 완료");
        }

        /// <summary>
        /// 지정한 인터페이스 타입의 컴포넌트를 모든 오브젝트에서 탐색
        /// </summary>
        private T Resolve<T>() where T : class
        {
            var objects = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in objects)
            {
                if (obj is T target)
                {
                    //Debug.Log($"[CombatBootstrapInstaller] {typeof(T).Name} 구현체 발견 → {obj.name}");
                    return target;
                }
            }

            //Debug.LogWarning($"[CombatBootstrapInstaller] {typeof(T).Name} 구현체를 찾지 못했습니다.");
            return null;
        }

        /// <summary>
        /// 외부에서 호출 가능한 수동 초기화 메서드
        /// </summary>
        public void Initialize()
        {
            InjectDependencies();
        }
    }
}
