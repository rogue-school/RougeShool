using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 플레이어가 선택한 캐릭터 정보를 제공하는 선택기 인터페이스입니다.
    /// 주로 캐릭터 선택 화면 이후에 전투 씬에서 사용됩니다.
    /// </summary>
    public interface IPlayerCharacterSelector
    {
        /// <summary>
        /// 현재 선택된 플레이어 캐릭터 데이터를 반환합니다.
        /// </summary>
        /// <returns>선택된 플레이어 캐릭터 데이터.</returns>
        PlayerCharacterData GetSelectedCharacter();
    }
}
