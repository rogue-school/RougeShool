using UnityEngine;
using System;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Zenject;

namespace Game.CombatSystem.Manager
{
    public class CombatTurnManager : MonoBehaviour, ICombatTurnManager
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
            Debug.Log("[CombatTurnManager] 초기화 완료 (초기 상태 전이 없음)");
            currentState = null;
            pendingNextState = null;
            reservedEnemySlot = null;
            isTurnReady = false;
        }

        private void Update()
        {
            if (pendingNextState != null)
            {
                ApplyPendingState();
            }

            currentState?.ExecuteState();
        }

        public void RequestStateChange(ICombatTurnState nextState)
        {
            Debug.Log($"[CombatTurnManager] 상태 전이 요청됨 → {nextState.GetType().Name}");
            pendingNextState = nextState;
        }

        public void ApplyPendingState()
        {
            if (pendingNextState == null) return;

            Debug.Log($"[CombatTurnManager] 상태 전이 실행 → {pendingNextState.GetType().Name}");
            ChangeState(pendingNextState);
            pendingNextState = null;
        }

        public void ChangeState(ICombatTurnState newState)
        {
            currentState?.ExitState();
            Debug.Log($"[CombatTurnManager] 상태 종료 → {currentState?.GetType().Name}");

            currentState = newState;

            Debug.Log($"[CombatTurnManager] 상태 진입 → {currentState?.GetType().Name}");
            currentState?.EnterState();
        }

        public ICombatTurnState GetCurrentState() => currentState;

        public ICombatStateFactory GetStateFactory() => stateFactory;

        public void Reset()
        {
            Debug.Log("[CombatTurnManager] 상태 초기화");
            currentState = null;
            pendingNextState = null;
            reservedEnemySlot = null;
            isTurnReady = false;
        }

        public void UpdateTurnReady()
        {
            bool ready =
                cardRegistry.GetCardInSlot(CombatSlotPosition.FIRST) != null &&
                cardRegistry.GetCardInSlot(CombatSlotPosition.SECOND) != null;

            if (isTurnReady != ready)
            {
                isTurnReady = ready;
                Debug.Log($"[CombatTurnManager] 턴 시작 가능 상태 변경 → {(ready ? "가능" : "불가능")}");
                OnTurnReadyChanged?.Invoke(isTurnReady);
            }
        }

        public void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            cardRegistry.RegisterCard(slot, card, ui, owner);
            Debug.Log($"[CombatTurnManager] 카드 등록 완료 → {card.GetCardName()} → 슬롯: {slot}");
            UpdateTurnReady();
        }

        public bool CanStartTurn() => isTurnReady;

        public bool IsPlayerInputTurn() => currentState is State.CombatPlayerInputState;

        public void ReserveNextEnemySlot(CombatSlotPosition slot) => reservedEnemySlot = slot;

        public CombatSlotPosition? GetReservedEnemySlot() => reservedEnemySlot;
    }
}
