using Game.CharacterSystem.Data;

namespace Game.Utility.GameFlow
{
    public class GameContext : IGameContext
    {
        public PlayerCharacterData SelectedCharacter { get; private set; }

        public void SetSelectedCharacter(PlayerCharacterData data)
        {
            SelectedCharacter = data;
        }
    }
}