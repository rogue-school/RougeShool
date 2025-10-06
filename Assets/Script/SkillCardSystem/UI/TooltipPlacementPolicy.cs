using UnityEngine;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 툴팁 배치 보조 유틸 (좌우 폴백, 경계 클램프).
    /// 기존 SkillCardTooltip 내부 로직을 보조합니다.
    /// </summary>
    public static class TooltipPlacementPolicy
    {
        public static Vector2 PlaceRightOrLeft(Vector2 cardLocalPoint, RectTransform tooltip, float mouseOffsetX)
        {
            var parentRect = tooltip.parent as RectTransform;
            var canvasRect = parentRect.rect;
            var tooltipRect = tooltip.rect;
            var pivot = tooltip.pivot;

            float rightSpace = canvasRect.xMax - cardLocalPoint.x;
            bool canRight = rightSpace >= tooltipRect.width + mouseOffsetX;

            Vector2 pos = cardLocalPoint;
            if (canRight)
                pos.x += mouseOffsetX + tooltipRect.width * pivot.x;
            else
                pos.x -= mouseOffsetX + tooltipRect.width * (1f - pivot.x);

            return pos;
        }
    }
}


