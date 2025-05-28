using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    public interface IHandCardSlot
    {
        void SetCard(ISkillCard card);
        void Clear();
        ISkillCard GetCard();

        SkillCardSlotPosition GetSlotPosition();
        SlotOwner GetOwner();

        bool HasCard();
        ISkillCardUI GetCardUI();

        // ✅ 추가할 메서드들:
        SkillCardUI AttachCard(ISkillCard card); // 카드 UI를 연결하고 반환
        void DetachCard();                       // 카드 UI를 제거
    }
}
