using Game.CombatSystem.Interface;
using Game.CombatSystem.State;
using Game.CombatSystem.Manager;
using Game.IManager;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 전투 상태(FSM)를 생성하는 팩토리 클래스입니다.
    /// 각 상태는 필요한 의존성을 생성자가 아닌 외부에서 주입받아 생성됩니다.
    /// </summary>
    public class CombatStateFactory : ICombatStateFactory
    {
        private readonly ICombatTurnManager combatTurnManager;
        private readonly ITurnStateController turnStateController;
        private readonly IPlayerHandManager playerHandManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly IEnemySpawnerManager enemySpawnerManager;
        private readonly ICombatSlotManager combatSlotManager;
        private readonly ICardExecutionContext cardExecutionContext;
        private readonly IStageManager stageManager;
        private readonly IVictoryManager victoryManager;
        private readonly IGameOverManager gameOverManager;

        public CombatStateFactory(
            CombatTurnManager combatTurnManager,
            IPlayerHandManager playerHandManager,
            IEnemyHandManager enemyHandManager,
            IEnemySpawnerManager enemySpawnerManager,
            ICombatSlotManager combatSlotManager,
            IStageManager stageManager,
            IVictoryManager victoryManager,
            IGameOverManager gameOverManager)
        {
            this.combatTurnManager = combatTurnManager;
            this.turnStateController = combatTurnManager;
            this.cardExecutionContext = combatTurnManager;
            this.playerHandManager = playerHandManager;
            this.enemyHandManager = enemyHandManager;
            this.enemySpawnerManager = enemySpawnerManager;
            this.combatSlotManager = combatSlotManager;
            this.stageManager = stageManager;
            this.victoryManager = victoryManager;
            this.gameOverManager = gameOverManager;
        }

        public ICombatTurnState CreatePrepareState()
        {
            return new CombatPrepareState(
                controller: combatTurnManager,
                spawnerManager: enemySpawnerManager,
                enemyHandManager: enemyHandManager,
                stateFactory: this
            );
        }

        public ICombatTurnState CreatePlayerInputState()
        {
            return new CombatPlayerInputState(
                controller: turnStateController,
                playerHandManager: playerHandManager,
                stateFactory: this
            );
        }

        public ICombatTurnState CreateFirstAttackState()
        {
            return new CombatFirstAttackState(
                controller: turnStateController,
                slotManager: combatSlotManager,
                executionContext: cardExecutionContext,
                stateFactory: this
            );
        }

        public ICombatTurnState CreateSecondAttackState()
        {
            return new CombatSecondAttackState(
                controller: turnStateController,
                slotManager: combatSlotManager,
                context: cardExecutionContext,
                stateFactory: this
            );
        }

        public ICombatTurnState CreateResultState()
        {
            return new CombatResultState(
                controller: turnStateController,
                context: cardExecutionContext,
                enemyHandManager: enemyHandManager,
                stageManager: stageManager,
                playerHandManager: playerHandManager,
                stateFactory: this
            );
        }

        public ICombatTurnState CreateVictoryState()
        {
            return new CombatVictoryState(victoryManager);
        }

        public ICombatTurnState CreateGameOverState()
        {
            return new CombatGameOverState(gameOverManager);
        }
    }
}
