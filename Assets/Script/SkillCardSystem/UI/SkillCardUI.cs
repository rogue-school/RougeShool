using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Manager;
using Game.CombatSystem.DragDrop;
using Game.CoreSystem.Utility;
using Zenject;
using DG.Tweening;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 스킬 카드의 이름, 설명, 이미지 등 UI를 제어합니다.
    /// 카드가 드래그 가능한 상태인지 여부도 제어합니다.
    /// 툴팁 기능도 포함합니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour, ISkillCardUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region UI Components

        [Header("카드 정보 UI")]
        [SerializeField] private TextMeshProUGUI cardNameText;      // 선택사항: 카드명 표시
        [SerializeField] private TextMeshProUGUI damageText;        // 선택사항: 데미지 표시
        [SerializeField] private TextMeshProUGUI descriptionText;   // 선택사항: 설명 표시
        [SerializeField] private Image cardArtImage;               // 필수: 카드 아트워크

        [Header("카드 배경 이미지")]
        [Tooltip("플레이어 카드 배경 이미지")]
        [SerializeField] private Sprite playerCardBackground;
        [Tooltip("적 카드 배경 이미지")]
        [SerializeField] private Sprite enemyCardBackground;

        [Header("UI 그룹")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("툴팁 설정")]
        [Tooltip("툴팁 활성화 여부")]
        [SerializeField] private bool enableTooltip = true;

        [Tooltip("툴팁 표시 지연 시간 (초)")]
        [SerializeField] private float tooltipDelay = 0.5f;

        [Tooltip("드래그 중 툴팁 숨김 여부")]
        [SerializeField] private bool hideTooltipOnDrag = true;

        [Header("호버 효과 설정")]
        [Tooltip("호버 시 스케일 (플레이어 카드만 적용)")]
        [SerializeField] private float hoverScale = 1.05f;

        #endregion

        #region Private Fields

        private ISkillCard card;
        
        [Inject] private SkillCardTooltipManager tooltipManager;
        
        // 툴팁 매니저 지연 초기화
        private SkillCardTooltipManager GetTooltipManager()
        {
            if (tooltipManager == null)
            {
                // Zenject 컨테이너에서 직접 가져오기
                var container = FindFirstObjectByType<Zenject.SceneContext>()?.Container;
                if (container != null)
                {
                    tooltipManager = container.TryResolve<SkillCardTooltipManager>();
                }
                
                // 여전히 null이면 직접 찾기
                if (tooltipManager == null)
                {
                    tooltipManager = FindFirstObjectByType<SkillCardTooltipManager>();
                }
            }
            return tooltipManager;
        }
        
        // 툴팁 관련 상태
        private bool isHovering = false;
        private bool isDragging = false;
        private Coroutine tooltipCoroutine;

        // 카드 애니메이션 상태 플래그
        public bool IsAnimating { get; private set; }

        // 호버 효과 관련
        private Tween scaleTween;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // DI를 통해 tooltipManager가 자동으로 주입됨
        }

        private void OnEnable()
        {
            RegisterToTooltipManager();
        }

        private void OnDisable()
        {
            UnregisterFromTooltipManager();
            scaleTween?.Kill();
        }

        private void OnDestroy()
        {
            UnregisterFromTooltipManager();
            scaleTween?.Kill();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 애니메이션 상태를 설정합니다.
        /// </summary>
        /// <param name="value">애니메이션 중 여부</param>
        public void SetAnimating(bool value)
        {
            IsAnimating = value;
        }

        /// <summary>
        /// 스킬 카드 데이터를 설정하고 UI를 초기화합니다.
        /// 설명, 데미지, 카드명 필드는 선택사항이며 null이어도 정상 작동합니다.
        /// </summary>
        /// <param name="newCard">연결할 카드 인스턴스</param>
        public void SetCard(ISkillCard newCard)
        {
            if (card != null)
            {
                UnregisterFromTooltipManager();
            }

            card = newCard;

            if (card == null)
            {
                Debug.LogWarning("[SkillCardUI] 설정할 카드가 null입니다.");
                return;
            }

            RegisterToTooltipManager();

            // 플레이어 마커 카드인 경우: 완전히 보이지 않게 처리
            bool isPlayerMarker = card.CardDefinition?.cardId == "PLAYER_MARKER";
            if (isPlayerMarker)
            {
                // 자식 텍스트 및 아트 오브젝트 비활성화
                if (cardNameText != null) cardNameText.gameObject.SetActive(false);
                if (damageText != null) damageText.gameObject.SetActive(false);
                if (descriptionText != null) descriptionText.gameObject.SetActive(false);
                if (cardArtImage != null) cardArtImage.gameObject.SetActive(false);

                // 부모(Image) 컴포넌트 완전히 숨김 처리
                var rootImage = GetComponent<UnityEngine.UI.Image>();
                if (rootImage != null)
                {
                    // 투명하게 만들어서 보이지 않게 함
                    rootImage.color = new Color(1f, 1f, 1f, 0f);
                    rootImage.raycastTarget = false;
                }

                // 마커는 상호작용/드래그 비활성화
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.alpha = 0f; // 완전히 투명하게 설정
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
                    // 카드 인스턴스의 실제 데미지 사용 (데미지 오버라이드 포함)
                    int damage = card.GetBaseDamage();
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

            // 카드 배경 이미지 설정 (플레이어/적 구분)
            SetCardBackgroundImage(card);

            // 소유자에 따라 드래그/상호작용 제어 (드래그는 플레이어만, 툴팁은 모든 카드)
            bool isPlayerCard = card.IsFromPlayer();
            if (canvasGroup != null)
            {
                // 드래그는 플레이어 카드만 가능하지만, 툴팁을 위한 레이캐스트는 모든 카드에서 활성화
                canvasGroup.interactable = true; // 모든 카드에서 상호작용 허용 (툴팁을 위해)
                canvasGroup.blocksRaycasts = true; // 모든 카드에서 레이캐스트 허용 (툴팁을 위해)
            }
            if (TryGetComponent(out Game.CombatSystem.DragDrop.CardDragHandler dragHandler))
            {
                dragHandler.enabled = isPlayerCard; // 드래그는 플레이어 카드만
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

        #region Tooltip Events

        /// <summary>
        /// 마우스가 카드에 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 호버 확대 효과 (플레이어 카드만)
            if (card != null && card.IsFromPlayer())
            {
                scaleTween?.Kill();
                scaleTween = transform.DOScale(hoverScale, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(true);
            }

            var currentTooltipManager = GetTooltipManager();

            if (!enableTooltip || currentTooltipManager == null || card == null || isHovering)
            {
                return;
            }

            // 카드가 등록되지 않았으면 등록
            RegisterToTooltipManager();

            isHovering = true;

            // 드래그 중이면 무시
            if (isDragging && hideTooltipOnDrag)
            {
                return;
            }

            // 지연된 툴팁 표시 시작
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
            }
            tooltipCoroutine = StartCoroutine(ShowTooltipDelayed());
        }

        /// <summary>
        /// 마우스가 카드에서 이탈했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            // 호버 확대 효과 해제 (플레이어 카드만)
            if (card != null && card.IsFromPlayer())
            {
                scaleTween?.Kill();
                scaleTween = transform.DOScale(1f, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(true);
            }

            var currentTooltipManager = GetTooltipManager();

            if (!enableTooltip || currentTooltipManager == null || !isHovering)
            {
                return;
            }

            isHovering = false;

            // 진행 중인 툴팁 표시 코루틴 중지
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }

            // 툴팁 숨김
            currentTooltipManager.OnCardHoverExit();
        }

        /// <summary>
        /// 마우스 클릭 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            var currentTooltipManager = GetTooltipManager();
            GameLogger.LogInfo($"OnPointerClick 호출됨 - 버튼: {eventData.button}, enableTooltip: {enableTooltip}, tooltipManager: {currentTooltipManager != null}, card: {card != null}", GameLogger.LogCategory.UI);
            
            if (!enableTooltip)
            {
                GameLogger.LogWarning("툴팁이 비활성화되어 있습니다.", GameLogger.LogCategory.UI);
                return;
            }
            
            if (currentTooltipManager == null)
            {
                GameLogger.LogWarning("tooltipManager를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
                return;
            }
            
            if (card == null)
            {
                GameLogger.LogWarning("card가 null입니다.", GameLogger.LogCategory.UI);
                return;
            }
            
        }

        #endregion

        #region Tooltip Coroutines

        /// <summary>
        /// 지연된 툴팁 표시를 처리합니다.
        /// </summary>
        private System.Collections.IEnumerator ShowTooltipDelayed()
        {
            // 툴팁 지연 시작
            yield return new WaitForSeconds(tooltipDelay);
            
            // 지연 후에도 여전히 호버 중이고 드래그 중이 아니면 툴팁 표시
            if (isHovering && !isDragging)
            {
                var currentTooltipManager = GetTooltipManager();
                if (currentTooltipManager != null && card != null)
                {
                    // 툴팁 표시
                    currentTooltipManager.OnCardHoverEnter(card);
                }
                else
                {
                    GameLogger.LogWarning($"[SkillCardUI] 툴팁 표시 실패 - tooltipManager: {currentTooltipManager != null}, card: {card != null}", GameLogger.LogCategory.UI);
                }
            }
            else
            {
                // 툴팁 표시 취소
            }
        }

        #endregion

        #region Drag State Management

        /// <summary>
        /// 드래그 시작 시 호출됩니다. (CardDragHandler에서 호출)
        /// </summary>
        public void OnDragStart()
        {
            isDragging = true;
            
            // 드래그 중 툴팁 숨김 설정이 활성화되어 있으면 툴팁 숨김
            if (hideTooltipOnDrag && tooltipManager != null)
            {
                tooltipManager.OnCardHoverExit();
            }
        }

        /// <summary>
        /// 드래그 종료 시 호출됩니다. (CardDragHandler에서 호출)
        /// </summary>
        public void OnDragEnd()
        {
            isDragging = false;

            // 드래그 종료 후 여전히 호버 중이면 툴팁 다시 표시
            if (isHovering && enableTooltip && tooltipManager != null && card != null)
            {
                tooltipCoroutine = StartCoroutine(ShowTooltipDelayed());
            }
        }

        #endregion

        #region Tooltip Manager Registration

        /// <summary>
        /// 툴팁 매니저에 카드 UI를 등록합니다.
        /// </summary>
        private void RegisterToTooltipManager()
        {
            var currentTooltipManager = GetTooltipManager();
            if (currentTooltipManager != null && card != null)
            {
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    currentTooltipManager.RegisterCardUI(card, rectTransform);
                }
            }
        }

        /// <summary>
        /// 툴팁 매니저에서 카드 UI 등록을 해제합니다.
        /// </summary>
        private void UnregisterFromTooltipManager()
        {
            var currentTooltipManager = GetTooltipManager();
            if (currentTooltipManager != null && card != null)
            {
                currentTooltipManager.UnregisterCardUI(card);
            }
        }

        /// <summary>
        /// 카드의 소유자에 따라 배경 이미지를 설정합니다.
        /// </summary>
        /// <param name="card">설정할 카드</param>
        private void SetCardBackgroundImage(ISkillCard card)
        {
            if (card == null) return;

            // 루트 Image 컴포넌트 가져오기 (카드 배경)
            var rootImage = GetComponent<UnityEngine.UI.Image>();
            if (rootImage == null)
            {
                Debug.LogWarning("[SkillCardUI] 루트 Image 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            // 카드 소유자에 따라 배경 이미지 설정
            bool isPlayerCard = card.IsFromPlayer();
            Sprite backgroundSprite = isPlayerCard ? playerCardBackground : enemyCardBackground;

            if (backgroundSprite != null)
            {
                rootImage.sprite = backgroundSprite;
                
                // 적 카드인 경우 이미지 색상을 #9D2933으로 설정
                if (!isPlayerCard)
                {
                    if (ColorUtility.TryParseHtmlString("#9D2933", out Color enemyColor))
                    {
                        rootImage.color = enemyColor;
                        GameLogger.LogInfo($"[SkillCardUI] 적 카드 이미지 색상 설정: #9D2933", GameLogger.LogCategory.UI);
                    }
                    else
                    {
                        // 폴백: 직접 RGB 값 사용
                        rootImage.color = new Color(157f / 255f, 41f / 255f, 51f / 255f, 1f);
                        GameLogger.LogInfo($"[SkillCardUI] 적 카드 이미지 색상 설정 (폴백): #9D2933", GameLogger.LogCategory.UI);
                    }
                }
                else
                {
                    // 플레이어 카드는 기본 색상(흰색) 유지
                    rootImage.color = Color.white;
                }
                
                GameLogger.LogInfo($"[SkillCardUI] 카드 배경 설정: {(isPlayerCard ? "플레이어" : "적")} 카드", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning($"[SkillCardUI] {(isPlayerCard ? "플레이어" : "적")} 카드 배경 이미지가 설정되지 않았습니다.", GameLogger.LogCategory.UI);
            }
        }

        #endregion
    }
}