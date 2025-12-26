using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Effect;
using Game.CoreSystem.Utility;

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
        
        [Header("런타임 상태 (디버그용)")]
        [Tooltip("현재 공격력 스택 수")]
        [SerializeField] private int currentAttackPowerStacks = 0;
        
        /// <summary>
        /// 카드의 효과 목록을 생성합니다 (기존 시스템 호환성)
        /// </summary>
        /// <returns>효과 목록</returns>
        public List<SkillCardEffectSO> CreateEffects()
        {
            return configuration.hasEffects ? 
                configuration.effects.ConvertAll(e => e.effectSO) : 
                new List<SkillCardEffectSO>();
        }
        
        /// <summary>
        /// 현재 공격력 스택 수를 반환합니다.
        /// </summary>
        /// <returns>현재 스택 수</returns>
        public int GetAttackPowerStacks()
        {
            return currentAttackPowerStacks;
        }
        
        /// <summary>
        /// 공격력 스택을 증가시킵니다.
        /// </summary>
        /// <param name="maxStacks">최대 스택 수</param>
        public void IncrementAttackPowerStacks(int maxStacks = 5)
        {
            if (currentAttackPowerStacks < maxStacks)
            {
                currentAttackPowerStacks++;
                Debug.Log($"[SkillCardDefinition] '{displayName}' 스택 증가: {currentAttackPowerStacks}/{maxStacks}");
            }
            else
            {
                Debug.Log($"[SkillCardDefinition] '{displayName}' 최대 스택 도달: {currentAttackPowerStacks}/{maxStacks}");
            }
        }
        
        /// <summary>
        /// 공격력 스택을 초기화합니다.
        /// </summary>
        public void ResetAttackPowerStacks()
        {
            currentAttackPowerStacks = 0;
            GameLogger.LogDebug($"[SkillCardDefinition] '{displayName}' 스택 초기화", GameLogger.LogCategory.SkillCard);
        }
        
        /// <summary>
        /// 카드 정의 자기 자신을 반환합니다 (기존 시스템 호환성)
        /// </summary>
        public SkillCardDefinition Definition => this;
        
        /// <summary>
        /// 카드 정의 자기 자신을 반환합니다 (기존 시스템 호환성)
        /// </summary>
        public SkillCardDefinition Card => this;
        
        /// <summary>
        /// 효과음 클립 (데미지 설정에서 가져옴, 기존 시스템 호환성)
        /// </summary>
        public AudioClip SfxClip => configuration.hasDamage ? configuration.damageConfig.sfxClip : null;
        
        /// <summary>
        /// 시각 효과 프리팹 (데미지 설정에서 가져옴, 기존 시스템 호환성)
        /// </summary>
        public GameObject VisualEffectPrefab => configuration.hasDamage ? configuration.damageConfig.visualEffectPrefab : null;
        
        /// <summary>
        /// 카드 이름 (기존 시스템 호환성)
        /// </summary>
        public string Name => displayName;
        
        /// <summary>
        /// 카드 ID (기존 시스템 호환성)
        /// </summary>
        public string CardId => cardId;
        
        /// <summary>
        /// 카드 이름 (기존 시스템 호환성)
        /// </summary>
        public string CardName => displayName;
        
        /// <summary>
        /// 카드 설명 (기존 시스템 호환성)
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// 카드 비용 (현재 0, 기존 시스템 호환성)
        /// </summary>
        public int Cost => 0;
        
        /// <summary>
        /// 카드 타입 (기본값: "SkillCard", 기존 시스템 호환성)
        /// </summary>
        public string CardType => "SkillCard";
    }

    /// <summary>
    /// 카드의 연출 관련 설정을 담는 클래스입니다.
    /// 핵심 연출 요소만 포함하며, 타이밍은 각 시스템에서 관리합니다.
    /// </summary>
    [System.Serializable]
    public class CardPresentation
    {
        [Header("사운드")]
        [Tooltip("카드 사용 시 재생할 고유 사운드")]
        public AudioClip sfxClip;
        
        [Header("비주얼 이펙트")]
        [Tooltip("카드 사용 시 생성할 고유 비주얼 이펙트 프리팹")]
        public GameObject visualEffectPrefab;
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
        
        [Header("리소스 구성 (선택적)")]
        [Tooltip("자원을 소모하는 카드인지")]
        public bool hasResource = false;

        [Tooltip("자원 소모 설정")] 
        public ResourceConfiguration resourceConfig = new();

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
    /// 자원 소모 관련 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ResourceConfiguration
    {
        [Tooltip("카드 사용 시 소모할 리소스 양 (0이면 소모 없음)")]
        public int cost = 0;
    }

    /// <summary>
    /// 데미지 관련 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class DamageConfiguration
    {
        [Header("데미지 수치")]
        [Tooltip("기본 데미지")]
        public int baseDamage = 0;
        
        [Tooltip("공격 횟수")]
        public int hits = 1;
        
        [Tooltip("방어 무효화 여부")]
        public bool ignoreGuard = false;

        [Tooltip("반격 버프 무효화 여부")]
        public bool ignoreCounter = false;

        [Header("랜덤 데미지 설정")]
        [Tooltip("랜덤 데미지를 사용할지 여부 (true이면 최소/최대값 사이에서 무작위 선택)")]
        public bool useRandomDamage = false;

        [Tooltip("랜덤 데미지 최소값")]
        public int minDamage = 0;

        [Tooltip("랜덤 데미지 최대값")]
        public int maxDamage = 0;

        [Header("데미지 이펙트/사운드")]
        [Tooltip("데미지 공격 시 재생할 사운드")]
        public AudioClip sfxClip;

        [Tooltip("데미지 공격 시 재생할 비주얼 이펙트 프리팹")]
        public GameObject visualEffectPrefab;
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
        
        [Tooltip("공격 횟수")]
        public int damageHits = 1;
        
        [Tooltip("방어 무효화")]
        public bool ignoreGuard = false;

        [Tooltip("반격 무효화")]
        public bool ignoreCounter = false;
        
        
        [Header("출혈 효과 설정")]
        [Tooltip("출혈량")]
        public int bleedAmount = 0;
        
        [Tooltip("출혈 지속 시간")]
        public int bleedDuration = 0;

        [Header("출혈 효과 아이콘/이펙트/사운드")]
        [Tooltip("출혈 효과 UI 아이콘")]
        public Sprite bleedIcon;

        [Tooltip("출혈 효과 적용 시 재생할 비주얼 이펙트 프리팹")]
        public GameObject bleedActivateEffectPrefab;

        [Tooltip("출혈 효과 적용 시 재생할 사운드")]
        public AudioClip bleedActivateSfxClip;

        [Tooltip("출혈 피해 발생 시 매 턴 재생할 비주얼 이펙트 프리팹")]
        public GameObject bleedPerTurnEffectPrefab;

        [Tooltip("출혈 피해 발생 시 매 턴 재생할 사운드")]
        public AudioClip bleedPerTurnSfxClip;

        [Header("반격 효과 설정")]
        [Tooltip("반격 지속 턴 수")]
        public int counterDuration = 1;

        [Header("반격 효과 아이콘/이펙트/사운드")]
        [Tooltip("반격 효과 UI 아이콘")]
        public Sprite counterIcon;

        [Tooltip("반격 버프 적용 시 재생할 비주얼 이펙트 프리팹")]
        public GameObject counterActivateEffectPrefab;

        [Tooltip("반격 버프 적용 시 재생할 사운드")]
        public AudioClip counterActivateSfxClip;
        
        [Header("가드 효과 설정")]
        [Tooltip("가드 지속 턴 수")]
        public int guardDuration = 1;

        [Header("가드 효과 아이콘/이펙트/사운드")]
        [Tooltip("가드 효과 UI 아이콘")]
        public Sprite guardIcon;

        [Tooltip("가드 버프 적용 시 재생할 비주얼 이펙트 프리팹")]
        public GameObject guardActivateEffectPrefab;

        [Tooltip("가드 버프 적용 시 재생할 사운드")]
        public AudioClip guardActivateSfxClip;

        [Tooltip("가드가 데미지를 차단할 때 재생할 비주얼 이펙트 프리팹")]
        public GameObject guardBlockEffectPrefab;

        [Tooltip("가드가 데미지를 차단할 때 재생할 사운드")]
        public AudioClip guardBlockSfxClip;
        
        [Header("스턴 효과 설정")]
        [Tooltip("스턴 지속 턴 수")]
        public int stunDuration = 1;

        [Header("스턴 효과 아이콘")]
        [Tooltip("스턴 효과 UI 아이콘")]
        public Sprite stunIcon;

        [Header("치유 효과 설정")]
        [Tooltip("치유량")]
        public int healAmount = 0;

        [Header("치유 효과 이펙트/사운드")]
        [Tooltip("치유 시 재생할 비주얼 이펙트 프리팹")]
        public GameObject healEffectPrefab;

        [Tooltip("치유 시 재생할 사운드")]
        public AudioClip healSfxClip;
        
        [Header("드로우 효과 설정")]
        [Tooltip("드로우 수")]
        public int drawCount = 0;
        
        [Header("리소스 효과 설정")]
        [Tooltip("리소스 변화량")]
        public int resourceDelta = 0;
        
        [Tooltip("자원 획득 시 재생할 사운드")]
        public AudioClip resourceGainSfxClip;
        
        [Header("카드 사용 스택 효과 설정")]
        [Tooltip("카드 사용 시 증가할 스택 수")]
        public int stackIncreasePerUse = 1;
        
        [Tooltip("최대 스택 수 (0 = 무제한)")]
        public int maxStacks = 5;
        
        [Tooltip("감지할 카드 ID (비어있으면 모든 카드)")]
        public string targetCardId = "";
        
        [Header("무적 효과 설정")]
        [Tooltip("무적 지속 턴 수")]
        public int invincibilityDuration = 2;
        
        [Header("분신 효과 설정")]
        [Tooltip("분신 추가 체력")]
        public int cloneHP = 10;
        
        [Header("시공간 역행 효과 설정")]
        [Tooltip("몇 턴 전의 HP로 되돌릴지")]
        public int spaceTimeReversalTurnsAgo = 3;
        
        [Header("운명의 실 효과 설정")]
        [Tooltip("운명의 실 디버프 지속 턴 수")]
        public int threadOfFateDuration = 1;
        
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