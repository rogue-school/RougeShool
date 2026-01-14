using Game.CharacterSystem.Data;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// 플레이어 캐릭터 선택 관리 인터페이스
    /// </summary>
    public interface IPlayerCharacterSelectionManager
    {
        /// <summary>
        /// 선택된 캐릭터 데이터
        /// </summary>
        PlayerCharacterData SelectedCharacter { get; }
        
        /// <summary>
        /// 캐릭터 선택 이벤트
        /// </summary>
        System.Action<PlayerCharacterData> OnCharacterSelected { get; set; }
        
        /// <summary>
        /// 캐릭터 선택
        /// </summary>
        /// <param name="characterData">선택할 캐릭터 데이터</param>
        void SelectCharacter(PlayerCharacterData characterData);
        
        /// <summary>
        /// 선택된 캐릭터 초기화
        /// </summary>
        void ClearSelection();
        
        /// <summary>
        /// 캐릭터 선택 가능 여부 확인
        /// </summary>
        /// <param name="characterData">확인할 캐릭터 데이터</param>
        /// <returns>선택 가능하면 true</returns>
        bool CanSelectCharacter(PlayerCharacterData characterData);
        
        /// <summary>
        /// 선택된 캐릭터가 있는지 확인
        /// </summary>
        /// <returns>선택된 캐릭터가 있으면 true</returns>
        bool HasSelectedCharacter();
        
        /// <summary>
        /// 선택된 캐릭터 데이터 반환
        /// </summary>
        /// <returns>선택된 캐릭터 데이터 (없으면 null)</returns>
        PlayerCharacterData GetSelectedCharacter();
    }
}