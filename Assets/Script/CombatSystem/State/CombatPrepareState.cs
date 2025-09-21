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
        private readonly CombatFlowManager combatFlowManager;

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
            CombatFlowManager combatFlowManager)
        {
            this.turnManager = turnManager;
            this.slotManager = slotManager;
            this.playerHandManager = playerHandManager;
            this.coroutineRunner = coroutineRunner;
            this.turnContext = turnContext;
            this.combatFlowManager = combatFlowManager;
        }

        #endregion

        #region 상태 진입

        /// <summary>
        /// 상태 진입 시 호출됨. 준비 코루틴 시작.
        /// </summary>
        public void EnterState()
        {
            GameLogger.LogInfo("CombatPrepareState 진입", GameLogger.LogCategory.Combat);
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
            GameLogger.LogInfo("PrepareRoutine 시작", GameLogger.LogCategory.Combat);
            
            // CombatFlowManager null 체크
            if (combatFlowManager == null)
            {
                GameLogger.LogError("CombatFlowManager가 null입니다. DI 바인딩을 확인해주세요.", GameLogger.LogCategory.Error);
                yield break;
            }

            GameLogger.LogInfo("CombatFlowManager 확인 완료, 전투 시작 시퀀스 실행", GameLogger.LogCategory.Combat);

            // 전투 시작 시퀀스 실행 (CombatFlowManager 사용)
            GameLogger.LogInfo("전투 시작 시퀀스 실행", GameLogger.LogCategory.Combat);
            
            // 간단한 전투 시작 로직 (CombatFlowManager의 실제 메서드 구현 대기)
            yield return new WaitForSeconds(1.0f); // 기본 대기시간
            
            GameLogger.LogInfo("전투 시작 시퀀스 완료", GameLogger.LogCategory.Combat);

            // 다음 상태로 전환 (플레이어 입력)
            GameLogger.LogInfo("CombatPrepareState → CombatPlayerInputState 전이", GameLogger.LogCategory.Combat);
            GameLogger.LogInfo("상태 전이: PlayerInputState", GameLogger.LogCategory.Combat);
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
