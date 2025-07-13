using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.Utility;
using Game.CombatSystem.Context;
using Game.CombatSystem;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 준비 상태. 슬롯 초기화, 적 소환, 적 카드 준비 등을 처리합니다.
    /// </summary>
    public class CombatPrepareState : ICombatTurnState
    {
        #region 필드

        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly IPlayerHandManager playerHandManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ISlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TurnContext turnContext;

        #endregion

        #region 생성자

        /// <summary>
        /// 전투 준비 상태 생성자
        /// </summary>
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

        #endregion

        #region 상태 진입

        /// <summary>
        /// 상태 진입 시 호출됨. 준비 코루틴 시작.
        /// </summary>
        public void EnterState()
        {
            Debug.Log("<color=cyan>[STATE] CombatPrepareState 진입</color>");
            CombatEvents.RaiseCombatStarted();
            turnContext.Reset();
            coroutineRunner.RunCoroutine(PrepareRoutine());
        }

        #endregion

        #region 준비 루틴

        /// <summary>
        /// 전투 준비 루틴. 슬롯 확인, 적 생성 및 카드 등록을 처리.
        /// </summary>
        private IEnumerator PrepareRoutine()
        {
            Debug.Log("[CombatPrepareState] PrepareRoutine 시작");
            // 슬롯 초기화 대기
            yield return new WaitUntil(() => slotRegistry.IsInitialized);

            // 선공/후공 무작위 결정 (매 전투마다)
            flowCoordinator.IsEnemyFirst = UnityEngine.Random.value < 0.5f;

            // 적이 없으면 생성
            if (!flowCoordinator.HasEnemy())
            {
                flowCoordinator.SpawnNextEnemy();
                yield return new WaitUntil(() => flowCoordinator.GetEnemy() != null);
            }

            // 적 초기화
            var enemy = flowCoordinator.GetEnemy();
            if (!enemyHandManager.HasInitializedEnemy(enemy))
            {
                enemyHandManager.Initialize(enemy);
                yield return new WaitForEndOfFrame();
            }

            // 적 핸드 슬롯 채우기
            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);

            // ENEMY_SLOT_1에 카드 준비될 때까지 대기
            yield return WaitForEnemyCardReady();

            // 카드 등록 (애니메이션 포함)
            yield return enemyHandManager.PopCardAndRegisterToCombatSlotCoroutine(flowCoordinator);

            // 빈 슬롯 다시 채우기
            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);

            // 다음 상태로 전환 (플레이어 입력)
            Debug.Log("<color=cyan>[STATE] CombatPrepareState → CombatPlayerInputState 전이</color>");
            var next = turnManager.GetStateFactory().CreatePlayerInputState();
            Debug.Log("[CombatPrepareState] 상태 전이: PlayerInputState");
            turnManager.RequestStateChange(next);
        }


        /// <summary>
        /// 적 카드 슬롯이 준비될 때까지 대기합니다.
        /// </summary>
        private IEnumerator WaitForEnemyCardReady()
        {
            while (true)
            {
                var (card, ui) = enemyHandManager.PeekCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
                if (card != null && ui != null)
                    break;

                yield return null;
            }
        }

        #endregion

        #region 상태 실행 및 종료

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=cyan>[STATE] CombatPrepareState 종료</color>");
        }

        #endregion
    }
}
