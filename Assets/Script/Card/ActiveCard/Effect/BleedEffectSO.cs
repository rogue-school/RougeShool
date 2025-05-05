using UnityEngine;
using Game.Interface;
using Game.Characters;
using Game.Effect;

namespace Game.Cards
{
    /// <summary>
    /// 턴마다 체력을 감소시키는 출혈 효과입니다. (지속 턴 설정 가능)
    /// </summary>
    [CreateAssetMenu(menuName = "CardEffects/BleedEffect")]
    public class BleedEffectSO : ScriptableObject, ICardEffect
    {
        [SerializeField] private int duration = 3;

        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int power)
        {
            target.RegisterPerTurnEffect(new BleedEffect(power, duration));
        }

        public string GetEffectDescription()
        {
            return $"출혈: 턴마다 체력 {1} 감소 ({duration}턴)";
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

        public void OnTurnStart(CharacterBase owner)
        {
            if (!owner.IsDead)
            {
                owner.TakeDamage(damage);
                remainingTurns--;
            }
        }

        public bool IsExpired => remainingTurns <= 0;
    }
}
