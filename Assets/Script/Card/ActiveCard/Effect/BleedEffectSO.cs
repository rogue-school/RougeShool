using Game.Effect;
using Game.Interface;

namespace Game.Cards
{
    public class BleedEffect : IPerTurnEffect
    {
        private readonly int damage;
        private int remainingTurns;

        public BleedEffect(int damage, int duration)
        {
            this.damage = damage;
            this.remainingTurns = duration;
        }

        public void ApplyPerTurn(ICharacter target)
        {
            if (!target.IsDead())
            {
                target.TakeDamage(damage);
                remainingTurns--;
            }
        }

        public bool IsFinished()
        {
            return remainingTurns <= 0;
        }
    }
}
