using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardRegistrar
    {
        void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI ui);
    }
}
