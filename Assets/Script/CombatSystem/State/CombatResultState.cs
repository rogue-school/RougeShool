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

            // 플레이어, 적 턴 효과 처리
            context.GetPlayer()?.ProcessTurnEffects();
            context.GetEnemy()?.ProcessTurnEffects();

            // 플레이어 카드 쿨타임 처리
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

            // 적 핸드 클리어
            enemyHandManager.ClearAllSlots();
            enemyHandManager.ClearAllUI();

            // 적 사망 시 다음 적 스폰
            var enemy = context.GetEnemy();
            if (enemy == null || enemy.IsDead())
            {
                stageManager.SpawnNextEnemy();
            }

            // 다음 상태로 전이
            var nextState = stateFactory.CreatePrepareState();
            controller.RequestStateChange(nextState);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatResultState] 결과 정리 종료");
        }
    }
}
