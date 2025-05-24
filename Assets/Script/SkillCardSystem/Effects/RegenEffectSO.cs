using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(fileName = "RegenEffect", menuName = "SkillEffects/RegenEffect")]
    public class RegenEffectSO : SkillCardEffectSO
    {
        [SerializeField] private int healPerTurn;
        [SerializeField] private int duration;

        public override void ApplyEffect(ICardExecutionContext context, int power, ITurnStateController controller)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[RegenEffectSO] 대상이 null입니다.");
                return;
            }

            var totalHeal = healPerTurn + power;
            var effect = new RegenEffect(totalHeal, duration);
            context.Target.RegisterPerTurnEffect(effect);

            Debug.Log($"[RegenEffectSO] {context.Target.GetCharacterName()} 재생 효과 적용: {totalHeal} (지속 {duration}턴)");
        }
    }
}
