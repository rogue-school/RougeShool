using UnityEngine;
using Game.IManager;
using Game.CombatSystem.Core;
using Game.CombatSystem.Executor;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Service;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Executor;
using Game.SkillCardSystem.Core;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Context;
using Game.Manager;

public class CombatBootstrapInstaller : MonoBehaviour
{
    [Header("컴포넌트 할당")]
    [SerializeField] private CombatFlowCoordinator flowCoordinator;
    [SerializeField] private CombatTurnManager combatTurnManager;
    [SerializeField] private EnemySpawnerManager enemySpawnerManager;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private EnemyHandManager enemyHandManager;
    [SerializeField] private PlayerHandManager playerHandManager;
    [SerializeField] private SlotRegistry slotRegistry;
    [SerializeField] private SlotInitializer slotInitializer; // 주입 대상

    private void Awake()
    {
        // 1. 슬롯 레지스트리 초기화 및 주입
        slotRegistry.Initialize();
        slotInitializer.Inject(slotRegistry);
        slotInitializer.AutoBindAllSlots();

        // 2. 매니저 간 의존성 주입
        stageManager.InjectEnemySpawner(enemySpawnerManager);
        enemySpawnerManager.InjectStageManager(stageManager);
        enemySpawnerManager.InjectEnemyManager(enemyManager);
        enemyManager.SetEnemyHandManager(enemyHandManager);
        playerManager.InjectHandManager(playerHandManager);
        flowCoordinator.InjectCombatTurnManager(combatTurnManager);
        combatTurnManager.SetFlowCoordinator(flowCoordinator);

        // 3. 서비스 구성
        var contextProvider = new DefaultCardExecutionContextProvider(playerManager, enemyManager);
        var cardExecutor = new CardExecutor();
        var placementService = new CardPlacementService();
        var turnCardRegistry = new TurnCardRegistry();

        var executorService = new CombatExecutorService(
            cardExecutor, contextProvider, turnCardRegistry, combatTurnManager);

        var preparationService = new CombatPreparationService(
            playerManager, enemySpawnerManager, enemyManager,
            enemyHandManager, slotRegistry, turnCardRegistry,
            placementService, combatTurnManager
        );

        var playerInputController = new PlayerInputController(
            flowCoordinator, turnCardRegistry, combatTurnManager);

        // 4. FlowCoordinator에 서비스 주입
        flowCoordinator.ConstructServices(
            preparationService,
            playerInputController,
            executorService,
            turnCardRegistry
        );
    }
}
