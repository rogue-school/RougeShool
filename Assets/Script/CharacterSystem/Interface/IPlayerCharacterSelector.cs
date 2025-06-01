using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Interface
{
    public interface IPlayerCharacterSelector
    {
        PlayerCharacterData GetSelectedCharacter();
    }
}