using UnityEngine;
using DG.Tweening;
using AnimationSystem.Interface;

namespace AnimationSystem.Animator.SkillCardAnimation.MoveAnimation
{
    [RequireComponent(typeof(RectTransform))]
    public class DefaultSkillCardMoveAnimation : MonoBehaviour, ISkillCardMoveAnimationScript
    {
        [Header("Animation Settings")]
        [SerializeField] private float moveDuration = 0.07f; // 0.15f보다 더 빠르게
        [SerializeField] private Ease moveEase = Ease.OutQuad; // 더 빠른 느낌의 이징
        [SerializeField] private float scaleDuration = 0.07f; // 0.15f보다 더 빠르게
        [SerializeField] private Ease scaleEase = Ease.OutQuad; // 더 빠른 느낌의 이징

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// IAnimationScript 인터페이스 구현 (애니메이션 타입 무시, 오로지 하나의 이동 애니메이션만)
        /// </summary>
        public void PlayAnimation(RectTransform targetSlot, System.Action onComplete = null)
        {
            PlayMoveAnimationToSlot(targetSlot, moveDuration, onComplete);
        }

        // 기존 PlayAnimation 오버로드(타입 기반)는 사용 금지(호환성 위해 남겨두지만 내부적으로 경고)
        public void PlayAnimation(string animationType, RectTransform targetSlot, System.Action onComplete = null)
        {
            Debug.LogWarning("[DefaultSkillCardMoveAnimation] 애니메이션 타입은 무시됩니다. PlayAnimation(RectTransform, Action)만 사용하세요.");
            PlayAnimation(targetSlot, onComplete);
        }
        public void PlayAnimation(string animationType, System.Action onComplete = null)
        {
            Debug.LogWarning("[DefaultSkillCardMoveAnimation] 애니메이션 타입은 무시됩니다. PlayAnimation(RectTransform, Action)만 사용하세요.");
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
        /// 슬롯 이동 애니메이션 (절대 좌표 기반)
        /// </summary>
        public void PlaySlotMoveAnimation(Vector2 startPosition, Vector2 targetPosition, System.Action onComplete = null)
        {
            if (rectTransform == null) return;

            rectTransform.anchoredPosition = startPosition; // 시작 위치로 이동

            Sequence sequence = DOTween.Sequence();
            sequence.Append(
                rectTransform.DOAnchorPos(targetPosition, moveDuration)
                    .SetEase(moveEase)
            );
            sequence.OnComplete(() => {
                rectTransform.anchoredPosition = targetPosition; // 목표 위치에 정확히 고정
                onComplete?.Invoke();
            });
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
            // 현재 위치에서 (0,0)으로 이동
            PlaySlotMoveAnimation(rectTransform.anchoredPosition, Vector2.zero, null);
        }

        /// <summary>
        /// 기존: PlaySlotMoveAnimation(targetPosition, onComplete)
        /// 새 시그니처에 맞게 오버로드 추가 (호환성)
        /// </summary>
        public void PlaySlotMoveAnimation(Vector2 targetPosition, System.Action onComplete = null)
        {
            PlaySlotMoveAnimation(rectTransform.anchoredPosition, targetPosition, onComplete);
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
            PlaySlotMoveAnimation(rectTransform.anchoredPosition, Vector2.zero, () => tcs.SetResult(true));
            await tcs.Task;
        }
        
        /// <summary>
        /// 기본 슬롯 이동 애니메이션 (코루틴)
        /// </summary>
        public System.Collections.IEnumerator PlayMoveAnimationCoroutine()
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            PlaySlotMoveAnimation(rectTransform.anchoredPosition, Vector2.zero, () => tcs.SetResult(true));
            
            while (!tcs.Task.IsCompleted)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 슬롯 간 이동(부모 변경 포함) 시 월드 좌표 변환을 적용한 자연스러운 이동
        /// </summary>
        public void MoveCardToSlot(RectTransform targetSlot, float duration = 0.3f, System.Action onComplete = null)
        {
            if (rectTransform == null || targetSlot == null) return;

            Vector3 worldStart = rectTransform.position;
            rectTransform.SetParent(targetSlot, false); // 부모 변경
            rectTransform.position = worldStart; // 부모 변경 후에도 화면상 위치 유지

            Vector3 worldTarget = targetSlot.position;
            rectTransform.DOMove(worldTarget, duration)
                .SetEase(moveEase)
                .OnComplete(() => {
                    rectTransform.anchoredPosition = Vector2.zero; // 슬롯 내에서 정확히 정렬
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// move 타입도 절대 좌표 기반으로 이동하도록 오버로드 추가
        /// </summary>
        public void PlayMoveAnimationToSlot(RectTransform targetSlot, float duration = 0.3f, System.Action onComplete = null)
        {
            MoveCardToSlot(targetSlot, duration, onComplete);
        }
    }
}
