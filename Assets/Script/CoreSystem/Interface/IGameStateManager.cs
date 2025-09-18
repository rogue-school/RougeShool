using System.Threading.Tasks;
using Game.CharacterSystem.Data;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// 게임 상태 관리 인터페이스
    /// </summary>
    public interface IGameStateManager
    {
        /// <summary>
        /// 현재 게임 상태
        /// </summary>
        GameState CurrentGameState { get; }
        
        /// <summary>
        /// 선택된 캐릭터 데이터
        /// </summary>
        PlayerCharacterData SelectedCharacter { get; }
        
        /// <summary>
        /// 게임 상태 변경 이벤트
        /// </summary>
        System.Action<GameState> OnGameStateChanged { get; set; }
        
        /// <summary>
        /// 게임 상태 변경
        /// </summary>
        void ChangeGameState(GameState newState);
        
        /// <summary>
        /// 캐릭터 선택
        /// </summary>
        void SelectCharacter(PlayerCharacterData characterData);
        
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        void PauseGame();
        
        /// <summary>
        /// 게임 재개
        /// </summary>
        void ResumeGame();
        
        /// <summary>
        /// 진행 초기화
        /// </summary>
        Task ResetProgress();
        
        /// <summary>
        /// 세션 초기화
        /// </summary>
        void ResetSession();
        
        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        void GoToMainMenu();
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        void ExitGame();
    }
    
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameState
    {
        MainMenu,    // 메인 메뉴
        Playing,     // 게임 진행 중
        Paused       // 일시정지
    }
}
