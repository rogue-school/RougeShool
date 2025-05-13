using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 카드 UI를 관리하는 컴포넌트입니다.
    /// 카드 이름, 파워, 아트워크를 표시합니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private Image cardArtImage;

        [Header("쿨타임 표시")]
        [SerializeField] private GameObject coolTimeOverlay;
        [SerializeField] private TextMeshProUGUI coolTimeText;
        [SerializeField] private CanvasGroup canvasGroup;


        private ISkillCard card;

        /// <summary>
        /// 카드 데이터를 바탕으로 UI를 업데이트합니다.
        /// </summary>
        /// <param name="newCard">설정할 카드 데이터</param>
        public void SetCard(ISkillCard newCard)
        {
            card = newCard;

            if (card == null)
            {
               // Debug.LogWarning("[SkillCardUI] 카드가 null입니다.");
                return;
            }

            cardNameText.text = card.GetCardName();
            powerText.text = $"Power: {card.GetEffectPower(null)}";

            Sprite art = card.GetArtwork();
            if (cardArtImage != null && art != null)
            {
                cardArtImage.sprite = art;
            }
            else
            {
                //Debug.LogWarning("[SkillCardUI] 아트워크 이미지가 없습니다.");
            }

            //Debug.Log($"[SkillCardUI] 카드 UI 설정 완료: {card.GetCardName()}");
        }

        public void SetInteractable(bool value)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = value ? 1f : 0.5f;
        }

        public void ShowCoolTime(int coolTime, bool show)
        {
            if (coolTimeOverlay != null) coolTimeOverlay.SetActive(show);
            if (coolTimeText != null) coolTimeText.text = show ? coolTime.ToString() : "";
        }

        /// <summary>
        /// 현재 설정된 카드 데이터 반환
        /// </summary>
        public ISkillCard GetCard() => card;
    }
}
