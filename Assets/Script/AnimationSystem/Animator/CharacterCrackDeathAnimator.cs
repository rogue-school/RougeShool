using UnityEngine;
using DG.Tweening;
using AnimationSystem.Data;
using AnimationSystem.Interface;

namespace AnimationSystem.Animator
{
    /// <summary>
    /// 캐릭터가 금이 가면서 깨지는 사망 애니메이션
    /// </summary>
    public class CharacterCrackDeathAnimator : MonoBehaviour, IAnimationScript
    {
        [Header("깨지는 효과 설정")]
        [SerializeField] private float crackDuration = 1.5f;
        [SerializeField] private float shatterDuration = 0.8f;
        [SerializeField] private Color crackColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private float shakeIntensity = 0.15f;
        [SerializeField] private float shakeDuration = 0.3f;
        
        [Header("파편 효과")]
        [SerializeField] private GameObject shatterParticlePrefab;
        [SerializeField] private int fragmentCount = 8;
        [SerializeField] private float fragmentSpeed = 3f;
        
        private SpriteRenderer spriteRenderer;
        private Vector3 originalPosition;
        private Color originalColor;
        private Vector3 originalScale;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            originalPosition = transform.position;
            originalScale = transform.localScale;
        }
        
        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="animationType">애니메이션 타입 (사용하지 않음)</param>
        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            StartCoroutine(PlayCrackDeathAnimationCoroutine(onComplete));
        }

        private System.Collections.IEnumerator PlayCrackDeathAnimationCoroutine(System.Action onComplete)
        {
            yield return new UnityEngine.WaitForSeconds(1.5f); // 실제 연출 시간에 맞게 조정
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 금이 가면서 깨지는 애니메이션을 시작합니다.
        /// </summary>
        private void StartCrackDeathAnimation()
        {
            // 1. 초기 진동 효과
            transform.DOShakePosition(shakeDuration, shakeIntensity, 15, 90, false, true)
                .SetEase(Ease.OutQuad);
            
            // 2. 색상 변화 (회색으로 변하면서 금이 가는 효과)
            if (spriteRenderer != null)
            {
                spriteRenderer.DOColor(crackColor, crackDuration * 0.4f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => {
                        // 3. 금이 가는 효과 (크기 변화)
                        transform.DOScale(originalScale * 1.1f, crackDuration * 0.3f)
                            .SetEase(Ease.OutQuad)
                            .OnComplete(() => {
                                // 4. 파편 효과 생성
                                CreateShatterParticles();
                                
                                // 5. 최종 파괴 효과
                                transform.DOScale(Vector3.zero, shatterDuration)
                                    .SetEase(Ease.InQuad)
                                    .OnComplete(() => {
                                        // 6. 오브젝트 비활성화
                                        gameObject.SetActive(false);
                                    });
                            });
                    });
            }
            
            // 7. 회전 효과 (깨지면서 회전)
            transform.DORotate(new Vector3(0, 0, 360f), crackDuration + shatterDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InQuad);
        }
        
        /// <summary>
        /// 파편 파티클 효과를 생성합니다.
        /// </summary>
        private void CreateShatterParticles()
        {
            if (shatterParticlePrefab != null)
            {
                for (int i = 0; i < fragmentCount; i++)
                {
                    GameObject fragment = Instantiate(shatterParticlePrefab, transform.position, Quaternion.identity);
                    
                    // 랜덤한 방향으로 파편 날리기
                    Vector3 randomDirection = Random.insideUnitSphere.normalized;
                    randomDirection.z = 0; // 2D 게임이므로 Z축은 0으로
                    
                    fragment.transform.DOMove(
                        transform.position + randomDirection * fragmentSpeed,
                        shatterDuration
                    ).SetEase(Ease.OutQuad);
                    
                    // 파편 회전
                    fragment.transform.DORotate(
                        new Vector3(0, 0, Random.Range(0f, 360f)),
                        shatterDuration,
                        RotateMode.FastBeyond360
                    ).SetEase(Ease.OutQuad);
                    
                    // 파편 페이드 아웃
                    SpriteRenderer fragmentRenderer = fragment.GetComponent<SpriteRenderer>();
                    if (fragmentRenderer != null)
                    {
                        fragmentRenderer.DOFade(0f, shatterDuration)
                            .SetEase(Ease.InQuad)
                            .OnComplete(() => Destroy(fragment));
                    }
                    else
                    {
                        Destroy(fragment, shatterDuration);
                    }
                }
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
            transform.rotation = Quaternion.identity;
            gameObject.SetActive(false);
        }
    }
} 