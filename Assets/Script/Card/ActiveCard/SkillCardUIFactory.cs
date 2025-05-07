using UnityEngine;
using Game.Interface;
using Game.UI;

namespace Game.Utils
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
            var instance = Object.Instantiate(prefab, parent);
            instance.SetCard(card);
            return instance;
        }
    }
}