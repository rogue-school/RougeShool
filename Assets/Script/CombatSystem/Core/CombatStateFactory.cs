using Game.CombatSystem.Interface;
using Game.CombatSystem.State;
using Game.IManager;
using Game.CombatSystem.Slot;

public class CombatStateFactory : ICombatStateFactory
{
    private readonly ICombatTurnManager combatTurnManager;
    private readonly ITurnStateController turnStateController;
    private readonly ICardExecutionContext cardExecutionContext;
    private readonly ICombatFlowCoordinator flowCoordinator;
    private readonly IPlayerHandManager playerHandManager;
    private readonly IEnemyHandManager enemyHandManager;
    private readonly IEnemySpawnerManager spawnerManager;
    private readonly ICombatSlotManager combatSlotManager;
    private readonly IStageManager stageManager;
    private readonly IVictoryManager victoryManager;
    private readonly IGameOverManager gameOverManager;
    private readonly ICombatSlotRegistry combatSlotRegistry;

    public CombatStateFactory(
        ICombatTurnManager combatTurnManager,
        ITurnStateController turnStateController,
        ICardExecutionContext cardExecutionContext,
        ICombatFlowCoordinator flowCoordinator,
        IPlayerHandManager playerHandManager,
        IEnemyHandManager enemyHandManager,
        IEnemySpawnerManager spawnerManager,
        ICombatSlotManager combatSlotManager,
        IStageManager stageManager,
        IVictoryManager victoryManager,
        IGameOverManager gameOverManager,
        ISlotRegistry slotRegistry)
    {
        this.combatTurnManager = combatTurnManager;
        this.turnStateController = turnStateController;
        this.cardExecutionContext = cardExecutionContext;
        this.flowCoordinator = flowCoordinator;
        this.playerHandManager = playerHandManager;
        this.enemyHandManager = enemyHandManager;
        this.spawnerManager = spawnerManager;
        this.combatSlotManager = combatSlotManager;
        this.stageManager = stageManager;
        this.victoryManager = victoryManager;
        this.gameOverManager = gameOverManager;
        this.combatSlotRegistry = slotRegistry.GetCombatSlotRegistry();
    }

    public ICombatTurnState CreatePrepareState() => new CombatPrepareState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
    public ICombatTurnState CreatePlayerInputState() => new CombatPlayerInputState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
    public ICombatTurnState CreateFirstAttackState() => new CombatFirstAttackState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
    public ICombatTurnState CreateSecondAttackState() => new CombatSecondAttackState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
    public ICombatTurnState CreateResultState() => new CombatResultState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
    public ICombatTurnState CreateVictoryState() => new CombatVictoryState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
    public ICombatTurnState CreateGameOverState() => new CombatGameOverState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
}
