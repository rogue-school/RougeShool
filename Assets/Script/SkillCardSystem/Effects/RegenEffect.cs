using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    public class RegenEffect : IPerTurnEffect
    {
        private int healAmount;
        private int remainingTurns;

        public RegenEffect(int healAmount, int duration)
        {
            this.healAmount = healAmount;
            this.remainingTurns = duration;
        }

        public bool IsExpired => remainingTurns <= 0;

        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;

            target.Heal(healAmount);
            remainingTurns--;

            Debug.Log($"[RegenEffect] {target.GetCharacterName()} 회복: {healAmount} (남은 턴: {remainingTurns})");
        }
    }
}
