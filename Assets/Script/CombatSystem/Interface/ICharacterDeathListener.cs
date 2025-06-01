using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICharacterDeathListener
    {
        void OnCharacterDied(ICharacter character);
    }
}