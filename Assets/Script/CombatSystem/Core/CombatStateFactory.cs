using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.State;
using Game.IManager;
using System;

namespace Game.CombatSystem.Core
{
    public class CombatStateFactory : ICombatStateFactory
    {
        private readonly ICombatTurnManager combatTurnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly IPlayerHandManager playerHandManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly IEnemySpawnerManager enemySpawnerManager;
        private readonly ICombatSlotManager combatSlotManager;
        private readonly IStageManager stageManager;
        private readonly IVictoryManager victoryManager;
        private readonly IGameOverManager gameOverManager;
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
            IPlayerHandManager playerHandManager,
            IEnemyHandManager enemyHandManager,
            IEnemySpawnerManager enemySpawnerManager,
            ICombatSlotManager combatSlotManager,
            IStageManager stageManager,
            IVictoryManager victoryManager,
            IGameOverManager gameOverManager,
            IHandSlotRegistry handSlotRegistry,
            ICombatSlotRegistry combatSlotRegistry,
            ICharacterSlotRegistry characterSlotRegistry
        )
        {
            this.combatTurnManager = combatTurnManager ?? throw new ArgumentNullException(nameof(combatTurnManager));
            this.turnStateController = turnStateController ?? throw new ArgumentNullException(nameof(turnStateController));
            this.cardExecutionContext = cardExecutionContext ?? throw new ArgumentNullException(nameof(cardExecutionContext));
            this.flowCoordinator = flowCoordinator ?? throw new ArgumentNullException(nameof(flowCoordinator));
            this.playerHandManager = playerHandManager ?? throw new ArgumentNullException(nameof(playerHandManager));
            this.enemyHandManager = enemyHandManager ?? throw new ArgumentNullException(nameof(enemyHandManager));
            this.enemySpawnerManager = enemySpawnerManager ?? throw new ArgumentNullException(nameof(enemySpawnerManager));
            this.combatSlotManager = combatSlotManager ?? throw new ArgumentNullException(nameof(combatSlotManager));
            this.stageManager = stageManager ?? throw new ArgumentNullException(nameof(stageManager));
            this.victoryManager = victoryManager ?? throw new ArgumentNullException(nameof(victoryManager));
            this.gameOverManager = gameOverManager ?? throw new ArgumentNullException(nameof(gameOverManager));
            this.handSlotRegistry = handSlotRegistry ?? throw new ArgumentNullException(nameof(handSlotRegistry));
            this.combatSlotRegistry = combatSlotRegistry ?? throw new ArgumentNullException(nameof(combatSlotRegistry));
            this.characterSlotRegistry = characterSlotRegistry ?? throw new ArgumentNullException(nameof(characterSlotRegistry));
        }

        public ICombatTurnState CreatePrepareState() =>
            new CombatPrepareState(combatTurnManager, flowCoordinator, this, characterSlotRegistry);

        public ICombatTurnState CreatePlayerInputState() =>
            new CombatPlayerInputState(combatTurnManager, flowCoordinator, this, handSlotRegistry);

        public ICombatTurnState CreateFirstAttackState() =>
            new CombatFirstAttackState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

        public ICombatTurnState CreateSecondAttackState() =>
            new CombatSecondAttackState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

        public ICombatTurnState CreateResultState() =>
            new CombatResultState(combatTurnManager, flowCoordinator, this, combatSlotRegistry);

        public ICombatTurnState CreateVictoryState() =>
            new CombatVictoryState(combatTurnManager, flowCoordinator, this, characterSlotRegistry);

        public ICombatTurnState CreateGameOverState() =>
            new CombatGameOverState(combatTurnManager, flowCoordinator, this, characterSlotRegistry);
    }
}
