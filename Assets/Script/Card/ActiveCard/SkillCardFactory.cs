using Game.Player;
using Game.Cards;
using Game.Interface;
using System.Collections.Generic;
using Game.Enemy;

namespace Game.Utility
{
    /// <summary>
    /// 카드 데이터(ScriptableObject)와 외부 수치를 결합하여
    /// 런타임 카드 인스턴스를 생성하는 팩토리입니다.
    /// </summary>
    public static class SkillCardFactory
    {
        public static ISkillCard CreatePlayerCard(PlayerSkillCard cardData, int damage, int coolTime)
        {
            return new RuntimeSkillCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                cardData.GetEffects(),
                damage,
                coolTime
            );
        }
        public static ISkillCard CreateEnemyCard(EnemySkillCard cardData, int damage)
        {
            return new RuntimeSkillCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                cardData.GetEffects(),
                damage,
                0 // 적은 쿨타임 없음
            );
        }
    }
}
