using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.CoreSystem.Utility;
using Game.CoreSystem.UI;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Save;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// CoreScene의 모든 시스템을 자동으로 초기화하는 중앙 관리자 (Zenject DI 기반)
    /// </summary>
    public class CoreSystemInitializer : MonoBehaviour
    {
        // 의존성 주입
        private List<ICoreSystemInitializable> coreSystems;
        private ISceneTransitionManager sceneTransitionManager;
        
        [Inject]
        public void Construct(List<ICoreSystemInitializable> coreSystems, ISceneTransitionManager sceneTransitionManager)
        {
            this.coreSystems = coreSystems;
            this.sceneTransitionManager = sceneTransitionManager;
        }
        
        private void Awake()
        {
            // 디버그 로깅 설정
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CoreSystemInitializer 초기화 시작", GameLogger.LogCategory.UI);
            }
        }

        #region 초기화 설정
        [Header("초기화 설정")]
        [SerializeField] private bool enableAutoInitialization = true;
        [SerializeField] private bool enableDebugLogging = true; // 상단(전체) 진행 로그
        [SerializeField] private bool logPerSystemDetails = false; // 시스템별 상세 시작/완료 로그
        [SerializeField] private bool autoGoToMainAfterInit = true;
        #endregion

        #region 초기화 상태
        [Header("초기화 상태")]
        [SerializeField] private bool isInitializationComplete = false;
        
        public bool IsInitializationComplete => isInitializationComplete;
        
        // 이벤트
        public System.Action OnAllSystemsInitialized;
        public System.Action<string> OnSystemInitialized;
        public System.Action<string> OnInitializationFailed;
        #endregion

        #region 초기화 시작
        private void Start()
        {
            if (enableAutoInitialization && coreSystems != null)
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
            
            // Zenject DI로 주입된 시스템들을 순차적으로 초기화
            foreach (var system in coreSystems)
            {
                if (system != null)
                {
                    yield return StartCoroutine(InitializeSystem(system, system.GetType().Name));
                }
            }
            
            // 초기화 완료
            CompleteInitialization();
        }
        #endregion

        #region 시스템 초기화 (Zenject DI 기반)
        private System.Collections.IEnumerator InitializeSystem(ICoreSystemInitializable system, string systemName)
        {
            if (logPerSystemDetails)
            {
                GameLogger.LogInfo($"{systemName} 초기화 시작", GameLogger.LogCategory.UI);
            }
            
            System.Collections.IEnumerator initCoroutine = system.Initialize();
            
            while (initCoroutine.MoveNext())
            {
                yield return initCoroutine.Current;
            }
            
            // 초기화 완료 후 성공 처리
            if (logPerSystemDetails)
            {
                GameLogger.LogInfo($"{systemName} 초기화 완료", GameLogger.LogCategory.UI);
            }
            
            OnSystemInitialized?.Invoke(systemName);
        }
        #endregion

        #region 초기화 완료
        private void CompleteInitialization()
        {
            isInitializationComplete = true;
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CoreSystem 초기화 완료", GameLogger.LogCategory.UI);
            }
            
            OnAllSystemsInitialized?.Invoke();

            if (autoGoToMainAfterInit && sceneTransitionManager != null)
            {
                // 코어 초기화 완료 후 메인 씬으로 자동 전환(옵션)
                StartCoroutine(GoToMainSceneNextFrame());
            }
        }
        #endregion

        #region 자동 바인딩 (간소화됨)
        private System.Collections.IEnumerator PerformAutoBinding()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("자동 바인딩 시작", GameLogger.LogCategory.UI);
            }
            
            // Canvas 자동 바인딩
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                BindCanvasComponents(canvas);
            }
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("자동 바인딩 완료", GameLogger.LogCategory.UI);
            }
            
            yield return null;
        }
        
        private void BindCanvasComponents(Canvas canvas)
        {
            // LoadingScreenController 바인딩
            var loadingController = canvas.GetComponentInChildren<LoadingScreenController>();
            if (loadingController != null)
            {
                // 필요한 바인딩 로직
            }
        }

        private System.Collections.IEnumerator GoToMainSceneNextFrame()
        {
            // 한 프레임 대기 후 전환(씬 로딩 안정화)
            yield return null;
            var task = sceneTransitionManager.TransitionToMainScene();
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }
        #endregion
    }
}