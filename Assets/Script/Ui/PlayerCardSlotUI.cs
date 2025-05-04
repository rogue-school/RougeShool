using UnityEngine;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 플레이어 전용 카드 슬롯 UI입니다. 카드 배치 및 제거, 전투 처리용으로 사용됩니다.
    /// </summary>
    public class PlayerCardSlotUI : BattleCardSlotUI
    {
        /// <summary>
        /// 슬롯에 설정된 카드를 가져옵니다.
        /// </summary>
        public new ISkillCard GetCard()
        {
            return base.GetCard();
        }

        /// <summary>
        /// 슬롯에 설정된 카드를 제거합니다.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
        }
    }
}
