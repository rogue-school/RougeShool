using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Service
{
    public class RandomEnemyCardSelector : IEnemyCardSelector
    {
        public ISkillCard SelectCard(IEnemyCharacter enemy)
        {
            return enemy?.GetRandomCardEntry()?.CreateRuntimeCard();
        }
    }
}
