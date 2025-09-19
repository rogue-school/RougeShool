using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Context;
using Game.CombatSystem;
using Game.CombatSystem.Manager;

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
        private readonly ISlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TurnContext turnContext;
        private readonly CombatStartupManager startupManager;

        #endregion

        #region 생성자

        /// <summary>
        /// 전투 준비 상태 생성자
        /// </summary>
        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            IPlayerHandManager playerHandManager,
            ISlotRegistry slotRegistry,
            ICoroutineRunner coroutineRunner,
            TurnContext turnContext,
            CombatStartupManager startupManager)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.playerHandManager = playerHandManager;
            this.slotRegistry = slotRegistry;
            this.coroutineRunner = coroutineRunner;
            this.turnContext = turnContext;
            this.startupManager = startupManager;
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
        /// 전투 준비 루틴. 새로운 CombatStartupManager를 사용하여 전투 시작 시퀀스를 실행합니다.
        /// </summary>
        private IEnumerator PrepareRoutine()
        {
            Debug.Log("[CombatPrepareState] PrepareRoutine 시작");
            
            // CombatStartupManager null 체크
            if (startupManager == null)
            {
                Debug.LogError("[CombatPrepareState] CombatStartupManager가 null입니다. DI 바인딩을 확인해주세요.");
                yield break;
            }

            Debug.Log("[CombatPrepareState] CombatStartupManager 확인 완료, 전투 시작 시퀀스 실행");

            // 전투 시작 시퀀스 실행
            bool startupComplete = false;
            startupManager.StartCombatSequence((success) => {
                startupComplete = true;
                if (success)
                {
                    Debug.Log("[CombatPrepareState] 전투 시작 시퀀스 완료");
                }
                else
                {
                    Debug.LogError("[CombatPrepareState] 전투 시작 시퀀스 실패");
                }
            });

            yield return new WaitUntil(() => startupComplete);

            // 다음 상태로 전환 (플레이어 입력)
            Debug.Log("<color=cyan>[STATE] CombatPrepareState → CombatPlayerInputState 전이</color>");
            var next = turnManager.GetStateFactory().CreatePlayerInputState();
            Debug.Log("[CombatPrepareState] 상태 전이: PlayerInputState");
            turnManager.RequestStateChange(next);
        }


        /// <summary>
        /// 적 카드 슬롯이 준비될 때까지 대기합니다.
        /// </summary>
        private IEnumerator WaitForEnemyCardReady() { yield break; }

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
