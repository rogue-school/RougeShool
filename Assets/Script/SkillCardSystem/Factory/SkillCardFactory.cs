using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using System.Collections.Generic;
using UnityEngine;

public static class SkillCardFactory
{
    public static ISkillCard CreateEnemyCard(EnemySkillCard cardData, int damage)
    {
        if (cardData == null)
        {
            Debug.LogError("[SkillCardFactory] EnemySkillCard 데이터가 null입니다.");
            return null;
        }

        var effects = CloneEffects(cardData.CreateEffects());

        return new EnemySkillCardRuntime(
            cardData.CardData,
            effects,
            damage
        );
    }

    private static List<ICardEffect> CloneEffects(List<ICardEffect> original)
    {
        var clone = new List<ICardEffect>();
        foreach (var effect in original)
            clone.Add(effect); // 향후 Clone() 확장 가능

        return clone;
    }
}
