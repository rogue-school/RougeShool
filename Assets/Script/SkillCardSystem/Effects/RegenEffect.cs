using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 일정 턴 동안 대상에게 체력 회복 효과를 부여하는 지속 효과입니다.
    /// </summary>
    public class RegenEffect : IPerTurnEffect
    {
        private readonly int healAmount;
        private int remainingTurns;

        /// <summary>
        /// 회복 효과를 초기화합니다.
        /// </summary>
        /// <param name="healAmount">턴마다 회복할 체력량</param>
        /// <param name="duration">지속 턴 수</param>
        public RegenEffect(int healAmount, int duration)
        {
            this.healAmount = healAmount;
            this.remainingTurns = duration;
        }

        /// <summary>
        /// 효과가 만료되었는지 여부를 반환합니다.
        /// </summary>
        public bool IsExpired => remainingTurns <= 0;

        /// <summary>
        /// 턴 시작 시 회복 효과를 적용합니다.
        /// </summary>
        /// <param name="target">회복 효과를 적용할 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null)
            {
                Debug.LogWarning("[RegenEffect] 대상 캐릭터가 null입니다.");
                return;
            }

            target.Heal(healAmount);
            remainingTurns--;

            Debug.Log($"[RegenEffect] {target.GetCharacterName()} 체력 {healAmount} 회복됨 (남은 턴: {remainingTurns})");
        }
    }
}
