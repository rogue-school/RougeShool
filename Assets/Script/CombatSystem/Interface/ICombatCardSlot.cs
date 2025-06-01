using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Interface
{
    public interface ICombatCardSlot
    {
        CombatSlotPosition Position { get; }
        CombatFieldSlotPosition GetCombatPosition();

        ISkillCard GetCard();
        void SetCard(ISkillCard card);

        ISkillCardUI GetCardUI();
        void SetCardUI(ISkillCardUI cardUI);

        void Clear();

        bool HasCard();
        bool IsEmpty();

        void ExecuteCardAutomatically();
        void ExecuteCardAutomatically(ICardExecutionContext ctx);

        Transform GetTransform();
    }
}
