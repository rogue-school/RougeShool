using UnityEngine;
using System;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Zenject;
using Game.CombatSystem.State;

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

        /// <summary>
        /// 전투 시작 가능 여부를 계산
        /// 현재는 두 슬롯이 모두 채워져 있어야 함
        /// </summary>
        public void UpdateTurnReady()
        {
            bool ready =
                cardRegistry.GetCardInSlot(CombatSlotPosition.FIRST) != null &&
                cardRegistry.GetCardInSlot(CombatSlotPosition.SECOND) != null;

            if (isTurnReady != ready)
            {
                isTurnReady = ready;
                OnTurnReadyChanged?.Invoke(isTurnReady);
            }
        }

        /// <summary>
        /// 카드 등록 시 호출되어 전투 준비 상태 갱신
        /// </summary>
        public void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardSystem.UI.SkillCardUI ui, SlotOwner owner)
        {
            cardRegistry.RegisterCard(slot, card, ui, owner);
            UpdateTurnReady();
        }

        public bool CanStartTurn() => isTurnReady;

        public void ReserveNextEnemySlot(CombatSlotPosition slot) => reservedEnemySlot = slot;
        public CombatSlotPosition? GetReservedEnemySlot() => reservedEnemySlot;

        public ICombatStateFactory GetStateFactory() => stateFactory;

        public bool IsPlayerInputTurn()
        {
            return currentState is CombatPlayerInputState;
        }
    }
}
