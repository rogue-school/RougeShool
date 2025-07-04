using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.DragDrop;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 스킬 카드의 이름, 설명, 이미지, 쿨타임 등 UI를 제어합니다.
    /// 카드가 드래그 가능한 상태인지 여부도 제어합니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour, ISkillCardUI
    {
        #region UI Components

        [Header("카드 정보 UI")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image cardArtImage;

        [Header("쿨타임 UI")]
        [SerializeField] private GameObject coolTimeOverlay;
        [SerializeField] private TextMeshProUGUI coolTimeText;
        [SerializeField] private CanvasGroup canvasGroup;

        #endregion

        private ISkillCard card;

        #region Public Methods

        /// <summary>
        /// 스킬 카드 데이터를 설정하고 UI를 초기화합니다.
        /// </summary>
        /// <param name="newCard">연결할 카드 인스턴스</param>
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
        /// 현재 카드의 쿨타임 정보를 바탕으로 UI를 갱신합니다.
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

            if (cardArtImage != null)
            {
                // 회색 음영 처리: 쿨타임 중이면 회색, 아니면 원래 색
                cardArtImage.color = isCooling ? new Color(0.3f, 0.3f, 0.3f, 1f) : Color.white;
            }

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

        /// <summary>
        /// 현재 UI에 설정된 카드를 반환합니다.
        /// </summary>
        public ISkillCard GetCard() => card;

        /// <summary>
        /// 카드의 상호작용 가능 여부를 설정합니다 (투명도 조절).
        /// </summary>
        /// <param name="value">true 시 정상 표시, false 시 투명도 낮춤</param>
        public void SetInteractable(bool value)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = value ? 1f : 0.4f;
        }

        /// <summary>
        /// 카드의 드래그 가능 여부를 설정합니다.
        /// </summary>
        /// <param name="isEnabled">true 시 드래그 가능</param>
        public void SetDraggable(bool isEnabled)
        {
            if (!card?.IsFromPlayer() ?? true) return;

            if (TryGetComponent(out CardDragHandler dragHandler))
                dragHandler.enabled = isEnabled;
        }

        /// <summary>
        /// 외부에서 강제로 쿨타임 UI를 갱신합니다. (디버깅 또는 테스트용)
        /// </summary>
        /// <param name="coolTime">표시할 쿨타임</param>
        /// <param name="show">쿨타임 UI 표시 여부</param>
        public void ShowCoolTime(int coolTime, bool show)
        {
            if (coolTimeOverlay != null)
                coolTimeOverlay.SetActive(show);

            if (coolTimeText != null)
                coolTimeText.text = show ? coolTime.ToString() : "";
        }

        #endregion
    }
}