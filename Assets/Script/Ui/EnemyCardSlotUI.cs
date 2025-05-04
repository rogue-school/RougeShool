using UnityEngine;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 적 전용 카드 슬롯 UI입니다. 턴 시작 시 적의 카드를 이 슬롯에서 꺼냅니다.
    /// </summary>
    public class EnemyCardSlotUI : BattleCardSlotUI
    {
        /// <summary>
        /// 슬롯에 설정된 카드를 가져옵니다.
        /// </summary>
        public new ISkillCard GetCard()
        {
            return base.GetCard();
        }

        /// <summary>
        /// 슬롯을 초기화하여 적 카드 연결을 제거합니다.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
        }
    }
}
