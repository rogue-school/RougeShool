using Game.Units;

namespace Game.Effects
{
    public class BlockEffect : ICardEffect
    {
        private int amount;
        public BlockEffect(int amount) => this.amount = amount;

        public void ExecuteEffect(Unit caster, Unit target)
        {
            caster?.AddBlock(amount);
        }
    }
}
