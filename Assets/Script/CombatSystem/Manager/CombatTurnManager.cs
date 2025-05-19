using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    public class CombatTurnManager : MonoBehaviour,
        ICombatTurnManager, ITurnStateController, ICardExecutionContext
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
            {
                ApplyPendingState();
            }
        }

        public void InjectFactory(ICombatStateFactory factory)
        {
            this.stateFactory = factory;
        }

        public void InjectManagers(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
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

        public void ChangeState(ICombatTurnState newState)
        {
            if (newState == null || currentState == newState)
                return;

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

        // ----------------------------
        // ITurnStateController 구현
        // ----------------------------
        public void RegisterPlayerGuard()
        {
            isPlayerGuarded = true;
        }

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

        // ----------------------------
        // 전투 카드 등록 및 전투 개시
        // ----------------------------
        public void RegisterPlayerCard(ISkillCard card)
        {
            registeredPlayerCard = card;
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            registeredEnemyCard = card;
        }

        public bool AreBothSlotsReady() => registeredPlayerCard != null && registeredEnemyCard != null;

        public void ExecuteCombat()
        {
            var next = stateFactory?.CreateFirstAttackState();
            if (next != null)
                RequestStateChange(next);
            else
                Debug.LogError("[CombatTurnManager] FirstAttackState 생성 실패");
        }

        // ----------------------------
        // ICardExecutionContext 구현
        // ----------------------------
        public IPlayerCharacter GetPlayer()
        {
            var player = playerManager?.GetPlayer();
            if (player == null)
                Debug.LogError("[CombatTurnManager] 플레이어 캐릭터 참조 실패");
            return player;
        }

        public IEnemyCharacter GetEnemy()
        {
            var enemy = enemyManager?.GetEnemy();
            if (enemy == null)
                Debug.LogError("[CombatTurnManager] 적 캐릭터 참조 실패");
            return enemy;
        }
    }
}
