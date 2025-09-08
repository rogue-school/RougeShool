using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Turn
{
    public class CardRegistrationService : ICardRegistrationService
    {
        private ISkillCard playerCard;
        private ISkillCard enemyCard;

        public void RegisterPlayerCard(ISkillCard card) => playerCard = card;
        public void RegisterEnemyCard(ISkillCard card) => enemyCard = card;

        public (ISkillCard player, ISkillCard enemy) GetRegisteredCards() => (playerCard, enemyCard);

        public void Clear()
        {
            playerCard = null;
            enemyCard = null;
        }
    }
}