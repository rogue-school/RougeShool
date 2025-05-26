using UnityEngine;
using System;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

public class CombatTurnManager : MonoBehaviour,
    ICombatTurnManager,
    ITurnStateController,
    ITurnStartConditionChecker,
    ITurnCardRegistry,
    IEnemySlotReservationService
{
    private ICombatStateFactory stateFactory;
    private ICombatTurnState currentState;
    private ICombatTurnState pendingNextState;

    private CombatSlotPosition? reservedEnemySlot;

    private ISkillCard registeredEnemyCard;
    private ISkillCard registeredPlayerCard;

    private bool isTurnReady;
    public event Action<bool> OnTurnReadyChanged;

    public void InjectFactory(ICombatStateFactory factory) => stateFactory = factory;

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

    public void RequestStateChange(ICombatTurnState nextState) => pendingNextState = nextState;

    private void ApplyPendingState()
    {
        ChangeState(pendingNextState);
        pendingNextState = null;
    }

    public void ChangeState(ICombatTurnState newState)
    {
        if (newState == null || currentState == newState)
            return;

        Debug.Log($"[CombatTurnManager] 상태 전이: {currentState?.GetType().Name ?? "None"} → {newState.GetType().Name}");

        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public ICombatTurnState GetCurrentState() => currentState;

    public void RegisterEnemyCard(ISkillCard card)
    {
        if (card == null)
        {
            Debug.LogError("[CombatTurnManager] 적 카드 등록 실패: null");
            return;
        }

        registeredEnemyCard = card;
        Debug.Log($"[CombatTurnManager] 적 카드 등록됨: {card.CardData?.Name ?? "Unknown"}");
        UpdateTurnReady();
    }

    public void RegisterPlayerCard(ISkillCard card)
    {
        if (card == null)
        {
            Debug.LogError("[CombatTurnManager] 플레이어 카드 등록 실패: null");
            return;
        }

        registeredPlayerCard = card;
        Debug.Log($"[CombatTurnManager] 플레이어 카드 등록됨: {card.CardData?.Name ?? "Unknown"}");
        UpdateTurnReady();
    }

    public void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card)
    {
        Debug.Log($"[CombatTurnManager] RegisterPlayerCard 위치: {position}, 카드: {card?.CardData?.Name ?? "null"}");
        RegisterPlayerCard(card);
    }

    private void UpdateTurnReady()
    {
        bool ready = registeredEnemyCard != null && registeredPlayerCard != null;
        if (isTurnReady != ready)
        {
            isTurnReady = ready;
            Debug.Log($"[CombatTurnManager] 전투 시작 가능 상태 변경 → {isTurnReady}");
            NotifyTurnReadyChanged();
        }
    }

    private void NotifyTurnReadyChanged()
    {
        try
        {
            OnTurnReadyChanged?.Invoke(isTurnReady);
        }
        catch (Exception e)
        {
            Debug.LogError($"[CombatTurnManager] OnTurnReadyChanged 예외: {e.Message}");
        }
    }

    public void ReserveNextEnemySlot(CombatSlotPosition slot)
    {
        reservedEnemySlot = slot;
        Debug.Log($"[CombatTurnManager] 다음 적 슬롯 예약됨: {slot}");
    }

    public CombatSlotPosition? GetReservedEnemySlot() => reservedEnemySlot;

    public bool CanStartTurn() => isTurnReady;

    public void Reset()
    {
        Debug.Log("[CombatTurnManager] Reset 호출됨");
        ClearRegisteredCards();
        reservedEnemySlot = null;
        currentState = null;
        pendingNextState = null;
    }

    private void ClearRegisteredCards()
    {
        registeredEnemyCard = null;
        registeredPlayerCard = null;
        isTurnReady = false;
    }
}
