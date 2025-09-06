using Game.CharacterSystem.Data;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// 플레이어 캐릭터 선택 관리 인터페이스
    /// </summary>
    public interface IPlayerCharacterSelectionManager
    {
        /// <summary>
        /// 캐릭터 선택
        /// </summary>
        /// <param name="characterData">선택된 캐릭터 데이터</param>
        void SelectCharacter(PlayerCharacterData characterData);
        
        /// <summary>
        /// 선택된 캐릭터 데이터 반환
        /// </summary>
        /// <returns>선택된 캐릭터 데이터</returns>
        PlayerCharacterData GetSelectedCharacter();
        
        /// <summary>
        /// 캐릭터가 선택되었는지 확인
        /// </summary>
        /// <returns>선택 여부</returns>
        bool HasSelectedCharacter();
        
        /// <summary>
        /// 선택 초기화
        /// </summary>
        void ClearSelection();
    }
}
