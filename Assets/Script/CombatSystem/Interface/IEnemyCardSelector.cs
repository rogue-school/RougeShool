using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface IEnemyCardSelector
    {
        ISkillCard SelectCard(IEnemyCharacter enemy);
    }
}
