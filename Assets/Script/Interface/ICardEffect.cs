using Game.Units;

namespace Game.Effects
{
    public interface ICardEffect
    {
        void ExecuteEffect(Unit caster, Unit target);
    }
}
