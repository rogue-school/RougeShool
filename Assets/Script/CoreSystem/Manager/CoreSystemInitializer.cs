using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Save;
using Game.CoreSystem.Interface;
using Zenject;
using DG.Tweening;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// CoreScene의 모든 시스템을 자동으로 초기화하는 중앙 관리자 (Zenject DI 기반)
    /// </summary>
    public class CoreSystemInitializer : BaseCoreManager<ICoreSystemInitializable>
    {
        // 의존성 주입
        private List<ICoreSystemInitializable> coreSystems;
        private ISceneTransitionManager sceneTransitionManager;
        
        [Inject(Optional = true)]
        private Game.CombatSystem.UI.VictoryUI victoryUI;
        
        [Inject]
        public void Construct(List<ICoreSystemInitializable> coreSystems, ISceneTransitionManager sceneTransitionManager)
        {
            this.coreSystems = coreSystems;
            this.sceneTransitionManager = sceneTransitionManager;
        }
        
        protected override void Awake()
        {
            base.Awake();
            // 베이스 클래스에서 기본 초기화 처리
        }

        #region CoreSystem 전용 설정
        [Header("CoreSystem 전용 설정")]
        [Tooltip("시스템별 상세 시작/완료 로그")]
        [SerializeField] private bool logPerSystemDetails = false;
        
        [Tooltip("초기화 완료 후 자동으로 메인 씬으로 이동")]
        [SerializeField] private bool autoGoToMainAfterInit = true;
        
        [Tooltip("필수 시스템만 초기화 (불필요한 시스템 제외)")]
        [SerializeField] private bool initializeEssentialSystemsOnly = true;
        #endregion

        #region 초기화 상태
        [Header("초기화 상태")]
        [SerializeField] private bool isInitializationComplete = false;
        
        public bool IsInitializationComplete => isInitializationComplete;
        
        // 이벤트
        public System.Action OnAllSystemsInitialized;
        public System.Action<string> OnSystemInitialized;
        public new System.Action<string> OnInitializationFailed;
        #endregion

        #region 초기화 시작
        protected override void Start()
        {
            base.Start();
            // 베이스 클래스에서 자동 초기화 처리
            
            // CoreScene 로드 시 VictoryUI 패널 숨기기
            try
            {
                var vui = victoryUI ?? FindFirstObjectByType<Game.CombatSystem.UI.VictoryUI>(FindObjectsInactive.Include);
                if (vui != null)
                {
                    vui.Hide();
                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo("[CoreSystemInitializer] CoreScene 로드 시 VictoryUI 패널 숨김", GameLogger.LogCategory.UI);
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CoreSystemInitializer] VictoryUI 숨김 중 경고: {ex.Message}", GameLogger.LogCategory.UI);
            }
        }

        protected override System.Collections.IEnumerator OnInitialize()
        {
            // 1. DOTween Pro 초기화 (최우선)
            yield return StartCoroutine(InitializeDOTweenPro());
            
            // 2. 코어 시스템 초기화
            if (coreSystems != null && coreSystems.Count > 0)
            {
                yield return StartCoroutine(InitializeAllSystems());
            }
            else
            {
                // coreSystems가 null이거나 비어있어도 초기화 완료 처리
                if (enableDebugLogging)
                {
                    GameLogger.LogWarning("[CoreSystemInitializer] coreSystems가 null이거나 비어있습니다. 초기화를 완료합니다.", GameLogger.LogCategory.Core);
                }
                CompleteInitialization();
            }
        }
        
        /// <summary>
        /// DOTween Pro를 최적화된 설정으로 초기화합니다.
        /// </summary>
        private System.Collections.IEnumerator InitializeDOTweenPro()
        {
            if (enableDebugLogging)
            {
            }
            
            // DOTween 초기화
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
            DOTween.SetTweensCapacity(500, 100); // 대규모 프로젝트 최적화
            
            // 안전성 설정
            DOTween.useSafeMode = true;
            
            // 성능 설정
            DOTween.defaultEaseType = Ease.OutQuad;
            DOTween.defaultAutoKill = true;
            DOTween.defaultRecyclable = false;
            
            // TextMeshPro 모듈 활성화 확인
            if (enableDebugLogging)
            {
            }
            
            yield return null;
        }

        private System.Collections.IEnumerator InitializeAllSystems()
        {
            if (enableDebugLogging)
            {
            }
            
            // 필수 시스템만 초기화하는 경우 필터링
            var systemsToInitialize = initializeEssentialSystemsOnly 
                ? FilterEssentialSystems(coreSystems)
                : coreSystems;
            
            // Zenject DI로 주입된 시스템들을 순차적으로 초기화
            foreach (var system in systemsToInitialize)
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
            }
            
            System.Collections.IEnumerator initCoroutine = system.Initialize();
            
            while (initCoroutine.MoveNext())
            {
                yield return initCoroutine.Current;
            }
            
            // 초기화 완료 후 성공 처리
            if (logPerSystemDetails)
            {
            }
            
            OnSystemInitialized?.Invoke(systemName);
        }
        #endregion

        #region 필수 시스템 필터링
        
        /// <summary>
        /// 현재 게임에 필수적인 시스템들만 필터링합니다.
        /// </summary>
        private List<ICoreSystemInitializable> FilterEssentialSystems(List<ICoreSystemInitializable> allSystems)
        {
            var essentialSystems = new List<ICoreSystemInitializable>();
            
            foreach (var system in allSystems)
            {
                var systemType = system.GetType();
                var systemName = systemType.Name;
                
                // 필수 시스템들만 포함
                if (IsEssentialSystem(systemName))
                {
                    essentialSystems.Add(system);
                    if (enableDebugLogging)
                    {
                    }
                }
                else
                {
                    if (enableDebugLogging)
                    {
                    }
                }
            }
            
            return essentialSystems;
        }
        
        /// <summary>
        /// 시스템이 필수적인지 판단합니다.
        /// </summary>
        private bool IsEssentialSystem(string systemName)
        {
            // 현재 게임에 필수적인 시스템들
            var essentialSystemNames = new HashSet<string>
            {
                "AudioManager",
                "SaveManager", 
                "PlayerCharacterSelectionManager",
                "CoreSystemInitializer"
            };
            
            return essentialSystemNames.Contains(systemName);
        }
        
        #endregion

        #region 초기화 완료
        private void CompleteInitialization()
        {
            isInitializationComplete = true;
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("[CoreSystemInitializer] 모든 시스템 초기화 완료", GameLogger.LogCategory.Core);
            }
            
            OnAllSystemsInitialized?.Invoke();

            if (autoGoToMainAfterInit)
            {
                if (sceneTransitionManager != null)
                {
                    // 코어 초기화 완료 후 메인 씬으로 자동 전환(옵션)
                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo("[CoreSystemInitializer] 메인 씬으로 자동 전환 시작", GameLogger.LogCategory.Core);
                    }
                    StartCoroutine(GoToMainSceneNextFrame());
                }
                else
                {
                    GameLogger.LogError("[CoreSystemInitializer] SceneTransitionManager가 null입니다. 메인 씬으로 전환할 수 없습니다!", GameLogger.LogCategory.Error);
                }
            }
            else
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("[CoreSystemInitializer] 자동 메인 씬 전환 비활성화됨", GameLogger.LogCategory.Core);
                }
            }
        }
        #endregion

        #region 자동 바인딩 (간소화됨)
        private System.Collections.IEnumerator PerformAutoBinding()
        {
            if (enableDebugLogging)
            {
            }
            
            // Canvas 자동 바인딩
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                BindCanvasComponents(canvas);
            }
            
            if (enableDebugLogging)
            {
            }
            
            yield return null;
        }
        
        private void BindCanvasComponents(Canvas canvas)
        {
            // 현재 바인딩할 항목 없음
        }

        private System.Collections.IEnumerator GoToMainSceneNextFrame()
        {
            // 한 프레임 대기 후 전환(씬 로딩 안정화)
            yield return null;
            
            if (sceneTransitionManager == null)
            {
                GameLogger.LogError("[CoreSystemInitializer] SceneTransitionManager가 null입니다. 메인 씬으로 전환할 수 없습니다!", GameLogger.LogCategory.Error);
                yield break;
            }
            
            // 코루틴에서는 try-catch 내부에서 yield를 사용할 수 없으므로
            // Task를 시작하고 완료를 기다림
            var task = sceneTransitionManager.TransitionToMainScene();
            
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            // 완료 후 결과 확인
            if (task.IsFaulted)
            {
                GameLogger.LogError($"[CoreSystemInitializer] 메인 씬 전환 실패: {task.Exception?.GetBaseException()?.Message}", GameLogger.LogCategory.Error);
            }
            else if (task.IsCanceled)
            {
                GameLogger.LogWarning("[CoreSystemInitializer] 메인 씬 전환이 취소되었습니다.", GameLogger.LogCategory.Core);
            }
            else if (enableDebugLogging)
            {
                GameLogger.LogInfo("[CoreSystemInitializer] 메인 씬 전환 완료", GameLogger.LogCategory.Core);
            }
        }
        #endregion

        #region Reset 구현

        public override void Reset()
        {
            isInitializationComplete = false;
            IsInitialized = false;
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CoreSystemInitializer 리셋 완료", GameLogger.LogCategory.UI);
            }
        }

        #endregion
    }
}