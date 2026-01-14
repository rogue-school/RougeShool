using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
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

            // 카드 등록 완료
        }

        public void RegisterPlayerCard(ISkillCard card)
        {
            playerCard = card;
            // 플레이어 카드 등록 완료
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            enemyCard = card;
            // 적 카드 등록 완료
        }

        public (ISkillCard player, ISkillCard enemy) GetRegisteredCards()
        {
            return (playerCard, enemyCard);
        }

        public void Clear()
        {
            playerCard = null;
            enemyCard = null;
            GameLogger.LogInfo("[Registrar] 등록된 카드들 초기화", GameLogger.LogCategory.Combat);
        }
    }
}
