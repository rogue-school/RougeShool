using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Core;
using Game.CombatSystem.Manager;

namespace Game.Utility
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
                !TryCast(out ISlotRegistry slotRegistry, slotRegistrySource))
            {
                Debug.LogError("[CombatStateFactoryInstaller] 필수 매니저 캐스팅 실패");
                enabled = false;
                return;
            }

            if (!(combatTurnManager is ITurnStateController turnController) ||
                !(combatTurnManager is ICardExecutionContext executionContext))
            {
                Debug.LogError("[CombatStateFactoryInstaller] CombatTurnManager는 필수 인터페이스를 구현해야 합니다.");
                enabled = false;
                return;
            }

            factory = new CombatStateFactory(
                combatTurnManager,
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
