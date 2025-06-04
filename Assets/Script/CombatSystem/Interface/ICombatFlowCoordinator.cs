using System;
using System.Collections;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

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
        void RegisterCardToCombatSlot(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui);
        ITurnCardRegistry GetTurnCardRegistry();

        void ShowPlayerCardSelectionUI();
        void HidePlayerCardSelectionUI();

        void EnableStartButton();
        void DisableStartButton();

        void RegisterStartButton(Action onClick);
        void UnregisterStartButton();

        void CleanupAfterVictory();
        void ClearEnemyCombatSlots(); // 적 카드만 제거하는 메서드

        ISkillCard GetCardInSlot(CombatSlotPosition pos); // 슬롯에서 카드 가져오기
    }
}
