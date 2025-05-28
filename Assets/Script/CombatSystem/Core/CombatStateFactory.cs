using Game.CombatSystem.Interface;
using Game.CombatSystem.State;

public class CombatStateFactory : ICombatStateFactory
{
    private readonly ICombatTurnManager combatTurnManager;
    private readonly ICombatFlowCoordinator flowCoordinator;
    private readonly ITurnStateController turnStateController;
    private readonly ICardExecutionContext cardExecutionContext;

    private readonly IHandSlotRegistry handSlotRegistry;
    private readonly ICombatSlotRegistry combatSlotRegistry;
    private readonly ICharacterSlotRegistry characterSlotRegistry;

    public CombatStateFactory(
        ICombatTurnManager combatTurnManager,
        ITurnStateController turnStateController,
        ICardExecutionContext cardExecutionContext,
        ICombatFlowCoordinator flowCoordinator,
        IHandSlotRegistry handSlotRegistry,
        ICombatSlotRegistry combatSlotRegistry,
        ICharacterSlotRegistry characterSlotRegistry)
    {
        this.combatTurnManager = combatTurnManager;
        this.turnStateController = turnStateController;
        this.cardExecutionContext = cardExecutionContext;
        this.flowCoordinator = flowCoordinator;
        this.handSlotRegistry = handSlotRegistry;
        this.combatSlotRegistry = combatSlotRegistry;
        this.characterSlotRegistry = characterSlotRegistry;
    }

    public ICombatTurnState CreatePrepareState() =>
        new CombatPrepareState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

    public ICombatTurnState CreatePlayerInputState() =>
        new CombatPlayerInputState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

    public ICombatTurnState CreateFirstAttackState() =>
        new CombatFirstAttackState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

    public ICombatTurnState CreateSecondAttackState() =>
        new CombatSecondAttackState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

    public ICombatTurnState CreateResultState() =>
        new CombatResultState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

    public ICombatTurnState CreateVictoryState() =>
        new CombatVictoryState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

    public ICombatTurnState CreateGameOverState() =>
        new CombatGameOverState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);
}
