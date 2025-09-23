using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 일정 턴 동안 대상에게 매 턴 피해를 입히는 출혈 효과입니다.
    /// </summary>
    public class BleedEffect : IPerTurnEffect
    {
        private readonly int amount;
        private int remainingTurns;
        private readonly Sprite icon;

        /// <summary>
        /// 출혈 효과 생성자
        /// </summary>
        /// <param name="amount">매 턴 입힐 피해량</param>
        /// <param name="duration">지속 턴 수</param>
        public BleedEffect(int amount, int duration, Sprite icon = null)
        {
            this.amount = amount;
            this.remainingTurns = duration;
            this.icon = icon;
        }

        /// <summary>
        /// 출혈 효과가 만료되었는지 여부를 반환합니다.
        /// </summary>
        public bool IsExpired => remainingTurns <= 0;
        public int RemainingTurns => remainingTurns;
        public Sprite Icon => icon;

        /// <summary>
        /// 턴 시작 시 대상에게 피해를 입히고 남은 턴을 감소시킵니다.
        /// </summary>
        /// <param name="target">출혈 피해를 받을 대상 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null)
            {
                Debug.LogWarning("[BleedEffect] 대상이 null입니다. 출혈 효과 무시됨.");
                return;
            }
            target.TakeDamage(amount);
            remainingTurns--;

            Debug.Log($"[BleedEffect] {target.GetCharacterName()} 출혈 피해: {amount} (남은 턴: {remainingTurns})");
        }
    }
}
