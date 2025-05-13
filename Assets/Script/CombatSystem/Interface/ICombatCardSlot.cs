using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    public interface ICombatCardSlot
    {
        void SetCard(ISkillCard card);
        void Clear();
        void SetCardUI(SkillCardUI cardUI);
        SkillCardUI GetCardUI();
        ISkillCard GetCard();

        CombatSlotPosition GetCombatPosition();
        SlotOwner GetOwner();

        /// <summary>
        /// 슬롯에 있는 카드를 자동 실행합니다.
        /// </summary>
        void ExecuteCardAutomatically();
    }
}
