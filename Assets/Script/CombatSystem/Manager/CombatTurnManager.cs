using UnityEngine;
using System;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Zenject;

namespace Game.CombatSystem.Manager
{
    public class CombatTurnManager : MonoBehaviour, ICombatTurnManager, ITurnStateController
    {
        [Inject] private ICombatStateFactory stateFactory;
        [Inject] private ITurnCardRegistry cardRegistry;

        private ICombatTurnState currentState;
        private ICombatTurnState pendingNextState;

        private CombatSlotPosition? reservedEnemySlot;
        private bool isTurnReady;

        public event Action<bool> OnTurnReadyChanged;

        public void Initialize()
        {
            var prepareState = stateFactory.CreatePrepareState();
            if (prepareState != null)
                RequestStateChange(prepareState);
        }

        private void Update()
        {
            if (pendingNextState != null)
                ApplyPendingState();

            currentState?.ExecuteState();
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
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }

        public ICombatTurnState GetCurrentState() => currentState;

        public void Reset()
        {
            currentState = null;
            pendingNextState = null;
            isTurnReady = false;
            reservedEnemySlot = null;
        }

        public void UpdateTurnReady()
        {
            bool ready = cardRegistry.GetEnemyCard() != null &&
                         cardRegistry.GetPlayerCard(CombatSlotPosition.FIRST) != null;

            if (isTurnReady != ready)
            {
                isTurnReady = ready;
                OnTurnReadyChanged?.Invoke(isTurnReady);
            }
        }

        public void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card)
        {
            cardRegistry.RegisterPlayerCard(slot, card);
            UpdateTurnReady();
        }

        public bool CanStartTurn() => isTurnReady;

        public void ReserveNextEnemySlot(CombatSlotPosition slot) => reservedEnemySlot = slot;
        public CombatSlotPosition? GetReservedEnemySlot() => reservedEnemySlot;

        // FSM 상태에서 사용할 팩토리 접근용 메서드
        public ICombatStateFactory GetStateFactory() => stateFactory;
    }
}
