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
    public class SkillCardUI : MonoBehaviour, ISkillCardUI
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

        public void SetCard(ISkillCard newCard)
        {
            card = newCard;

            if (card == null) return;

            cardNameText.text = card.GetCardName();
            powerText.text = $"Power: {card.GetEffectPower(null)}";

            Sprite art = card.GetArtwork();
            if (cardArtImage != null && art != null)
                cardArtImage.sprite = art;
        }

        public ISkillCard GetCard() => card;

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
    }
}
