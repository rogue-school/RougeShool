using Game.Characters;

namespace Game.Interface
{
    public interface ICardEffect
    {
        void ExecuteEffect(CharacterBase caster, CharacterBase target);
    }
}
