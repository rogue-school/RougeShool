using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Core
{
    /// <summary>
    /// 적이 사용하는 스킬 카드 데이터입니다.
    /// 카드 기본 정보 및 적용될 이펙트 목록을 포함합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/SkillCard/Enemy Skill Card")]
    public class EnemySkillCard : ScriptableObject
    {

        [Header("카드 정의")]
        /// <summary>
        /// 카드의 정의 정보
        /// </summary>
        [field: SerializeField]
        public SkillCardDefinition CardDefinition { get; private set; }

        [Header("카드 실행 시 적용할 효과 목록")]
        [SerializeField]
        private List<SkillCardEffectSO> effects = new();

        /// <summary>
        /// 카드 실행 시 적용할 이펙트 리스트를 반환합니다.
        /// </summary>
        /// <returns>적용할 효과 목록</returns>
        public List<SkillCardEffectSO> CreateEffects() => effects;

        /// <summary>
        /// 카드의 정의 객체를 반환합니다.
        /// </summary>
        public SkillCardDefinition GetCardDefinition() => CardDefinition;

        /// <summary>
        /// 카드의 이름을 반환합니다.
        /// </summary>
        public string GetCardName() => CardDefinition?.displayName ?? "Unnamed Card";

        /// <summary>
        /// 카드의 데미지 수치를 반환합니다.
        /// </summary>
        public int GetDamage() => CardDefinition?.configuration.hasDamage == true ? CardDefinition.configuration.damageConfig.baseDamage : 0;

        /// <summary>
        /// 카드의 쿨타임 값을 반환합니다. (새 구조에서는 쿨타임 제거됨)
        /// </summary>
        public int GetCoolTime() => 0;
    }
}
