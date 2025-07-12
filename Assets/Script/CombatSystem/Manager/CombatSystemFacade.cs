using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
using Game.CombatSystem.State;
using Game.CombatSystem;
using Game.Utility;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// CombatSystem의 모든 기능을 통합하는 파사드 구현체
    /// 복잡한 의존성을 단순화하고 클라이언트가 쉽게 사용할 수 있도록 합니다.
    /// </summary>
    public class CombatSystemFacade : MonoBehaviour, ICombatSystemFacade
    {
        #region 의존성 주입
        [Inject] private ICombatFlowCoordinator flowCoordinator;
        [Inject] private ICombatTurnManager turnManager;
        [Inject] private ICombatStateFactory stateFactory;
        [Inject] private IStageManager stageManager;
        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private IEnemyHandManager enemyHandManager;
        [Inject] private ICombatSlotManager combatSlotManager;
        [Inject] private IVictoryManager victoryManager;
        [Inject] private IGameOverManager gameOverManager;
        [Inject] private ITurnCardRegistry turnCardRegistry;
        [Inject] private ICombatPreparationService preparationService;
        [Inject] private ISlotRegistry slotRegistry;
        [Inject] private ICardExecutor cardExecutor;
        [Inject] private ICoroutineRunner coroutineRunner;
        #endregion

        #region 상태 관리
        private bool isCombatActive = false;
        private ICombatTurnState currentState;
        #endregion

        #region 프로퍼티 구현
        public ICombatFlowCoordinator FlowCoordinator => flowCoordinator;
        public ICombatTurnManager TurnManager => turnManager;
        public ICombatStateFactory StateFactory => stateFactory;
        public IStageManager StageManager => stageManager;
        public IPlayerManager PlayerManager => playerManager;
        public IEnemyManager EnemyManager => enemyManager;
        public IPlayerHandManager PlayerHandManager => playerHandManager;
        public IEnemyHandManager EnemyHandManager => enemyHandManager;
        public ICombatSlotManager CombatSlotManager => combatSlotManager;
        public IVictoryManager VictoryManager => victoryManager;
        public IGameOverManager GameOverManager => gameOverManager;
        public ITurnCardRegistry TurnCardRegistry => turnCardRegistry;
        public ICombatPreparationService PreparationService => preparationService;
        public ISlotRegistry SlotRegistry => slotRegistry;
        public ICardExecutor CardExecutor => cardExecutor;
        public bool IsCombatActive => isCombatActive;
        public ICombatTurnState CurrentState => currentState;
        #endregion

        #region 초기화
        public void Initialize()
        {
            Debug.Log("[CombatSystemFacade] 초기화 시작");
            
            // 상태 초기화
            isCombatActive = false;
            currentState = null;
            
            // 이벤트 구독
            SubscribeToCombatEvents();
            
            Debug.Log("[CombatSystemFacade] 초기화 완료");
        }

        public void Cleanup()
        {
            Debug.Log("[CombatSystemFacade] 정리 시작");
            
            // 이벤트 구독 해제
            UnsubscribeFromCombatEvents();
            
            // 상태 정리
            isCombatActive = false;
            currentState = null;
            
            Debug.Log("[CombatSystemFacade] 정리 완료");
        }
        #endregion

        #region 전투 제어
        public void StartCombat()
        {
            if (isCombatActive)
            {
                Debug.LogWarning("[CombatSystemFacade] 전투가 이미 진행 중입니다.");
                return;
            }

            Debug.Log("[CombatSystemFacade] 전투 시작");
            isCombatActive = true;
            
            // 준비 상태로 시작
            ChangeState<CombatPrepareState>();
        }

        public void EndCombat(bool isVictory)
        {
            if (!isCombatActive)
            {
                Debug.LogWarning("[CombatSystemFacade] 전투가 진행 중이 아닙니다.");
                return;
            }

            Debug.Log($"[CombatSystemFacade] 전투 종료 - 승리: {isVictory}");
            isCombatActive = false;
            
            // 결과 상태로 전환
            if (isVictory)
                ChangeState<CombatVictoryState>();
            else
                ChangeState<CombatGameOverState>();
        }

        public void PauseCombat()
        {
            if (!isCombatActive)
            {
                Debug.LogWarning("[CombatSystemFacade] 전투가 진행 중이 아닙니다.");
                return;
            }

            Debug.Log("[CombatSystemFacade] 전투 일시정지");
            flowCoordinator.DisablePlayerInput();
        }

        public void ResumeCombat()
        {
            if (!isCombatActive)
            {
                Debug.LogWarning("[CombatSystemFacade] 전투가 진행 중이 아닙니다.");
                return;
            }

            Debug.Log("[CombatSystemFacade] 전투 재개");
            flowCoordinator.EnablePlayerInput();
        }
        #endregion

        #region 상태 관리
        public void ChangeState<T>() where T : ICombatTurnState
        {
            var newState = stateFactory.CreateState<T>();
            if (newState == null)
            {
                Debug.LogError($"[CombatSystemFacade] 상태 생성 실패: {typeof(T).Name}");
                return;
            }

            Debug.Log($"[CombatSystemFacade] 상태 변경: {currentState?.GetType().Name} -> {typeof(T).Name}");
            
            // 현재 상태 종료
            currentState?.ExitState();
            
            // 새 상태 설정
            currentState = newState;
            turnManager.RequestStateChange(newState);
        }
        #endregion

        #region 이벤트 관리
        public void SubscribeToCombatEvents()
        {
            CombatEvents.OnCombatStarted += OnCombatStarted;
            CombatEvents.OnCombatEnded += OnCombatEnded;
            CombatEvents.OnPlayerCharacterDeath += OnPlayerCharacterDeath;
            CombatEvents.OnEnemyCharacterDeath += OnEnemyCharacterDeath;
        }

        public void UnsubscribeFromCombatEvents()
        {
            CombatEvents.OnCombatStarted -= OnCombatStarted;
            CombatEvents.OnCombatEnded -= OnCombatEnded;
            CombatEvents.OnPlayerCharacterDeath -= OnPlayerCharacterDeath;
            CombatEvents.OnEnemyCharacterDeath -= OnEnemyCharacterDeath;
        }

        private void OnCombatStarted()
        {
            Debug.Log("[CombatSystemFacade] 전투 시작 이벤트 수신");
        }

        private void OnCombatEnded(bool isVictory)
        {
            Debug.Log($"[CombatSystemFacade] 전투 종료 이벤트 수신 - 승리: {isVictory}");
            EndCombat(isVictory);
        }

        private void OnPlayerCharacterDeath(Game.CharacterSystem.Data.PlayerCharacterData data, GameObject obj)
        {
            Debug.Log($"[CombatSystemFacade] 플레이어 캐릭터 사망: {data?.name ?? "Unknown"}");
        }

        private void OnEnemyCharacterDeath(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj)
        {
            Debug.Log($"[CombatSystemFacade] 적 캐릭터 사망: {data?.name ?? "Unknown"}");
        }
        #endregion

        #region Unity 생명주기
        private void Awake()
        {
            // Zenject가 의존성을 주입한 후 초기화
            coroutineRunner.RunCoroutine(InitializeAfterInjection());
        }

        private System.Collections.IEnumerator InitializeAfterInjection()
        {
            // 의존성 주입 완료 대기
            yield return new WaitForEndOfFrame();
            
            // 초기화
            Initialize();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
        #endregion
    }
} 