using System.Collections.Generic;
using UnityEngine;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Effect;
using Game.ItemSystem.Interface;

namespace Game.ItemSystem.Data
{
    /// <summary>
    /// 아이템 타겟 타입을 정의합니다.
    /// </summary>
    public enum ItemTargetType
    {
        /// <summary>자신에게 사용 (회복, 버프 등)</summary>
        Self,

        /// <summary>적에게 사용 (디버프, 방해 효과 등)</summary>
        Enemy,

        /// <summary>양쪽 모두에게 적용 (특수 아이템)</summary>
        Both
    }

    /// <summary>
    /// 액티브 아이템 정의 ScriptableObject입니다.
    /// 사용 가능한 아이템의 데이터를 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ActiveItem", menuName = "ItemSystem/ActiveItemDefinition")]
    public class ActiveItemDefinition : ItemDefinition
    {
        [Header("아이템 타겟 설정")]
        [Tooltip("아이템의 대상 타입 (Self: 자신, Enemy: 적, Both: 양쪽)")]
        public ItemTargetType targetType = ItemTargetType.Self;

        [Header("지속 효과 설정")]
        [Tooltip("아이템 효과의 턴 감소 정책 (Immediate: 즉시, EveryTurn: 매 턴, TargetTurnOnly: 대상 턴만)")]
        public ItemEffectTurnPolicy turnPolicy = ItemEffectTurnPolicy.Immediate;

        [Header("연출 구성")]
        [Tooltip("아이템 연출 설정")]
        public ItemPresentation presentation = new();

        [Header("효과 구성")]
        [Tooltip("아이템 효과 설정")]
        public ItemEffectConfiguration effectConfiguration = new();

        #region Properties

        /// <summary>
        /// 아이템 타입 (액티브)
        /// </summary>
        public override ItemType Type => ItemType.Active;

        /// <summary>
        /// 아이템 연출 설정을 반환합니다.
        /// </summary>
        public ItemPresentation Presentation => presentation;

        /// <summary>
        /// 아이템 효과 설정을 반환합니다.
        /// </summary>
        public ItemEffectConfiguration EffectConfiguration => effectConfiguration;

        #endregion

        #region Validation

        /// <summary>
        /// 아이템 정의가 유효한지 확인합니다.
        /// </summary>
        /// <returns>유효성 여부</returns>
        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;

            if (effectConfiguration.effects.Count == 0)
            {
                GameLogger.LogError($"아이템 '{DisplayName}'에 효과가 설정되지 않았습니다", GameLogger.LogCategory.Core);
                return false;
            }

            foreach (var effectConfig in effectConfiguration.effects)
            {
                if (effectConfig.effectSO == null)
                {
                    GameLogger.LogError($"아이템 '{DisplayName}'에 null 효과가 있습니다", GameLogger.LogCategory.Core);
                    return false;
                }
            }

            return true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            foreach (var effectConfig in effectConfiguration.effects)
            {
                if (effectConfig.effectSO != null && effectConfig.useCustomSettings)
                {
                    bool needsUpdate = false;

                    if (effectConfig.effectSO is HealEffectSO && !(effectConfig.customSettings is HealEffectCustomSettings))
                        needsUpdate = true;
                    else if (effectConfig.effectSO is AttackBuffEffectSO && !(effectConfig.customSettings is AttackBuffEffectCustomSettings))
                        needsUpdate = true;
                    else if (effectConfig.effectSO is ClownPotionEffectSO && !(effectConfig.customSettings is ClownPotionEffectCustomSettings))
                        needsUpdate = true;
                    else if (effectConfig.effectSO is RerollEffectSO && effectConfig.customSettings != null)
                        needsUpdate = true;
                    else if (effectConfig.effectSO is ShieldBreakerEffectSO && !(effectConfig.customSettings is ShieldBreakerEffectCustomSettings))
                        needsUpdate = true;
                    else if (effectConfig.effectSO is TimeStopEffectSO && !(effectConfig.customSettings is TimeStopEffectCustomSettings))
                        needsUpdate = true;
                    else if (effectConfig.effectSO is DiceOfFateEffectSO && !(effectConfig.customSettings is DiceOfFateEffectCustomSettings))
                        needsUpdate = true;
                    else if (effectConfig.effectSO is ReviveEffectSO && effectConfig.customSettings != null)
                        needsUpdate = true;

                    if (needsUpdate)
                    {
                        effectConfig.OnEffectSOChanged();
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 아이템의 연출 관련 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ItemPresentation
    {
        [Header("사운드")]
        [Tooltip("아이템 사용 시 재생할 고유 사운드")]
        public AudioClip sfxClip;
        
        [Header("비주얼 이펙트")]
        [Tooltip("아이템 사용 시 생성할 고유 비주얼 이펙트 프리팹")]
        public GameObject visualEffectPrefab;
    }

    /// <summary>
    /// 아이템의 효과 구성 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ItemEffectConfiguration
    {
        [Header("효과 목록")]
        [Tooltip("아이템 효과 목록")]
        public List<ItemEffectConfig> effects = new();
    }

    /// <summary>
    /// 아이템 효과 구성 설정을 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ItemEffectConfig
    {
        [Tooltip("효과 SO")]
        public ItemEffectSO effectSO;
        
        [Tooltip("커스텀 설정 사용 여부")]
        public bool useCustomSettings = false;
        
        [Tooltip("커스텀 설정 (효과 타입에 따라 자동으로 적절한 타입으로 변환)")]
        [SerializeReference]
        public ItemEffectCustomSettings customSettings;
        
        [Tooltip("실행 순서 (낮을수록 먼저 실행)")]
        public int executionOrder = 0;

        /// <summary>
        /// 효과 SO가 변경될 때 적절한 커스텀 설정을 자동으로 생성합니다.
        /// </summary>
        public void OnEffectSOChanged()
        {
            if (effectSO == null)
            {
                customSettings = null;
                return;
            }

            // 효과 타입에 따라 적절한 설정 클래스 생성
            if (effectSO is HealEffectSO)
            {
                customSettings = new HealEffectCustomSettings();
            }
            else if (effectSO is AttackBuffEffectSO)
            {
                customSettings = new AttackBuffEffectCustomSettings();
            }
            else if (effectSO is ClownPotionEffectSO)
            {
                customSettings = new ClownPotionEffectCustomSettings();
            }
            else if (effectSO is RerollEffectSO)
            {
                customSettings = null;
            }
            else if (effectSO is ShieldBreakerEffectSO)
            {
                customSettings = new ShieldBreakerEffectCustomSettings();
            }
            else if (effectSO is TimeStopEffectSO)
            {
                customSettings = new TimeStopEffectCustomSettings();
            }
            else if (effectSO is DiceOfFateEffectSO)
            {
                customSettings = new DiceOfFateEffectCustomSettings();
            }
            else if (effectSO is ReviveEffectSO)
            {
                customSettings = null;
            }
            else
            {
                customSettings = null;
            }
        }
    }

    /// <summary>
    /// 아이템 효과의 커스텀 설정을 담는 추상 클래스입니다.
    /// </summary>
    [System.Serializable]
    public abstract class ItemEffectCustomSettings
    {
        // 기본 설정은 없음 - 각 효과별로 구체적인 설정 구현
    }

    /// <summary>
    /// 회복 효과의 커스텀 설정입니다.
    /// </summary>
    [System.Serializable]
    public class HealEffectCustomSettings : ItemEffectCustomSettings
    {
        [Header("회복 효과 설정")]
        [Tooltip("회복량")]
        public int healAmount = 0;
    }

    /// <summary>
    /// 공격력 버프 효과의 커스텀 설정입니다.
    /// </summary>
    [System.Serializable]
    public class AttackBuffEffectCustomSettings : ItemEffectCustomSettings
    {
        [Header("버프 효과 설정")]
        [Tooltip("버프량")]
        public int buffAmount = 0;

        [Tooltip("지속 시간 (턴)")]
        public int duration = 1;
    }

    /// <summary>
    /// 광대 물약 랜덤 효과의 커스텀 설정입니다.
    /// </summary>
    [System.Serializable]
    public class ClownPotionEffectCustomSettings : ItemEffectCustomSettings
    {
        [Header("랜덤 효과 설정")]
        [Tooltip("회복 효과가 발생할 확률 (0~100%)")]
        [Range(0, 100)]
        public int healChance = 50;

        [Tooltip("회복할 체력량")]
        public int healAmount = 5;

        [Tooltip("입힐 데미지량")]
        public int damageAmount = 5;
    }

    /// <summary>
    /// 시간 정지 효과의 커스텀 설정입니다.
    /// </summary>
    [System.Serializable]
    public class TimeStopEffectCustomSettings : ItemEffectCustomSettings
    {
        [Header("시간 정지 설정")]
        [Tooltip("봉인할 적 카드 수")]
        public int sealCount = 1;

    }

    /// <summary>
    /// 운명의 주사위 효과의 커스텀 설정입니다.
    /// </summary>
    [System.Serializable]
    public class DiceOfFateEffectCustomSettings : ItemEffectCustomSettings
    {
        [Header("운명의 주사위 설정")]
        [Tooltip("변경할 적 스킬 수")]
        public int changeCount = 1;

    }

    /// <summary>
    /// 실드 브레이커 효과의 커스텀 설정입니다.
    /// </summary>
    [System.Serializable]
    public class ShieldBreakerEffectCustomSettings : ItemEffectCustomSettings
    {
        [Header("실드 브레이커 효과 설정")]
        [Tooltip("지속 시간 (턴)")]
        public int duration = 2;
    }

    // 리롤과 부활은 커스텀 설정이 필요 없으므로 클래스 자체를 제거하거나
    // 비워둘 수 있습니다. 하지만 기존 코드 호환성을 위해 남겨둡니다.

    /// <summary>
    /// 리롤 효과의 커스텀 설정입니다.
    /// 참고: 리롤은 핸드 전체(3장)를 교체하므로 실제로는 설정이 불필요합니다.
    /// </summary>
    [System.Serializable]
    public class RerollEffectCustomSettings : ItemEffectCustomSettings
    {
        // 핸드 크기가 고정이므로 설정 불필요
        // 호환성을 위해 클래스는 유지하되 필드는 비움
    }
}