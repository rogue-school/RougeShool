using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Data
{
    /// <summary>
    /// 패시브 아이템의 정의를 담는 ScriptableObject입니다.
    /// 강화 단계 시스템을 통해 스킬의 데미지를 영구적으로 증가시키는 아이템들을 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "PassiveItem", menuName = "Game/Item/Passive Item Definition")]
    public class PassiveItemDefinition : ItemDefinition
    {
        #region 패시브 아이템 설정

        [Tooltip("대상 스킬 ID (참조가 없을 때 폴백으로 사용)")]
        [HideInInspector]
        [SerializeField] private string targetSkillId;

        [Tooltip("대상 스킬 참조(안전) - 설정 시 ID 대신 이 참조를 사용")]
        [SerializeField] private Game.SkillCardSystem.Data.SkillCardDefinition targetSkill;

        [Tooltip("강화 단계별 추가 수치 (1단계, 2단계, 3단계 순) — 누적 합계")]
        [SerializeField] private int[] enhancementIncrements = new int[] { 1, 1, 1 };

        [Tooltip("아이템 카테고리 (검/활/지팡이/공용)")]
        [SerializeField] private PassiveItemCategory category = PassiveItemCategory.Common;

        [Tooltip("보너스 적용 타입")]
        [SerializeField] private PassiveBonusType bonusType = PassiveBonusType.SkillDamage;

        // 고정 보너스는 제거되었습니다. 모든 보너스는 강화 단계별 누적 합계로 계산됩니다.

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 아이템 타입 (패시브)
        /// </summary>
        public override ItemType Type => ItemType.Passive;

        /// <summary>
        /// 대상 스킬 ID
        /// </summary>
        public string TargetSkillId => targetSkill != null ? (targetSkill.displayName ?? targetSkill.cardId) : targetSkillId;

        /// <summary>
        /// 대상 스킬 참조(있을 경우 연결 안전)
        /// </summary>
        public Game.SkillCardSystem.Data.SkillCardDefinition TargetSkill => targetSkill;

        /// <summary>
        /// 강화 단계별 추가 데미지 배열 (1~3단계)
        /// </summary>
        public System.ReadOnlySpan<int> EnhancementIncrements => enhancementIncrements;

        /// <summary>
        /// 최대 강화 단계
        /// </summary>
        public int MaxEnhancementLevel => Mathf.Min(enhancementIncrements != null ? enhancementIncrements.Length : 0, Game.ItemSystem.Constants.ItemConstants.MAX_ENHANCEMENT_LEVEL);

        /// <summary>
        /// 아이템 카테고리
        /// </summary>
        public PassiveItemCategory Category => category;

        /// <summary>
        /// 스킬별 데미지 보너스 여부
        /// </summary>
        public bool IsSkillDamageBonus => bonusType == PassiveBonusType.SkillDamage;

        // 고정 보너스 프로퍼티 제거됨

        /// <summary>
        /// 플레이어 최대 체력 보너스 타입 여부
        /// </summary>
        public bool IsPlayerHealthBonus => bonusType == PassiveBonusType.PlayerMaxHealth;

        #endregion

        #region 유효성 검증

        /// <summary>
        /// 패시브 아이템 정의의 유효성을 검증합니다.
        /// </summary>
        /// <returns>유효성 여부</returns>
        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;

            // 스킬 데미지 보너스형이면 타겟 스킬 ID/참조 필수
            if (IsSkillDamageBonus && targetSkill == null && string.IsNullOrEmpty(targetSkillId))
            {
                GameLogger.LogError($"[PassiveItemDefinition] 대상 스킬 ID가 비어있습니다: {ItemId}", GameLogger.LogCategory.Core);
                return false;
            }

            if (enhancementIncrements == null || enhancementIncrements.Length == 0)
            {
                GameLogger.LogError($"[PassiveItemDefinition] 강화 단계별 보너스가 비어있습니다: {ItemId}", GameLogger.LogCategory.Core);
                return false;
            }

            // 각 단계는 0보다 커야 함
            for (int i = 0; i < enhancementIncrements.Length; i++)
            {
                if (enhancementIncrements[i] <= 0)
                {
                    GameLogger.LogError($"[PassiveItemDefinition] 강화 단계 {i + 1} 보너스가 0 이하입니다: {ItemId}", GameLogger.LogCategory.Core);
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Unity Editor

        protected override void OnValidate()
        {
            base.OnValidate();

            // 강화 단계 배열 기본화 및 정규화
            if (enhancementIncrements == null || enhancementIncrements.Length == 0)
            {
                enhancementIncrements = new int[] { 1, 1, 1 };
            }

            // 최대 강화 단계에 맞춰 길이 보정(자르거나 채우기)
            int maxLevel = Game.ItemSystem.Constants.ItemConstants.MAX_ENHANCEMENT_LEVEL;
            if (enhancementIncrements.Length != maxLevel)
            {
                var newArr = new int[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                {
                    newArr[i] = (i < enhancementIncrements.Length) ? Mathf.Max(1, enhancementIncrements[i]) : 1;
                }
                enhancementIncrements = newArr;
            }

            // 각 값이 1 미만이면 1로 보정
            for (int i = 0; i < enhancementIncrements.Length; i++)
            {
                if (enhancementIncrements[i] < 1) enhancementIncrements[i] = 1;
            }
        }

        #endregion
    }

    /// <summary>
    /// 패시브 아이템 카테고리 열거형입니다.
    /// </summary>
    public enum PassiveItemCategory
    {
        Sword,      // 검
        Bow,        // 활
        Staff,      // 지팡이
        Common      // 공용
    }

    /// <summary>
    /// 패시브 보너스 타입입니다.
    /// </summary>
    public enum PassiveBonusType
    {
        SkillDamage,     // 스킬 데미지 증가
        PlayerMaxHealth  // 플레이어 최대 체력 증가
    }
}
