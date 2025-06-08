using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 전투 슬롯에 스킬 카드를 배치하는 서비스를 제공합니다.
    /// </summary>
    public class CardPlacementService : ICardPlacementService
    {
        #region 카드 배치

        /// <summary>
        /// 지정된 슬롯에 카드와 UI를 배치합니다.
        /// </summary>
        /// <param name="card">배치할 스킬 카드</param>
        /// <param name="ui">해당 카드의 UI</param>
        /// <param name="slot">카드를 배치할 전투 슬롯</param>
        public void PlaceCardInSlot(ISkillCard card, ISkillCardUI ui, ICombatCardSlot slot)
        {
            if (card == null || ui == null || slot == null)
            {
                Debug.LogError("[CardPlacementService] 카드, UI, 슬롯 중 하나 이상이 null입니다.");
                return;
            }

            // 카드 설정
            slot.SetCard(card);
            slot.SetCardUI(ui);

            // UI 오브젝트 위치 정렬
            if (ui is MonoBehaviour uiMb)
            {
                uiMb.transform.SetParent(((MonoBehaviour)slot).transform);
                uiMb.transform.localPosition = Vector3.zero;
                uiMb.transform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogWarning("[CardPlacementService] 카드 UI가 MonoBehaviour가 아닙니다. Transform 설정을 건너뜁니다.");
            }

            Debug.Log($"[CardPlacementService] 카드 '{card.GetCardName()}' 슬롯 {slot.GetCombatPosition()}에 배치 완료");
        }

        #endregion
    }
}
