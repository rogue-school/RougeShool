using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Effect; // ⬅️ 누락되었던 부분

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(fileName = "BleedEffect", menuName = "SkillEffects/BleedEffect")]
    public class BleedEffectSO : SkillCardEffectSO
    {
        [SerializeField] private int bleedAmount;
        [SerializeField] private int duration;

        public override void ApplyEffect(ICardExecutionContext context, int power, ITurnStateController controller)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[BleedEffectSO] 대상이 null입니다.");
                return;
            }

            var effect = new BleedEffect(bleedAmount + power, duration);
            context.Target.RegisterPerTurnEffect(effect);

            Debug.Log($"[BleedEffectSO] {context.Target.GetCharacterName()}에게 출혈 효과 적용: {bleedAmount + power} (지속 {duration}턴)");
        }
    }
}
