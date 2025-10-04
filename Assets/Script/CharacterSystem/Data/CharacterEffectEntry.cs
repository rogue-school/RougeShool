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
    /// </summary>
    [System.Serializable]
    public class CharacterEffectCustomSettings
    {
        [Header("소환 이펙트 설정")]
        [Tooltip("소환이 발동되는 체력 비율 (0.5 = 50%)")]
        [Range(0f, 1f)]
        public float healthThreshold = 0.5f;

        [Tooltip("소환할 적 캐릭터 데이터")]
        public EnemyCharacterData summonTarget;
    }
}
