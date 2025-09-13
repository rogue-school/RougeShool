using UnityEngine;
using Game.SkillCardSystem.Data;

namespace Game.AnimationSystem.Data
{
    /// <summary>
    /// 통합된 스킬카드 애니메이션 엔트리
    /// 플레이어와 적 스킬카드의 모든 애니메이션 타입을 포함합니다.
    /// </summary>
    [System.Serializable]
    public class UnifiedSkillCardAnimationEntry
    {
        [Header("스킬카드 정의")]
        [Tooltip("애니메이션을 지정할 스킬카드 정의")]
        [SerializeField] private SkillCardDefinition skillCardDefinition;
        public SkillCardDefinition SkillCardDefinition => skillCardDefinition;

        [Header("소유자 정책")]
        [Tooltip("이 애니메이션이 적용되는 소유자")]
        [SerializeField] private OwnerPolicy ownerPolicy = OwnerPolicy.Shared;
        public OwnerPolicy OwnerPolicy => ownerPolicy;

        [Header("생성 애니메이션")]
        [Tooltip("스킬카드 생성 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings spawnAnimation;
        public SkillCardAnimationSettings SpawnAnimation => spawnAnimation;

        [Header("이동 애니메이션 (적 전용)")]
        [Tooltip("스킬카드 이동 시 애니메이션 (적 카드의 핸드 슬롯 간 이동)")]
        [SerializeField] private SkillCardAnimationSettings moveAnimation;
        public SkillCardAnimationSettings MoveAnimation => moveAnimation;

        [Header("전투슬롯 이동 애니메이션 (적 전용)")]
        [Tooltip("전투슬롯으로 이동 시 애니메이션 (적 카드의 전투 슬롯 등록)")]
        [SerializeField] private SkillCardAnimationSettings moveToCombatSlotAnimation;
        public SkillCardAnimationSettings MoveToCombatSlotAnimation => moveToCombatSlotAnimation;

        [Header("사용 애니메이션")]
        [Tooltip("스킬카드 사용 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings useAnimation;
        public SkillCardAnimationSettings UseAnimation => useAnimation;

        [Header("드래그 애니메이션 (플레이어 전용)")]
        [Tooltip("스킬카드 드래그 시 애니메이션 (플레이어 카드의 드래그 시작)")]
        [SerializeField] private SkillCardAnimationSettings dragAnimation;
        public SkillCardAnimationSettings DragAnimation => dragAnimation;

        [Header("드랍 애니메이션 (플레이어 전용)")]
        [Tooltip("스킬카드 드랍 시 애니메이션 (플레이어 카드의 드롭 완료)")]
        [SerializeField] private SkillCardAnimationSettings dropAnimation;
        public SkillCardAnimationSettings DropAnimation => dropAnimation;

        [Header("소멸 애니메이션")]
        [Tooltip("스킬카드 소멸 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings vanishAnimation;
        public SkillCardAnimationSettings VanishAnimation => vanishAnimation;

        /// <summary>
        /// 애니메이션 타입에 따른 설정을 반환합니다.
        /// </summary>
        public SkillCardAnimationSettings GetSettingsByType(string type)
        {
            if (string.IsNullOrEmpty(type)) return null;
            
            string lowerType = type.ToLower();
            return lowerType switch
            {
                "spawn" => SpawnAnimation,
                "move" => MoveAnimation,
                "movetocombatslot" => MoveToCombatSlotAnimation,
                "use" => UseAnimation,
                "drag" => DragAnimation,
                "drop" => DropAnimation,
                "vanish" => VanishAnimation,
                _ => null
            };
        }

        /// <summary>
        /// 소유자 정책에 따라 애니메이션을 사용할 수 있는지 확인합니다.
        /// </summary>
        public bool CanUseAnimation(Owner cardOwner, string animationType)
        {
            // 소유자 정책 확인
            if (ownerPolicy != OwnerPolicy.Shared)
            {
                if (ownerPolicy == OwnerPolicy.Player && cardOwner != Owner.Player) return false;
                if (ownerPolicy == OwnerPolicy.Enemy && cardOwner != Owner.Enemy) return false;
            }

            // 애니메이션 타입별 소유자 제한 (실제 사용 패턴에 따라)
            string lowerType = animationType.ToLower();
            return lowerType switch
            {
                "drag" or "drop" => cardOwner == Owner.Player, // 드래그/드롭은 플레이어 전용
                "move" or "movetocombatslot" => cardOwner == Owner.Enemy, // 이동 애니메이션은 적 전용
                _ => true // 나머지는 모든 소유자 사용 가능
            };
        }
    }
}
