using UnityEngine;
using Game.Interface;
using Game.Battle;

namespace Game.UI
{
    /// <summary>
    /// 전투 카드 슬롯 UI입니다. 슬롯 이름 기반으로 위치를 자동 지정합니다.
    /// </summary>
    public class BattleCardSlotUI : BaseCardSlotUI
    {
        [Tooltip("이 슬롯의 위치 (자동 설정됩니다)")]
        [SerializeField] private SlotPosition position;

        public override SlotPosition Position => position;

        private void Awake()
        {
            AutoBind(); // 자동 위치 설정
        }

        /// <summary>
        /// 오브젝트 이름을 기반으로 슬롯 위치를 자동 추론합니다.
        /// </summary>
        public override void AutoBind()
        {
            foreach (SlotPosition pos in System.Enum.GetValues(typeof(SlotPosition)))
            {
                if (name.ToUpper().Contains(pos.ToString().ToUpper()))
                {
                    position = pos;
                    break;
                }
            }
        }
    }
}
