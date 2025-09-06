using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.CoreSystem.Utility;
using Game.CoreSystem.UI;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Save;
using Game.CoreSystem.Animation;
using Game.CoreSystem.Interface;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// CoreScene의 모든 시스템을 자동으로 초기화하는 중앙 관리자
    /// </summary>
    public class CoreSystemInitializer : MonoBehaviour
    {
        #region Singleton
        public static CoreSystemInitializer Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // 디버그 로깅 설정
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("CoreSystemInitializer 초기화 시작", GameLogger.LogCategory.UI);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region 초기화 설정
        [Header("초기화 설정")]
        [SerializeField] private bool enableAutoInitialization = true;
        [SerializeField] private bool enableAutoBinding = true;
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("초기화 순서")]
        [SerializeField] private List<string> initializationOrder = new List<string>
        {
            "CoroutineRunner", 
            "GameStateManager",
            "SceneTransitionManager",
            "AudioManager",
            "SaveManager",
            "AnimationDatabaseManager",
            "AnimationManager",
            "SettingsManager",
            "LoadingScreenController",
            "TransitionEffectController"
        };
        #endregion

        #region 초기화 상태
        private Dictionary<string, bool> initializationStatus = new Dictionary<string, bool>();
        private Dictionary<string, MonoBehaviour> initializedSystems = new Dictionary<string, MonoBehaviour>();
        private bool isInitializationComplete = false;
        #endregion

        #region 이벤트
        public System.Action<string> OnSystemInitialized;
        public System.Action OnAllSystemsInitialized;
        public System.Action<string> OnInitializationFailed;
        #endregion

        #region 초기화 시작
        private void Start()
        {
            if (enableAutoInitialization)
            {
                StartCoroutine(InitializeAllSystems());
            }
        }

        private System.Collections.IEnumerator InitializeAllSystems()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CoreSystem 자동 초기화 시작", GameLogger.LogCategory.UI);
            }
            
            // 1단계: 기본 시스템 초기화
            yield return StartCoroutine(InitializeBasicSystems());
            
            // 2단계: 매니저 시스템 초기화
            yield return StartCoroutine(InitializeManagerSystems());
            
            // 3단계: UI 시스템 초기화
            yield return StartCoroutine(InitializeUISystems());
            
            // 4단계: 자동 바인딩 수행
            if (enableAutoBinding)
            {
                yield return StartCoroutine(PerformAutoBinding());
            }
            
            // 5단계: 초기화 완료
            CompleteInitialization();
        }
        #endregion

        #region 기본 시스템 초기화
        private System.Collections.IEnumerator InitializeBasicSystems()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("기본 시스템 초기화 시작", GameLogger.LogCategory.UI);
            }
            
            // CoroutineRunner 초기화
            yield return StartCoroutine(InitializeSystem<CoroutineRunner>("CoroutineRunner"));
            
            yield return null;
        }
        #endregion

        #region 매니저 시스템 초기화
        private System.Collections.IEnumerator InitializeManagerSystems()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("매니저 시스템 초기화 시작", GameLogger.LogCategory.UI);
            }
            
            // GameStateManager 초기화
            yield return StartCoroutine(InitializeSystem<GameStateManager>("GameStateManager"));
            
            // SceneTransitionManager 초기화
            yield return StartCoroutine(InitializeSystem<SceneTransitionManager>("SceneTransitionManager"));
            
            // AudioManager 초기화
            yield return StartCoroutine(InitializeSystem<AudioManager>("AudioManager"));
            
            // SaveManager 초기화
            yield return StartCoroutine(InitializeSystem<SaveManager>("SaveManager"));
            
            // AnimationDatabaseManager 초기화
            yield return StartCoroutine(InitializeSystem<AnimationDatabaseManager>("AnimationDatabaseManager"));
            
            // AnimationManager 초기화
            yield return StartCoroutine(InitializeSystem<AnimationManager>("AnimationManager"));
            
            yield return null;
        }
        #endregion

        #region UI 시스템 초기화
        private System.Collections.IEnumerator InitializeUISystems()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("UI 시스템 초기화 시작", GameLogger.LogCategory.UI);
            }
            
            // LoadingScreenController 초기화
            yield return StartCoroutine(InitializeSystem<LoadingScreenController>("LoadingScreenController"));
            
            // TransitionEffectController 초기화
            yield return StartCoroutine(InitializeSystem<TransitionEffectController>("TransitionEffectController"));
            
            yield return null;
        }
        #endregion

        #region 시스템 초기화 헬퍼
        private System.Collections.IEnumerator InitializeSystem<T>(string systemName) where T : MonoBehaviour
        {
            // 이미 초기화된 경우 스킵
            if (initializationStatus.ContainsKey(systemName) && initializationStatus[systemName])
            {
                yield break;
            }
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{systemName} 초기화 시작", GameLogger.LogCategory.UI);
            }
            
            // 시스템 오브젝트 찾기 또는 생성
            T systemInstance = FindOrCreateSystem<T>(systemName);
            
            if (systemInstance != null)
            {
                // 시스템 초기화 인터페이스 호출
                if (systemInstance is ICoreSystemInitializable initializable)
                {
                    yield return StartCoroutine(initializable.Initialize());
                }
                
                // 초기화 완료 표시
                MarkSystemAsInitialized(systemName, systemInstance);
                OnSystemInitialized?.Invoke(systemName);
                
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo($"{systemName} 초기화 완료", GameLogger.LogCategory.UI);
                }
            }
            else
            {
                GameLogger.LogError($"{systemName} 초기화 실패: 시스템 인스턴스를 찾거나 생성할 수 없습니다", GameLogger.LogCategory.Error);
                OnInitializationFailed?.Invoke(systemName);
            }
            
            yield return null;
        }
        #endregion

        #region 시스템 오브젝트 관리
        private T FindOrCreateSystem<T>(string systemName) where T : MonoBehaviour
        {
            // 기존 인스턴스 찾기
            T existingInstance = FindFirstObjectByType<T>();
            if (existingInstance != null)
            {
                return existingInstance;
            }
            
            // CoreSystem 폴더에서 해당 이름의 오브젝트 찾기
            GameObject systemObject = FindSystemObject(systemName);
            if (systemObject != null)
            {
                T component = systemObject.GetComponent<T>();
                if (component == null)
                {
                    component = systemObject.AddComponent<T>();
                }
                return component;
            }
            
            // 오브젝트가 없으면 생성
            GameObject newObject = new GameObject(systemName);
            newObject.transform.SetParent(transform);
            return newObject.AddComponent<T>();
        }
        
        private GameObject FindSystemObject(string systemName)
        {
            // CoreSystem 하위에서 해당 이름의 오브젝트 찾기
            Transform[] children = GetComponentsInChildren<Transform>();
            return children.FirstOrDefault(child => child.name == systemName)?.gameObject;
        }
        #endregion

        #region 자동 바인딩
        private System.Collections.IEnumerator PerformAutoBinding()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("자동 바인딩 시작", GameLogger.LogCategory.UI);
            }
            
            // AudioManager 바인딩
            yield return StartCoroutine(BindAudioManager());
            
            // UI 컨트롤러 바인딩
            yield return StartCoroutine(BindUIControllers());
            
            // SceneTransitionManager 바인딩
            yield return StartCoroutine(BindSceneTransitionManager());
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("자동 바인딩 완료", GameLogger.LogCategory.UI);
            }
        }
        
        private System.Collections.IEnumerator BindAudioManager()
        {
            AudioManager audioManager = GetSystem<AudioManager>("AudioManager");
            if (audioManager != null)
            {
                // AudioSource 자동 연결
                AudioSource bgmSource = FindAudioSource("BGMSource");
                AudioSource sfxSource = FindAudioSource("SFXSource");
                
                if (bgmSource != null && sfxSource != null)
                {
                    // 리플렉션을 사용하여 private 필드 설정
                    SetPrivateField(audioManager, "bgmSource", bgmSource);
                    SetPrivateField(audioManager, "sfxSource", sfxSource);
                    
                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo("AudioManager 바인딩 완료", GameLogger.LogCategory.Audio);
                    }
                }
            }
            yield return null;
        }
        
        private System.Collections.IEnumerator BindUIControllers()
        {
            // LoadingScreenController 바인딩
            LoadingScreenController loadingController = GetSystem<LoadingScreenController>("LoadingScreenController");
            if (loadingController != null)
            {
                BindLoadingScreenController(loadingController);
            }
            
            // TransitionEffectController 바인딩
            TransitionEffectController transitionController = GetSystem<TransitionEffectController>("TransitionEffectController");
            if (transitionController != null)
            {
                BindTransitionEffectController(transitionController);
            }
            
            yield return null;
        }
        
        private System.Collections.IEnumerator BindSceneTransitionManager()
        {
            SceneTransitionManager sceneManager = GetSystem<SceneTransitionManager>("SceneTransitionManager");
            if (sceneManager != null)
            {
                // CanvasGroup 자동 연결 (Canvas에서 CanvasGroup 찾기)
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                    }
                    
                    SetPrivateField(sceneManager, "transitionCanvas", canvasGroup);
                    
                    // TransitionImage도 바인딩
                    UnityEngine.UI.Image transitionImage = canvas.GetComponentInChildren<UnityEngine.UI.Image>();
                    if (transitionImage != null)
                    {
                        SetPrivateField(sceneManager, "transitionImage", transitionImage);
                    }
                    
                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo("SceneTransitionManager 바인딩 완료", GameLogger.LogCategory.UI);
                    }
                }
            }
            yield return null;
        }
        #endregion

        #region 바인딩 헬퍼 메서드
        private void BindLoadingScreenController(LoadingScreenController controller)
        {
            // LoadingPanel 찾기
            GameObject loadingPanel = FindUIElement("LoadingPanel");
            if (loadingPanel != null)
            {
                SetPrivateField(controller, "loadingPanel", loadingPanel);
                
                // 하위 UI 요소들 바인딩
                Slider progressBar = loadingPanel.GetComponentInChildren<Slider>();
                UnityEngine.UI.Text progressText = loadingPanel.GetComponentInChildren<UnityEngine.UI.Text>();
                
                if (progressBar != null) SetPrivateField(controller, "progressBar", progressBar);
                if (progressText != null) SetPrivateField(controller, "progressText", progressText);
                
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("LoadingScreenController 바인딩 완료", GameLogger.LogCategory.UI);
                }
            }
        }
        
        private void BindTransitionEffectController(TransitionEffectController controller)
        {
            // TransitionPanel 찾기
            GameObject transitionPanel = FindUIElement("TransitionPanel");
            if (transitionPanel != null)
            {
                CanvasGroup fadeCanvas = transitionPanel.GetComponent<CanvasGroup>();
                UnityEngine.UI.Image fadeImage = transitionPanel.GetComponentInChildren<UnityEngine.UI.Image>();
                
                if (fadeCanvas != null) SetPrivateField(controller, "fadeCanvas", fadeCanvas);
                if (fadeImage != null) SetPrivateField(controller, "fadeImage", fadeImage);
                
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("TransitionEffectController 바인딩 완료", GameLogger.LogCategory.UI);
                }
            }
        }
        
        private AudioSource FindAudioSource(string sourceName)
        {
            Transform audioSourcesParent = transform.Find("AudioSources");
            if (audioSourcesParent != null)
            {
                Transform sourceTransform = audioSourcesParent.Find(sourceName);
                return sourceTransform?.GetComponent<AudioSource>();
            }
            return null;
        }
        
        private GameObject FindUIElement(string elementName)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform elementTransform = canvas.transform.Find(elementName);
                return elementTransform?.gameObject;
            }
            return null;
        }
        
        private void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
        #endregion

        #region 시스템 관리
        private T GetSystem<T>(string systemName) where T : MonoBehaviour
        {
            if (initializedSystems.ContainsKey(systemName))
            {
                return initializedSystems[systemName] as T;
            }
            return null;
        }
        
        private void MarkSystemAsInitialized(string systemName, MonoBehaviour systemInstance = null)
        {
            initializationStatus[systemName] = true;
            if (systemInstance != null)
            {
                initializedSystems[systemName] = systemInstance;
            }
        }
        
        private void CompleteInitialization()
        {
            isInitializationComplete = true;
            OnAllSystemsInitialized?.Invoke();
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CoreSystem 자동 초기화 완료", GameLogger.LogCategory.UI);
            }
            
            // CoreScene 초기화 완료 후 자동으로 MainScene으로 이동
            StartCoroutine(AutoTransitionToMainScene());
        }
        
        private System.Collections.IEnumerator AutoTransitionToMainScene()
        {
            // 잠시 대기 (UI 업데이트를 위해)
            yield return new WaitForSeconds(0.5f);
            
            // SceneTransitionManager를 통해 MainScene으로 이동
            if (SceneTransitionManager.Instance != null)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("CoreScene 초기화 완료 - MainScene으로 자동 이동", GameLogger.LogCategory.UI);
                }
                
                // async Task를 IEnumerator로 변환
                yield return StartCoroutine(ConvertTaskToCoroutine(SceneTransitionManager.Instance.TransitionToMainScene()));
            }
            else
            {
                GameLogger.LogError("SceneTransitionManager를 찾을 수 없습니다", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// Task를 IEnumerator로 변환하는 헬퍼 메서드
        /// </summary>
        private System.Collections.IEnumerator ConvertTaskToCoroutine(System.Threading.Tasks.Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.IsFaulted)
            {
                GameLogger.LogError($"씬 전환 중 오류 발생: {task.Exception}", GameLogger.LogCategory.Error);
            }
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 특정 시스템이 초기화되었는지 확인
        /// </summary>
        public bool IsSystemInitialized(string systemName)
        {
            return initializationStatus.ContainsKey(systemName) && initializationStatus[systemName];
        }
        
        /// <summary>
        /// 모든 시스템이 초기화되었는지 확인
        /// </summary>
        public bool IsAllSystemsInitialized()
        {
            return isInitializationComplete;
        }
        
        /// <summary>
        /// 초기화된 시스템 인스턴스 가져오기
        /// </summary>
        public T GetInitializedSystem<T>(string systemName) where T : MonoBehaviour
        {
            return GetSystem<T>(systemName);
        }
        #endregion
    }
}
