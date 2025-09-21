using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropRegistrar
    {
        private ISkillCard playerCard;
        private ISkillCard enemyCard;

        public void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI ui)
        {
            var execSlot = slot.Position;
            card.SetCombatSlot(execSlot);

            slot.SetCard(card);
            slot.SetCardUI(ui);

            CardSlotHelper.AttachCardToSlot(ui, (MonoBehaviour)slot);

            Debug.Log($"[Registrar] 카드 등록 완료: {card.CardDefinition?.CardName ?? "Unknown"} → {execSlot}");
        }

        public void RegisterPlayerCard(ISkillCard card)
        {
            playerCard = card;
            Debug.Log($"[Registrar] 플레이어 카드 등록: {card.CardDefinition?.CardName ?? "Unknown"}");
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            enemyCard = card;
            Debug.Log($"[Registrar] 적 카드 등록: {card.CardDefinition?.CardName ?? "Unknown"}");
        }

        public (ISkillCard player, ISkillCard enemy) GetRegisteredCards()
        {
            return (playerCard, enemyCard);
        }

        public void Clear()
        {
            playerCard = null;
            enemyCard = null;
            Debug.Log("[Registrar] 등록된 카드들 초기화");
        }
    }
}
