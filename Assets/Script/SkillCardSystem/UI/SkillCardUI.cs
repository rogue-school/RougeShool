using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.DragDrop;

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
        [SerializeField] private TextMeshProUGUI descriptionText;

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

            if (cardNameText != null) cardNameText.text = card.GetCardName();
            if (damageText != null) damageText.text = $"Damage: {card.CardData?.Damage ?? 0}";
            if (descriptionText != null) descriptionText.text = card.GetDescription();
            if (cardArtImage != null && card.GetArtwork() != null)
                cardArtImage.sprite = card.GetArtwork();

            UpdateCoolTimeDisplay();
        }

        public void UpdateCoolTimeDisplay()
        {
            if (card == null) return;

            int currentCoolTime = card.GetCurrentCoolTime();
            bool isCooling = currentCoolTime > 0;

            if (coolTimeOverlay != null)
                coolTimeOverlay.SetActive(isCooling);

            if (coolTimeText != null)
                coolTimeText.text = isCooling ? currentCoolTime.ToString() : "";

            // 플레이어 카드일 때만 상호작용 설정
            if (card.IsFromPlayer())
            {
                SetDraggable(!isCooling);
                SetInteractable(!isCooling);
            }
            else
            {
                SetDraggable(false);
                SetInteractable(false);
            }
        }

        public ISkillCard GetCard() => card;

        public void SetInteractable(bool value)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = value ? 1f : 0.5f;
        }

        public void SetDraggable(bool isEnabled)
        {
            if (!card?.IsFromPlayer() ?? true) return;

            if (TryGetComponent(out CardDragHandler dragHandler))
            {
                dragHandler.enabled = isEnabled;
            }
        }

        /// <summary>
        /// 외부에서 수동으로 쿨타임 UI를 표시하고 싶을 때 사용
        /// </summary>
        public void ShowCoolTime(int coolTime, bool show)
        {
            if (coolTimeOverlay != null)
                coolTimeOverlay.SetActive(show);

            if (coolTimeText != null)
                coolTimeText.text = show ? coolTime.ToString() : "";
        }
    }
}
