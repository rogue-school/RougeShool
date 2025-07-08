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
            StartCoroutine(PlayShiftAnimationCoroutine(onComplete));
        }

        private System.Collections.IEnumerator PlayShiftAnimationCoroutine(System.Action onComplete)
        {
            yield return new UnityEngine.WaitForSeconds(0.5f); // 카드 이동 연출 시간에 맞게 조정
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
                Debug.Log($"SkillCardShift animation completed for target: {targetRect?.name ?? "null"}");
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
