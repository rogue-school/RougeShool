using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using System;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 턴 상태 전이와 일부 상태 기반 로직을 제어하는 매니저
    /// </summary>
    public class CombatTurnManager : MonoBehaviour, ICombatTurnManager, ITurnStateController, ITurnStartConditionChecker
    {
        private ICombatStateFactory stateFactory;
        private ICombatTurnState currentState;
        private ICombatTurnState pendingNextState;

        private CombatSlotPosition? reservedEnemySlot;

        private ISkillCard registeredEnemyCard;
        private ISkillCard registeredPlayerCard;

        //  외부 UI/로직이 조건 변경을 감지할 수 있도록 이벤트 제공
        public event Action<bool> OnTurnReadyChanged;

        private bool isTurnReady;

        public void InjectFactory(ICombatStateFactory factory)
        {
            stateFactory = factory;
        }

        public void Initialize()
        {
            if (stateFactory == null)
            {
                Debug.LogError("[CombatTurnManager] 상태 팩토리 주입 누락");
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

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
            Debug.Log($"[CombatTurnManager] 다음 적 슬롯 예약됨: {slot}");
        }

        public CombatSlotPosition? GetReservedEnemySlot() => reservedEnemySlot;

        public void RegisterPlayerGuard()
        {
            Debug.Log("[CombatTurnManager] RegisterPlayerGuard 호출됨 (현재는 별도 동작 없음)");
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            registeredEnemyCard = card;
            Debug.Log($"[CombatTurnManager] 적 카드 등록됨: {card?.CardData.Name}");
            UpdateTurnReady();
        }

        public void RegisterPlayerCard(ISkillCard card)
        {
            registeredPlayerCard = card;
            Debug.Log($"[CombatTurnManager] 플레이어 카드 등록됨: {card?.CardData.Name}");
            UpdateTurnReady();
        }

        private void UpdateTurnReady()
        {
            bool current = registeredEnemyCard != null && registeredPlayerCard != null;

            if (isTurnReady != current)
            {
                isTurnReady = current;
                Debug.Log($"[CombatTurnManager] 전투 시작 가능 상태 변경됨 → {isTurnReady}");
                OnTurnReadyChanged?.Invoke(isTurnReady);
            }
        }

        public bool CanStartTurn() => isTurnReady;
    }
}
