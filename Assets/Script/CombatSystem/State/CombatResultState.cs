using UnityEngine;
using System.Collections;
using Game.CombatSystem.Manager;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.State
{
    public class CombatResultState
    {
        private readonly TurnManager turnManager;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatResultState(
            TurnManager turnManager,
            ICoroutineRunner coroutineRunner)
        {
            this.turnManager = turnManager;
            this.coroutineRunner = coroutineRunner;
        }

        public void EnterState()
        {
            GameLogger.LogInfo("[STATE] Result 진입 - 실행 결과 정리는 ExecutionManager에서 처리됨", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 상태 실행 중 특별한 작업 없음
        /// </summary>
        public void ExecuteState() { }

        public void ExitState()
        {
            GameLogger.LogInfo("[STATE] Result 종료", GameLogger.LogCategory.Combat);
        }
    }
}
