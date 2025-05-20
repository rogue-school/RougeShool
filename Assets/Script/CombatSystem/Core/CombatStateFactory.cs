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
        private readonly ISlotRegistry slotRegistry;

        public CombatStateFactory(
            ICombatTurnManager combatTurnManager,
            ICombatFlowCoordinator flowCoordinator,
            IPlayerHandManager playerHandManager,
            IEnemyHandManager enemyHandManager,
            IEnemySpawnerManager enemySpawnerManager,
            ICombatSlotManager combatSlotManager,
            IStageManager stageManager,
            IVictoryManager victoryManager,
            IGameOverManager gameOverManager,
            ISlotRegistry slotRegistry)
        {
            this.combatTurnManager = combatTurnManager ?? throw new ArgumentNullException(nameof(combatTurnManager));
            this.turnStateController = combatTurnManager as ITurnStateController ?? throw new ArgumentException("combatTurnManager는 ITurnStateController를 구현해야 합니다.");
            this.cardExecutionContext = combatTurnManager as ICardExecutionContext ?? throw new ArgumentException("combatTurnManager는 ICardExecutionContext를 구현해야 합니다.");

            this.flowCoordinator = flowCoordinator ?? throw new ArgumentNullException(nameof(flowCoordinator));
            this.playerHandManager = playerHandManager ?? throw new ArgumentNullException(nameof(playerHandManager));
            this.enemyHandManager = enemyHandManager ?? throw new ArgumentNullException(nameof(enemyHandManager));
            this.enemySpawnerManager = enemySpawnerManager ?? throw new ArgumentNullException(nameof(enemySpawnerManager));
            this.combatSlotManager = combatSlotManager ?? throw new ArgumentNullException(nameof(combatSlotManager));
            this.stageManager = stageManager ?? throw new ArgumentNullException(nameof(stageManager));
            this.victoryManager = victoryManager ?? throw new ArgumentNullException(nameof(victoryManager));
            this.gameOverManager = gameOverManager ?? throw new ArgumentNullException(nameof(gameOverManager));
        }

        public ICombatTurnState CreatePrepareState()
        {
            return new CombatPrepareState(combatTurnManager, flowCoordinator, this, slotRegistry);
        }

        public ICombatTurnState CreatePlayerInputState()
        {
            return new CombatPlayerInputState(combatTurnManager, flowCoordinator, this, slotRegistry);
        }

        public ICombatTurnState CreateFirstAttackState()
        {
            return new CombatFirstAttackState(combatTurnManager, flowCoordinator, this, slotRegistry);
        }

        public ICombatTurnState CreateSecondAttackState()
        {
            return new CombatSecondAttackState(combatTurnManager, flowCoordinator, this, slotRegistry);
        }

        public ICombatTurnState CreateResultState()
        {
            return new CombatResultState(combatTurnManager, flowCoordinator, this, slotRegistry);
        }

        public ICombatTurnState CreateVictoryState()
        {
            return new CombatVictoryState(combatTurnManager, flowCoordinator, this, slotRegistry);
        }

        public ICombatTurnState CreateGameOverState()
        {
            return new CombatGameOverState(combatTurnManager, flowCoordinator, this, slotRegistry);
        }

    }
}
