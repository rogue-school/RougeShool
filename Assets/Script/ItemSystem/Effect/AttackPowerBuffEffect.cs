using Game.CharacterSystem.Interface;
using Game.ItemSystem.Interface;
using Game.SkillCardSystem.Effect;
using UnityEngine;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 공격력 증가 버프 효과입니다.
    /// 플레이어의 공격력에 일정 수치를 추가합니다.
    /// ItemEffectBase를 상속하여 아이템 전용 턴 관리 시스템을 사용합니다.
    /// </summary>
    public class AttackPowerBuffEffect : ItemEffectBase
    {
        /// <summary>버프로 증가하는 공격력 수치</summary>
        public int AttackPowerBonus { get; private set; }

        /// <summary>원본 스킬 효과 이름 (스킬 기반 버프 식별용, 아이템 버프는 null)</summary>
        public string SourceEffectName { get; private set; }

        /// <summary>
        /// 공격력 버프 효과를 생성합니다.
        /// </summary>
        /// <param name="attackPowerBonus">공격력 보너스</param>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="turnPolicy">턴 감소 정책</param>
        /// <param name="icon">UI 아이콘</param>
        /// <param name="sourceItemName">원본 아이템 이름 (선택적, 아이템 유래 버프용)</param>
        /// <param name="sourceEffectName">원본 스킬 효과 이름 (선택적, 스킬 유래 버프용)</param>
        public AttackPowerBuffEffect(
            int attackPowerBonus,
            int duration,
            ItemEffectTurnPolicy turnPolicy,
            Sprite icon = null,
            string sourceItemName = null,
            string sourceEffectName = null)
            : base(duration, turnPolicy, icon, sourceItemName)
        {
            AttackPowerBonus = attackPowerBonus;
            SourceEffectName = sourceEffectName;
        }

        /// <summary>
        /// 턴 감소 시 추가 동작 (공격력 버프는 별도 동작 없음)
        /// </summary>
        protected override void OnTurnDecrement(ICharacter target)
        {
            // 공격력 버프는 턴 감소 외 특별한 동작 없음
        }

        /// <summary>
        /// 현재 공격력 보너스를 반환합니다.
        /// </summary>
        /// <returns>공격력 보너스</returns>
        public int GetAttackPowerBonus()
        {
            return AttackPowerBonus;
        }
    }
}
