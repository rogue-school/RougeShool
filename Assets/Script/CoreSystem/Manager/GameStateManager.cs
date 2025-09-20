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
    public class GameStateManager : BaseCoreManager<IGameStateManager>, IGameStateManager
    {
        #region GameStateManager 전용 설정
        
        [Header("게임 상태")]
        [Tooltip("현재 게임 상태")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        
        [Header("캐릭터 선택")]
        [Tooltip("선택된 플레이어 캐릭터 데이터")]
        [SerializeField] private PlayerCharacterData selectedCharacter;
        
        #endregion
        
        // 이벤트
        public System.Action<GameState> OnGameStateChanged { get; set; }
        
        // 초기화 상태는 베이스 클래스에서 관리
        
        // 의존성 주입
        private ISceneTransitionManager sceneTransitionManager;
        
        [Inject]
        public void Construct(ISceneTransitionManager sceneTransitionManager)
        {
            this.sceneTransitionManager = sceneTransitionManager;
        }
        
        protected override void Awake()
        {
            base.Awake();
            // 베이스 클래스에서 DontDestroyOnLoad 처리
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
        
        #region 베이스 클래스 구현

        protected override System.Collections.IEnumerator OnInitialize()
        {
            // 초기 상태 설정
            currentGameState = GameState.MainMenu;
            
            // UI 연결
            ConnectUI();
            
            // 참조 검증
            ValidateReferences();
            
            yield return null;
        }

        public override void Reset()
        {
            selectedCharacter = null;
            currentGameState = GameState.MainMenu;
            Time.timeScale = 1f;
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("GameStateManager 리셋 완료", GameLogger.LogCategory.UI);
            }
        }

        #endregion
    }
}
