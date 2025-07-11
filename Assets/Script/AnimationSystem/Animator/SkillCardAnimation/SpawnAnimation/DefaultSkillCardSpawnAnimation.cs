using UnityEngine;
using DG.Tweening;
using AnimationSystem.Interface;

namespace AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation
{
    /// <summary>
    /// 스킬 카드가 생성될 때 재생되는 애니메이션과 그림자 효과를 처리합니다.
    /// 그림자는 카드의 위치에 맞춰 생성되며, 카드가 떨어지면서 자연스럽게 변합니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class DefaultSkillCardSpawnAnimation : MonoBehaviour, ISkillCardSpawnAnimationScript
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip spawnSound;

        // 스킬 카드 전용 파라미터

        [Header("Visual Effect")]
        [SerializeField] private GameObject spawnEffectPrefab;

        [Header("Animation Settings")]
        [SerializeField] private float spawnDuration = 0.3f;
        [SerializeField] private float scaleDuration = 0.3f;
        [SerializeField] private Ease spawnEase = Ease.OutBack;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            Debug.Log($"[DefaultSkillCardSpawnAnimation][Awake] 초기화 완료: {gameObject.name}, rectTransform: {rectTransform != null}, canvasGroup: {canvasGroup != null}");
        }

        /// <summary>
        /// IAnimationScript 인터페이스 구현
        /// </summary>
        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            Debug.Log($"[DefaultSkillCardSpawnAnimation][PlayAnimation] 호출됨: {gameObject.name}, 타입: {animationType}");
            StartCoroutine(PlaySpawnAnimationCoroutine(onComplete));
        }
        
        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        public void StopAnimation()
        {
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);
        }
        
        /// <summary>
        /// 애니메이션을 즉시 완료합니다.
        /// </summary>
        public void CompleteAnimation()
        {
            StopAnimation();
            rectTransform.localScale = Vector3.one;
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 카드 생성 애니메이션
        /// </summary>
        public void PlayCastAnimation(System.Action onComplete = null)
        {
            Debug.Log($"[DefaultSkillCardSpawnAnimation][PlayCastAnimation] 시작: {gameObject.name}");
            
            if (rectTransform == null || canvasGroup == null) 
            {
                Debug.LogError($"[DefaultSkillCardSpawnAnimation][PlayCastAnimation] 필수 컴포넌트 없음: {gameObject.name}");
                onComplete?.Invoke();
                return;
            }

            // 초기 상태 설정
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;

            Sequence sequence = DOTween.Sequence();

            // 사운드 재생
            sequence.OnStart(() =>
            {
                Debug.Log($"[DefaultSkillCardSpawnAnimation][DOTween Start] 애니메이션 시작: {gameObject.name}");
                if (audioSource != null && spawnSound != null)
                    audioSource.PlayOneShot(spawnSound);
            });

            // 스케일 애니메이션
            sequence.Append(
                rectTransform.DOScale(Vector3.one, scaleDuration)
                    .SetEase(scaleEase)
            );

            // 페이드 인
            sequence.Join(
                canvasGroup.DOFade(1f, spawnDuration)
                    .SetEase(spawnEase)
            );

            // 완료 콜백
            sequence.OnComplete(() =>
            {
                Debug.Log($"[DefaultSkillCardSpawnAnimation][DOTween Complete] 애니메이션 완료: {gameObject.name}");
                if (spawnEffectPrefab != null)
                {
                    GameObject fx = Instantiate(spawnEffectPrefab, rectTransform.position, Quaternion.identity);
                    Destroy(fx, 1.5f);
                }
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 카드 사용 애니메이션
        /// </summary>
        public void PlayUseAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Vector3 originalScale = rectTransform.localScale;

            Sequence sequence = DOTween.Sequence();

            // 스케일 업
            sequence.Append(
                rectTransform.DOScale(originalScale * 1.1f, 0.1f)
                    .SetEase(Ease.OutQuad)
            );

            // 스케일 다운
            sequence.Append(
                rectTransform.DOScale(originalScale, 0.1f)
                    .SetEase(Ease.InQuad)
            );

            // 완료 콜백
            sequence.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 카드 호버 애니메이션
        /// </summary>
        public void PlayHoverAnimation(System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Vector3 originalScale = rectTransform.localScale;

            Sequence sequence = DOTween.Sequence();

            // 스케일 업
            sequence.Append(
                rectTransform.DOScale(originalScale * 1.05f, 0.2f)
                    .SetEase(Ease.OutQuad)
            );

            // 완료 콜백
            sequence.OnComplete(() => onComplete?.Invoke());
        }
        
        /// <summary>
        /// 카드 생성 애니메이션 (기본 메서드)
        /// </summary>
        public void PlaySpawnAnimation()
        {
            PlayCastAnimation();
        }
        
        /// <summary>
        /// 카드 생성 애니메이션 (비동기)
        /// </summary>
        public async System.Threading.Tasks.Task PlaySpawnAnimationAsync()
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            PlayCastAnimation(() => tcs.SetResult(true));
            await tcs.Task;
        }
        
        /// <summary>
        /// 카드 생성 애니메이션 (코루틴)
        /// </summary>
        public System.Collections.IEnumerator PlaySpawnAnimationCoroutine(System.Action onComplete)
        {
            Debug.Log($"[DefaultSkillCardSpawnAnimation][Coroutine Start] 시작: {gameObject.name}");
            float startTime = Time.time;

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            PlayCastAnimation(() => {
                Debug.Log($"[DefaultSkillCardSpawnAnimation][CastAnimation Complete] 완료: {gameObject.name}");
                tcs.SetResult(true);
            });
            
            while (!tcs.Task.IsCompleted)
            {
                yield return null;
            }
            
            Debug.Log($"[DefaultSkillCardSpawnAnimation][Coroutine End] 완료: {gameObject.name}");
            onComplete?.Invoke();
        }
    }
}
