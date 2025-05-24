using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Core;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Context;

namespace Game.CombatSystem.Utility
{
    public class CombatStateFactoryInstaller : MonoBehaviour, ICombatStateFactory
    {
        private ICombatStateFactory factory;

        [Header("필요한 매니저")]
        [SerializeField] private MonoBehaviour playerHandManagerSource;
        [SerializeField] private MonoBehaviour enemyHandManagerSource;
        [SerializeField] private MonoBehaviour spawnerManagerSource;
        [SerializeField] private MonoBehaviour combatSlotManagerSource;
        [SerializeField] private MonoBehaviour stageManagerSource;
        [SerializeField] private MonoBehaviour victoryManagerSource;
        [SerializeField] private MonoBehaviour gameOverManagerSource;
        [SerializeField] private MonoBehaviour flowCoordinatorSource;
        [SerializeField] private MonoBehaviour slotRegistrySource;

        [SerializeField] private CombatTurnManager combatTurnManager;
        [SerializeField] private MonoBehaviour turnStateControllerSource;

        private void Awake() => Initialize();

        public void Initialize()
        {
            if (!TryCast(out IPlayerHandManager playerHandManager, playerHandManagerSource) ||
                !TryCast(out IEnemyHandManager enemyHandManager, enemyHandManagerSource) ||
                !TryCast(out IEnemySpawnerManager spawnerManager, spawnerManagerSource) ||
                !TryCast(out ICombatSlotManager combatSlotManager, combatSlotManagerSource) ||
                !TryCast(out IStageManager stageManager, stageManagerSource) ||
                !TryCast(out IVictoryManager victoryManager, victoryManagerSource) ||
                !TryCast(out IGameOverManager gameOverManager, gameOverManagerSource) ||
                !TryCast(out ICombatFlowCoordinator flowCoordinator, flowCoordinatorSource) ||
                !TryCast(out ISlotRegistry slotRegistry, slotRegistrySource) ||
                !TryCast(out ITurnStateController turnController, turnStateControllerSource))
            {
                Debug.LogError("[CombatStateFactoryInstaller] 필수 매니저 캐스팅 실패");
                enabled = false;
                return;
            }

            // DefaultCardExecutionContext는 MonoBehaviour가 아니므로 직접 생성
            var executionContext = new DefaultCardExecutionContext(null, null, null);

            factory = new CombatStateFactory(
                combatTurnManager,
                turnController,
                executionContext,
                flowCoordinator,
                playerHandManager,
                enemyHandManager,
                spawnerManager,
                combatSlotManager,
                stageManager,
                victoryManager,
                gameOverManager,
                slotRegistry
            );

            combatTurnManager.InjectFactory(this);
            Debug.Log("[CombatStateFactoryInstaller] 팩토리 생성 및 주입 완료");
        }

        private bool TryCast<T>(out T result, MonoBehaviour source) where T : class
        {
            result = source as T;
            if (result == null)
            {
                Debug.LogError($"[CombatStateFactoryInstaller] {typeof(T).Name} 캐스팅 실패: {source?.name ?? "null"}");
                return false;
            }
            return true;
        }

        public ICombatTurnState CreatePrepareState() => factory.CreatePrepareState();
        public ICombatTurnState CreatePlayerInputState() => factory.CreatePlayerInputState();
        public ICombatTurnState CreateFirstAttackState() => factory.CreateFirstAttackState();
        public ICombatTurnState CreateSecondAttackState() => factory.CreateSecondAttackState();
        public ICombatTurnState CreateResultState() => factory.CreateResultState();
        public ICombatTurnState CreateVictoryState() => factory.CreateVictoryState();
        public ICombatTurnState CreateGameOverState() => factory.CreateGameOverState();
    }
}
