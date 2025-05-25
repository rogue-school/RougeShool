using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    public class BleedEffect : IPerTurnEffect
    {
        private int amount;
        private int remainingTurns;

        public BleedEffect(int amount, int duration)
        {
            this.amount = amount;
            this.remainingTurns = duration;
        }

        public bool IsExpired => remainingTurns <= 0;

        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;

            target.TakeDamage(amount);
            remainingTurns--;

            Debug.Log($"[BleedEffect] {target.GetCharacterName()} 출혈 피해: {amount} (남은 턴: {remainingTurns})");
        }
    }
}
