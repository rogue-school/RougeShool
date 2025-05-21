using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.IManager;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Manager
{
    public class CombatTurnManager : MonoBehaviour,
        ICombatTurnManager,
        ITurnStateController,
        ICardExecutionContext,
        ITurnStartConditionChecker
    {
        private ICombatStateFactory stateFactory;
        private ICombatTurnState currentState;
        private ICombatTurnState pendingNextState;

        private ISkillCard registeredPlayerCard;
        private ISkillCard registeredEnemyCard;

        private bool isPlayerGuarded = false;
        private CombatSlotPosition reservedEnemySlot = CombatSlotPosition.NONE;

        private IPlayerManager playerManager;
        private IEnemyManager enemyManager;


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

        private void Update()
        {
            currentState?.ExecuteState();
            if (pendingNextState != null)
                ApplyPendingState();
        }

        public void InjectFactory(ICombatStateFactory factory) => stateFactory = factory;

        public void InjectManagers(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
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

        public void ChangeState(ICombatTurnState newState)
        {
            if (newState == null || currentState == newState) return;

            Debug.Log($"[CombatTurnManager] 상태 전이: {currentState?.GetType().Name ?? "None"} → {newState.GetType().Name}");

            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }

        public ICombatTurnState GetCurrentState() => currentState;

        public void RegisterPlayerGuard() => isPlayerGuarded = true;
        public bool IsPlayerGuarded() => isPlayerGuarded;
        public void ReserveEnemySlot(CombatSlotPosition slot) => reservedEnemySlot = slot;
        public CombatSlotPosition GetReservedEnemySlot() => reservedEnemySlot;
        public void ResetGuardAndReservation()
        {
            isPlayerGuarded = false;
            reservedEnemySlot = CombatSlotPosition.NONE;
        }

        public void RegisterPlayerCard(ISkillCard card) => registeredPlayerCard = card;
        public void RegisterEnemyCard(ISkillCard card) => registeredEnemyCard = card;

        public bool AreBothSlotsReady() => registeredPlayerCard != null && registeredEnemyCard != null;
        public bool CanStartTurn() => AreBothSlotsReady();

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

        public IPlayerCharacter GetPlayer() => playerManager?.GetPlayer();
        public IEnemyCharacter GetEnemy() => enemyManager?.GetEnemy();

        public ICharacter GetSourceCharacter() => null;
        public ICharacter GetTargetCharacter() => null;
    }
}
