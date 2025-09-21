using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CoreSystem.Utility;
using Zenject;
using System.Diagnostics;

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

        [System.Serializable]
        public class ManagerSettings
        {
            [Header("기본 설정")]
            [Tooltip("디버그 로깅 활성화")]
            public bool enableDebugLogging = true;

            [Tooltip("자동 초기화 활성화")]
            public bool autoInitialize = true;

            [Tooltip("씬 전환 시 유지 여부")]
            public bool persistAcrossScenes = false;

            [Tooltip("필수 참조가 없어도 초기화 진행")]
            public bool initializeWithoutRequiredReferences = false;

            [Space(5)]
            [Header("성능 설정")]
            [Tooltip("카드 풀링 활성화")]
            public bool enableCardPooling = true;

            [Tooltip("최대 풀 크기")]
            [Range(10, 100)]
            public int maxPoolSize = 50;
        }

        [Header("⚙️ 매니저 설정")]
        [SerializeField] protected ManagerSettings managerSettings = new ManagerSettings();

        [Header("DI 최적화 설정")]
#pragma warning disable CS0414 // 사용하지 않는 필드 경고 억제 (향후 사용 예정)
        [SerializeField] private bool enableDIPerformanceLogging = false;
        [SerializeField] private bool enableLazyInitialization = true;
#pragma warning restore CS0414

        #endregion

        #region 카드 데이터 및 설정

        [System.Serializable]
        public class CardSettings
        {
            [Header("프리팹 설정")]
            [Tooltip("카드 프리팹 (SkillCardUI 컴포넌트 포함)")]
            public SkillCardUI cardPrefab;

            [Space(5)]
            [Header("데이터 설정")]
            [Tooltip("카드 설정 데이터")]
            public ScriptableObject cardConfig;

            [Tooltip("카드 데이터베이스")]
            public ScriptableObject cardDatabase;
        }

        [Header("🃏 카드 설정")]
        [SerializeField] protected CardSettings cardSettings = new CardSettings();

        #endregion

        #region 덱 및 핸드 설정

        [System.Serializable]
        public class DeckHandSettings
        {
            [Header("핸드 설정")]
            [Tooltip("최대 핸드 크기")]
            [Range(3, 15)]
            public int maxHandSize = 7;

            [Tooltip("초기 핸드 크기")]
            [Range(3, 10)]
            public int initialHandSize = 5;

            [Space(5)]
            [Header("덱 설정")]
            [Tooltip("덱 셔플 활성화")]
            public bool enableDeckShuffle = true;

            [Tooltip("덱 크기")]
            [Range(20, 100)]
            public int deckSize = 30;

            [Tooltip("드로우 카드 수")]
            [Range(1, 5)]
            public int drawCardCount = 1;
        }

        [Header("🎴 덱 및 핸드 설정")]
        [SerializeField] protected DeckHandSettings deckHandSettings = new DeckHandSettings();

        #endregion

        #region UI 연결

        [System.Serializable]
        public class UISettings
        {
            [Header("컨테이너 설정")]
            [Tooltip("핸드 UI 컨테이너")]
            public Transform handContainer;

            [Tooltip("덱 UI 컨테이너")]
            public Transform deckContainer;

            [Tooltip("카드 드롭 영역")]
            public Transform dropArea;

            [Space(5)]
            [Header("컨트롤러 설정")]
            [Tooltip("카드 UI 컨트롤러")]
            public MonoBehaviour cardUIController;

            [Tooltip("드래그 앤 드롭 핸들러")]
            public MonoBehaviour dragDropHandler;
        }

        [Header("🖥️ UI 연결")]
        [SerializeField] protected UISettings uiSettings = new UISettings();

        #endregion

        #region 의존성 및 서비스

        [System.Serializable]
        public class ServiceSettings
        {
            [Header("핵심 서비스")]
            [Tooltip("카드 팩토리")]
            public MonoBehaviour cardFactory;

            [Tooltip("카드 검증기")]
            public MonoBehaviour cardValidator;

            [Tooltip("카드 순환 시스템")]
            public MonoBehaviour cardCirculationSystem;

            [Space(5)]
            [Header("추가 서비스")]
            [Tooltip("카드 이벤트 매니저")]
            public MonoBehaviour cardEventManager;

            [Tooltip("카드 애니메이션 매니저")]
            public MonoBehaviour cardAnimationManager;
        }

        [Header("🔧 서비스 연결")]
        [SerializeField] protected ServiceSettings serviceSettings = new ServiceSettings();

        #endregion

        #region 초기화 상태

        public bool IsInitialized { get; protected set; } = false;

        #endregion

        #region 공통 초기화

        protected virtual void Awake()
        {
            if (managerSettings.persistAcrossScenes && transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else if (managerSettings.persistAcrossScenes)
            {
                GameLogger.LogWarning("루트 오브젝트가 아니므로 DontDestroyOnLoad를 적용할 수 없습니다.", GameLogger.LogCategory.SkillCard);
            }

            if (managerSettings.enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 시작", GameLogger.LogCategory.SkillCard);
            }
        }

        protected virtual void Start()
        {
            if (managerSettings.autoInitialize)
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

            if (managerSettings.enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 중...", GameLogger.LogCategory.SkillCard);
            }

            // 서브클래스에서 구현할 초기화 로직
            yield return StartCoroutine(OnInitialize());

            IsInitialized = true;

            if (managerSettings.enableDebugLogging)
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

            if (cardSettings.cardPrefab == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 카드 프리팹이 할당되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                if (!managerSettings.initializeWithoutRequiredReferences)
                {
                    isValid = false;
                }
            }

            // 매니저 타입에 따라 필요한 컨테이너만 검증
            if (RequiresHandContainer() && uiSettings.handContainer == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 핸드 컨테이너가 할당되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                if (!managerSettings.initializeWithoutRequiredReferences)
                {
                    isValid = false;
                }
            }

            if (RequiresDeckContainer() && uiSettings.deckContainer == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 덱 컨테이너가 할당되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                if (!managerSettings.initializeWithoutRequiredReferences)
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// 카드 UI를 연결합니다.
        /// </summary>
        protected virtual void ConnectCardUI()
        {
            if (uiSettings.cardUIController != null)
            {
                GameLogger.LogInfo($"{GetType().Name}: 카드 UI 컨트롤러 연결 - {uiSettings.cardUIController.GetType().Name}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 핸드 컨테이너가 필요한지 확인합니다.
        /// 서브클래스에서 오버라이드하여 매니저별 요구사항을 정의합니다.
        /// </summary>
        protected virtual bool RequiresHandContainer()
        {
            // 기본적으로는 핸드 컨테이너가 필요하지 않음
            // PlayerHandManager에서 오버라이드하여 true 반환
            return false;
        }

        /// <summary>
        /// 덱 컨테이너가 필요한지 확인합니다.
        /// 서브클래스에서 오버라이드하여 매니저별 요구사항을 정의합니다.
        /// </summary>
        protected virtual bool RequiresDeckContainer()
        {
            // 기본적으로는 덱 컨테이너가 필요하지 않음
            // PlayerDeckManager에서 오버라이드하여 true 반환
            return false;
        }

        /// <summary>
        /// 매니저 상태를 로깅합니다.
        /// </summary>
        protected virtual void LogManagerState()
        {
            if (managerSettings.enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 상태: 초기화={IsInitialized}, 디버그={managerSettings.enableDebugLogging}, 자동초기화={managerSettings.autoInitialize}, 최대핸드={deckHandSettings.maxHandSize}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 카드 설정을 검증합니다.
        /// </summary>
        protected virtual bool ValidateCardSettings()
        {
            bool isValid = true;

            if (deckHandSettings.maxHandSize <= 0)
            {
                GameLogger.LogError($"{GetType().Name}: 최대 핸드 크기가 0 이하입니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (deckHandSettings.initialHandSize < 0 || deckHandSettings.initialHandSize > deckHandSettings.maxHandSize)
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
            if (managerSettings.enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 파괴됨", GameLogger.LogCategory.SkillCard);
            }
        }

        #endregion
    }
}
