using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardDropValidator
    {
        bool IsValidDrop(ISkillCard card, ICombatCardSlot slot, out string reason);
    }
}