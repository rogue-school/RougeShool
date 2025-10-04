using UnityEngine;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Manager;
using Game.SkillCardSystem.Manager;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 상태 간 공유되는 컨텍스트 정보
    /// 상태 머신이 동작하는데 필요한 모든 매니저와 데이터에 대한 접근을 제공합니다.
    /// </summary>
    public class CombatStateContext
    {
        // 핵심 매니저들
        public CombatExecutionManager ExecutionManager { get; set; }
        public TurnManager TurnManager { get; set; }
        public PlayerManager PlayerManager { get; set; }
        public EnemyManager EnemyManager { get; set; }
        public PlayerHandManager HandManager { get; set; }

        // 상태 머신 참조
        public CombatStateMachine StateMachine { get; set; }

        // 현재 실행 중인 카드 (Execution 상태에서 사용)
        public Game.SkillCardSystem.Interface.ISkillCard CurrentExecutingCard { get; set; }
        public Game.CombatSystem.Slot.CombatSlotPosition? CurrentExecutingSlot { get; set; }

        // 플래그들
        public bool IsInitialized { get; set; }
        public bool IsPaused { get; set; }

        /// <summary>
        /// 컨텍스트 초기화
        /// </summary>
        public void Initialize(
            CombatStateMachine stateMachine,
            CombatExecutionManager executionManager,
            TurnManager turnManager,
            PlayerManager playerManager,
            EnemyManager enemyManager,
            PlayerHandManager handManager)
        {
            StateMachine = stateMachine;
            ExecutionManager = executionManager;
            TurnManager = turnManager;
            PlayerManager = playerManager;
            EnemyManager = enemyManager;
            HandManager = handManager;
            IsInitialized = true;
            IsPaused = false;

            Game.CoreSystem.Utility.GameLogger.LogInfo(
                "[CombatStateContext] 컨텍스트 초기화 완료",
                Game.CoreSystem.Utility.GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 모든 필수 매니저가 유효한지 검증
        /// </summary>
        public bool ValidateManagers()
        {
            if (ExecutionManager == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogError(
                    "[CombatStateContext] ExecutionManager가 null입니다",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.Error);
                return false;
            }

            if (TurnManager == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogError(
                    "[CombatStateContext] TurnManager가 null입니다",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.Error);
                return false;
            }

            if (PlayerManager == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogError(
                    "[CombatStateContext] PlayerManager가 null입니다",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.Error);
                return false;
            }

            if (EnemyManager == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogError(
                    "[CombatStateContext] EnemyManager가 null입니다",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 컨텍스트 리셋
        /// </summary>
        public void Reset()
        {
            CurrentExecutingCard = null;
            CurrentExecutingSlot = null;
            IsPaused = false;

            Game.CoreSystem.Utility.GameLogger.LogInfo(
                "[CombatStateContext] 컨텍스트 리셋 완료",
                Game.CoreSystem.Utility.GameLogger.LogCategory.Combat);
        }
    }
}
