using System.Collections;

namespace Game.CombatSystem.Interface
{
    public interface ICombatFlowCoordinator
    {
        IEnumerator PerformCombatPreparation();
        IEnumerator EnablePlayerInput();
        IEnumerator DisablePlayerInput();
        IEnumerator PerformFirstAttack();
        IEnumerator PerformSecondAttack();
        IEnumerator PerformResultPhase();
        IEnumerator PerformVictoryPhase();
        IEnumerator PerformGameOverPhase();
    }
}
