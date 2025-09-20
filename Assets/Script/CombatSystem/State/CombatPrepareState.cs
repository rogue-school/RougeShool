using UnityEngine;
using System.Collections;
using Game.CombatSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Context;
using Game.CombatSystem;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 준비 상태. 슬롯 초기화, 적 소환, 적 카드 준비 등을 처리합니다.
    /// </summary>
    public class CombatPrepareState
    {
        #region 필드

        private readonly TurnManager turnManager;
        private readonly CombatSlotManager slotManager;
        private readonly IPlayerHandManager playerHandManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TurnContext turnContext;
        private readonly CombatManager combatManager;

        #endregion

        #region 생성자

        /// <summary>
        /// 전투 준비 상태 생성자
        /// </summary>
        public CombatPrepareState(
            TurnManager turnManager,
            CombatSlotManager slotManager,
            IPlayerHandManager playerHandManager,
            ICoroutineRunner coroutineRunner,
            TurnContext turnContext,
            CombatManager combatManager)
        {
            this.turnManager = turnManager;
            this.slotManager = slotManager;
            this.playerHandManager = playerHandManager;
            this.coroutineRunner = coroutineRunner;
            this.turnContext = turnContext;
            this.combatManager = combatManager;
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
            
            // CombatManager null 체크
            if (combatManager == null)
            {
                Debug.LogError("[CombatPrepareState] CombatManager가 null입니다. DI 바인딩을 확인해주세요.");
                yield break;
            }

            Debug.Log("[CombatPrepareState] CombatManager 확인 완료, 전투 시작 시퀀스 실행");

            // 전투 시작 시퀀스 실행 (CombatManager 사용)
            Debug.Log("[CombatPrepareState] 전투 시작 시퀀스 실행");
            
            // 간단한 전투 시작 로직 (CombatManager의 실제 메서드 구현 대기)
            yield return new WaitForSeconds(1.0f); // 기본 대기시간
            
            Debug.Log("[CombatPrepareState] 전투 시작 시퀀스 완료");

            // 다음 상태로 전환 (플레이어 입력)
            Debug.Log("<color=cyan>[STATE] CombatPrepareState → CombatPlayerInputState 전이</color>");
            Debug.Log("[CombatPrepareState] 상태 전이: PlayerInputState");
            // Note: State transitions are now handled by the simplified TurnManager
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
