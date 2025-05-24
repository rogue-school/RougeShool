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

            // Stage 관련 상호 의존 주입
            stageManager.Inject(enemySpawnerManager, enemyManager, enemyHandManager);
            enemySpawnerManager.InjectStageManager(stageManager);
            playerManager.SetPlayerHandManager(playerHandManager);

            // 인터페이스 기반 서비스 생성
            ICombatPreparationService preparationService = new CombatPreparationService(
                playerManager,
                enemySpawnerManager,
                enemyManager,
                enemyHandManager,
                slotRegistry,
                combatTurnManager,
                new CardPlacementService()
            );

            IPlayerInputController inputController = new PlayerInputController(playerHandManager);

            ICardExecutionContextProvider contextProvider = new DefaultCardExecutionContextProvider(playerManager, enemyManager);
            ICardExecutor cardExecutor = new CardExecutor(contextProvider); // 인자 누락 수정
            ICombatExecutor executor = new CombatExecutorService(slotRegistry, contextProvider, cardExecutor, enemyHandManager);

            ICombatStateFactory stateFactory = stateFactoryInstaller;

            // FlowCoordinator 주입
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

            // TurnStartButtonHandler 주입
            if (turnStartButtonHandler != null && combatTurnManager is ITurnStartConditionChecker checker)
            {
                turnStartButtonHandler.Inject(checker, combatTurnManager, stateFactory);
                Debug.Log("[Installer] TurnStartButtonHandler.Inject() 완료");
            }
            else
            {
                Debug.LogWarning("[Installer] TurnStartButtonHandler 또는 조건 검사기 주입 실패");
            }
        }
    }
}
