using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Deck;
using Game.CharacterSystem.Effect;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 적 캐릭터의 페이즈 정보를 담는 데이터 클래스입니다.
    /// 각 페이즈는 체력 임계값, 최대 체력, 덱, 이펙트 등을 가질 수 있습니다.
    /// </summary>
    [System.Serializable]
    public class EnemyPhaseData
    {
        [Header("페이즈 기본 정보")]
        [Tooltip("페이즈 이름 (예: \"1페이즈\", \"2페이즈\") - 표시용 이름입니다")]
        public string phaseName = "페이즈 1";

        [Tooltip("이전 페이즈의 체력 기준으로 이 페이즈로 전환되는 임계값 (0.0 ~ 1.0, 예: 0.2 = 20% 이하)\n" +
                 "Phases[0] (2페이즈)의 임계값은 1페이즈(기본 정보)의 체력 기준으로 계산됩니다.\n" +
                 "Phases[1] (3페이즈)의 임계값은 2페이즈의 체력 기준으로 계산됩니다.")]
        [Range(0f, 1f)]
        public float healthThreshold = 1.0f;

        [Header("페이즈별 표시 정보 (선택적)")]
        [Tooltip("이 페이즈에서 표시할 이름 (null이면 기본 DisplayName 사용)")]
        public string phaseDisplayName;

        [Tooltip("이 페이즈에서 사용할 인덱스 아이콘 (null이면 기본 IndexIcon 사용)")]
        public Sprite phaseIndexIcon;

        [Tooltip("이 페이즈에서 사용할 Portrait 프리팹 (null이면 기본 PortraitPrefab 사용)")]
        public GameObject phasePortraitPrefab;

        [Header("페이즈 체력 설정")]
        [Tooltip("이 페이즈의 최대 체력 (0이면 기본 MaxHP 유지)")]
        public int phaseMaxHP = 0;

        [Header("페이즈 전용 덱")]
        [Tooltip("이 페이즈에서 사용할 스킬 덱 (null이면 기본 덱 사용)")]
        public EnemySkillDeck phaseDeck;

        [Header("페이즈 전용 이펙트")]
        [Tooltip("이 페이즈로 전환될 때 적용할 캐릭터 이펙트 목록")]
        public List<CharacterEffectEntry> phaseEffects = new List<CharacterEffectEntry>();

        [Header("페이즈 전환 연출")]
        [Tooltip("페이즈 전환 시 재생할 VFX 프리팹 (선택적)")]
        public GameObject phaseTransitionVFX;

        [Tooltip("페이즈 전환 시 재생할 SFX 클립 (선택적)")]
        public AudioClip phaseTransitionSFX;

        /// <summary>
        /// 현재 체력 비율이 이 페이즈의 임계값 이하인지 확인합니다.
        /// </summary>
        /// <param name="currentHP">현재 체력</param>
        /// <param name="maxHP">최대 체력</param>
        /// <returns>임계값 이하이면 true</returns>
        public bool IsThresholdReached(int currentHP, int maxHP)
        {
            if (maxHP <= 0) return false;
            float healthRatio = (float)currentHP / maxHP;
            return healthRatio <= healthThreshold;
        }

        /// <summary>
        /// 페이즈 데이터가 유효한지 확인합니다.
        /// </summary>
        /// <returns>유효하면 true</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(phaseName) && healthThreshold >= 0f && healthThreshold <= 1f;
        }
    }
}

