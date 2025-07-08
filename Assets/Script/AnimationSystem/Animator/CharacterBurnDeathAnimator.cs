using UnityEngine;
using DG.Tweening;
using AnimationSystem.Data;
using AnimationSystem.Interface;

namespace AnimationSystem.Animator
{
    /// <summary>
    /// 캐릭터가 불타면서 사라지는 사망 애니메이션
    /// </summary>
    public class CharacterBurnDeathAnimator : MonoBehaviour, IAnimationScript
    {
        [Header("불타는 효과 설정")]
        [SerializeField] private float burnDuration = 2.0f;
        [SerializeField] private float fadeOutDuration = 1.0f;
        [SerializeField] private Color burnColor = new Color(1f, 0.5f, 0f, 1f);
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeDuration = 0.5f;
        
        [Header("파티클 효과")]
        [SerializeField] private GameObject fireParticlePrefab;
        [SerializeField] private GameObject smokeParticlePrefab;
        
        private SpriteRenderer spriteRenderer;
        private Vector3 originalPosition;
        private Color originalColor;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            originalPosition = transform.position;
        }
        
        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="animationType">애니메이션 타입 (사용하지 않음)</param>
        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            StartCoroutine(PlayBurnDeathAnimationCoroutine(onComplete));
        }

        private System.Collections.IEnumerator PlayBurnDeathAnimationCoroutine(System.Action onComplete)
        {
            yield return new UnityEngine.WaitForSeconds(1.5f); // 실제 연출 시간에 맞게 조정
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 불타면서 사라지는 애니메이션을 시작합니다.
        /// </summary>
        private void StartBurnDeathAnimation()
        {
            // 1. 초기 진동 효과
            transform.DOShakePosition(shakeDuration, shakeIntensity, 10, 90, false, true)
                .SetEase(Ease.OutQuad);
            
            // 2. 색상 변화 (빨간색으로 변하면서 불타는 효과)
            if (spriteRenderer != null)
            {
                spriteRenderer.DOColor(burnColor, burnDuration * 0.3f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => {
                        // 3. 파티클 효과 생성
                        CreateFireParticles();
                        
                        // 4. 연기 효과 생성
                        CreateSmokeParticles();
                        
                        // 5. 최종 페이드 아웃
                        spriteRenderer.DOFade(0f, fadeOutDuration)
                            .SetEase(Ease.InQuad)
                            .OnComplete(() => {
                                // 6. 오브젝트 비활성화
                                gameObject.SetActive(false);
                            });
                    });
            }
            
            // 7. 크기 변화 (약간 커졌다가 작아지면서 사라짐)
            transform.DOScale(transform.localScale * 1.2f, burnDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    transform.DOScale(Vector3.zero, fadeOutDuration)
                        .SetEase(Ease.InQuad);
                });
        }
        
        /// <summary>
        /// 불 파티클 효과를 생성합니다.
        /// </summary>
        private void CreateFireParticles()
        {
            if (fireParticlePrefab != null)
            {
                GameObject fireParticles = Instantiate(fireParticlePrefab, transform.position, Quaternion.identity);
                Destroy(fireParticles, burnDuration + fadeOutDuration);
            }
        }
        
        /// <summary>
        /// 연기 파티클 효과를 생성합니다.
        /// </summary>
        private void CreateSmokeParticles()
        {
            if (smokeParticlePrefab != null)
            {
                GameObject smokeParticles = Instantiate(smokeParticlePrefab, transform.position, Quaternion.identity);
                Destroy(smokeParticles, burnDuration + fadeOutDuration);
            }
        }
        
        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        public void StopAnimation()
        {
            DOTween.Kill(transform);
            if (spriteRenderer != null)
            {
                DOTween.Kill(spriteRenderer);
            }
        }
        
        /// <summary>
        /// 애니메이션을 즉시 완료합니다.
        /// </summary>
        public void CompleteAnimation()
        {
            StopAnimation();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            }
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
} 