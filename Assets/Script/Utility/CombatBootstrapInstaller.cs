using UnityEngine;
using Game.CombatSystem.Core;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Stage;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.Utility
{
    public class CombatBootstrapInstaller : MonoBehaviour
    {
        [Header("전투 흐름 관리자")]
        [SerializeField] private CombatFlowCoordinator combatFlowCoordinator;

        [Header("SkillCard UI 프리팹")]
        [SerializeField] private SkillCardUI skillCardUIPrefab;

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

            playerInitializer?.Inject(playerManager);
            if (playerHandManager != null)
                playerManager.SetPlayerHandManager(playerHandManager);

            if (stageManager is StageManager concreteStage &&
                spawner is EnemySpawnerManager concreteSpawner &&
                enemyManager != null && enemyHandManager != null)
            {
                concreteStage.Inject(concreteSpawner, enemyManager, enemyHandManager);
                concreteSpawner.InjectStageManager(concreteStage);
            }

            // 프리팹 주입 포함
            combatFlowCoordinator.Inject(
                slotRegistry,
                enemyHandManager,
                playerHandManager,
                spawner,
                playerManager,
                stageManager,
                enemyManager,
                turnManager,
                stateFactory
            );

            combatFlowCoordinator.InjectUI(skillCardUIPrefab);
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
        public void Initialize()
        {
            InjectDependencies();
        }
    }
}

