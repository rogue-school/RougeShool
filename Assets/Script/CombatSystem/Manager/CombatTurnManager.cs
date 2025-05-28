using UnityEngine;
using System;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

public class CombatTurnManager : MonoBehaviour, ICombatTurnManager, ITurnStateController
{
    private ICombatStateFactory stateFactory;
    private ICombatTurnState currentState;
    private ICombatTurnState pendingNextState;
    private ITurnCardRegistry cardRegistry;

    private CombatSlotPosition? reservedEnemySlot;

    public event Action<bool> OnTurnReadyChanged;
    private bool isTurnReady;

    public void Inject(ICombatStateFactory stateFactory, ITurnCardRegistry cardRegistry)
    {
        this.stateFactory = stateFactory;
        this.cardRegistry = cardRegistry;
    }

    public void Initialize()
    {
        if (stateFactory == null)
        {
            Debug.LogError("[CombatTurnManager] 상태 팩토리 주입 누락됨");
            return;
        }

        var prepareState = stateFactory.CreatePrepareState();
        if (prepareState == null)
        {
            Debug.LogError("[CombatTurnManager] 준비 상태 생성 실패");
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

    private void ChangeState(ICombatTurnState newState)
    {
        if (newState == null || currentState == newState)
            return;

        Debug.Log($"[CombatTurnManager] 상태 전이: {currentState?.GetType().Name ?? "None"} → {newState.GetType().Name}");
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public ICombatTurnState GetCurrentState() => currentState;

    public void Reset()
    {
        Debug.Log("[CombatTurnManager] Reset 호출됨");
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
            Debug.Log($"[CombatTurnManager] 전투 시작 가능 상태 변경 → {isTurnReady}");
            OnTurnReadyChanged?.Invoke(isTurnReady);
        }
    }

    public void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card)
    {
        cardRegistry?.RegisterPlayerCard(slot, card);
        UpdateTurnReady();
    }

    public bool CanStartTurn() => isTurnReady;

    // --- [New] ITurnStateController 구현 ---
    public void ReserveNextEnemySlot(CombatSlotPosition slot)
    {
        reservedEnemySlot = slot;
        Debug.Log($"[CombatTurnManager] 다음 적 슬롯 예약됨 → {slot}");
    }

    public CombatSlotPosition? GetReservedEnemySlot() => reservedEnemySlot;
}
