using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 분신 버프 효과 클래스입니다.
    /// 추가 체력 10을 제공하며, 추가 체력이 존재하는 동안 자신의 스킬 데미지가 2배로 증가합니다.
    /// 추가 체력이 0이 되면 버프가 자동으로 해제됩니다.
    /// </summary>
    public class CloneBuff : OwnTurnEffectBase
    {
        /// <summary>
        /// 분신 추가 체력
        /// </summary>
        public int CloneHP { get; private set; }

        /// <summary>
        /// 분신 버프를 생성합니다.
        /// </summary>
        /// <param name="cloneHP">추가 체력 (기본값: 10)</param>
        /// <param name="icon">UI 아이콘</param>
        public CloneBuff(int cloneHP = 10, Sprite icon = null) : base(0, icon)
        {
            CloneHP = cloneHP;
            // 분신은 턴 기반이 아니므로 RemainingTurns를 0으로 설정 (영구 지속)
        }

        /// <summary>
        /// 분신 추가 체력을 설정합니다.
        /// </summary>
        /// <param name="value">설정할 추가 체력 값</param>
        public void SetCloneHP(int value)
        {
            CloneHP = Mathf.Max(0, value);
        }

        /// <summary>
        /// 턴 감소 시 동작 (분신은 턴 기반이 아니므로 아무 동작 없음)
        /// </summary>
        /// <param name="target">분신 버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            // 분신은 턴 기반이 아니므로 아무 동작 없음
        }

        /// <summary>
        /// 분신 버프가 만료되었는지 확인합니다.
        /// 추가 체력이 0 이하면 만료된 것으로 간주합니다.
        /// </summary>
        public new bool IsExpired => CloneHP <= 0;
    }
}

