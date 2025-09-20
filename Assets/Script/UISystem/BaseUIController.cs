using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.UISystem
{
    /// <summary>
    /// UISystem 컨트롤러들의 공통 베이스 클래스
    /// 인스펙터 필드 표준화 및 공통 기능을 제공합니다.
    /// </summary>
    public abstract class BaseUIController : MonoBehaviour
    {
        #region 기본 설정

        [Header("UI 기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] protected bool enableDebugLogging = true;

        [Tooltip("자동 초기화 활성화")]
        [SerializeField] protected bool autoInitialize = true;

        [Tooltip("씬 전환 시 유지 여부")]
        [SerializeField] protected bool persistAcrossScenes = false;

        #endregion

        #region UI 컴포넌트

        [Header("UI 컴포넌트")]
        [Tooltip("메인 캔버스")]
        [SerializeField] protected Canvas mainCanvas;

        [Tooltip("UI 패널")]
        [SerializeField] protected GameObject uiPanel;

        [Tooltip("UI 버튼들")]
        [SerializeField] protected Button[] uiButtons;

        [Tooltip("UI 텍스트들")]
        [SerializeField] protected TMPro.TextMeshProUGUI[] uiTexts;

        [Tooltip("UI 이미지들")]
        [SerializeField] protected Image[] uiImages;

        #endregion

        #region 애니메이션 설정

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인/아웃 활성화")]
        [SerializeField] protected bool enableFadeAnimation = true;

        [Tooltip("슬라이드 애니메이션 활성화")]
        [SerializeField] protected bool enableSlideAnimation = false;

        [Tooltip("애니메이션 지속 시간")]
        [SerializeField] protected float animationDuration = 0.3f;

        [Tooltip("애니메이션 커브")]
        [SerializeField] protected AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        #endregion

        #region 이벤트 설정

        [Header("이벤트 설정")]
        [Tooltip("클릭 사운드 활성화")]
        [SerializeField] protected bool enableClickSound = true;

        [Tooltip("호버 효과 활성화")]
        [SerializeField] protected bool enableHoverEffect = true;

        [Tooltip("키보드 네비게이션 활성화")]
        [SerializeField] protected bool enableKeyboardNavigation = true;

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

        #region 초기화

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

            // UI 이벤트 설정
            SetupUIEvents();

            // 애니메이션 설정
            SetupAnimations();

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
        /// UI 리셋 로직
        /// </summary>
        public abstract void Reset();

        #endregion

        #region UI 이벤트 설정

        /// <summary>
        /// UI 이벤트를 설정합니다.
        /// </summary>
        protected virtual void SetupUIEvents()
        {
            if (uiButtons != null)
            {
                foreach (var button in uiButtons)
                {
                    if (button != null)
                    {
                        button.onClick.AddListener(() => OnButtonClicked(button));
                    }
                }
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name}: UI 이벤트 설정 완료", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 버튼 클릭 이벤트 처리
        /// </summary>
        protected virtual void OnButtonClicked(Button button)
        {
            if (enableClickSound)
            {
                // 클릭 사운드 재생 (AudioManager 사용)
                Debug.Log($"[{GetType().Name}] 버튼 클릭: {button.name}");
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"버튼 클릭: {button.name}", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region 애니메이션 설정

        /// <summary>
        /// 애니메이션을 설정합니다.
        /// </summary>
        protected virtual void SetupAnimations()
        {
            if (enableFadeAnimation && uiPanel != null)
            {
                var canvasGroup = uiPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = uiPanel.AddComponent<CanvasGroup>();
                }
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name}: 애니메이션 설정 완료", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        public virtual void FadeIn()
        {
            if (!enableFadeAnimation || uiPanel == null)
            {
                return;
            }

            var canvasGroup = uiPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f));
            }
        }

        /// <summary>
        /// 페이드 아웃 애니메이션
        /// </summary>
        public virtual void FadeOut()
        {
            if (!enableFadeAnimation || uiPanel == null)
            {
                return;
            }

            var canvasGroup = uiPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f));
            }
        }

        private System.Collections.IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
        {
            canvasGroup.alpha = startAlpha;
            
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                
                yield return null;
            }
            
            canvasGroup.alpha = endAlpha;
        }

        #endregion

        #region 공통 유틸리티

        /// <summary>
        /// 필수 참조 필드의 유효성을 검사합니다.
        /// </summary>
        protected virtual bool ValidateReferences()
        {
            bool isValid = true;

            if (mainCanvas == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: 메인 캔버스가 할당되지 않았습니다.", GameLogger.LogCategory.UI);
            }

            if (uiPanel == null)
            {
                GameLogger.LogWarning($"{GetType().Name}: UI 패널이 할당되지 않았습니다.", GameLogger.LogCategory.UI);
            }

            if (uiButtons == null || uiButtons.Length == 0)
            {
                GameLogger.LogWarning($"{GetType().Name}: UI 버튼이 할당되지 않았습니다.", GameLogger.LogCategory.UI);
            }

            return isValid;
        }

        /// <summary>
        /// UI 상태를 로깅합니다.
        /// </summary>
        protected virtual void LogUIState()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 상태: 초기화={IsInitialized}, 디버그={enableDebugLogging}, 자동초기화={autoInitialize}, 페이드={enableFadeAnimation}", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region Unity 생명주기

        protected virtual void OnDestroy()
        {
            // 이벤트 구독 해제
            if (uiButtons != null)
            {
                foreach (var button in uiButtons)
                {
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                    }
                }
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 파괴됨", GameLogger.LogCategory.UI);
            }
        }

        #endregion
    }
}
