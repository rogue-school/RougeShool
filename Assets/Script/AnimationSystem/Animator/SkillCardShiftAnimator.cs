using UnityEngine;
using DG.Tweening;

namespace AnimationSystem.Animator
{
    [RequireComponent(typeof(RectTransform))]
    public class SkillCardShiftAnimator : MonoBehaviour, AnimationSystem.Interface.IAnimationScript
    {
        [Header("Animation Settings")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutCubic;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// IAnimationScript 인터페이스 구현
        /// </summary>
        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            switch (animationType.ToLower())
            {
                case "move":
                    // move 애니메이션은 실제 슬롯 간 이동을 시뮬레이션
                    StartCoroutine(PlaySlotMoveSimulationCoroutine(onComplete));
                    break;
                case "moveToCombatSlot":
                    StartCoroutine(PlayMoveToCombatSlotAnimationCoroutine(onComplete));
                    break;
                default:
                    // 알 수 없는 애니메이션 타입은 무시 (개발 중에만 필요하면 주석 해제)
                    // Debug.LogWarning($"[SkillCardShiftAnimator] 알 수 없는 애니메이션 타입: {animationType}");
                    onComplete?.Invoke();
                    break;
            }
        }

        private System.Collections.IEnumerator PlaySlotMoveSimulationCoroutine(System.Action onComplete)
        {
            if (rectTransform == null)
            {
                Debug.LogError("[SkillCardShiftAnimator] RectTransform이 null입니다.");
                onComplete?.Invoke();
                yield break;
            }

            Vector2 originalPosition = rectTransform.anchoredPosition;
            Vector3 originalScale = rectTransform.localScale;

            bool animationCompleted = false;

            Sequence sequence = DOTween.Sequence();

            // 1단계: 위로 올라가면서 스케일 업 (이동 시작)
            sequence.Append(
                rectTransform.DOAnchorPos(originalPosition + new Vector2(0, 30f), moveDuration * 0.25f)
                    .SetEase(Ease.OutQuad)
            );
            sequence.Join(
                rectTransform.DOScale(originalScale * 1.1f, moveDuration * 0.25f)
                    .SetEase(Ease.OutQuad)
            );

            // 2단계: 옆으로 이동 (슬롯 간 이동 시뮬레이션)
            sequence.Append(
                rectTransform.DOAnchorPos(originalPosition + new Vector2(80f, 30f), moveDuration * 0.5f)
                    .SetEase(Ease.InOutQuad)
            );

            // 3단계: 아래로 내려가면서 스케일 다운 (이동 완료)
            sequence.Append(
                rectTransform.DOAnchorPos(originalPosition + new Vector2(80f, 0f), moveDuration * 0.25f)
                    .SetEase(Ease.InQuad)
            );
            sequence.Join(
                rectTransform.DOScale(originalScale, moveDuration * 0.25f)
                    .SetEase(Ease.InQuad)
            );

            // 완료 콜백
            sequence.OnComplete(() => {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = originalScale;
                animationCompleted = true;
            });

            yield return new WaitUntil(() => animationCompleted);
            onComplete?.Invoke();
        }

        private System.Collections.IEnumerator PlayMoveToCombatSlotAnimationCoroutine(System.Action onComplete)
        {
            if (rectTransform == null)
            {
                Debug.LogError("[SkillCardShiftAnimator] RectTransform이 null입니다.");
                onComplete?.Invoke();
                yield break;
            }

            Vector2 originalPosition = rectTransform.anchoredPosition;
            Vector3 originalScale = rectTransform.localScale;

            // 전투 슬롯 등록 애니메이션 실행
            bool animationCompleted = false;
            PlayBattleSlotPlaceAnimation(originalPosition, () => {
                Debug.Log("[SkillCardShiftAnimator] 전투 슬롯 등록 애니메이션 완료");
                animationCompleted = true;
            });

            // 애니메이션 완료까지 대기
            yield return new WaitUntil(() => animationCompleted);
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        public void StopAnimation()
        {
            DOTween.Kill(rectTransform);
        }
        
        /// <summary>
        /// 애니메이션을 즉시 완료합니다.
        /// </summary>
        public void CompleteAnimation()
        {
            StopAnimation();
            // 기본 위치로 즉시 이동
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// 슬롯 이동 애니메이션
        /// </summary>
        public void PlaySlotMoveAnimation(Vector2 targetPosition, System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Sequence sequence = DOTween.Sequence();

            // 이동 애니메이션
            sequence.Append(
                rectTransform.DOAnchorPos(targetPosition, moveDuration)
                    .SetEase(moveEase)
            );

            // 완료 콜백
            sequence.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 전투 슬롯 등록 애니메이션
        /// </summary>
        public void PlayBattleSlotPlaceAnimation(Vector2 targetPosition, System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            Vector3 originalScale = rectTransform.localScale;
            Vector2 originalPosition = rectTransform.anchoredPosition;

            Sequence sequence = DOTween.Sequence();

            // 1단계: 스케일 업
            sequence.Append(
                rectTransform.DOScale(originalScale * 1.2f, scaleDuration * 0.5f)
                    .SetEase(scaleEase)
            );

            // 2단계: 이동
            sequence.Append(
                rectTransform.DOAnchorPos(targetPosition, moveDuration)
                    .SetEase(moveEase)
            );

            // 3단계: 스케일 다운
            sequence.Append(
                rectTransform.DOScale(originalScale, scaleDuration * 0.5f)
                    .SetEase(scaleEase)
            );

            // 완료 콜백
            sequence.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 기본 슬롯 이동 애니메이션 (매개변수 없음)
        /// </summary>
        public void PlaySlotMoveAnimation()
        {
            // 기본 위치로 이동하는 애니메이션
            Vector2 defaultPosition = new Vector2(0, 0);
            PlaySlotMoveAnimation(defaultPosition);
        }

        /// <summary>
        /// 기본 전투 슬롯 등록 애니메이션 (매개변수 없음)
        /// </summary>
        public void PlayBattleSlotPlaceAnimation()
        {
            // 기본 위치로 이동하는 애니메이션
            Vector2 defaultPosition = new Vector2(0, 0);
            PlayBattleSlotPlaceAnimation(defaultPosition);
        }
        
        /// <summary>
        /// 슬롯 이동 애니메이션 (비동기)
        /// </summary>
        public async System.Threading.Tasks.Task PlayMoveAnimationAsync(RectTransform targetRect)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            PlaySlotMoveAnimation(targetRect.anchoredPosition, () => tcs.SetResult(true));
            await tcs.Task;
        }
        
        /// <summary>
        /// 슬롯 이동 애니메이션 (코루틴)
        /// </summary>
        public System.Collections.IEnumerator PlayMoveAnimationCoroutine(RectTransform targetRect)
        {
            float startTime = Time.time;

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            PlaySlotMoveAnimation(targetRect.anchoredPosition, () => {
                tcs.SetResult(true);
            });
            
            while (!tcs.Task.IsCompleted)
            {
                yield return null;
            }
        }
        
        /// <summary>
        /// 기본 슬롯 이동 애니메이션 (비동기)
        /// </summary>
        public async System.Threading.Tasks.Task PlayMoveAnimationAsync()
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            PlaySlotMoveAnimation(new Vector2(0, 0), () => tcs.SetResult(true));
            await tcs.Task;
        }
        
        /// <summary>
        /// 기본 슬롯 이동 애니메이션 (코루틴)
        /// </summary>
        public System.Collections.IEnumerator PlayMoveAnimationCoroutine()
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            PlaySlotMoveAnimation(new Vector2(0, 0), () => tcs.SetResult(true));
            
            while (!tcs.Task.IsCompleted)
            {
                yield return null;
            }
        }
    }
}
