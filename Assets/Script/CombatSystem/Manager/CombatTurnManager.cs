using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    public class CombatTurnManager : MonoBehaviour,
        ICombatTurnManager,
        ITurnStateController,
        ICardExecutionContext,
        ITurnStartConditionChecker // ← 신규 인터페이스로 분리
    {
        [SerializeField] private bool autoStart = true;

        private ICombatStateFactory stateFactory;
        private ICombatTurnState currentState;
        private ICombatTurnState pendingNextState;

        private ISkillCard registeredPlayerCard;
        private ISkillCard registeredEnemyCard;

        private bool isPlayerGuarded = false;
        private CombatSlotPosition reservedEnemySlot = CombatSlotPosition.NONE;

        private IPlayerManager playerManager;
        private IEnemyManager enemyManager;

        private void Start()
        {
            if (autoStart)
                Initialize();
        }

        private void Update()
        {
            currentState?.ExecuteState();
            if (pendingNextState != null)
                ApplyPendingState();
        }

        public void Initialize()
        {
            if (stateFactory == null)
            {
                Debug.LogError("[CombatTurnManager] 상태 팩토리가 주입되지 않았습니다.");
                return;
            }

            var prepareState = stateFactory.CreatePrepareState();
            if (prepareState == null)
            {
                Debug.LogError("[CombatTurnManager] PrepareState 생성 실패");
                return;
            }

            RequestStateChange(prepareState);
        }

        public void InjectFactory(ICombatStateFactory factory) => stateFactory = factory;

        public void InjectManagers(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        public void ChangeState(ICombatTurnState newState)
        {
            if (newState == null || currentState == newState) return;

            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }

        public void RequestStateChange(ICombatTurnState nextState)
        {
            pendingNextState = nextState;
        }

        private void ApplyPendingState()
        {
            ChangeState(pendingNextState);
            pendingNextState = null;
        }

        public ICombatTurnState GetCurrentState() => currentState;

        // Guard 및 슬롯 예약
        public void RegisterPlayerGuard() => isPlayerGuarded = true;
        public bool IsPlayerGuarded() => isPlayerGuarded;
        public void ReserveEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
            Debug.Log($"[CombatTurnManager] 적 공격 슬롯 예약됨 → {slot}");
        }
        public CombatSlotPosition GetReservedEnemySlot() => reservedEnemySlot;
        public void ResetGuardAndReservation()
        {
            isPlayerGuarded = false;
            reservedEnemySlot = CombatSlotPosition.NONE;
        }

        // 전투 카드 등록 및 실행
        public void RegisterPlayerCard(ISkillCard card) => registeredPlayerCard = card;
        public void RegisterEnemyCard(ISkillCard card) => registeredEnemyCard = card;

        public bool AreBothSlotsReady() => registeredPlayerCard != null && registeredEnemyCard != null;
        public bool CanStartTurn() => AreBothSlotsReady(); // ITurnStartConditionChecker용

        public void ExecuteCombat()
        {
            if (!CanStartTurn())
            {
                Debug.LogWarning("[CombatTurnManager] 전투 시작 조건 미충족");
                return;
            }

            var next = stateFactory?.CreateFirstAttackState();
            if (next != null)
                RequestStateChange(next);
            else
                Debug.LogError("[CombatTurnManager] FirstAttackState 생성 실패");
        }

        // 카드 실행 컨텍스트
        public IPlayerCharacter GetPlayer() => playerManager?.GetPlayer();
        public IEnemyCharacter GetEnemy() => enemyManager?.GetEnemy();
        public ICharacter GetSourceCharacter()
        {
            Debug.LogWarning("[CombatTurnManager] GetSourceCharacter() 호출됨 - 기본 없음");
            return null;
        }
        public ICharacter GetTargetCharacter()
        {
            Debug.LogWarning("[CombatTurnManager] GetTargetCharacter() 호출됨 - 기본 없음");
            return null;
        }
    }
}
