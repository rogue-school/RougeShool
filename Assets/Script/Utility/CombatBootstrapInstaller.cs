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
        [Header("전투 흐름 관리자 (Flow Coordinator)")]
        [SerializeField] private CombatFlowCoordinator combatFlowCoordinator;

        private void Start()
        {
            Debug.Log("[CombatBootstrapInstaller] Start() 진입");

            if (combatFlowCoordinator == null)
            {
                Debug.LogError("[CombatBootstrapInstaller] CombatFlowCoordinator가 연결되지 않았습니다.");
                return;
            }

            InjectDependencies();
            combatFlowCoordinator.StartCombatFlow();
        }

        private void InjectDependencies()
        {
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

            if (playerManager == null) return;

            // PlayerCharacterInitializer에 PlayerManager 주입
            playerInitializer?.Inject(playerManager);

            // PlayerManager에 핸드 매니저 연결
            if (playerHandManager != null)
                playerManager.SetPlayerHandManager(playerHandManager);

            // StageManager <-> SpawnerManager 상호 주입
            if (stageManager is StageManager concreteStage &&
                spawner is EnemySpawnerManager concreteSpawner &&
                enemyManager != null && enemyHandManager != null)
            {
                concreteStage.Inject(concreteSpawner, enemyManager, enemyHandManager);
                concreteSpawner.InjectStageManager(concreteStage);
            }

            // CombatFlowCoordinator에 모든 의존성 주입
            combatFlowCoordinator.Inject(
                slotInitializer,
                playerInitializer,
                enemyInitializer,
                playerHandManager,
                enemyHandManager,
                enemyManager,
                spawner,
                turnManager,
                stageManager,
                stateFactory,
                slotRegistry,
                playerManager
            );
        }

        private T Resolve<T>() where T : class
        {
            var objects = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in objects)
            {
                if (obj is T target)
                    return target;
            }
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
