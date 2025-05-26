using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 카드 UI 프리팹 인스턴스를 생성하고 설정하는 팩토리
    /// </summary>
    public static class SkillCardUIFactory
    {
        /// <summary>
        /// 카드 UI 오브젝트를 생성하고 카드 데이터를 적용합니다.
        /// </summary>
        /// <param name="prefab">UI 프리팹</param>
        /// <param name="parent">부모 트랜스폼</param>
        /// <param name="card">카드 데이터</param>
        /// <returns>생성된 SkillCardUI 인스턴스</returns>
        public static SkillCardUI CreateUI(SkillCardUI prefab, Transform parent, ISkillCard card)
        {
            if (prefab == null || parent == null || card == null)
            {
                Debug.LogError("[SkillCardUIFactory] 카드 UI 생성 실패 - null 인자 존재");
                return null;
            }

            //Debug.Log($"[SkillCardUIFactory] 카드 UI 생성 시작 - 카드: {card.GetCardName()}");

            var instance = Object.Instantiate(prefab, parent, false);
            if (instance == null)
            {
                Debug.LogError("[SkillCardUIFactory] 프리팹 인스턴스화 실패");
                return null;
            }

            instance.SetCard(card);

            if (instance.TryGetComponent(out RectTransform rect))
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            //Debug.Log($"[SkillCardUIFactory] 카드 UI 생성 완료 - {card.GetCardName()}");
            return instance;
        }
    }
}
