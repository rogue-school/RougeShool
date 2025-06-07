using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.Utility;
using Game.CharacterSystem.Interface;
using Game.Manager;
using Game.IManager;

public class CombatGameOverState : ICombatTurnState
{
    private readonly ICombatTurnManager turnManager;
    private readonly ICombatFlowCoordinator flowCoordinator;
    private readonly ICombatSlotRegistry slotRegistry;
    private readonly ICoroutineRunner coroutineRunner;
    private readonly DeathUIManager deathUIManager;
    private readonly IPlayerManager playerManager;


    public CombatGameOverState(
        ICombatTurnManager turnManager,
        ICombatFlowCoordinator flowCoordinator,
        ICombatSlotRegistry slotRegistry,
        ICoroutineRunner coroutineRunner,
        DeathUIManager deathUIManager,
        IPlayerManager playerManager
    )   
    {
        this.turnManager = turnManager;
        this.flowCoordinator = flowCoordinator;
        this.slotRegistry = slotRegistry;
        this.coroutineRunner = coroutineRunner;
        this.deathUIManager = deathUIManager;
        this.playerManager = playerManager;
    }


    public void EnterState()
    {
        Debug.Log("[CombatGameOverState] 상태 진입 - 게임 오버 처리 시작");
        coroutineRunner.RunCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return flowCoordinator.PerformGameOverPhase();

        if (CheckPlayerDeath())
        {
            deathUIManager.ShowDeathUI();
            Debug.Log("[CombatGameOverState] 플레이어 패배 - 게임 오버 UI 표시");
        }

        Debug.Log("[CombatGameOverState] 게임 오버 처리 완료");
    }

    private bool CheckPlayerDeath()
    {
        var player = playerManager.GetPlayer();
        return player == null || player.IsDead();
    }

    public void ExecuteState() { }
    public void ExitState() { }
}
