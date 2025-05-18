using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ITurnStateController controller;
        private readonly ICardExecutionContext context;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly IStageManager stageManager;
        private readonly IPlayerHandManager playerHandManager;
        private readonly ICombatStateFactory stateFactory;

        public CombatResultState(
            ITurnStateController controller,
            ICardExecutionContext context,
            IEnemyHandManager enemyHandManager,
            IStageManager stageManager,
            IPlayerHandManager playerHandManager,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.context = context;
            this.enemyHandManager = enemyHandManager;
            this.stageManager = stageManager;
            this.playerHandManager = playerHandManager;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[CombatResultState] 결과 정리 시작");

            context.GetPlayer()?.ProcessTurnEffects();
            context.GetEnemy()?.ProcessTurnEffects();

            foreach (var slot in playerHandManager.GetAllHandSlots())
            {
                var card = slot.GetCard();
                if (card is PlayerSkillCardRuntime runtime)
                {
                    runtime.ActivateCoolTime();
                    runtime.TickCoolTime();
                    playerHandManager.RestoreCardToHand(runtime);
                    slot.Clear();
                }
            }

            enemyHandManager.ClearAllSlots();
            enemyHandManager.ClearAllUI();

            var enemy = context.GetEnemy();
            if (enemy == null || enemy.IsDead())
            {
                stageManager.SpawnNextEnemy();
            }

            controller.RequestStateChange(stateFactory.CreatePrepareState());
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatResultState] 결과 정리 종료");
        }
    }
}
