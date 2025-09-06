using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.DropAnimation
{
    [RequireComponent(typeof(RectTransform))]
    public class DefaultSkillCardDropAnimation : MonoBehaviour, ISkillCardDropAnimationScript
    {
        [Header("드롭 애니메이션 설정")]
        [SerializeField] private float dropScaleUp = 1.15f;
        [SerializeField] private float dropScaleDuration = 0.12f;
        [SerializeField] private float dropScaleDownDuration = 0.10f;
        [SerializeField] private float shakeStrength = 10f;
        [SerializeField] private int shakeVibrato = 10;
        [SerializeField] private float shakeDuration = 0.18f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;
        [SerializeField] private bool useGlow = true;
        [SerializeField] private Color glowColor = Color.yellow;
        [SerializeField] private float glowFadeIn = 0.08f;
        [SerializeField] private float glowFadeOut = 0.18f;

        private RectTransform rectTransform;
        private Vector3 originalScale;
        private GameObject glowObject;
        private UnityEngine.UI.Image glowImage;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            if (useGlow)
                CreateGlowObject();
        }

        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            // 드롭 애니메이션은 타입 구분 없이 항상 동일하게 동작
            PlayDropAnimation(onComplete);
        }

        public void PlayAnimation(RectTransform targetSlot, System.Action onComplete = null)
        {
            PlayDropAnimation(onComplete);
        }

        public void StopAnimation()
        {
            DOTween.Kill(rectTransform);
            if (glowImage != null)
                DOTween.Kill(glowImage);
        }

        public void CompleteAnimation()
        {
            StopAnimation();
            rectTransform.localScale = originalScale;
            if (glowObject != null)
                glowObject.SetActive(false);
        }

        public void PlayDropAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;
            Sequence seq = DOTween.Sequence();

            // 1. 스케일 업
            seq.Append(rectTransform.DOScale(originalScale * dropScaleUp, dropScaleDuration).SetEase(scaleEase));
            // 2. 스케일 다운
            seq.Append(rectTransform.DOScale(originalScale, dropScaleDownDuration).SetEase(Ease.InQuad));
            // 3. 진동(Shake)
            seq.Join(rectTransform.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato));

            // 4. 글로우 효과
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
            glowObject = new GameObject("DropGlow");
            glowObject.transform.SetParent(transform);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = Vector3.one * 1.1f;
            glowObject.transform.SetSiblingIndex(transform.childCount);
            glowImage = glowObject.AddComponent<UnityEngine.UI.Image>();
            glowImage.sprite = GetComponent<UnityEngine.UI.Image>()?.sprite;
            glowImage.color = glowColor;
            glowImage.raycastTarget = false;
            var glowRect = glowObject.GetComponent<RectTransform>();
            if (glowRect != null)
            {
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.offsetMin = Vector2.zero;
                glowRect.offsetMax = Vector2.zero;
            }
            glowObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
} 