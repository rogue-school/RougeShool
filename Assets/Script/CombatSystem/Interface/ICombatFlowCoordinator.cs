using System;
using System.Collections;

namespace Game.CombatSystem.Interface
{
    public interface ICombatFlowCoordinator
    {
        IEnumerator PerformCombatPreparation();
        IEnumerator PerformCombatPreparation(Action<bool> onComplete);

        void EnablePlayerInput();
        void DisablePlayerInput();

        IEnumerator PerformFirstAttack();
        IEnumerator PerformSecondAttack();
        IEnumerator PerformResultPhase();
        IEnumerator PerformVictoryPhase();
        IEnumerator PerformGameOverPhase();

        bool IsPlayerDead();
        bool IsEnemyDead();
        bool CheckHasNextEnemy();
        bool IsPlayerInputEnabled();

        void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory);
        void StartCombatFlow();
        void RequestCombatPreparation(Action<bool> onComplete);
        void RequestFirstAttack(Action onComplete = null);

        void ShowPlayerCardSelectionUI();
        void HidePlayerCardSelectionUI();

        void EnableStartButton();
        void DisableStartButton();

        void RegisterStartButton(Action onClick);
        void UnregisterStartButton();

        void CleanupAfterVictory();
    }
}
