using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Game.CoreSystem.Utility;
using Game.UtilitySystem;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 캐릭터 이펙트 발동 시 알림을 표시하는 UI 패널입니다.
    /// 코어씬에 배치되어 모든 씬에서 사용할 수 있습니다.
    /// </summary>
    public class EffectNotificationPanel : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [Tooltip("알림 패널 GameObject")]
        [SerializeField] private GameObject panel;

        [Tooltip("알림 텍스트")]
        [SerializeField] private TextMeshProUGUI notificationText;

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인 시간 (초)")]
        [SerializeField] private float fadeInDuration = 0.3f;

        [Tooltip("표시 지속 시간 (초)")]
        [SerializeField] private float displayDuration = 2.5f;

        [Tooltip("페이드 아웃 시간 (초)")]
        [SerializeField] private float fadeOutDuration = 0.3f;

        [Header("설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true;

        private CanvasGroup canvasGroup;
        private Image panelImage;
        private Sequence currentSequence;

        private void Awake()
        {
            InitializeComponents();
        }

        private void OnDisable()
        {
            // DOTween 시퀀스 정리
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
                currentSequence = null;
            }
        }

        private void OnDestroy()
        {
            // DOTween 시퀀스 정리
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
                currentSequence = null;
            }
        }

        /// <summary>
        /// UI 컴포넌트를 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            if (panel != null)
            {
                // CanvasGroup이 이미 있으면 사용 (자동 추가하지 않음)
                canvasGroup = panel.GetComponent<CanvasGroup>();
                
                // CanvasGroup이 없으면 Image 컴포넌트 사용
                if (canvasGroup == null)
                {
                    panelImage = panel.GetComponent<Image>();
                    if (panelImage == null)
                    {
                        GameLogger.LogWarning("[EffectNotificationPanel] CanvasGroup 또는 Image 컴포넌트가 없습니다. 알림이 제대로 표시되지 않을 수 있습니다.", GameLogger.LogCategory.UI);
                    }
                }

                // 초기 상태: 비활성화
                if (panel.activeSelf)
                {
                    panel.SetActive(false);
                }
            }
            else
            {
                GameLogger.LogWarning("[EffectNotificationPanel] 패널이 설정되지 않았습니다.", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 이펙트 알림을 표시합니다.
        /// </summary>
        /// <param name="effectName">이펙트 이름</param>
        /// <param name="targetName">대상 이름 (선택적)</param>
        public void ShowNotification(string effectName, string targetName = null)
        {
            if (panel == null || notificationText == null)
            {
                GameLogger.LogWarning("[EffectNotificationPanel] 패널 또는 텍스트가 설정되지 않았습니다.", GameLogger.LogCategory.UI);
                return;
            }

            // 메시지 구성: 이펙트 이름만 사용
            string message = effectName;

            if (enableDebugLogging)
            {
            }

            // 기존 시퀀스 정리
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
            }

            // 텍스트 설정
            notificationText.text = message;

            // 패널 활성화 (모든 부모 체인 확인 및 활성화)
            if (enableDebugLogging)
            {
            }

            if (!panel.activeInHierarchy)
            {
                if (enableDebugLogging)
                {
                }

                // 모든 부모를 확인하고 활성화
                Transform current = panel.transform;
                while (current != null)
                {
                    if (!current.gameObject.activeSelf)
                    {
                        if (enableDebugLogging)
                        {
                        }
                        current.gameObject.SetActive(true);
                    }
                    current = current.parent;
                }
                
                // Canvas도 확인
                Canvas canvas = panel.GetComponentInParent<Canvas>();
                if (canvas != null && !canvas.gameObject.activeInHierarchy)
                {
                    if (enableDebugLogging)
                    {
                    }
                    canvas.gameObject.SetActive(true);
                }
                
                panel.SetActive(true);
            }
            
            // 패널이 활성화되어 있는지 최종 확인
            if (!panel.activeInHierarchy)
            {
                GameLogger.LogError($"[EffectNotificationPanel] 패널 활성화 실패! activeSelf={panel.activeSelf}, activeInHierarchy={panel.activeInHierarchy}", GameLogger.LogCategory.UI);
                return;
            }
            
            if (enableDebugLogging)
            {
            }

            // 애니메이션 시작
            if (canvasGroup != null)
            {
                // CanvasGroup 사용
                if (enableDebugLogging)
                {
                }
                
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;

                // Sequence 패턴이므로 UIAnimationHelper를 Sequence 내부에서 사용
                currentSequence = DOTween.Sequence()
                    // 페이드 인 (UIAnimationHelper 사용)
                    .Append(UIAnimationHelper.FadeIn(canvasGroup, fadeInDuration, Ease.OutQuad, null, true))
                    // 표시 지속
                    .AppendInterval(displayDuration)
                    // 페이드 아웃 (UIAnimationHelper 사용)
                    .Append(UIAnimationHelper.FadeOut(canvasGroup, fadeOutDuration, Ease.InQuad, null, true))
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        panel.SetActive(false);
                        canvasGroup.blocksRaycasts = false;
                        currentSequence = null;
                    });
            }
            else if (panelImage != null)
            {
                // Image 컴포넌트의 Color 직접 조작
                Color originalColor = panelImage.color;
                Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                Color opaqueColor = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a);

                panelImage.color = transparentColor;

                currentSequence = DOTween.Sequence()
                    // 페이드 인
                    .Append(panelImage.DOColor(opaqueColor, fadeInDuration).SetEase(Ease.OutQuad))
                    // 표시 지속
                    .AppendInterval(displayDuration)
                    // 페이드 아웃
                    .Append(panelImage.DOColor(transparentColor, fadeOutDuration).SetEase(Ease.InQuad))
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        panel.SetActive(false);
                        currentSequence = null;
                    });
            }
            else
            {
                // 컴포넌트가 없으면 단순히 표시만
                GameLogger.LogWarning("[EffectNotificationPanel] CanvasGroup 또는 Image가 없어 애니메이션 없이 표시합니다.", GameLogger.LogCategory.UI);
                currentSequence = DOTween.Sequence()
                    .AppendInterval(displayDuration)
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        panel.SetActive(false);
                        currentSequence = null;
                    });
            }
        }

        /// <summary>
        /// 알림을 즉시 숨깁니다.
        /// </summary>
        public void HideNotification()
        {
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
                currentSequence = null;
            }

            if (panel != null)
            {
                panel.SetActive(false);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.blocksRaycasts = false;
                }
                else if (panelImage != null)
                {
                    Color currentColor = panelImage.color;
                    panelImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
                }
            }
        }
    }
}

