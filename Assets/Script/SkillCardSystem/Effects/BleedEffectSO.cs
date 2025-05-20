using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "Game/CardEffects/BleedEffect")]
    public class BleedEffectSO : ScriptableObject, ICardEffect
    {
        [SerializeField] private int bleedDamage = 1;
        [SerializeField] private int duration = 3;

        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int ignoredPower, ITurnStateController controller = null)
        {
            target.RegisterPerTurnEffect(new BleedEffect(bleedDamage, duration));
        }

        public string GetEffectName()
        {
            return "Bleed";
        }

        public string GetDescription()
        {
            return $"출혈: 매턴 {bleedDamage} 피해, {duration}턴 지속";
        }
    }

    public class BleedEffect : IPerTurnEffect
    {
        private readonly int damage;
        private int remainingTurns;

        public BleedEffect(int damage, int duration)
        {
            this.damage = damage;
            this.remainingTurns = duration;
        }

        public void OnTurnStart(ICharacter owner)
        {
            if (!owner.IsDead())
            {
                owner.TakeDamage(damage);
                remainingTurns--;
            }
        }

        public bool IsExpired => remainingTurns <= 0;
    }
}
