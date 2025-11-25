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
        
        [Header("UI 참조")]
        [Tooltip("CoreScene에서 사용하는 VictoryUI (선택)")]
        [SerializeField] private Game.CombatSystem.UI.VictoryUI victoryUI;
        [Tooltip("자동 바인딩에 사용할 메인 Canvas (선택)")]
        [SerializeField] private Canvas mainCanvas;
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
                if (victoryUI != null)
                {
                    victoryUI.Hide();
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
                GameLogger.LogInfo("초기화할 추가 시스템이 없습니다. 모든 코어 시스템이 이미 초기화되었습니다.", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// DOTween Pro를 최적화된 설정으로 초기화합니다.
        /// </summary>
        private System.Collections.IEnumerator InitializeDOTweenPro()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("DOTween Pro 초기화 시작", GameLogger.LogCategory.UI);
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
                GameLogger.LogInfo("DOTween Pro 초기화 완료 - TextMeshPro 지원 활성화됨", GameLogger.LogCategory.UI);
            }
            
            yield return null;
        }

        private System.Collections.IEnumerator InitializeAllSystems()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CoreSystem 자동 초기화 시작", GameLogger.LogCategory.UI);
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
                        GameLogger.LogInfo($"필수 시스템 포함: {systemName}", GameLogger.LogCategory.UI);
                    }
                }
                else
                {
                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo($"불필요한 시스템 제외: {systemName}", GameLogger.LogCategory.UI);
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
            Canvas canvas = mainCanvas;
            if (canvas != null)
            {
                BindCanvasComponents(canvas);
            }
            else if (enableDebugLogging)
            {
                GameLogger.LogWarning("[CoreSystemInitializer] mainCanvas가 설정되지 않았습니다. 자동 바인딩을 건너뜁니다.", GameLogger.LogCategory.UI);
            }
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("자동 바인딩 완료", GameLogger.LogCategory.UI);
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
            var task = sceneTransitionManager.TransitionToMainScene();
            while (!task.IsCompleted)
            {
                yield return null;
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