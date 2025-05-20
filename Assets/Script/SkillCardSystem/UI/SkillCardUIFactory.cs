using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 카드 UI 프리팹 인스턴스를 생성하고 설정하는 팩토리 클래스입니다.
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
            if (prefab == null)
            {
                Debug.LogError("[SkillCardUIFactory] 카드 UI 프리팹이 null입니다.");
                return null;
            }

            if (parent == null)
            {
                Debug.LogError("[SkillCardUIFactory] 부모 트랜스폼이 null입니다.");
                return null;
            }

            if (card == null)
            {
                Debug.LogError("[SkillCardUIFactory] 카드 데이터가 null입니다.");
                return null;
            }

            Debug.Log("[SkillCardUIFactory] 카드 UI 인스턴스 생성 시작");

            var instance = Object.Instantiate(prefab, parent, false);
            if (instance == null)
            {
                Debug.LogError("[SkillCardUIFactory] 인스턴스화 실패 - 프리팹 오류?");
                return null;
            }

            instance.SetCard(card);

            // RectTransform 초기화
            RectTransform rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            Debug.Log($"[SkillCardUIFactory] 카드 UI 생성 완료 → {card.GetCardName()}");

            return instance;
        }
    }
}
