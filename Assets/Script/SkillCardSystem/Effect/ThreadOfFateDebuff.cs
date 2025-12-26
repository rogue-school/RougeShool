using Game.CharacterSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 운명의 실 디버프 효과 클래스입니다.
    /// 플레이어에게 적용되며, 해당 디버프가 있을 때 플레이어는 스킬을 원하는 대로 사용할 수 없습니다.
    /// 핸드에서 3장을 뽑고 2개를 제거한 후 나머지 1개를 전투 슬롯으로 이동시킵니다.
    /// </summary>
    public class ThreadOfFateDebuff : OwnTurnEffectBase
    {
        /// <summary>
        /// 운명의 실 디버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        /// <param name="icon">UI 아이콘</param>
        public ThreadOfFateDebuff(int duration = 1, Sprite icon = null) : base(duration, icon)
        {
        }

        /// <summary>
        /// 턴 감소 시 동작 (현재는 없음)
        /// </summary>
        /// <param name="target">운명의 실 디버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            // 턴 감소 시 특별한 동작 없음
        }
    }
}

