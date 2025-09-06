using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using Game.UtilitySystem.GameFlow;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// 3개 씬 구조를 위한 씬 전환 매니저
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour, ISceneLoader, ICoreSystemInitializable
    {
        public static SceneTransitionManager Instance { get; private set; }
        
        [Header("씬 설정")]
        [SerializeField] private string coreSceneName = "CoreScene";
        [SerializeField] private string mainSceneName = "MainScene";
        [SerializeField] private string battleSceneName = "BattleScene";
        
        [Header("전환 설정")]
        [SerializeField] private float transitionDuration = 1f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("UI 참조")]
        [SerializeField] private CanvasGroup transitionCanvas;
        [SerializeField] private UnityEngine.UI.Image transitionImage;
        
        // 초기화 상태
        public bool IsInitialized { get; private set; } = false;
        [SerializeField] private UnityEngine.UI.Text loadingText;
        
        // 이벤트
        public System.Action<string> OnSceneTransitionStart;
        public System.Action<string> OnSceneTransitionEnd;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTransition();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 전환 시스템 초기화
        /// </summary>
        private void InitializeTransition()
        {
            // 전환 캔버스 설정
            if (transitionCanvas == null)
            {
                var canvasObj = new GameObject("TransitionCanvas");
                canvasObj.transform.SetParent(transform);
                
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000; // 최상위 레이어
                
                transitionCanvas = canvasObj.AddComponent<CanvasGroup>();
                transitionCanvas.alpha = 0f;
                transitionCanvas.interactable = false;
                transitionCanvas.blocksRaycasts = false;
            }
            
            Debug.Log("[SceneTransitionManager] 초기화 완료");
        }
        
        /// <summary>
        /// 코어 씬으로 전환
        /// </summary>
        public async Task TransitionToCoreScene()
        {
            await TransitionToScene(coreSceneName, TransitionType.Fade);
        }
        
        /// <summary>
        /// 메인 씬으로 전환
        /// </summary>
        public async Task TransitionToMainScene()
        {
            await TransitionToScene(mainSceneName, TransitionType.Fade);
        }
        
        /// <summary>
        /// 전투 씬으로 전환
        /// </summary>
        public async Task TransitionToBattleScene()
        {
            await TransitionToScene(battleSceneName, TransitionType.Fade);
        }
        
        /// <summary>
        /// 씬 전환 실행
        /// </summary>
        public async Task TransitionToScene(string sceneName, TransitionType transitionType = TransitionType.Fade)
        {
            Debug.Log($"[SceneTransitionManager] 씬 전환 시작: {sceneName}");
            
            OnSceneTransitionStart?.Invoke(sceneName);
            
            // 전환 시작
            await StartTransition(transitionType);
            
            // 씬 로딩
            await LoadSceneAsync(sceneName);
            
            // 전환 종료
            await EndTransition(transitionType);
            
            OnSceneTransitionEnd?.Invoke(sceneName);
            
            Debug.Log($"[SceneTransitionManager] 씬 전환 완료: {sceneName}");
        }
        
        /// <summary>
        /// 비동기 씬 로딩
        /// </summary>
        private async Task LoadSceneAsync(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            
            while (operation.progress < 0.9f)
            {
                // 로딩 진행률 표시
                float progress = operation.progress / 0.9f;
                UpdateLoadingProgress(progress);
                
                await Task.Yield();
            }
            
            // 씬 활성화
            operation.allowSceneActivation = true;
            
            // 씬 로딩 완료 대기
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
        
        /// <summary>
        /// 로딩 진행률 업데이트
        /// </summary>
        private void UpdateLoadingProgress(float progress)
        {
            if (loadingText != null)
            {
                loadingText.text = $"로딩 중... {Mathf.RoundToInt(progress * 100)}%";
            }
        }
        
        /// <summary>
        /// 전환 시작
        /// </summary>
        private async Task StartTransition(TransitionType transitionType)
        {
            if (transitionCanvas != null)
            {
                transitionCanvas.gameObject.SetActive(true);
                transitionCanvas.blocksRaycasts = true;
            }
            
            switch (transitionType)
            {
                case TransitionType.Fade:
                    await FadeTransition(true);
                    break;
                case TransitionType.Slide:
                    await SlideTransition(true);
                    break;
            }
        }
        
        /// <summary>
        /// 전환 종료
        /// </summary>
        private async Task EndTransition(TransitionType transitionType)
        {
            switch (transitionType)
            {
                case TransitionType.Fade:
                    await FadeTransition(false);
                    break;
                case TransitionType.Slide:
                    await SlideTransition(false);
                    break;
            }
            
            if (transitionCanvas != null)
            {
                transitionCanvas.gameObject.SetActive(false);
                transitionCanvas.blocksRaycasts = false;
            }
        }
        
        /// <summary>
        /// 페이드 전환
        /// </summary>
        private async Task FadeTransition(bool fadeIn)
        {
            if (transitionCanvas == null) return;
            
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            
            transitionCanvas.alpha = startAlpha;
            
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / transitionDuration;
                float curveValue = transitionCurve.Evaluate(progress);
                
                transitionCanvas.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                
                await Task.Yield();
            }
            
            transitionCanvas.alpha = endAlpha;
        }
        
        /// <summary>
        /// 슬라이드 전환
        /// </summary>
        private async Task SlideTransition(bool slideIn)
        {
            // 슬라이드 전환 로직 구현 (필요시)
            await Task.Delay(Mathf.RoundToInt(transitionDuration * 1000));
        }
        
        /// <summary>
        /// ISceneLoader 인터페이스 구현
        /// </summary>
        public void LoadScene(string sceneName)
        {
            // 동기적으로 씬 로드 (기존 SceneLoader와 호환성을 위해)
            SceneManager.LoadScene(sceneName);
        }
        
        #region ICoreSystemInitializable 구현
        /// <summary>
        /// 시스템 초기화 수행
        /// </summary>
        public IEnumerator Initialize()
        {
            GameLogger.LogInfo("SceneTransitionManager 초기화 시작", GameLogger.LogCategory.UI);
            
            // 초기 상태 설정
            if (transitionCanvas != null)
            {
                transitionCanvas.alpha = 0f;
                transitionCanvas.interactable = false;
                transitionCanvas.blocksRaycasts = false;
            }
            
            // 초기화 완료
            IsInitialized = true;
            
            GameLogger.LogInfo("SceneTransitionManager 초기화 완료", GameLogger.LogCategory.UI);
            yield return null;
        }
        
        /// <summary>
        /// 초기화 실패 시 호출
        /// </summary>
        public void OnInitializationFailed()
        {
            GameLogger.LogError("SceneTransitionManager 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }
        #endregion
    }
    
    /// <summary>
    /// 전환 타입 열거형
    /// </summary>
    public enum TransitionType
    {
        Fade,   // 페이드
        Slide   // 슬라이드
    }
}
