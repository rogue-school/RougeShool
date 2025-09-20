using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// SkillCardSystem 매니저들의 공통 베이스 클래스
    /// 인스펙터 필드 표준화 및 공통 기능을 제공합니다.
    /// </summary>
    public abstract class BaseSkillCardManager<T> : MonoBehaviour 
        where T : class
    {
        #region 기본 설정

        [Header("매니저 기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] protected bool enableDebugLogging = true;

        [Tooltip("자동 초기화 활성화")]
        [SerializeField] protected bool autoInitialize = true;

        [Tooltip("씬 전환 시 유지 여부")]
        [SerializeField] protected bool persistAcrossScenes = false;

        #endregion

        #region 카드 데이터 및 설정

        [Header("카드 데이터 및 설정")]
        [Tooltip("카드 설정 데이터")]
        [SerializeField] protected ScriptableObject cardConfig;

        [Tooltip("카드 프리팹")]
        [SerializeField] protected GameObject cardPrefab;

        [Tooltip("카드 UI 프리팹")]
        [SerializeField] protected GameObject cardUIPrefab;

        #endregion

        #region 덱 및 핸드 설정

        [Header("덱 및 핸드 설정")]
        [Tooltip("최대 핸드 크기")]
        [SerializeField] protected int maxHandSize = 7;

        [Tooltip("초기 핸드 크기")]
        [SerializeField] protected int initialHandSize = 5;

        [Tooltip("덱 셔플 활성화")]
        [SerializeField] protected bool enableDeckShuffle = true;

        #endregion

        #region UI 연결

        [Header("UI 연결")]
        [Tooltip("카드 UI 컨트롤러")]
        [SerializeField] protected MonoBehaviour cardUIController;

        [Tooltip("핸드 UI 컨테이너")]
        [SerializeField] protected Transform handContainer;

        [Tooltip("덱 UI 컨테이너")]
        [SerializeField] protected Transform deckContainer;

        #endregion

        #region 의존성 및 서비스

        [Header("의존성 및 서비스")]
        [Tooltip("카드 팩토리")]
        [SerializeField] protected MonoBehaviour cardFactory;

        [Tooltip("카드 검증기")]
        [SerializeField] protected MonoBehaviour cardValidator;

        [Tooltip("카드 순환 시스템")]
        [SerializeField] protected MonoBehaviour cardCirculationSystem;

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
                GameLogger.LogWarning("루트 오브젝트가 아니므로 DontDestroyOnLoad를 적용할 수 없습니다.", GameLogger.LogCategory.SkillCard);
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 시작", GameLogger.LogCategory.SkillCard);
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

        #region 초기화

        public virtual System.Collections.IEnumerator Initialize()
        {
            if (IsInitialized)
            {
                yield break;
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 중...", GameLogger.LogCategory.SkillCard);
            }

            // 서브클래스에서 구현할 초기화 로직
            yield return StartCoroutine(OnInitialize());

            IsInitialized = true;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 완료", GameLogger.LogCategory.SkillCard);
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

            if (cardPrefab == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 카드 프리팹이 할당되지 않았습니다.", GameLogger.LogCategory.SkillCard);
            }

            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 카드 UI 프리팹이 할당되지 않았습니다.", GameLogger.LogCategory.SkillCard);
            }

            if (handContainer == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 핸드 컨테이너가 할당되지 않았습니다.", GameLogger.LogCategory.SkillCard);
            }

            if (deckContainer == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 덱 컨테이너가 할당되지 않았습니다.", GameLogger.LogCategory.SkillCard);
            }

            return isValid;
        }

        /// <summary>
        /// 카드 UI를 연결합니다.
        /// </summary>
        protected virtual void ConnectCardUI()
        {
            if (cardUIController != null)
            {
                GameLogger.LogInfo($"{GetType().Name}: 카드 UI 컨트롤러 연결 - {cardUIController.GetType().Name}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 매니저 상태를 로깅합니다.
        /// </summary>
        protected virtual void LogManagerState()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 상태: 초기화={IsInitialized}, 디버그={enableDebugLogging}, 자동초기화={autoInitialize}, 최대핸드={maxHandSize}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 카드 설정을 검증합니다.
        /// </summary>
        protected virtual bool ValidateCardSettings()
        {
            bool isValid = true;

            if (maxHandSize <= 0)
            {
                GameLogger.LogError($"{GetType().Name}: 최대 핸드 크기가 0 이하입니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (initialHandSize < 0 || initialHandSize > maxHandSize)
            {
                GameLogger.LogError($"{GetType().Name}: 초기 핸드 크기가 유효하지 않습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            return isValid;
        }

        #endregion

        #region Unity 생명주기

        protected virtual void OnDestroy()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 파괴됨", GameLogger.LogCategory.SkillCard);
            }
        }

        #endregion
    }
}
