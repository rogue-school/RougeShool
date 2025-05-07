using Game.Player;
using Game.Cards;
using Game.Interface;
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
            var card = new RuntimeSkillCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                cardData.CreateEffects(), // 4번째: 효과 리스트
                damage,                   // 5번째: 공격력
                coolTime                  // 6번째: 쿨타임
            );

            return card;
        }

        public static ISkillCard CreateEnemyCard(EnemySkillCard cardData, int damage)
        {
            var card = new RuntimeSkillCard(
                cardData.GetCardName(),
                cardData.GetDescription(),
                cardData.GetArtwork(),
                cardData.CreateEffects(), // 4번째: 효과 리스트
                damage,                   // 5번째: 공격력
                0                         // 6번째: 쿨타임 없음
            );

            return card;
        }
    }
}
