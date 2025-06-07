using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(fileName = "BleedEffect", menuName = "SkillEffects/BleedEffect")]
    public class BleedEffectSO : SkillCardEffectSO
    {
        [SerializeField] private int bleedAmount;
        [SerializeField] private int duration;

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new BleedEffectCommand(bleedAmount + power, duration);
        }

        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            var bleed = new BleedEffect(value, duration); // duration 필드 사용
            context.Target?.RegisterPerTurnEffect(bleed);
            Debug.Log($"[BleedEffectSO] {context.Target?.GetCharacterName()}에게 출혈 {value} 적용 (지속 {duration}턴)");
        }
    }
}
