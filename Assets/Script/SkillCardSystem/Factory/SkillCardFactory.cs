using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Effects;
using System.Collections.Generic;
using UnityEngine;

public static class SkillCardFactory
{
    public static ISkillCard CreateEnemyCard(EnemySkillCard cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("[SkillCardFactory] EnemySkillCard 데이터가 null입니다.");
            return null;
        }

        var effects = CloneEffects(cardData.CreateEffects());

        return new EnemySkillCardRuntime(
            cardData.CardData,
            effects
        );
    }

    private static List<SkillCardEffectSO> CloneEffects(List<SkillCardEffectSO> original)
    {
        var clone = new List<SkillCardEffectSO>();
        foreach (var effect in original)
            clone.Add(effect); // 추후 DeepClone() 확장 가능

        return clone;
    }
}
