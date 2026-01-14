using Game.CharacterSystem.Data;

namespace Game.UtilitySystem.GameFlow
{
    public interface IGameContext
    {
        PlayerCharacterData SelectedCharacter { get; }
        void SetSelectedCharacter(PlayerCharacterData data);
    }
}