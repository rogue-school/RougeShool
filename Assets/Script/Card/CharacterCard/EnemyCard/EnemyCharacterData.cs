using UnityEngine;
using Game.Enemy;

namespace Game.Data
{
    /// <summary>
    /// 적 캐릭터의 능력치 및 스킬 덱을 저장하는 데이터
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character/Enemy Character Data")]
    public class EnemyCharacterData : ScriptableObject
    {
        public string displayName;
        public int maxHP;
        public EnemySkillCard[] skillDeck;

        /// <summary>
        /// 랜덤 스킬 카드 1장을 반환
        /// </summary>
        public EnemySkillCard GetRandomSkillCard()
        {
            if (skillDeck == null || skillDeck.Length == 0)
                return null;

            int index = Random.Range(0, skillDeck.Length);
            return skillDeck[index];
        }

        /// <summary>
        /// 카드별 공격력 수치를 계산 (데이터에 따라 확장 가능)
        /// </summary>
        public int GetSkillPowerFor(EnemySkillCard card)
        {
            return Random.Range(5, 15); // 임시 기본값
        }
    }
}
