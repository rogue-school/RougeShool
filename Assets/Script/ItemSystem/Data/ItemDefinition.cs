using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Data
{
    /// <summary>
    /// 아이템 타입 열거형입니다.
    /// </summary>
    public enum ItemType
    {
        Active,     // 액티브 아이템
        Passive     // 패시브 아이템
    }

    /// <summary>
    /// 아이템의 기본 정의를 담는 추상 ScriptableObject입니다.
    /// 모든 아이템은 이 클래스를 상속하여 구현됩니다.
    /// </summary>
    public abstract class ItemDefinition : ScriptableObject
    {
        #region 기본 정보

        [Header("기본 정보")]
        [Tooltip("아이템 고유 식별자")]
        [SerializeField] private string itemId;

        [Tooltip("아이템 표시 이름")]
        [SerializeField] private string displayName;

        [Tooltip("아이템 설명")]
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Tooltip("아이템 아이콘")]
        [SerializeField] private Sprite icon;

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 아이템 고유 식별자
        /// </summary>
        public string ItemId => itemId;

        /// <summary>
        /// 아이템 표시 이름
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// 아이템 설명
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 아이템 아이콘
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// 아이템 타입 (추상 프로퍼티)
        /// </summary>
        public abstract ItemType Type { get; }

        #endregion

        #region 유효성 검증

        /// <summary>
        /// 아이템 정의의 유효성을 검증합니다.
        /// </summary>
        /// <returns>유효성 여부</returns>
        public virtual bool IsValid()
        {
            if (string.IsNullOrEmpty(itemId))
            {
                GameLogger.LogError($"[ItemDefinition] 아이템 ID가 비어있습니다: {name}", GameLogger.LogCategory.Core);
                return false;
            }

            if (string.IsNullOrEmpty(displayName))
            {
                GameLogger.LogError($"[ItemDefinition] 아이템 이름이 비어있습니다: {itemId}", GameLogger.LogCategory.Core);
                return false;
            }

            if (icon == null)
            {
                GameLogger.LogWarning($"[ItemDefinition] 아이템 아이콘이 없습니다: {itemId}", GameLogger.LogCategory.Core);
            }

            return true;
        }

        #endregion

        #region Unity Editor

        protected virtual void OnValidate()
        {
            // 에디터에서 아이템 ID 자동 생성
            if (string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(name))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }
        }

        #endregion
    }
}
