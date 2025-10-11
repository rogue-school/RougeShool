using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Data
{
    /// <summary>
    /// 패시브 아이템의 정의를 담는 ScriptableObject입니다.
    /// 성급 시스템을 통해 스킬의 데미지를 영구적으로 증가시키는 아이템들을 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "PassiveItem", menuName = "Game/Item/Passive Item Definition")]
    public class PassiveItemDefinition : ItemDefinition
    {
        #region 패시브 아이템 설정

        [Header("패시브 아이템 설정")]
        [Tooltip("대상 스킬 ID (성급이 적용될 스킬)")]
        [SerializeField] private string targetSkillId;

        [Tooltip("성급당 데미지 보너스")]
        [Range(1, 10)]
        [SerializeField] private int damageBonusPerStar = 1;

        [Tooltip("최대 성급")]
        [Range(1, 5)]
        [SerializeField] private int maxStars = 3;

        [Tooltip("아이템 카테고리 (검/활/지팡이/공용)")]
        [SerializeField] private PassiveItemCategory category = PassiveItemCategory.Common;

        [Header("스킬 데미지 보너스 설정")]
        [Tooltip("스킬별 데미지 보너스 (베기 등 특정 스킬용)")]
        [SerializeField] private bool isSkillDamageBonus = false;

        [Tooltip("데미지 보너스량 (스킬별 보너스인 경우)")]
        [SerializeField] private int skillDamageBonus = 0;

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 아이템 타입 (패시브)
        /// </summary>
        public override ItemType Type => ItemType.Passive;

        /// <summary>
        /// 대상 스킬 ID
        /// </summary>
        public string TargetSkillId => targetSkillId;

        /// <summary>
        /// 성급당 데미지 보너스
        /// </summary>
        public int DamageBonusPerStar => damageBonusPerStar;

        /// <summary>
        /// 최대 성급
        /// </summary>
        public int MaxStars => maxStars;

        /// <summary>
        /// 아이템 카테고리
        /// </summary>
        public PassiveItemCategory Category => category;

        /// <summary>
        /// 스킬별 데미지 보너스 여부
        /// </summary>
        public bool IsSkillDamageBonus => isSkillDamageBonus;

        /// <summary>
        /// 스킬 데미지 보너스량
        /// </summary>
        public int SkillDamageBonus => skillDamageBonus;

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

            if (string.IsNullOrEmpty(targetSkillId))
            {
                GameLogger.LogError($"[PassiveItemDefinition] 대상 스킬 ID가 비어있습니다: {ItemId}", GameLogger.LogCategory.Core);
                return false;
            }

            if (damageBonusPerStar <= 0)
            {
                GameLogger.LogError($"[PassiveItemDefinition] 성급당 데미지 보너스가 0 이하입니다: {ItemId}", GameLogger.LogCategory.Core);
                return false;
            }

            if (maxStars <= 0)
            {
                GameLogger.LogError($"[PassiveItemDefinition] 최대 성급이 0 이하입니다: {ItemId}", GameLogger.LogCategory.Core);
                return false;
            }

            return true;
        }

        #endregion

        #region Unity Editor

        protected override void OnValidate()
        {
            base.OnValidate();

            // 성급당 데미지 보너스 검증
            if (damageBonusPerStar <= 0)
                damageBonusPerStar = 1;

            // 최대 성급 검증
            if (maxStars <= 0)
                maxStars = 3;
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
}
