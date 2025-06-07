using System.Collections;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICombatExecutor
    {
        IEnumerator PerformAttack(CombatSlotPosition slotPosition);

        void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor);

        /// <summary>
        /// 턴 관리자를 주입합니다. (이전: ITurnStateController → ICombatTurnManager)
        /// </summary>
        void SetTurnManager(ICombatTurnManager turnManager);
    }
}
