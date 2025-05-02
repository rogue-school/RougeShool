using Game.Units;

namespace Game.Effects
{
    public class SlashEffect : ICardEffect
    {
        private int damage;
        public SlashEffect(int damage) => this.damage = damage;

        public void ExecuteEffect(Unit caster, Unit target)
        {
            target?.TakeDamage(damage);
        }
    }
}
