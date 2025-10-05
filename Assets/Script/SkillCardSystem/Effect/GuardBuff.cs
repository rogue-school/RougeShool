using Game.CharacterSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 버프 효과 클래스입니다.
    /// 1턴 동안 모든 데미지와 상태이상을 차단합니다.
    /// </summary>
    public class GuardBuff : OwnTurnEffectBase
    {
        /// <summary>
        /// 가드 버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        /// <param name="icon">UI 아이콘</param>
        public GuardBuff(int duration = 1, Sprite icon = null) : base(duration, icon)
        {
        }

        /// <summary>
        /// 턴 감소 시 가드 상태를 업데이트합니다.
        /// </summary>
        /// <param name="target">가드 버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            target.SetGuarded(!IsExpired);
        }
    }
}
