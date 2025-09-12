using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;

namespace Game.AnimationSystem.Data
{
    [System.Serializable]
    public class PlayerSkillCardAnimationEntry
    {
        [Header("스킬카드 정의")]
        [Tooltip("애니메이션을 지정할 스킬카드 정의")]
        [SerializeField] private SkillCardDefinition skillCardDefinition;
        public SkillCardDefinition SkillCardDefinition => skillCardDefinition;

        [Header("생성 애니메이션")]
        [Tooltip("스킬카드 생성 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings spawnAnimation;
        public SkillCardAnimationSettings SpawnAnimation => spawnAnimation;

        [Header("사용 애니메이션")]
        [Tooltip("스킬카드 사용 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings useAnimation;
        public SkillCardAnimationSettings UseAnimation => useAnimation;

        [Header("드래그 애니메이션")]
        [Tooltip("스킬카드 드래그 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings dragAnimation;
        public SkillCardAnimationSettings DragAnimation => dragAnimation;

        [Header("드랍 애니메이션")]
        [Tooltip("스킬카드 드랍 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings dropAnimation;
        public SkillCardAnimationSettings DropAnimation => dropAnimation;

        [Header("소멸 애니메이션")]
        [Tooltip("스킬카드 소멸 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings vanishAnimation;
        public SkillCardAnimationSettings VanishAnimation => vanishAnimation;

        public SkillCardAnimationSettings GetSettingsByType(string type)
        {
            // 대소문자 구분 없이 처리
            string lowerType = type?.ToLower();
            switch(lowerType)
            {
                case "spawn": return SpawnAnimation;
                case "use": return UseAnimation;
                case "drag": return DragAnimation;
                case "drop": return DropAnimation;
                case "vanish": return VanishAnimation;
                default: return null;
            }
        }
    }

    [System.Serializable]
    public class EnemySkillCardAnimationEntry
    {
        [Header("적 스킬카드 SO")]
        [Tooltip("애니메이션을 지정할 적 스킬카드 SO (EnemySkillCard만 선택)")]
        [SerializeField] private EnemySkillCard enemySkillCard;
        public EnemySkillCard EnemySkillCard => enemySkillCard;
        
        [Header("생성 애니메이션")]
        [Tooltip("스킬카드 생성 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings spawnAnimation;
        public SkillCardAnimationSettings SpawnAnimation => spawnAnimation;

        [Header("이동 애니메이션")]
        [Tooltip("스킬카드 이동 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings moveAnimation;
        public SkillCardAnimationSettings MoveAnimation => moveAnimation;

        [Header("전투슬롯 이동 애니메이션")]
        [Tooltip("전투슬롯으로 이동 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings moveToCombatSlotAnimation;
        public SkillCardAnimationSettings MoveToCombatSlotAnimation => moveToCombatSlotAnimation;

        [Header("사용 애니메이션")]
        [Tooltip("스킬카드 사용 시 애니메이션")]
        [SerializeField] private SkillCardAnimationSettings useAnimation;
        public SkillCardAnimationSettings UseAnimation => useAnimation;

        [Header("소멸 애니메이션")]
        [Tooltip("스킬카드 소멸 시 애니메이션 (적 캐릭터가 죽었을 때)")]
        [SerializeField] private SkillCardAnimationSettings vanishAnimation;
        public SkillCardAnimationSettings VanishAnimation => vanishAnimation;

        public SkillCardAnimationSettings GetSettingsByType(string type)
        {
            // 대소문자 구분 없이 처리
            string lowerType = type?.ToLower();
            switch(lowerType)
            {
                case "spawn": return SpawnAnimation;
                case "move": return MoveAnimation;
                case "movetocombatslot": return MoveToCombatSlotAnimation;
                case "use": return UseAnimation;
                case "vanish": return VanishAnimation;
                default: return null;
            }
        }
    }
} 