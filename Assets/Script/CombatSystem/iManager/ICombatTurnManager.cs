using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI; // 필요

namespace Game.CombatSystem.Interface
{
    public interface ICombatTurnManager : ITurnStateController
    {
        void Initialize();
        void Reset();

        /// <summary>
        /// 현재 상태 가져오기
        /// </summary>
        ICombatTurnState GetCurrentState();

        /// <summary>
        /// 상태 즉시 전이
        /// </summary>
        void ChangeState(ICombatTurnState newState);

        /// <summary>
        /// 상태 생성 팩토리 접근
        /// </summary>
        ICombatStateFactory GetStateFactory();

        /// <summary>
        /// 플레이어 입력 가능한 상태인지 여부
        /// </summary>
        bool IsPlayerInputTurn();

        /// <summary>
        /// 카드 등록 시 전투 슬롯과 UI 포함하여 등록
        /// </summary>
        void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardUI ui, SlotOwner owner);
    }
}
