using Game.CharacterSystem.Data;

namespace Game.UtilitySystem.GameFlow
{
    /// <summary>
    /// 게임 실행 중 공유되는 컨텍스트를 저장합니다.
    /// 현재 선택된 플레이어 캐릭터 정보를 관리합니다.
    /// </summary>
    public class GameContext : IGameContext
    {
        /// <summary>
        /// 현재 선택된 플레이어 캐릭터 데이터입니다.
        /// </summary>
        public PlayerCharacterData SelectedCharacter { get; private set; }

        /// <summary>
        /// 선택된 플레이어 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="data">선택된 캐릭터 데이터</param>
        public void SetSelectedCharacter(PlayerCharacterData data)
        {
            SelectedCharacter = data;
        }
    }
}
