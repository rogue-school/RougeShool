using Game.CharacterSystem.Data;

namespace Game.Utility.GameFlow
{
    public interface IGameContext
    {
        PlayerCharacterData SelectedCharacter { get; }
        void SetSelectedCharacter(PlayerCharacterData data);
    }
}