using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "Game/CardEffects/RegenEffect")]
    public class RegenEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int power, ITurnStateController controller = null)
        {
            target.RegisterPerTurnEffect(new RegenEffect(power));
        }

        public string GetEffectName()
        {
            return "Regen";
        }

        public string GetDescription()
        {
            return "턴마다 일정량의 체력을 회복합니다. (영구 효과)";
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

        public bool IsExpired => false;
    }
}
