using UnityEngine;
using DG.Tweening;
using AnimationSystem.Interface;

namespace AnimationSystem.Animator.SkillCardAnimation.VanishAnimation
{
    [RequireComponent(typeof(RectTransform))]
    public class DefaultSkillCardVanishAnimation : MonoBehaviour, ISkillCardDeathAnimationScript
    {
        [Header("소멸 애니메이션 설정")]
        [SerializeField] private float vanishDuration = 0.25f;
        [SerializeField] private float vanishScale = 0.1f;
        [SerializeField] private float fadeDuration = 0.18f;
        [SerializeField] private bool useGlow = true;
        [SerializeField] private Color glowColor = new Color(1f, 0.5f, 0.1f, 0.5f);
        [SerializeField] private float glowFadeIn = 0.08f;
        [SerializeField] private float glowFadeOut = 0.18f;

        private RectTransform rectTransform;
        private Vector3 originalScale;
        private CanvasGroup canvasGroup;
        private GameObject glowObject;
        private UnityEngine.UI.Image glowImage;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            if (useGlow)
                CreateGlowObject();
                
        }

        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            PlayVanishAnimation(onComplete);
        }

        public void PlayAnimation(RectTransform targetSlot, System.Action onComplete = null)
        {
            PlayVanishAnimation(onComplete);
        }

        public void StopAnimation()
        {
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);
            if (glowImage != null)
                DOTween.Kill(glowImage);
        }

        public void CompleteAnimation()
        {
            StopAnimation();
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
            if (glowObject != null)
                glowObject.SetActive(false);
        }

        public void PlayVanishAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null || canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            Sequence seq = DOTween.Sequence();
            
            // 1. 스케일 다운
            seq.Append(rectTransform.DOScale(originalScale * vanishScale, vanishDuration).SetEase(Ease.InBack));
            
            // 2. 페이드 아웃
            seq.Join(canvasGroup.DOFade(0, fadeDuration));
            
            // 3. 글로우 효과
            if (useGlow && glowImage != null)
            {
                glowObject.SetActive(true);
                glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0);
                seq.Join(glowImage.DOFade(glowColor.a, glowFadeIn));
                seq.Append(glowImage.DOFade(0, glowFadeOut).SetEase(Ease.InQuad).OnComplete(() => glowObject.SetActive(false)));
            }
            
            seq.OnComplete(() => {
                onComplete?.Invoke();
            });
        }

        private void CreateGlowObject()
        {
            glowObject = new GameObject("VanishGlow");
            glowObject.transform.SetParent(transform);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = Vector3.one * 1.1f;
            glowObject.transform.SetSiblingIndex(transform.childCount);
            glowImage = glowObject.AddComponent<UnityEngine.UI.Image>();
            glowImage.sprite = GetComponent<UnityEngine.UI.Image>()?.sprite;
            glowImage.color = glowColor;
            glowImage.raycastTarget = false;
            var rect = glowObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            glowObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
} 