using Game.Player;
using Game.Cards;
using Game.Interface;
using Game.Enemy;
using System.Collections.Generic;
using UnityEngine;
using Game.Effect;

namespace Game.Utility
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
            return CreateRuntimeCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                cardData.CreateEffects(),
                damage,
                coolTime
            );
        }

        /// <summary>
        /// 적 카드 생성 (쿨타임은 항상 0)
        /// </summary>
        public static ISkillCard CreateEnemyCard(EnemySkillCard cardData, int damage)
        {
            return CreateRuntimeCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                cardData.CreateEffects(),
                damage,
                0
            );
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
            int coolTime)
        {
            return new RuntimeSkillCard(name, description, artwork, effects, power, coolTime);
        }
    }
}
