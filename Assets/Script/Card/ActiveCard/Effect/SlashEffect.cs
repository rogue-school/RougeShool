using Game.Interface;
using Game.Characters;

namespace Game.Cards
{
    public class SlashEffect : ICardEffect
    {
        private int damage;

        public SlashEffect(int damage)
        {
            this.damage = damage;
        }

        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            target?.TakeDamage(damage);
        }
    }
}
