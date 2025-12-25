using UnityEngine;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Manager;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 상태 간 공유되는 컨텍스트 정보
    /// 상태 머신이 동작하는데 필요한 모든 매니저와 데이터에 대한 접근을 제공합니다.
    /// </summary>
    public class CombatStateContext
    {
        // 핵심 매니저들 (레거시)
        public CombatExecutionManager ExecutionManager { get; set; }
        public TurnManager TurnManager { get; set; }
        public PlayerManager PlayerManager { get; set; }
        public EnemyManager EnemyManager { get; set; }
        public PlayerHandManager HandManager { get; set; }

        // 새로운 분리된 인터페이스들 (리팩토링)
        public ITurnController TurnController { get; set; }
        public ICardSlotRegistry SlotRegistry { get; set; }
        public ISlotMovementController SlotMovement { get; set; }
        
        // 추가 의존성
        public Game.StageSystem.Manager.StageManager StageManager { get; set; }
        public Game.ItemSystem.Service.ItemService ItemService { get; set; }
        public Game.CharacterSystem.UI.EnemyCharacterUIController EnemyCharacterUIController { get; set; }
        public Game.CharacterSystem.Manager.EnemyManager EnemyManagerForStates { get; set; }
        public Game.CombatSystem.UI.GameOverUI GameOverUI { get; set; }

        // 상태 머신 참조
        public CombatStateMachine StateMachine { get; set; }

        // 현재 실행 중인 카드 (Execution 상태에서 사용)
        public Game.SkillCardSystem.Interface.ISkillCard CurrentExecutingCard { get; set; }
        public Game.CombatSystem.Slot.CombatSlotPosition? CurrentExecutingSlot { get; set; }

        // 플래그들
        
        /// <summary>
        /// 컨텍스트가 초기화되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsInitialized { get; set; }
        
        /// <summary>
        /// 전투가 일시정지되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// 컨텍스트 초기화 (레거시 + 새로운 인터페이스)
        /// </summary>
        /// <param name="stateMachine">전투 상태 머신</param>
        /// <param name="executionManager">카드 실행 매니저</param>
        /// <param name="turnManager">턴 매니저</param>
        /// <param name="playerManager">플레이어 매니저</param>
        /// <param name="enemyManager">적 매니저</param>
        /// <param name="handManager">핸드 매니저</param>
        /// <param name="turnController">턴 컨트롤러</param>
        /// <param name="slotRegistry">카드 슬롯 레지스트리</param>
        /// <param name="slotMovement">슬롯 이동 컨트롤러</param>
        /// <param name="stageManager">스테이지 매니저 (옵션)</param>
        /// <param name="itemService">아이템 서비스 (옵션)</param>
        /// <param name="enemyCharacterUIController">적 캐릭터 UI 컨트롤러 (옵션)</param>
        /// <param name="enemyManagerForStates">상태용 적 매니저 (옵션)</param>
        /// <param name="gameOverUI">게임 오버 UI (옵션)</param>
        public void Initialize(
            CombatStateMachine stateMachine,
            CombatExecutionManager executionManager,
            TurnManager turnManager,
            PlayerManager playerManager,
            EnemyManager enemyManager,
            PlayerHandManager handManager,
            ITurnController turnController,
            ICardSlotRegistry slotRegistry,
            ISlotMovementController slotMovement,
            Game.StageSystem.Manager.StageManager stageManager = null,
                Game.ItemSystem.Service.ItemService itemService = null,
                Game.CharacterSystem.UI.EnemyCharacterUIController enemyCharacterUIController = null,
                Game.CharacterSystem.Manager.EnemyManager enemyManagerForStates = null,
                Game.CombatSystem.UI.GameOverUI gameOverUI = null)
        {
            StateMachine = stateMachine;

            // 레거시 매니저
            ExecutionManager = executionManager;
            TurnManager = turnManager;
            PlayerManager = playerManager;
            EnemyManager = enemyManager;
            HandManager = handManager;

            // 새로운 분리된 인터페이스들
            TurnController = turnController;
            SlotRegistry = slotRegistry;
            SlotMovement = slotMovement;
            
            // 추가 의존성
            StageManager = stageManager;
            ItemService = itemService;
            EnemyCharacterUIController = enemyCharacterUIController;
            EnemyManagerForStates = enemyManagerForStates;
            GameOverUI = gameOverUI;

            IsInitialized = true;
            IsPaused = false;

            Game.CoreSystem.Utility.GameLogger.LogInfo(
                "[CombatStateContext] 컨텍스트 초기화 완료 (레거시 + 리팩토링)",
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
