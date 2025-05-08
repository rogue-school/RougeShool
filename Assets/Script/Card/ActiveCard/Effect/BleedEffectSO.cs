using UnityEngine;
using Game.Effect;
using Game.Interface;
using Game.Characters;

namespace Game.Cards
{
    /// <summary>
    /// 출혈 효과를 생성하는 ScriptableObject입니다.
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

    /// <summary>
    /// 턴마다 체력을 깎는 지속 효과입니다.
    /// </summary>
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
