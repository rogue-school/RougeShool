using UnityEngine;
using Game.Interface;
using Game.Effect;
using Game.Characters;

namespace Game.Cards
{
    /// <summary>
    /// 턴마다 체력을 회복하는 재생 효과입니다. (영구 지속)
    /// </summary>
    [CreateAssetMenu(menuName = "CardEffects/RegenEffect")]
    public class RegenEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int power)
        {
            target.RegisterPerTurnEffect(new RegenEffect(power));
        }
        public string GetEffectDescription()
        {
            return $"턴마다 체력 {1} 회복 (영구 효과)";
        }
    }

    public class RegenEffect : IPerTurnEffect
    {
        private readonly int amount;

        public RegenEffect(int amount)
        {
            this.amount = amount;
        }

        public void OnTurnStart(ICharacter owner)
        {
            if (!owner.IsDead())
            {
                owner.Heal(amount);
            }
        }

        public bool IsExpired => false; // 무제한 지속
    }
}
