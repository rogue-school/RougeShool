using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 스킬 카드를 실행하여 그 안의 이펙트들을 순차적으로 실행하는 서비스.
    /// </summary>
    public class CardExecutor : ICardExecutor
    {
        public void Execute(ISkillCard card, ICardExecutionContext context, ITurnStateController controller)
        {
            if (card == null || context == null)
            {
                Debug.LogError("[CardExecutor] 카드 또는 컨텍스트가 null입니다.");
                return;
            }

            List<SkillCardEffectSO> effects = card.CreateEffects() as List<SkillCardEffectSO>;

            if (effects == null || effects.Count == 0)
            {
                Debug.LogWarning($"[CardExecutor] 카드 '{card.GetCardName()}' 에 연결된 이펙트가 없습니다.");
                return;
            }

            int power = card.GetEffectPower(null); // 기본적으로 SkillCardData.Damage 사용

            Debug.Log($"[CardExecutor] {card.GetCardName()} → 효과 {effects.Count}개 실행 (power: {power})");

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                ICardEffectCommand command = effect.CreateEffectCommand(power);
                command.Execute(context, controller);
            }
        }
    }
}
