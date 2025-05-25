using UnityEngine;
using Game.CombatSystem.Core;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Context;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Service;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Executor;
using Game.SkillCardSystem.Interface;
using Game.Manager;
using Game.CombatSystem.Executor;
using Game.CombatSystem.DragDrop;

namespace Game.CombatSystem.Utility
{
    public class CombatBootstrapInstaller : MonoBehaviour
    {
        [Header("핵심 오브젝트")]
        [SerializeField] private CombatFlowCoordinator combatFlowCoordinator;
        [SerializeField] private TurnStartButtonHandler turnStartButtonHandler;
        [SerializeField] private SkillCardUI skillCardUIPrefab;

        [Header("의존 대상 (MonoBehaviour 기반 컴포넌트)")]
        [SerializeField] private CombatTurnManager combatTurnManager;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private EnemySpawnerManager enemySpawnerManager;
        [SerializeField] private PlayerHandManager playerHandManager;
        [SerializeField] private EnemyHandManager enemyHandManager;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SlotInitializer slotInitializer;
        [SerializeField] private CombatSlotManager slotManager;

        [Header("설정 대상")]
        [SerializeField] private SlotRegistry slotRegistry;
        [SerializeField] private CombatStateFactoryInstaller stateFactoryInstaller;

        public void Initialize()
        {
            Debug.Log("[Installer] Initialize() 호출됨");

            stageManager.Inject(enemySpawnerManager, enemyManager, enemyHandManager);
            enemySpawnerManager.InjectStageManager(stageManager);
            playerManager.SetPlayerHandManager(playerHandManager);
            playerManager.SetSlotRegistry(slotRegistry);

            ITurnCardRegistry turnCardRegistry = new TurnCardRegistry();
            enemyHandManager.InjectRegistry(turnCardRegistry);

            ICombatPreparationService preparationService = new CombatPreparationService(
                playerManager,
                enemySpawnerManager,
                enemyManager,
                enemyHandManager,
                slotRegistry,
                turnCardRegistry,
                new CardPlacementService(),
                combatTurnManager
            );

            IPlayerInputController inputController = new PlayerInputController(playerHandManager);
            ICardExecutionContextProvider contextProvider = null;
            ICardExecutor cardExecutor = new CardExecutor();

            ICombatExecutor executor = new CombatExecutorService(
                slotRegistry,
                contextProvider,
                cardExecutor,
                enemyHandManager
            );

            ICombatStateFactory stateFactory = stateFactoryInstaller;

            combatFlowCoordinator.Inject(
                slotRegistry,
                enemyHandManager,
                playerHandManager,
                enemySpawnerManager,
                playerManager,
                stageManager,
                enemyManager,
                combatTurnManager,
                stateFactory
            );

            combatFlowCoordinator.InjectUI(skillCardUIPrefab);
            combatFlowCoordinator.InjectTurnStateDependencies(combatTurnManager, stateFactory);
            combatFlowCoordinator.InjectExternalServices(preparationService, inputController, executor);

            if (turnStartButtonHandler != null && combatTurnManager is ITurnStartConditionChecker checker)
            {
                turnStartButtonHandler.Inject(checker, combatTurnManager, stateFactory);
                Debug.Log("[Installer] TurnStartButtonHandler.Inject() 완료");
            }

            var dropHandlers = FindObjectsByType<CardDropToSlotHandler>(FindObjectsSortMode.None);
            foreach (var handler in dropHandlers)
            {
                handler.InjectDependencies(turnCardRegistry, combatFlowCoordinator, combatTurnManager);
                Debug.Log($"[Installer] CardDropToSlotHandler 의존성 주입 완료: {handler.gameObject.name}");
            }
        }
    }
}