using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Data
{
    /// <summary>
    /// 스킬카드의 통합 정의 ScriptableObject입니다.
    /// 필수 정보, 연출 구성, 선택적 구성(데미지/효과)을 포함합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "SkillCardSystem/SkillCardDefinition", fileName = "SkillCardDefinition")]
    public class SkillCardDefinition : ScriptableObject
    {
        [Header("필수 정보")]
        [Tooltip("카드 고유 식별자")]
        public string cardId;
        
        [Tooltip("카드 표시 이름")]
        public string displayName;
        
        [Tooltip("카드 표시 이름 (한국어)")]
        public string displayNameKO;
        
        [Tooltip("카드 설명")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("카드 아트워크")]
        public Sprite artwork;
        
        // 기존 시스템 호환성을 위한 프로퍼티들
        public string id => cardId;
        public new string name => displayName;
        
        [Header("연출 구성")]
        [Tooltip("카드 연출 설정")]
        public CardPresentation presentation = new();
        
        [Header("선택적 구성")]
        [Tooltip("카드 구성 설정")]
        public CardConfiguration configuration = new();
        
        // 기존 시스템 호환성을 위한 메서드들
        public List<SkillCardEffectSO> CreateEffects()
        {
            return configuration.hasEffects ? 
                configuration.effects.ConvertAll(e => e.effectSO) : 
                new List<SkillCardEffectSO>();
        }
        
        public SkillCardDefinition Definition => this;
        public SkillCardDefinition Card => this;
        
        // 기존 시스템 호환성을 위한 추가 프로퍼티들
        public AudioClip SfxClip => presentation.sfxClip;
        public GameObject VisualEffectPrefab => presentation.visualEffectPrefab;
        public float EffectDuration => presentation.effectDuration;
        public string Name => displayName;
        public string CardId => cardId;
        public string CardName => displayName;
        public string Description => description;
        public int Cost => 0; // 비용 제거됨
        public string CardType => "SkillCard"; // 기본 타입
    }

    /// <summary>
    /// 카드의 연출 관련 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class CardPresentation
    {
        [Header("사운드")]
        [Tooltip("카드 사용 시 재생할 사운드")]
        public AudioClip sfxClip;
        
        [Tooltip("사운드 볼륨 (0.0 ~ 1.0)")]
        [Range(0f, 1f)]
        public float sfxVolume = 1.0f;
        
        [Tooltip("카드 사용 시작 시 즉시 재생")]
        public bool playOnStart = true;
        
        [Header("비주얼 이펙트")]
        [Tooltip("카드 사용 시 생성할 비주얼 이펙트 프리팹")]
        public GameObject visualEffectPrefab;
        
        [Tooltip("이펙트 지속 시간 (초)")]
        public float effectDuration = 2.0f;
        
        [Tooltip("대상 기준 이펙트 오프셋")]
        public Vector3 effectOffset = Vector3.zero;
        
        [Tooltip("대상을 따라 이동")]
        public bool followTarget = false;
        
        [Header("애니메이션")]
        [Tooltip("카드 애니메이션")]
        public AnimationClip cardAnimation;
        
        [Tooltip("대상 애니메이션")]
        public AnimationClip targetAnimation;
        
        [Tooltip("애니메이션 재생 속도")]
        public float animationSpeed = 1.0f;
        
        [Header("연출 타이밍")]
        [Tooltip("연출 타이밍 설정")]
        public PresentationTiming timing = new();
    }

    /// <summary>
    /// 연출 요소들의 타이밍을 제어하는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class PresentationTiming
    {
        [Tooltip("사운드 재생 지연 시간 (초)")]
        public float sfxDelay = 0f;
        
        [Tooltip("비주얼 이펙트 생성 지연 시간 (초)")]
        public float visualEffectDelay = 0f;
        
        [Tooltip("애니메이션 재생 지연 시간 (초)")]
        public float animationDelay = 0f;
        
        [Tooltip("애니메이션 완료까지 대기")]
        public bool waitForAnimation = false;
    }

    /// <summary>
    /// 카드의 게임 로직 구성 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class CardConfiguration
    {
        [Header("데미지 구성 (선택적)")]
        [Tooltip("데미지가 있는 카드인지")]
        public bool hasDamage = false;
        
        [Tooltip("데미지 설정")]
        public DamageConfiguration damageConfig = new();
        
        [Header("효과 구성 (선택적)")]
        [Tooltip("추가 효과가 있는 카드인지")]
        public bool hasEffects = false;
        
        [Tooltip("효과 목록")]
        public List<EffectConfiguration> effects = new();
        
        [Header("소유자 정책")]
        [Tooltip("카드 소유자 정책")]
        public OwnerPolicy ownerPolicy = OwnerPolicy.Shared;
        
        // 기존 시스템 호환성을 위한 프로퍼티들
        public List<SkillCardEffectSO> Effects => hasEffects ? this.effects.ConvertAll(e => e.effectSO) : new List<SkillCardEffectSO>();
        public string targetRule => "Enemy"; // 기본값
    }

    /// <summary>
    /// 데미지 관련 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class DamageConfiguration
    {
        [Tooltip("기본 데미지")]
        public int baseDamage = 0;
        
        [Tooltip("히트 수")]
        public int hits = 1;
        
        [Tooltip("가드 관통 여부")]
        public bool pierceable = false;
        
        [Tooltip("크리티컬 확률 (0.0 ~ 1.0)")]
        [Range(0f, 1f)]
        public float critChance = 0f;
    }

    /// <summary>
    /// 효과 구성 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class EffectConfiguration
    {
        [Tooltip("효과 SO")]
        public SkillCardEffectSO effectSO;
        
        [Tooltip("커스텀 설정 사용 여부")]
        public bool useCustomSettings = false;
        
        [Tooltip("커스텀 설정")]
        public EffectCustomSettings customSettings = new();
        
        [Tooltip("실행 순서 (낮을수록 먼저 실행)")]
        public int executionOrder = 0;
        
        // 기존 시스템 호환성을 위한 프로퍼티
        public int order => executionOrder;
    }

    /// <summary>
    /// 효과의 커스텀 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class EffectCustomSettings
    {
        [Header("데미지 효과 설정")]
        [Tooltip("데미지량")]
        public int damageAmount = 0;
        
        [Tooltip("히트 수")]
        public int damageHits = 1;
        
        [Tooltip("가드 관통")]
        public bool pierceable = false;
        
        [Tooltip("크리티컬 확률")]
        [Range(0f, 1f)]
        public float critChance = 0f;
        
        [Header("가드 효과 설정")]
        [Tooltip("가드량")]
        public int guardAmount = 0;
        
        [Tooltip("임시 체력으로 오버플로우")]
        public bool overflowToTempHP = false;
        
        [Header("출혈 효과 설정")]
        [Tooltip("출혈량")]
        public int bleedAmount = 0;
        
        [Tooltip("출혈 지속 시간")]
        public int bleedDuration = 0;
        
        [Header("치유 효과 설정")]
        [Tooltip("치유량")]
        public int healAmount = 0;
        
        [Header("드로우 효과 설정")]
        [Tooltip("드로우 수")]
        public int drawCount = 0;
        
        [Header("리소스 효과 설정")]
        [Tooltip("리소스 변화량")]
        public int resourceDelta = 0;
    }

    /// <summary>
    /// 카드 소유자 정책 열거형입니다.
    /// </summary>
    public enum OwnerPolicy
    {
        Shared,      // 공통 (플레이어/적 모두 사용)
        Player,      // 플레이어 전용
        Enemy        // 적 전용
    }

    /// <summary>
    /// 카드 소유자 열거형입니다.
    /// </summary>
    public enum Owner
    {
        Player,  // 플레이어
        Enemy,   // 적
        Shared   // 공용 (플레이어/적 모두 사용 가능)
    }
}