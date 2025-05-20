using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 카드 UI를 관리하는 컴포넌트입니다.
    /// 카드 이름, 데미지, 아트워크, 쿨타임을 표시합니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour, ISkillCardUI
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private Image cardArtImage;

        [Header("쿨타임 표시")]
        [SerializeField] private GameObject coolTimeOverlay;
        [SerializeField] private TextMeshProUGUI coolTimeText;
        [SerializeField] private CanvasGroup canvasGroup;

        private ISkillCard card;

        public void SetCard(ISkillCard newCard)
        {
            card = newCard;

            if (card == null)
            {
                Debug.LogWarning("[SkillCardUI] 설정할 카드가 null입니다.");
                return;
            }

            // 카드 이름 표시
            if (cardNameText != null)
                cardNameText.text = card.GetCardName();

            // 데미지 표시
            if (damageText != null)
                damageText.text = $"Damage: {card.CardData.Damage}";

            // 아트워크 설정
            if (cardArtImage != null && card.GetArtwork() != null)
                cardArtImage.sprite = card.GetArtwork();
        }

        public ISkillCard GetCard() => card;

        public void SetInteractable(bool value)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = value ? 1f : 0.5f;
        }

        public void ShowCoolTime(int coolTime, bool show)
        {
            if (coolTimeOverlay != null)
                coolTimeOverlay.SetActive(show);

            if (coolTimeText != null)
                coolTimeText.text = show ? coolTime.ToString() : "";
        }
    }
}
