using UnityEngine;

namespace Game.SkillCardSystem.Factory
{
    /// <summary>
    /// 캐릭터가 보유한 스킬 카드 및 해당 카드의 기본 데미지를 정의하는 구조입니다.
    /// 카드 객체는 PlayerSkillCard 또는 EnemySkillCard ScriptableObject를 참조합니다.
    /// </summary>
    [System.Serializable]
    public class SkillCardEntry
    {
        /// <summary>
        /// 참조할 카드 ScriptableObject입니다.
        /// PlayerSkillCard 또는 EnemySkillCard 중 하나입니다.
        /// </summary>
        [Tooltip("PlayerSkillCard 또는 EnemySkillCard를 참조")]
        public ScriptableObject card;

        /// <summary>
        /// 이 카드가 기본적으로 가질 데미지 수치입니다.
        /// </summary>
        [Tooltip("카드 기본 데미지")]
        public int damage;
    }
}
