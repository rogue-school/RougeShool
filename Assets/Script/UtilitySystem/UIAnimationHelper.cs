using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Game.CoreSystem.Utility;

namespace Game.UtilitySystem
{
    /// <summary>
    /// UI 애니메이션을 위한 공통 헬퍼 클래스
    /// 페이드 인/아웃 애니메이션을 통합하여 중복 코드를 제거합니다.
    /// </summary>
    public static class UIAnimationHelper
    {
        /// <summary>
        /// CanvasGroup 페이드 인 애니메이션
        /// </summary>
        /// <param name="canvasGroup">대상 CanvasGroup</param>
        /// <param name="duration">애니메이션 지속 시간 (기본값: 0.2f)</param>
        /// <param name="ease">이징 타입 (기본값: Ease.OutQuad)</param>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        /// <param name="enableInteraction">애니메이션 중 상호작용 활성화 여부 (기본값: true)</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween FadeIn(
            CanvasGroup canvasGroup,
            float duration = 0.2f,
            Ease ease = Ease.OutQuad,
            System.Action onComplete = null,
            bool enableInteraction = true)
        {
            if (canvasGroup == null)
            {
                GameLogger.LogWarning("[UIAnimationHelper] CanvasGroup이 null입니다", GameLogger.LogCategory.UI);
                return null;
            }

            canvasGroup.alpha = 0f;
            if (enableInteraction)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            return canvasGroup.DOFade(1f, duration)
                .SetEase(ease)
                .SetAutoKill(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// CanvasGroup 페이드 아웃 애니메이션
        /// </summary>
        /// <param name="canvasGroup">대상 CanvasGroup</param>
        /// <param name="duration">애니메이션 지속 시간 (기본값: 0.15f)</param>
        /// <param name="ease">이징 타입 (기본값: Ease.InQuad)</param>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        /// <param name="disableInteraction">애니메이션 중 상호작용 비활성화 여부 (기본값: true)</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween FadeOut(
            CanvasGroup canvasGroup,
            float duration = 0.15f,
            Ease ease = Ease.InQuad,
            System.Action onComplete = null,
            bool disableInteraction = true)
        {
            if (canvasGroup == null)
            {
                GameLogger.LogWarning("[UIAnimationHelper] CanvasGroup이 null입니다", GameLogger.LogCategory.UI);
                return null;
            }

            if (disableInteraction)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            return canvasGroup.DOFade(0f, duration)
                .SetEase(ease)
                .SetAutoKill(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 기존 Tween을 종료하고 새로운 페이드 인 애니메이션을 시작합니다
        /// </summary>
        /// <param name="previousTween">이전 Tween (null 가능)</param>
        /// <param name="canvasGroup">대상 CanvasGroup</param>
        /// <param name="duration">애니메이션 지속 시간</param>
        /// <param name="ease">이징 타입</param>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        /// <param name="enableInteraction">애니메이션 중 상호작용 활성화 여부</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween FadeInWithCleanup(
            ref Tween previousTween,
            CanvasGroup canvasGroup,
            float duration = 0.2f,
            Ease ease = Ease.OutQuad,
            System.Action onComplete = null,
            bool enableInteraction = true)
        {
            previousTween?.Kill();
            Tween newTween = FadeIn(canvasGroup, duration, ease, () =>
            {
                onComplete?.Invoke();
            }, enableInteraction);
            previousTween = newTween;
            return newTween;
        }

        /// <summary>
        /// 기존 Tween을 종료하고 새로운 페이드 아웃 애니메이션을 시작합니다
        /// </summary>
        /// <param name="previousTween">이전 Tween (null 가능)</param>
        /// <param name="canvasGroup">대상 CanvasGroup</param>
        /// <param name="duration">애니메이션 지속 시간</param>
        /// <param name="ease">이징 타입</param>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        /// <param name="disableInteraction">애니메이션 중 상호작용 비활성화 여부</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween FadeOutWithCleanup(
            ref Tween previousTween,
            CanvasGroup canvasGroup,
            float duration = 0.15f,
            Ease ease = Ease.InQuad,
            System.Action onComplete = null,
            bool disableInteraction = true)
        {
            previousTween?.Kill();
            Tween newTween = FadeOut(canvasGroup, duration, ease, () =>
            {
                onComplete?.Invoke();
            }, disableInteraction);
            previousTween = newTween;
            return newTween;
        }
    }
}

