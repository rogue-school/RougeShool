using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// 게임 상태를 관리하는 최소 매니저
    /// </summary>
    public class GameStateManager : MonoBehaviour, ICoreSystemInitializable
    {
        public static GameStateManager Instance { get; private set; }
        
        [Header("게임 상태")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        
        // 이벤트
        public System.Action<GameState> OnGameStateChanged;
        
        // 초기화 상태
        public bool IsInitialized { get; private set; } = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                GameLogger.LogInfo("GameStateManager 싱글톤 초기화 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 게임 상태 변경
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;
            
            var previousState = currentGameState;
            currentGameState = newState;
            
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"[GameStateManager] 게임 상태 변경: {previousState} → {newState}");
        }
        
        /// <summary>
        /// 진행 초기화 (메인으로 이동)
        /// </summary>
        public async Task ResetProgress()
        {
            Debug.Log("[GameStateManager] 진행 초기화 시작");
            
            // 게임 상태를 메인 메뉴로 변경
            ChangeGameState(GameState.MainMenu);
            
            // 메인 씬으로 전환
            await SceneTransitionManager.Instance.TransitionToMainScene();
            
            Debug.Log("[GameStateManager] 진행 초기화 완료");
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        public void ExitGame()
        {
            Debug.Log("[GameStateManager] 게임 종료");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        /// <summary>
        /// 현재 게임 상태 반환
        /// </summary>
        public GameState CurrentGameState => currentGameState;
        
        #region ICoreSystemInitializable 구현
        /// <summary>
        /// 시스템 초기화 수행
        /// </summary>
        public IEnumerator Initialize()
        {
            GameLogger.LogInfo("GameStateManager 초기화 시작", GameLogger.LogCategory.UI);
            
            // 초기 상태 설정
            currentGameState = GameState.MainMenu;
            
            // 초기화 완료
            IsInitialized = true;
            
            GameLogger.LogInfo("GameStateManager 초기화 완료", GameLogger.LogCategory.UI);
            yield return null;
        }
        
        /// <summary>
        /// 초기화 실패 시 호출
        /// </summary>
        public void OnInitializationFailed()
        {
            GameLogger.LogError("GameStateManager 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }
        #endregion
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
