using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(fileName = "RegenEffect", menuName = "SkillEffects/RegenEffect")]
    public class RegenEffectSO : SkillCardEffectSO
    {
        [SerializeField] private int healPerTurn;
        [SerializeField] private int duration;

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new RegenEffectCommand(healPerTurn + power, duration);
        }
    
    public override void ApplyEffect(ICardExecutionContext context, int value, ITurnStateController controller = null)
        {
            var regen = new RegenEffect(value, 2);
            context.Target?.RegisterPerTurnEffect(regen);
            Debug.Log($"[RegenEffectSO] {context.Target?.GetCharacterName()}에게 재생 {value} 적용");
        }
    }
}