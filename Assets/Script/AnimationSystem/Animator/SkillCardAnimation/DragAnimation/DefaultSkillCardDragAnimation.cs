using UnityEngine;
using DG.Tweening;
using AnimationSystem.Interface;

namespace AnimationSystem.Animator.SkillCardAnimation.DragAnimation
{
    [RequireComponent(typeof(RectTransform))]
    public class DefaultSkillCardDragAnimation : MonoBehaviour, ISkillCardDragAnimationScript
    {
        [Header("드래그 애니메이션 설정")]
        [SerializeField] private float dragScaleMultiplier = 1.1f;
        [SerializeField] private float dragScaleDuration = 0.15f;
        [SerializeField] private Ease dragScaleEase = Ease.OutBack;
        
        [Header("드래그 중 효과")]
        [SerializeField] private float dragRotationAngle = 5f;
        [SerializeField] private float dragRotationDuration = 0.1f;
        [SerializeField] private Ease dragRotationEase = Ease.OutQuad;
        
        [Header("그림자 효과")]
        [SerializeField] private bool useShadowEffect = true;
        [SerializeField] private float shadowOffset = 5f;
        [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.3f);
        [SerializeField] private float shadowDuration = 0.15f;
        
        [Header("글로우 효과")]
        [SerializeField] private bool useGlowEffect = true;
        [SerializeField] private Color glowColor = Color.yellow;
        [SerializeField] private float glowIntensity = 1.2f;
        [SerializeField] private float glowDuration = 0.2f;

        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Vector3 originalRotation;
        private GameObject shadowObject;
        private UnityEngine.UI.Image shadowImage;
        private UnityEngine.UI.Image glowImage;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            originalRotation = rectTransform.localEulerAngles;
            
            // 그림자 오브젝트 생성
            if (useShadowEffect)
            {
                CreateShadowObject();
            }
            
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
                    Debug.LogWarning($"[DefaultSkillCardDragAnimation] 알 수 없는 애니메이션 타입: {animationType}");
                    onComplete?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// 호환성을 위한 오버로드 (사용하지 않음)
        /// </summary>
        public void PlayAnimation(RectTransform targetSlot, System.Action onComplete = null)
        {
            Debug.LogWarning("[DefaultSkillCardDragAnimation] PlayAnimation(RectTransform, Action)는 사용하지 않습니다. PlayAnimation(string, Action)을 사용하세요.");
            onComplete?.Invoke();
        }

        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        public void StopAnimation()
        {
            DOTween.Kill(rectTransform);
            if (shadowObject != null)
                DOTween.Kill(shadowObject);
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
            
            if (shadowObject != null)
                shadowObject.SetActive(false);
            if (glowImage != null)
                glowImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 드래그 시작 애니메이션
        /// </summary>
        public void PlayDragStartAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Debug.Log("[DefaultSkillCardDragAnimation] 드래그 시작 애니메이션 실행");

            Sequence sequence = DOTween.Sequence();

            // 1. 스케일 업 애니메이션
            sequence.Append(
                rectTransform.DOScale(originalScale * dragScaleMultiplier, dragScaleDuration)
                    .SetEase(dragScaleEase)
            );

            // 2. 회전 애니메이션
            sequence.Join(
                rectTransform.DOLocalRotate(new Vector3(0, 0, dragRotationAngle), dragRotationDuration)
                    .SetEase(dragRotationEase)
            );

            // 3. 그림자 효과
            if (useShadowEffect && shadowObject != null)
            {
                shadowObject.SetActive(true);
                shadowObject.transform.localScale = Vector3.one;
                shadowImage.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0);
                
                sequence.Join(
                    shadowImage.DOFade(shadowColor.a, shadowDuration)
                        .SetEase(Ease.OutQuad)
                );
            }

            // 4. 글로우 효과
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
                Debug.Log("[DefaultSkillCardDragAnimation] 드래그 시작 애니메이션 완료");
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 드래그 종료 애니메이션
        /// </summary>
        public void PlayDragEndAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Debug.Log("[DefaultSkillCardDragAnimation] 드래그 종료 애니메이션 실행");

            Sequence sequence = DOTween.Sequence();

            // 1. 스케일 다운 애니메이션
            sequence.Append(
                rectTransform.DOScale(originalScale, dragScaleDuration)
                    .SetEase(dragScaleEase)
            );

            // 2. 회전 복원 애니메이션
            sequence.Join(
                rectTransform.DOLocalRotate(originalRotation, dragRotationDuration)
                    .SetEase(dragRotationEase)
            );

            // 3. 그림자 효과 제거
            if (useShadowEffect && shadowObject != null && shadowObject.activeSelf)
            {
                sequence.Join(
                    shadowImage.DOFade(0, shadowDuration)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => shadowObject.SetActive(false))
                );
            }

            // 4. 글로우 효과 제거
            if (useGlowEffect && glowImage != null && glowImage.gameObject.activeSelf)
            {
                sequence.Join(
                    glowImage.DOFade(0, glowDuration)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => glowImage.gameObject.SetActive(false))
                );
            }

            sequence.OnComplete(() => {
                Debug.Log("[DefaultSkillCardDragAnimation] 드래그 종료 애니메이션 완료");
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 그림자 오브젝트 생성
        /// </summary>
        private void CreateShadowObject()
        {
            shadowObject = new GameObject("DragShadow");
            shadowObject.transform.SetParent(transform);
            shadowObject.transform.localPosition = new Vector3(shadowOffset, -shadowOffset, 0);
            shadowObject.transform.localScale = Vector3.one;
            shadowObject.transform.SetSiblingIndex(0); // 맨 뒤로

            // 그림자 이미지 컴포넌트 추가
            shadowImage = shadowObject.AddComponent<UnityEngine.UI.Image>();
            shadowImage.sprite = GetComponent<UnityEngine.UI.Image>()?.sprite;
            shadowImage.color = shadowColor;
            shadowImage.raycastTarget = false;
            
            // RectTransform 설정
            var shadowRect = shadowObject.GetComponent<RectTransform>();
            if (shadowRect != null)
            {
                shadowRect.anchorMin = Vector2.zero;
                shadowRect.anchorMax = Vector2.one;
                shadowRect.offsetMin = Vector2.zero;
                shadowRect.offsetMax = Vector2.zero;
            }

            shadowObject.SetActive(false);
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

            // 그림자 위치 업데이트
            if (useShadowEffect && shadowObject != null && shadowObject.activeSelf)
            {
                shadowObject.transform.localPosition = new Vector3(
                    shadowOffset + dragDelta.x * 0.1f,
                    -shadowOffset + dragDelta.y * 0.1f,
                    0
                );
            }
        }

        /// <summary>
        /// 드래그 중단 (외부에서 호출)
        /// </summary>
        public void CancelDragAnimation()
        {
            StopAnimation();
            rectTransform.localScale = originalScale;
            rectTransform.localEulerAngles = originalRotation;
            
            if (shadowObject != null)
                shadowObject.SetActive(false);
            if (glowImage != null)
                glowImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
} 