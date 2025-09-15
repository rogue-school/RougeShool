using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    public class DefaultCardRegistrar : ICardRegistrar
    {
        public void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI ui)
        {
            var execSlot = slot.Position;
            card.SetCombatSlot(execSlot);

            slot.SetCard(card);
            slot.SetCardUI(ui);

            CardSlotHelper.AttachCardToSlot(ui, (MonoBehaviour)slot);

            Debug.Log($"[Registrar] 카드 등록 완료: {card.CardDefinition?.CardName ?? "Unknown"} → {execSlot}");
        }
    }
}
