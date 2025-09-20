using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Data;
using Zenject;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// 게임 상태를 관리하는 매니저 (Zenject DI 기반)
    /// </summary>
    public class GameStateManager : MonoBehaviour, ICoreSystemInitializable, IGameStateManager
    {
        [Header("게임 상태")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        
        [Header("캐릭터 선택")]
        [SerializeField] private PlayerCharacterData selectedCharacter;
        
        // 이벤트
        public System.Action<GameState> OnGameStateChanged { get; set; }
        
        // 초기화 상태
        public bool IsInitialized { get; private set; } = false;
        
        // 의존성 주입
        private ISceneTransitionManager sceneTransitionManager;
        
        [Inject]
        public void Construct(ISceneTransitionManager sceneTransitionManager)
        {
            this.sceneTransitionManager = sceneTransitionManager;
        }
        
        private void Awake()
        {
            // 씬 전환 시에도 선택된 캐릭터 등 상태 유지
            // 루트 오브젝트에서만 DontDestroyOnLoad를 적용해야 경고가 발생하지 않습니다.
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("[GameStateManager] 루트 오브젝트가 아니므로 DontDestroyOnLoad를 적용할 수 없습니다. 루트로 이동을 권장합니다.");
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
            await sceneTransitionManager.TransitionToMainScene();
            
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
        
        /// <summary>
        /// 선택된 캐릭터 데이터 반환
        /// </summary>
        public PlayerCharacterData SelectedCharacter => selectedCharacter;
        
        /// <summary>
        /// 캐릭터 선택
        /// </summary>
        public void SelectCharacter(PlayerCharacterData characterData)
        {
            selectedCharacter = characterData;
            GameLogger.LogInfo($"캐릭터 선택: {characterData?.DisplayName}", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            Time.timeScale = 0f;
            ChangeGameState(GameState.Paused);
            GameLogger.LogInfo("게임 일시정지", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            ChangeGameState(GameState.Playing);
            GameLogger.LogInfo("게임 재개", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 세션 초기화 (게임 진행 리셋)
        /// </summary>
        public void ResetSession()
        {
            selectedCharacter = null;
            Time.timeScale = 1f;
            ChangeGameState(GameState.MainMenu);
            GameLogger.LogInfo("세션 초기화", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public async void GoToMainMenu()
        {
            GameLogger.LogInfo("메인 메뉴로 이동", GameLogger.LogCategory.UI);
            await sceneTransitionManager.TransitionToMainScene();
        }
        
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
}
