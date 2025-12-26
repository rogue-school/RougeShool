using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시공의 폭풍 디버프 효과 클래스입니다.
    /// 플레이어에게 적용되며, 3턴 동안 목표 데미지를 입혀야 하는 기믹입니다.
    /// </summary>
    public class StormOfSpaceTimeDebuff : OwnTurnEffectBase
    {
        /// <summary>
        /// 목표 데미지 수치
        /// </summary>
        public int TargetDamage { get; private set; }

        /// <summary>
        /// 현재 누적된 데미지
        /// </summary>
        public int AccumulatedDamage { get; private set; }

        /// <summary>
        /// 목표 달성 여부
        /// </summary>
        public bool IsTargetAchieved => AccumulatedDamage >= TargetDamage;

        /// <summary>
        /// 시공의 폭풍 디버프를 생성합니다.
        /// </summary>
        /// <param name="targetDamage">목표 데미지 수치</param>
        /// <param name="duration">지속 턴 수 (기본값: 3)</param>
        /// <param name="icon">UI 아이콘</param>
        public StormOfSpaceTimeDebuff(int targetDamage, int duration = 3, Sprite icon = null) : base(duration, icon)
        {
            TargetDamage = targetDamage;
            AccumulatedDamage = 0;
        }

        /// <summary>
        /// 데미지를 누적합니다.
        /// </summary>
        /// <param name="damage">추가할 데미지</param>
        public void AddDamage(int damage)
        {
            if (damage > 0)
            {
                AccumulatedDamage += damage;
                Game.CoreSystem.Utility.GameLogger.LogInfo(
                    $"[StormOfSpaceTimeDebuff] 데미지 누적: {AccumulatedDamage}/{TargetDamage} (+{damage})",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 턴 감소 시 목표 달성 여부를 확인합니다.
        /// </summary>
        /// <param name="target">시공의 폭풍 디버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            if (IsTargetAchieved)
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo(
                    $"[StormOfSpaceTimeDebuff] {target.GetCharacterName()} 목표 달성! ({AccumulatedDamage}/{TargetDamage})",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                // 목표 달성 시 추가 처리 (필요 시)
            }
            else if (RemainingTurns <= 0)
            {
                Game.CoreSystem.Utility.GameLogger.LogWarning(
                    $"[StormOfSpaceTimeDebuff] {target.GetCharacterName()} 목표 미달성! ({AccumulatedDamage}/{TargetDamage})",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                // 목표 미달성 시 페널티 처리 (필요 시)
            }
        }
    }
}

