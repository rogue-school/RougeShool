using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.DragAnimation
{
    [RequireComponent(typeof(RectTransform))]
    public class DefaultSkillCardDragAnimation : MonoBehaviour, ISkillCardDragAnimationScript
    {
        [Header("드래그 애니메이션 설정")]
        [SerializeField] private float dragScaleMultiplier = 1.05f;
        [SerializeField] private float dragScaleDuration = 0.2f;
        
        [Header("드래그 중 효과")]
        [SerializeField] private float dragRotationAngle = 0f; // 각도 효과 비활성화
        
        
        [Header("글로우 효과")]
        [SerializeField] private bool useGlowEffect = true;
        [SerializeField] private Color glowColor = Color.yellow;
        [SerializeField] private float glowIntensity = 1.2f;
        [SerializeField] private float glowDuration = 0.2f;

        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Vector3 originalRotation;
        private UnityEngine.UI.Image glowImage;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            originalRotation = rectTransform.localEulerAngles;
            
            // 글로우 오브젝트 생성
            if (useGlowEffect)
            {
                CreateGlowObject();
            }
        }

        /// <summary>
        /// IAnimationScript 인터페이스 구현
        /// </summary>
        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            switch (animationType.ToLower())
            {
                case "start":
                    PlayDragStartAnimation(onComplete);
                    break;
                case "end":
                    PlayDragEndAnimation(onComplete);
                    break;
                default:
                    onComplete?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// 호환성을 위한 오버로드 (사용하지 않음)
        /// </summary>
        public void PlayAnimation(RectTransform targetSlot, System.Action onComplete = null)
        {
            onComplete?.Invoke();
        }

        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        public void StopAnimation()
        {
            DOTween.Kill(rectTransform);
            if (glowImage != null)
                DOTween.Kill(glowImage);
        }

        /// <summary>
        /// 애니메이션을 즉시 완료합니다.
        /// </summary>
        public void CompleteAnimation()
        {
            StopAnimation();
            rectTransform.localScale = originalScale;
            rectTransform.localEulerAngles = originalRotation;
            
            if (glowImage != null)
                glowImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 드래그 시작 애니메이션 (스케일 업 + 글로우 효과)
        /// </summary>
        public void PlayDragStartAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Sequence sequence = DOTween.Sequence();

            // 1. 부드러운 스케일 업 (선택된 느낌)
            sequence.Append(
                rectTransform.DOScale(originalScale * dragScaleMultiplier, dragScaleDuration)
                    .SetEase(Ease.OutCubic) // 더 부드러운 이징
            );

            // 2. 글로우 효과 (선택된 느낌)
            if (useGlowEffect && glowImage != null)
            {
                glowImage.gameObject.SetActive(true);
                glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0);
                
                sequence.Join(
                    glowImage.DOFade(glowColor.a * glowIntensity, glowDuration)
                        .SetEase(Ease.OutQuad)
                );
            }

            sequence.OnComplete(() => {
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 드래그 종료 애니메이션 (스케일 다운 + 글로우 제거)
        /// </summary>
        public void PlayDragEndAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Sequence sequence = DOTween.Sequence();

            // 1. 부드러운 스케일 다운 (착지 효과)
            sequence.Append(
                rectTransform.DOScale(originalScale, dragScaleDuration)
                    .SetEase(Ease.InCubic) // 부드러운 착지
            );

            // 2. 글로우 효과 제거
            if (useGlowEffect && glowImage != null && glowImage.gameObject.activeSelf)
            {
                sequence.Join(
                    glowImage.DOFade(0, glowDuration)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => glowImage.gameObject.SetActive(false))
                );
            }

            sequence.OnComplete(() => {
                onComplete?.Invoke();
            });
        }


        /// <summary>
        /// 글로우 오브젝트 생성
        /// </summary>
        private void CreateGlowObject()
        {
            var glowObject = new GameObject("DragGlow");
            glowObject.transform.SetParent(transform);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = Vector3.one * 1.1f; // 약간 크게
            glowObject.transform.SetSiblingIndex(transform.childCount); // 맨 앞으로

            // 글로우 이미지 컴포넌트 추가
            glowImage = glowObject.AddComponent<UnityEngine.UI.Image>();
            glowImage.sprite = GetComponent<UnityEngine.UI.Image>()?.sprite;
            glowImage.color = glowColor;
            glowImage.raycastTarget = false;
            
            // RectTransform 설정
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

        /// <summary>
        /// 드래그 중 실시간 업데이트 (외부에서 호출)
        /// </summary>
        public void UpdateDragAnimation(Vector2 dragDelta)
        {
            if (rectTransform == null) return;

            // 드래그 방향에 따른 미세한 회전
            float rotationZ = Mathf.Clamp(dragDelta.x * 0.5f, -dragRotationAngle, dragRotationAngle);
            rectTransform.localEulerAngles = new Vector3(0, 0, rotationZ);
        }

        /// <summary>
        /// 드래그 중단 (외부에서 호출)
        /// </summary>
        public void CancelDragAnimation()
        {
            StopAnimation();
            rectTransform.localScale = originalScale;
            rectTransform.localEulerAngles = originalRotation;
            
            if (glowImage != null)
                glowImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 드롭 실패 시 원래 자리로 돌아가는 애니메이션 (빠르고 부드러운 착지)
        /// </summary>
        public void PlayDropFailAnimation(Vector3 originalPosition, System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Sequence sequence = DOTween.Sequence();

            // 1. 빠른 이동 (원래 자리로)
            sequence.Append(
                rectTransform.DOMove(originalPosition, 0.3f)
                    .SetEase(Ease.OutCubic) // 부드러운 이동
            );

            // 2. 착지 효과 (스케일 다운)
            sequence.Join(
                rectTransform.DOScale(originalScale, 0.2f)
                    .SetEase(Ease.OutBack) // 살짝 튀는 착지 효과
            );

            // 3. 글로우 효과 제거
            if (useGlowEffect && glowImage != null && glowImage.gameObject.activeSelf)
            {
                sequence.Join(
                    glowImage.DOFade(0, 0.2f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => glowImage.gameObject.SetActive(false))
                );
            }

            sequence.OnComplete(() => {
                onComplete?.Invoke();
            });
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
} 