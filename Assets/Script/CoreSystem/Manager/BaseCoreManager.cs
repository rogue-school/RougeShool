using UnityEngine;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// CoreSystem 매니저들의 공통 베이스 클래스
    /// 인스펙터 필드 표준화 및 공통 기능을 제공합니다.
    /// </summary>
    public abstract class BaseCoreManager<T> : MonoBehaviour, ICoreSystemInitializable 
        where T : class
    {
        #region 기본 설정

        [Header("매니저 기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] protected bool enableDebugLogging = true;

        [Tooltip("자동 초기화 활성화")]
        [SerializeField] protected bool autoInitialize = true;

        [Tooltip("씬 전환 시 유지 여부")]
        [SerializeField] protected bool persistAcrossScenes = true;

        #endregion

        #region 데이터 및 설정

        [Header("데이터 및 설정")]
        [Tooltip("매니저 설정 데이터")]
        [SerializeField] protected ScriptableObject managerConfig;

        [Tooltip("관련 프리팹")]
        [SerializeField] protected GameObject relatedPrefab;

        #endregion

        #region UI 연결

        [Header("UI 연결")]
        [Tooltip("관련 UI 컨트롤러")]
        [SerializeField] protected MonoBehaviour uiController;

        [Tooltip("UI 패널 컨테이너")]
        [SerializeField] protected Transform uiContainer;

        #endregion

        #region 의존성 및 서비스

        [Header("의존성 및 서비스")]
        [Tooltip("의존성 매니저")]
        [SerializeField] protected MonoBehaviour dependencyManager;

        [Tooltip("서비스 컨테이너")]
        [SerializeField] protected Transform serviceContainer;

        #endregion

        #region 초기화 상태

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
                GameLogger.LogWarning("루트 오브젝트가 아니므로 DontDestroyOnLoad를 적용할 수 없습니다.", GameLogger.LogCategory.UI);
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
        /// 필수 참조 필드의 유효성을 검사합니다.
        /// </summary>
        protected virtual bool ValidateReferences()
        {
            bool isValid = true;

            if (managerConfig == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 매니저 설정 데이터가 할당되지 않았습니다.", GameLogger.LogCategory.UI);
            }

            if (relatedPrefab == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 관련 프리팹이 할당되지 않았습니다.", GameLogger.LogCategory.UI);
            }

            if (uiController == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: UI 컨트롤러가 할당되지 않았습니다.", GameLogger.LogCategory.UI);
            }

            return isValid;
        }

        /// <summary>
        /// UI 컨트롤러를 연결합니다.
        /// </summary>
        protected virtual void ConnectUI()
        {
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
