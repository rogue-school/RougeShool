using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.Runtime;
using Game.CombatSystem.Slot;

namespace Game.SkillCardSystem.Factory
{
    /// <summary>
    /// 카드 데이터(ScriptableObject)와 외부 수치를 결합하여
    /// 런타임 카드 인스턴스를 생성하는 팩토리입니다.
    /// </summary>
    public static class SkillCardFactory
    {
        /// <summary>
        /// 플레이어 카드 생성
        /// </summary>
        public static ISkillCard CreatePlayerCard(PlayerSkillCard cardData, int damage, int coolTime)
        {
            if (cardData == null)
            {
                Debug.LogError("[SkillCardFactory] PlayerSkillCard 데이터가 null입니다.");
                return null;
            }

            var effects = CloneEffects(cardData.CreateEffects());

            var card = CreateRuntimeCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                effects,
                damage,
                coolTime,
                SlotOwner.PLAYER
            );

            Debug.Log($"[SkillCardFactory] 플레이어 카드 생성 완료 → {cardData.GetCardName()}");
            return card;
        }

        /// <summary>
        /// 적 카드 생성 (쿨타임은 항상 0)
        /// </summary>
        public static ISkillCard CreateEnemyCard(EnemySkillCard cardData, int damage)
        {
            if (cardData == null)
            {
                Debug.LogError("[SkillCardFactory] EnemySkillCard 데이터가 null입니다.");
                return null;
            }

            var effects = CloneEffects(cardData.CreateEffects());

            var card = CreateRuntimeCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                effects,
                damage,
                0,
                SlotOwner.ENEMY
            );

            Debug.Log($"[SkillCardFactory] 적 카드 생성 완료 → {cardData.GetCardName()}");
            return card;
        }

        /// <summary>
        /// 런타임 카드 생성 공통 처리
        /// </summary>
        private static ISkillCard CreateRuntimeCard(
            string name,
            string description,
            Sprite artwork,
            List<ICardEffect> effects,
            int power,
            int coolTime,
            SlotOwner owner
        )
        {
            return new RuntimeSkillCard(name, description, artwork, effects, power, coolTime, owner);
        }

        /// <summary>
        /// 카드 효과 목록 복제
        /// </summary>
        private static List<ICardEffect> CloneEffects(List<ICardEffect> original)
        {
            if (original == null)
                return new List<ICardEffect>();

            var clone = new List<ICardEffect>(original.Count);
            foreach (var effect in original)
                clone.Add(effect); // 필요 시 ICardEffect.Clone() 도입 가능

            return clone;
        }
    }
}
