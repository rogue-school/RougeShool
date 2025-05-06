using UnityEngine;
using Game.Battle;
using Game.Slots;

namespace Game.Utility
{
    /// <summary>
    /// 전투 슬롯 위치를 통해 슬롯 Transform을 찾는 유틸리티 (임시 호환성용)
    /// </summary>
    public static class SlotUtility
    {
        public static Transform FindSlotByPosition(BattleSlotPosition position)
        {
            var anchors = GameObject.FindObjectsOfType<SlotAnchor>();
            foreach (var anchor in anchors)
            {
                if (anchor.battleSlotPosition == position)
                    return anchor.transform;
            }
            return null;
        }
    }
}
