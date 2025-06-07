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

        /// <summary>
        /// 카드 데이터를 설정하고 UI를 초기화합니다.
        /// </summary>
        public void SetCard(ISkillCard newCard)
        {
            card = newCard;

            if (card == null)
            {
                Debug.LogWarning("[SkillCardUI] 설정할 카드가 null입니다.");
                return;
            }

            cardNameText.text = card.GetCardName();
            descriptionText.text = card.GetDescription();

            if (cardArtImage != null && card.GetArtwork() != null)
                cardArtImage.sprite = card.GetArtwork();

            UpdateCoolTimeDisplay();
        }

        /// <summary>
        /// 쿨타임 UI를 갱신하고 드래그 가능 여부를 조정합니다.
        /// </summary>
        public void UpdateCoolTimeDisplay()
        {
            if (card == null) return;

            int currentCoolTime = card.GetCurrentCoolTime();
            bool isCooling = currentCoolTime > 0;

            if (coolTimeOverlay != null)
                coolTimeOverlay.SetActive(isCooling);

            if (coolTimeText != null)
                coolTimeText.text = isCooling ? currentCoolTime.ToString() : "";

            if (card.IsFromPlayer())
            {
                SetInteractable(!isCooling);
                SetDraggable(!isCooling);
            }
            else
            {
                SetInteractable(false);
                SetDraggable(false);
            }
        }

        public ISkillCard GetCard() => card;

        /// <summary>
        /// 카드의 시각적 상호작용 여부 설정 (투명도).
        /// </summary>
        public void SetInteractable(bool value)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = value ? 1f : 0.4f;
        }

        /// <summary>
        /// 드래그 가능 여부 설정 (핸들러 활성/비활성).
        /// </summary>
        public void SetDraggable(bool isEnabled)
        {
            if (!card?.IsFromPlayer() ?? true) return;

            if (TryGetComponent(out CardDragHandler dragHandler))
            {
                dragHandler.enabled = isEnabled;
            }
        }

        /// <summary>
        /// 외부에서 쿨타임 수동 갱신 시 호출 (예: 테스트).
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
