using UnityEngine;
using DG.Tweening;
using Game.CoreSystem.Utility;

namespace Game.UtilitySystem
{
    /// <summary>
    /// 호버 효과를 위한 공통 헬퍼 클래스
    /// 호버 스케일 효과를 통합하여 중복 코드를 제거합니다.
    /// </summary>
    public static class HoverEffectHelper
    {
        /// <summary>
        /// 호버 시 스케일 효과를 재생합니다
        /// </summary>
        /// <param name="target">대상 Transform</param>
        /// <param name="hoverScale">호버 시 스케일 값 (기본값: 1.2f)</param>
        /// <param name="duration">애니메이션 지속 시간 (기본값: 0.2f)</param>
        /// <param name="ease">이징 타입 (기본값: Ease.OutQuad)</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween PlayHoverScale(
            Transform target,
            float hoverScale = 1.2f,
            float duration = 0.2f,
            Ease ease = Ease.OutQuad)
        {
            if (target == null)
            {
                GameLogger.LogWarning("[HoverEffectHelper] Transform이 null입니다", GameLogger.LogCategory.UI);
                return null;
            }

            return target.DOScale(hoverScale, duration)
                .SetEase(ease)
                .SetAutoKill(true);
        }

        /// <summary>
        /// 호버 종료 시 원래 크기로 복귀하는 애니메이션을 재생합니다
        /// </summary>
        /// <param name="target">대상 Transform</param>
        /// <param name="duration">애니메이션 지속 시간 (기본값: 0.2f)</param>
        /// <param name="ease">이징 타입 (기본값: Ease.OutQuad)</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween ResetScale(
            Transform target,
            float duration = 0.2f,
            Ease ease = Ease.OutQuad)
        {
            if (target == null)
            {
                GameLogger.LogWarning("[HoverEffectHelper] Transform이 null입니다", GameLogger.LogCategory.UI);
                return null;
            }

            return target.DOScale(1f, duration)
                .SetEase(ease)
                .SetAutoKill(true);
        }

        /// <summary>
        /// 기존 Tween을 종료하고 새로운 호버 스케일 애니메이션을 시작합니다
        /// </summary>
        /// <param name="previousTween">이전 Tween (null 가능)</param>
        /// <param name="target">대상 Transform</param>
        /// <param name="hoverScale">호버 시 스케일 값</param>
        /// <param name="duration">애니메이션 지속 시간</param>
        /// <param name="ease">이징 타입</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween PlayHoverScaleWithCleanup(
            ref Tween previousTween,
            Transform target,
            float hoverScale = 1.2f,
            float duration = 0.2f,
            Ease ease = Ease.OutQuad)
        {
            previousTween?.Kill();
            previousTween = PlayHoverScale(target, hoverScale, duration, ease);
            return previousTween;
        }

        /// <summary>
        /// 기존 Tween을 종료하고 원래 크기로 복귀하는 애니메이션을 시작합니다
        /// </summary>
        /// <param name="previousTween">이전 Tween (null 가능)</param>
        /// <param name="target">대상 Transform</param>
        /// <param name="duration">애니메이션 지속 시간</param>
        /// <param name="ease">이징 타입</param>
        /// <returns>생성된 Tween 인스턴스</returns>
        public static Tween ResetScaleWithCleanup(
            ref Tween previousTween,
            Transform target,
            float duration = 0.2f,
            Ease ease = Ease.OutQuad)
        {
            previousTween?.Kill();
            previousTween = ResetScale(target, duration, ease);
            return previousTween;
        }
    }
}

