using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.Utility;
using Game.CombatSystem.Context;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly IPlayerHandManager playerHandManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ISlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TurnContext turnContext;

        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            IPlayerHandManager playerHandManager,
            IEnemyHandManager enemyHandManager,
            ISlotRegistry slotRegistry,
            ICoroutineRunner coroutineRunner,
            TurnContext turnContext)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.playerHandManager = playerHandManager;
            this.enemyHandManager = enemyHandManager;
            this.slotRegistry = slotRegistry;
            this.coroutineRunner = coroutineRunner;
            this.turnContext = turnContext;
        }

        public void EnterState()
        {
            Debug.Log("<color=cyan>[CombatPrepareState] 상태 진입</color>");
            turnContext.Reset();
            coroutineRunner.RunCoroutine(PrepareRoutine());
        }

        private IEnumerator PrepareRoutine()
        {
            // 1. 슬롯 초기화 완료 대기
            yield return new WaitUntil(() => slotRegistry.IsInitialized);

            // 2. 적이 없으면 생성
            if (!flowCoordinator.HasEnemy())
            {
                Debug.Log("[CombatPrepareState] 적이 없어 새 적 생성 시작");
                flowCoordinator.SpawnNextEnemy();
                yield return new WaitUntil(() => flowCoordinator.GetEnemy() != null);
            }

            // 3. 적 핸드 초기화
            var enemy = flowCoordinator.GetEnemy();
            if (!enemyHandManager.HasInitializedEnemy(enemy))
            {
                enemyHandManager.Initialize(enemy);
                yield return new WaitForEndOfFrame();
            }

            // 4. 적 핸드 슬롯 이동 및 생성
            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);

            // 5. 적 카드 준비 완료까지 대기
            yield return WaitForEnemyCardReady();

            // 6. 적 전투 슬롯에 카드 등록
            enemyHandManager.PopCardAndRegisterToCombatSlot(flowCoordinator);

            // [ Delay 추가]
            yield return new WaitForSeconds(0.25f); // UI가 처리될 시간을 줌

            // 7. 다시 뒷칸부터 채우기
            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);


            // 8. 다음 상태 전이
            var next = turnManager.GetStateFactory().CreatePlayerInputState();
            turnManager.RequestStateChange(next);
        }
        private IEnumerator WaitForEnemyCardReady()
        {
            Debug.Log("[CombatPrepareState] 적 카드 등록 준비 대기 중...");
            while (true)
            {
                var (card, ui) = enemyHandManager.PeekCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
                if (card != null && ui != null)
                    break;

                yield return null;
            }
            Debug.Log("[CombatPrepareState] 적 카드 등록 준비 완료");
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatPrepareState] 상태 종료</color>");
        }
    }
}
