using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardRegistrationService
    {
        void RegisterPlayerCard(ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);
        (ISkillCard player, ISkillCard enemy) GetRegisteredCards();
        void Clear();
    }
}
