using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.UseAnimation
{
    [RequireComponent(typeof(RectTransform))]
    public class DefaultSkillCardUseAnimation : MonoBehaviour, ISkillCardUseAnimationScript
    {
        [Header("사용 애니메이션 설정")]
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] private float popDuration = 0.12f;
        [SerializeField] private float restoreDuration = 0.10f;
        [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.3f, 0.5f);
        [SerializeField] private float highlightFadeIn = 0.08f;
        [SerializeField] private float highlightFadeOut = 0.18f;
        [SerializeField] private bool useHighlight = true;

        private RectTransform rectTransform;
        private Vector3 originalScale;
        private GameObject highlightObject;
        private UnityEngine.UI.Image highlightImage;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            if (useHighlight)
                CreateHighlightObject();
        }

        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            PlayUseAnimation(onComplete);
        }

        public void PlayAnimation(RectTransform targetSlot, System.Action onComplete = null)
        {
            PlayUseAnimation(onComplete);
        }

        public void StopAnimation()
        {
            DOTween.Kill(rectTransform);
            if (highlightImage != null)
                DOTween.Kill(highlightImage);
        }

        public void CompleteAnimation()
        {
            StopAnimation();
            rectTransform.localScale = originalScale;
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }

        public void PlayUseAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;
            Sequence seq = DOTween.Sequence();
            // 1. 팝(Scale Up)
            seq.Append(rectTransform.DOScale(originalScale * popScale, popDuration).SetEase(Ease.OutBack));
            // 2. 복원
            seq.Append(rectTransform.DOScale(originalScale, restoreDuration).SetEase(Ease.InQuad));
            // 3. 하이라이트 효과
            if (useHighlight && highlightImage != null)
            {
                highlightObject.SetActive(true);
                highlightImage.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0);
                seq.Join(highlightImage.DOFade(highlightColor.a, highlightFadeIn));
                seq.Append(highlightImage.DOFade(0, highlightFadeOut).SetEase(Ease.InQuad).OnComplete(() => highlightObject.SetActive(false)));
            }
            seq.OnComplete(() => {
                onComplete?.Invoke();
            });
        }

        private void CreateHighlightObject()
        {
            highlightObject = new GameObject("UseHighlight");
            highlightObject.transform.SetParent(transform);
            highlightObject.transform.localPosition = Vector3.zero;
            highlightObject.transform.localScale = Vector3.one * 1.1f;
            highlightObject.transform.SetSiblingIndex(transform.childCount);
            highlightImage = highlightObject.AddComponent<UnityEngine.UI.Image>();
            highlightImage.sprite = GetComponent<UnityEngine.UI.Image>()?.sprite;
            highlightImage.color = highlightColor;
            highlightImage.raycastTarget = false;
            var rect = highlightObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            highlightObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
} 