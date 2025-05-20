using System;
using System.Collections;

namespace Game.CombatSystem.Interface
{
    public interface ICombatFlowCoordinator
    {
        IEnumerator PerformCombatPreparation();
        IEnumerator PerformCombatPreparation(Action<bool> onComplete);
        IEnumerator EnablePlayerInput();
        IEnumerator DisablePlayerInput();
        IEnumerator PerformFirstAttack();
        IEnumerator PerformSecondAttack();
        IEnumerator PerformResultPhase();
        IEnumerator PerformVictoryPhase();
        IEnumerator PerformGameOverPhase();

        bool IsPlayerDead();
        bool IsEnemyDead();
        bool CheckHasNextEnemy();

        void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory);
        void StartCombatFlow();
    }
}
