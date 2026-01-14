using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Interface
{
    public interface ICharacterEffect
    {
        string GetEffectName();
        string GetDescription();
        void Initialize(ICharacter character);
        void OnHealthChanged(ICharacter character, int previousHP, int currentHP);
        void OnDeath(ICharacter character);
        void Cleanup(ICharacter character);
    }
}
