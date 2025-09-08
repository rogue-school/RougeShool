using UnityEngine;
using Game.SkillCardSystem.Effect;
using Game.SkillCardSystem.Data;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Core
{
    /// <summary>
    /// 플레이어가 사용하는 스킬 카드의 데이터를 보관하는 ScriptableObject입니다.
    /// 기본 정보와 카드 효과를 포함합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/SkillCard/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject
    {
        [field: Header("카드 데이터")]
        /// <summary>
        /// 카드의 기본 정보 (이름, 쿨타임 등)
        /// </summary>
        [field: SerializeField]
        public SkillCardData CardData { get; private set; }

        [Header("카드 효과 목록")]
        [SerializeField]
        private List<SkillCardEffectSO> effects = new();

        /// <summary>
        /// 카드의 쿨타임을 반환합니다.
        /// </summary>
        public int GetCoolTime() => CardData?.CoolTime ?? 0;

        /// <summary>
        /// 카드 실행 시 사용할 효과 리스트를 생성합니다.
        /// </summary>
        public List<SkillCardEffectSO> CreateEffects() => effects;
    }
}
