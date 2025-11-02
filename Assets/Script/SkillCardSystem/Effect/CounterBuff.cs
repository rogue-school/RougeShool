using Game.CharacterSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 반격 버프: 1턴 동안 받는 피해의 100%를 공격자에게 반사.
    /// 대상은 데미지를 받지 않고, 공격자가 원래 데미지의 100%를 받습니다.
    /// 또한 버프 지속 중에는 적의 상태이상 효과를 무효화합니다.
    /// </summary>
    public class CounterBuff : OwnTurnEffectBase
    {
        /// <summary>
        /// 최근 공격자를 추적하기 위한 참조. DamageEffectCommand에서 설정합니다.
        /// </summary>
        public ICharacter LastAttacker { get; set; }

        /// <summary>
        /// 반격 버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        /// <param name="icon">UI 아이콘</param>
        public CounterBuff(int duration = 1, Sprite icon = null) : base(duration, icon)
        {
        }

        /// <summary>
        /// 턴 감소 시 추가 동작 (현재는 없음)
        /// </summary>
        /// <param name="target">반격 버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            // 반격은 턴 감소 시 특별한 동작이 없음
        }
    }
}
