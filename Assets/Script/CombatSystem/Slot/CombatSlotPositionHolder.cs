using UnityEngine;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯의 위치 정보를 보관하는 컴포넌트입니다.
    /// 전투 실행 위치(선공/후공)와 실제 필드 위치(좌/우)를 설정합니다.
    /// </summary>
    public class CombatSlotPositionHolder : MonoBehaviour
    {
        #region 슬롯 위치 필드

        /// <summary>
        /// 전투 실행 슬롯 위치 (선공/후공)
        /// </summary>
        [field: SerializeField]
        public CombatSlotPosition SlotPosition { get; private set; }

        /// <summary>
        /// 실제 필드상 위치 (왼쪽/오른쪽)
        /// </summary>
        [field: SerializeField]
        public CombatFieldSlotPosition FieldSlotPosition { get; private set; }

        #endregion
    }
}
