using UnityEngine;
using Game.CharacterSystem.Effect;
using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 캐릭터 이펙트 설정을 담는 클래스입니다.
    /// SkillCardDefinition의 EffectConfiguration과 동일한 구조입니다.
    /// </summary>
    [System.Serializable]
    public class CharacterEffectEntry
    {
        [Tooltip("효과 ScriptableObject")]
        public CharacterEffectSO effectSO;

        [Tooltip("커스텀 설정 사용 여부")]
        public bool useCustomSettings = false;

        [Tooltip("커스텀 설정")]
        public CharacterEffectCustomSettings customSettings = new();

        [Tooltip("실행 우선순위 (낮을수록 먼저 실행)")]
        public int priority = 0;
    }

    /// <summary>
    /// 캐릭터 이펙트의 커스텀 설정을 담는 클래스입니다.
    /// PropertyDrawer를 통해 이펙트 타입에 따라 관련 설정만 표시됩니다.
    /// </summary>
    [System.Serializable]
    public class CharacterEffectCustomSettings
    {
        [Tooltip("소환이 발동되는 체력 비율 (0.5 = 50%) - SummonEffectSO 전용")]
        [Range(0f, 1f)]
        public float healthThreshold = 0.5f;

        [Tooltip("소환할 적 캐릭터 데이터 - SummonEffectSO 전용")]
        public EnemyCharacterData summonTarget;

        [Tooltip("스킬이 발동되는 체력 임계값 (절대값, 예: 30) - TriggerSkillOnHealthEffectSO 전용")]
        public int skillHealthThreshold = 30;

        [Tooltip("체력 비율 기반 사용 여부 (true면 비율, false면 절대값) - TriggerSkillOnHealthEffectSO 전용")]
        public bool useSkillHealthRatio = false;

        [Tooltip("발동할 스킬 카드 정의 - TriggerSkillOnHealthEffectSO 전용")]
        public SkillCardSystem.Data.SkillCardDefinition skillCardDefinition;

        [Tooltip("발동할 스킬 카드 ID (레거시 호환용, skillCardDefinition이 우선) - TriggerSkillOnHealthEffectSO 전용")]
        public string skillCardId = "";

        [Header("시공의 폭풍 효과 설정")]
        [Tooltip("목표 데미지 수치 - StormOfSpaceTimeEffectSO 전용")]
        public int stormOfSpaceTimeTargetDamage = 30;

        [Tooltip("시공의 폭풍 지속 턴 수 - StormOfSpaceTimeEffectSO 전용")]
        public int stormOfSpaceTimeDuration = 3;
    }
}
