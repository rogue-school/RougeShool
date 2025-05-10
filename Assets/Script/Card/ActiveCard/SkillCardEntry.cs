using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 캐릭터가 보유한 스킬 카드와 해당 카드의 데미지를 정의합니다.
    /// </summary>
    [System.Serializable]
    public class SkillCardEntry
    {
        public ScriptableObject card;  // PlayerSkillCard or EnemySkillCard
        public int damage;
    }
}
