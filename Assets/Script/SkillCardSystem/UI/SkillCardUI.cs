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
        [SerializeField] private TextMeshProUGUI cardNameText;      // 선택사항: 카드명 표시
        [SerializeField] private TextMeshProUGUI damageText;        // 선택사항: 데미지 표시
        [SerializeField] private TextMeshProUGUI descriptionText;   // 선택사항: 설명 표시
        [SerializeField] private Image cardArtImage;               // 필수: 카드 아트워크

        [Header("UI 그룹")]
        [SerializeField] private CanvasGroup canvasGroup;

        #endregion

        private ISkillCard card;

        // 카드 애니메이션 상태 플래그
        public bool IsAnimating { get; private set; }

        // 예시: 애니메이션 시작/종료 시점에 호출할 수 있는 메서드
        public void SetAnimating(bool value)
        {
            IsAnimating = value;
        }

        #region Public Methods

        /// <summary>
        /// 스킬 카드 데이터를 설정하고 UI를 초기화합니다.
        /// 설명, 데미지, 카드명 필드는 선택사항이며 null이어도 정상 작동합니다.
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

            // 플레이어 마커 카드인 경우: 자식 UI를 사용하지 않고 부모 이미지에 엠블럼만 표시
            bool isPlayerMarker = card.CardDefinition?.cardId == "PLAYER_MARKER";
            if (isPlayerMarker)
            {
                // 자식 텍스트 및 아트 오브젝트 비활성화
                if (cardNameText != null) cardNameText.gameObject.SetActive(false);
                if (damageText != null) damageText.gameObject.SetActive(false);
                if (descriptionText != null) descriptionText.gameObject.SetActive(false);
                if (cardArtImage != null) cardArtImage.gameObject.SetActive(false);

                // 부모(Image) 컴포넌트에 엠블럼 연결
                var rootImage = GetComponent<UnityEngine.UI.Image>();
                if (rootImage != null)
                {
                    var emblem = card.GetArtwork();
                    if (emblem != null)
                    {
                        rootImage.sprite = emblem;
                    }
                    // 마커는 상호작용/레이캐스트가 불필요
                    rootImage.raycastTarget = false;
                }

                // 마커는 상호작용/드래그 비활성화
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
                if (TryGetComponent(out Game.CombatSystem.DragDrop.CardDragHandler dragHandlerForMarker))
                {
                    dragHandlerForMarker.enabled = false;
                }

                return;
            }

            // 카드명 설정 (선택사항)
            if (cardNameText != null)
            {
                string cardName = card.GetCardName();
                cardNameText.text = !string.IsNullOrEmpty(cardName) ? cardName : "";
            }

            // 설명 설정 (선택사항)
            if (descriptionText != null)
            {
                string description = card.GetDescription();
                descriptionText.text = !string.IsNullOrEmpty(description) ? description : "";
            }

            // 데미지 설정 (선택사항)
            if (damageText != null)
            {
                if (card.CardDefinition?.configuration?.hasDamage == true)
                {
                    int damage = card.CardDefinition.configuration.damageConfig.baseDamage;
                    damageText.text = damage.ToString();
                }
                else
                {
                    damageText.text = ""; // 데미지가 없는 카드는 빈 문자열
                }
            }

            // 카드 아트워크 설정 (필수)
            if (cardArtImage != null)
            {
                Sprite artwork = card.GetArtwork();
                if (artwork != null)
                {
                    cardArtImage.sprite = artwork;
                }
                else
                {
                    Debug.LogWarning("[SkillCardUI] 카드 아트워크가 null입니다. 기본 이미지를 설정해주세요.");
                }
            }

            // 소유자에 따라 드래그/상호작용 제어 (플레이어만 드래그 가능)
            bool isPlayerCard = card.IsFromPlayer();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = isPlayerCard;
                canvasGroup.blocksRaycasts = isPlayerCard;
            }
            if (TryGetComponent(out Game.CombatSystem.DragDrop.CardDragHandler dragHandler))
            {
                dragHandler.enabled = isPlayerCard;
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


        #endregion
    }
}