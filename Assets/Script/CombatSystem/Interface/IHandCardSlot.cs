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

        SkillCardUI AttachCard(ISkillCard card);                         // Zenject 프리팹 기반
        SkillCardUI AttachCard(ISkillCard card, SkillCardUI prefab);    // 명시적 프리팹 전달
        void DetachCard();                                              // 카드 및 UI 제거
    }
}
