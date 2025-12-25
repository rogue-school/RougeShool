using UnityEngine;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// CoreSystem 매니저들의 공통 베이스 클래스
    /// 핵심 기능만 제공하고 각 매니저의 특화된 역할을 보장합니다.
    /// </summary>
    public abstract class BaseCoreManager<T> : MonoBehaviour, ICoreSystemInitializable 
        where T : class
    {
        #region 핵심 설정 (모든 매니저 공통)

        [Header("핵심 매니저 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] protected bool enableDebugLogging = true;

        [Tooltip("자동 초기화 활성화")]
        [SerializeField] protected bool autoInitialize = true;

        [Tooltip("씬 전환 시 유지 여부")]
        [SerializeField] protected bool persistAcrossScenes = true;

        [Tooltip("필수 참조가 없어도 초기화 진행")]
        [SerializeField] protected bool initializeWithoutRequiredReferences = true;

        #endregion

        #region 매니저별 특화 설정 (하위 클래스에서 오버라이드)

        // 특화된 설정 메서드들은 ValidateReferences 섹션에서 정의됩니다.

        #endregion

        #region 초기화 상태

        /// <summary>
        /// 매니저가 초기화되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsInitialized { get; protected set; } = false;

        #endregion

        #region 공통 초기화

        protected virtual void Awake()
        {
            if (persistAcrossScenes && transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else if (persistAcrossScenes)
            {
                // 부모가 있으면 부모가 DontDestroyOnLoad되면 자동으로 함께 유지됨
                // 경고를 Info로 변경 (실제 문제는 아님)
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo($"{GetType().Name}: 부모 오브젝트가 DontDestroyOnLoad되면 함께 유지됩니다.", GameLogger.LogCategory.UI);
                }
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 시작", GameLogger.LogCategory.UI);
            }
        }

        protected virtual void Start()
        {
            if (autoInitialize)
            {
                StartCoroutine(Initialize());
            }
        }

        #endregion

        #region ICoreSystemInitializable 구현

        /// <summary>
        /// 시스템을 초기화합니다
        /// </summary>
        /// <returns>초기화 코루틴</returns>
        public virtual System.Collections.IEnumerator Initialize()
        {
            if (IsInitialized)
            {
                yield break;
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 중...", GameLogger.LogCategory.UI);
            }

            // 서브클래스에서 구현할 초기화 로직
            yield return StartCoroutine(OnInitialize());

            IsInitialized = true;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 완료", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 초기화 실패 시 호출됩니다
        /// </summary>
        public virtual void OnInitializationFailed()
        {
            GameLogger.LogError($"{GetType().Name} 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }

        #endregion

        #region 추상 메서드

        /// <summary>
        /// 서브클래스에서 구현할 초기화 로직
        /// </summary>
        protected abstract System.Collections.IEnumerator OnInitialize();

        /// <summary>
        /// 매니저 리셋 로직
        /// </summary>
        public abstract void Reset();

        #endregion

        #region 공통 유틸리티

        /// <summary>
        /// 필수 참조 필드의 유효성을 검사합니다. (하위 클래스에서 특화된 검사 추가 가능)
        /// </summary>
        protected virtual bool ValidateReferences()
        {
            bool isValid = true;
            
            // 매니저별 특화된 설정 검사
            var managerConfig = GetManagerConfig();
            if (managerConfig == null && RequiresManagerConfig())
            {
                GameLogger.LogWarning($"{GetType().Name}: 매니저 설정 데이터가 필요하지만 할당되지 않았습니다.", GameLogger.LogCategory.Core);
                if (!initializeWithoutRequiredReferences)
                {
                    isValid = false;
                }
            }
            
            // 매니저별 특화된 프리팹 검사
            var relatedPrefab = GetRelatedPrefab();
            if (relatedPrefab == null && RequiresRelatedPrefab())
            {
                GameLogger.LogWarning($"{GetType().Name}: 관련 프리팹이 필요하지만 할당되지 않았습니다.", GameLogger.LogCategory.Core);
                if (!initializeWithoutRequiredReferences)
                {
                    isValid = false;
                }
            }
            
            // 매니저별 특화된 UI 컨트롤러 검사
            var uiController = GetUIController();
            if (uiController == null && RequiresUIController())
            {
                GameLogger.LogWarning($"{GetType().Name}: UI 컨트롤러가 필요하지만 할당되지 않았습니다.", GameLogger.LogCategory.Core);
                if (!initializeWithoutRequiredReferences)
                {
                    isValid = false;
                }
            }
            
            return isValid;
        }

        /// <summary>
        /// 매니저 설정이 필요한지 확인 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual bool RequiresManagerConfig() => false;

        /// <summary>
        /// 관련 프리팹이 필요한지 확인 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual bool RequiresRelatedPrefab() => false;

        /// <summary>
        /// UI 컨트롤러가 필요한지 확인 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual bool RequiresUIController() => false;

        /// <summary>
        /// 매니저별 특화된 설정 데이터를 반환 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual ScriptableObject GetManagerConfig() => null;

        /// <summary>
        /// 매니저별 특화된 프리팹을 반환 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual GameObject GetRelatedPrefab() => null;

        /// <summary>
        /// 매니저별 특화된 UI 컨트롤러를 반환 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual MonoBehaviour GetUIController() => null;

        /// <summary>
        /// 매니저별 특화된 UI 컨테이너를 반환 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual Transform GetUIContainer() => null;

        /// <summary>
        /// UI 컨트롤러를 연결합니다.
        /// </summary>
        protected virtual void ConnectUI()
        {
            var uiController = GetUIController();
            if (uiController != null)
            {
                GameLogger.LogInfo($"{GetType().Name}: UI 컨트롤러 연결 - {uiController.GetType().Name}", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 매니저 상태를 로깅합니다.
        /// </summary>
        protected virtual void LogManagerState()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 상태: 초기화={IsInitialized}, 디버그={enableDebugLogging}, 자동초기화={autoInitialize}", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region Unity 생명주기

        protected virtual void OnDestroy()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 파괴됨", GameLogger.LogCategory.UI);
            }
        }

        #endregion
    }
}
