using UnityEngine;
using System;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 턴을 제어하고 상태 전이, 카드 등록, 턴 진행 가능 여부 등을 관리하는 클래스입니다.
    /// </summary>
    public class CombatTurnManager : MonoBehaviour, ICombatTurnManager
    {
        #region 의존성 주입

        [Inject] private ICombatStateFactory stateFactory;
        [Inject] private ITurnCardRegistry cardRegistry;

        #endregion

        #region 내부 상태

        private ICombatTurnState currentState;
        private ICombatTurnState pendingNextState;
        private CombatSlotPosition? reservedEnemySlot;
        private bool isTurnReady;

        /// <summary>
        /// 턴 시작 가능 상태가 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<bool> OnTurnReadyChanged;

        #endregion

        #region 초기화

        /// <summary>
        /// 턴 매니저를 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[CombatTurnManager] 초기화 완료 (초기 상태 전이 없음)");
            currentState = null;
            pendingNextState = null;
            reservedEnemySlot = null;
            isTurnReady = false;
        }

        /// <summary>
        /// 상태 실행 및 대기 중인 상태 전이를 처리합니다.
        /// </summary>
        private void Update()
        {
            if (pendingNextState != null)
            {
                ApplyPendingState();
            }

            currentState?.ExecuteState();
        }

        #endregion

        #region 상태 전이

        /// <summary>
        /// 다음 상태로의 전이를 요청합니다.
        /// </summary>
        /// <param name="nextState">전이할 상태</param>
        public void RequestStateChange(ICombatTurnState nextState)
        {
            Debug.Log($"[CombatTurnManager] 상태 전이 요청됨 → {nextState.GetType().Name}");
            pendingNextState = nextState;
        }

        /// <summary>
        /// 요청된 상태 전이를 실제로 적용합니다.
        /// </summary>
        public void ApplyPendingState()
        {
            if (pendingNextState == null) return;

            Debug.Log($"[CombatTurnManager] 상태 전이 실행 → {pendingNextState.GetType().Name}");
            ChangeState(pendingNextState);
            pendingNextState = null;
        }

        /// <summary>
        /// 현재 상태를 종료하고 새로운 상태로 전이합니다.
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        public void ChangeState(ICombatTurnState newState)
        {
            currentState?.ExitState();
            Debug.Log($"[CombatTurnManager] 상태 종료 → {currentState?.GetType().Name}");

            currentState = newState;

            Debug.Log($"[CombatTurnManager] 상태 진입 → {currentState?.GetType().Name}");
            currentState?.EnterState();
        }

        #endregion

        #region 상태 및 팩토리 접근

        /// <summary>
        /// 현재 상태를 반환합니다.
        /// </summary>
        public ICombatTurnState GetCurrentState() => currentState;

        /// <summary>
        /// 상태 생성 팩토리를 반환합니다.
        /// </summary>
        public ICombatStateFactory GetStateFactory() => stateFactory;

        #endregion

        #region 리셋 및 턴 준비 상태 관리

        /// <summary>
        /// 상태, 슬롯 예약, 준비 상태 등을 모두 초기화합니다.
        /// </summary>
        public void Reset()
        {
            Debug.Log("[CombatTurnManager] 상태 초기화");
            currentState = null;
            pendingNextState = null;
            reservedEnemySlot = null;
            isTurnReady = false;
        }

        /// <summary>
        /// 턴 시작 준비가 되었는지 여부를 확인하고 이벤트를 발생시킵니다.
        /// </summary>
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

        #endregion

        #region 카드 등록 및 턴 흐름 제어

        /// <summary>
        /// 슬롯에 카드를 등록하고 턴 준비 상태를 갱신합니다.
        /// </summary>
        /// <param name="slot">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">소유자</param>
        public void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            cardRegistry.RegisterCard(slot, card, ui, owner);
            Debug.Log($"[CombatTurnManager] 카드 등록 완료 → {card.GetCardName()} → 슬롯: {slot}");
            UpdateTurnReady();
        }

        /// <summary>
        /// 턴을 시작할 수 있는 상태인지 확인합니다.
        /// </summary>
        public bool CanStartTurn() => isTurnReady;

        /// <summary>
        /// 현재 상태가 플레이어 입력 상태인지 확인합니다.
        /// </summary>
        public bool IsPlayerInputTurn() => currentState is State.CombatPlayerInputState;

        #endregion

        #region 적 슬롯 예약

        /// <summary>
        /// 적 카드를 예약할 슬롯을 설정합니다.
        /// </summary>
        /// <param name="slot">예약할 슬롯 위치</param>
        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
        }

        /// <summary>
        /// 예약된 적 카드 슬롯 정보를 반환합니다.
        /// </summary>
        public CombatSlotPosition? GetReservedEnemySlot()
        {
            return reservedEnemySlot;
        }

        #endregion
    }
}
