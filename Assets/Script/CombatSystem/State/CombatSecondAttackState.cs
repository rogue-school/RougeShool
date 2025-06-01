using Game.CombatSystem.Interface;
using System.Collections;
using UnityEngine;

public class CombatSecondAttackState : ICombatTurnState
{
    private readonly ICombatTurnManager turnManager;
    private readonly ICombatFlowCoordinator flowCoordinator;
    private readonly ICombatStateFactory stateFactory;
    private readonly ICombatSlotRegistry slotRegistry;

    public CombatSecondAttackState(
        ICombatTurnManager turnManager,
        ICombatFlowCoordinator flowCoordinator,
        ICombatStateFactory stateFactory,
        ICombatSlotRegistry slotRegistry)
    {
        this.turnManager = turnManager;
        this.flowCoordinator = flowCoordinator;
        this.stateFactory = stateFactory;
        this.slotRegistry = slotRegistry;
    }

    public void EnterState()
    {
        if (flowCoordinator is MonoBehaviour mono)
            mono.StartCoroutine(AttackRoutine());
        else
            Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
    }

    private IEnumerator AttackRoutine()
    {
        yield return flowCoordinator.PerformSecondAttack();
        turnManager.RequestStateChange(stateFactory.CreateResultState());
    }

    public void ExecuteState() { }
    public void ExitState() { }
}
