using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Manager;
using Game.CombatSystem.DragDrop;
using Game.CoreSystem.Utility;
using Zenject;

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

        [Header("UI 그룹")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("툴팁 설정")]
        [Tooltip("툴팁 활성화 여부")]
        [SerializeField] private bool enableTooltip = true;

        [Tooltip("툴팁 표시 지연 시간 (초)")]
        [SerializeField] private float tooltipDelay = 0.5f;

        [Tooltip("드래그 중 툴팁 숨김 여부")]
        [SerializeField] private bool hideTooltipOnDrag = true;

        [Header("툴팁 고정 시각적 표시")]
        [Tooltip("툴팁 고정 시 표시할 테두리 이미지")]
        [SerializeField] private Image fixedTooltipBorder;
        
        [Tooltip("툴팁 고정 시 테두리 색상")]
        [SerializeField] private Color fixedTooltipColor = Color.yellow;
        
        [Tooltip("툴팁 고정 시 테두리 두께")]
        [SerializeField] private float fixedTooltipBorderWidth = 3f;

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
        private bool isTooltipFixed = false;
        private Coroutine tooltipCoroutine;

        // 카드 애니메이션 상태 플래그
        public bool IsAnimating { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // DI를 통해 tooltipManager가 자동으로 주입됨
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

            // 소유자에 따라 드래그/상호작용 제어 (드래그는 플레이어만, 툴팁은 모든 카드)
            bool isPlayerCard = card.IsFromPlayer();
            if (canvasGroup != null)
            {
                // 드래그는 플레이어 카드만 가능하지만, 툴팁을 위한 레이캐스트는 모든 카드에서 활성화
                canvasGroup.interactable = isPlayerCard;
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
            var currentTooltipManager = GetTooltipManager();
            GameLogger.LogInfo($"[SkillCardUI] OnPointerEnter 호출됨 - enableTooltip: {enableTooltip}, tooltipManager: {currentTooltipManager != null}, card: {card != null}, isPlayerCard: {card?.IsFromPlayer()}, isHovering: {isHovering}", GameLogger.LogCategory.UI);

            if (!enableTooltip)
            {
                GameLogger.LogWarning("[SkillCardUI] 툴팁이 비활성화되어 있습니다.", GameLogger.LogCategory.UI);
                return;
            }

            if (currentTooltipManager == null)
            {
                GameLogger.LogWarning("[SkillCardUI] tooltipManager를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
                return;
            }

            if (card == null)
            {
                GameLogger.LogWarning("[SkillCardUI] card가 null입니다.", GameLogger.LogCategory.UI);
                return;
            }

            // 이미 호버 중이면 무시
            if (isHovering)
            {
                GameLogger.LogInfo("[SkillCardUI] 이미 호버 중이므로 무시", GameLogger.LogCategory.UI);
                return;
            }

            // 모든 카드 타입에 대해 툴팁 표시 허용 (플레이어, 적, 선택 화면 등)
            GameLogger.LogInfo($"[SkillCardUI] 모든 카드 타입 툴팁 허용: {card.GetCardName()} (소유자: {card.GetOwner()})", GameLogger.LogCategory.UI);

            isHovering = true;

            // 고정된 툴팁이 있으면 일반 호버 무시
            if (isTooltipFixed)
            {
                GameLogger.LogInfo("[SkillCardUI] 툴팁이 이미 고정되어 있어 호버 무시", GameLogger.LogCategory.UI);
                return;
            }

            // 드래그 중이면 툴팁 표시하지 않음
            if (isDragging && hideTooltipOnDrag)
            {
                GameLogger.LogInfo("[SkillCardUI] 드래그 중이어서 툴팁 표시하지 않음", GameLogger.LogCategory.UI);
                return;
            }

            // 지연된 툴팁 표시 시작
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
            }
            tooltipCoroutine = StartCoroutine(ShowTooltipDelayed());

            GameLogger.LogInfo($"카드 호버 시작: {card.GetCardName()}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 마우스가 카드에서 이탈했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            var currentTooltipManager = GetTooltipManager();
            GameLogger.LogInfo($"[SkillCardUI] OnPointerExit 호출됨 - enableTooltip: {enableTooltip}, tooltipManager: {currentTooltipManager != null}, isHovering: {isHovering}", GameLogger.LogCategory.UI);

            if (!enableTooltip || currentTooltipManager == null)
            {
                GameLogger.LogInfo("[SkillCardUI] 툴팁 비활성화 또는 매니저 없음으로 종료", GameLogger.LogCategory.UI);
                return;
            }

            // 호버 중이 아니면 무시
            if (!isHovering)
            {
                GameLogger.LogInfo("[SkillCardUI] 호버 중이 아니므로 무시", GameLogger.LogCategory.UI);
                return;
            }

            isHovering = false;

            // 진행 중인 툴팁 표시 코루틴 중지
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }

            // 고정된 툴팁이 있으면 호버 종료 시에도 툴팁을 유지
            if (isTooltipFixed)
            {
                GameLogger.LogInfo("[SkillCardUI] 툴팁이 고정되어 있어 호버 종료 시에도 툴팁 유지", GameLogger.LogCategory.UI);
                return;
            }

            // 툴팁 숨김
            currentTooltipManager.OnCardHoverExit();

            GameLogger.LogInfo($"카드 호버 종료: {card?.GetCardName()}", GameLogger.LogCategory.UI);
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
            
            // 모든 카드 타입에 대해 툴팁 클릭 허용 (플레이어, 적, 선택 화면 등)
            GameLogger.LogInfo($"[SkillCardUI] 모든 카드 타입 클릭 허용: {card.GetCardName()} (소유자: {card.GetOwner()})", GameLogger.LogCategory.UI);

            // 우클릭: 툴팁 고정/해제 토글
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                GameLogger.LogInfo("우클릭 감지됨 - 툴팁 토글 실행", GameLogger.LogCategory.UI);
                ToggleTooltipFixed();
            }
            // 좌클릭: 고정된 툴팁 해제 (고정된 툴팁이 있을 때만)
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (isTooltipFixed)
                {
                    GameLogger.LogInfo("좌클릭 감지됨 - 고정된 툴팁 해제 실행", GameLogger.LogCategory.UI);
                    ReleaseFixedTooltip();
                }
            }
        }

        #endregion

        #region Tooltip Coroutines

        /// <summary>
        /// 지연된 툴팁 표시를 처리합니다.
        /// </summary>
        private System.Collections.IEnumerator ShowTooltipDelayed()
        {
            GameLogger.LogInfo($"[SkillCardUI] ShowTooltipDelayed 시작 - 지연 시간: {tooltipDelay}초", GameLogger.LogCategory.UI);
            yield return new WaitForSeconds(tooltipDelay);
            
            // 지연 후에도 여전히 호버 중이고 드래그 중이 아니면 툴팁 표시
            if (isHovering && !isDragging)
            {
                var currentTooltipManager = GetTooltipManager();
                if (currentTooltipManager != null && card != null)
                {
                    GameLogger.LogInfo($"[SkillCardUI] 툴팁 표시 요청: {card.GetCardName()}", GameLogger.LogCategory.UI);
                    currentTooltipManager.OnCardHoverEnter(card);
                }
                else
                {
                    GameLogger.LogWarning($"[SkillCardUI] 툴팁 표시 실패 - tooltipManager: {currentTooltipManager != null}, card: {card != null}", GameLogger.LogCategory.UI);
                }
            }
            else
            {
                GameLogger.LogInfo($"[SkillCardUI] 툴팁 표시 취소 - isHovering: {isHovering}, isDragging: {isDragging}", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region Tooltip Fixed Management

        /// <summary>
        /// 툴팁 고정 상태를 토글합니다.
        /// </summary>
        private void ToggleTooltipFixed()
        {
            GameLogger.LogInfo($"ToggleTooltipFixed 호출됨 - 현재 고정 상태: {isTooltipFixed}", GameLogger.LogCategory.UI);
            
            if (isTooltipFixed)
            {
                ReleaseFixedTooltip();
            }
            else
            {
                FixTooltip();
            }
        }

        /// <summary>
        /// 툴팁을 고정합니다.
        /// </summary>
        private void FixTooltip()
        {
            isTooltipFixed = true;
            
            // 지연된 툴팁 표시 취소
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }
            
            // 즉시 툴팁 표시
            var currentTooltipManager = GetTooltipManager();
            if (currentTooltipManager != null && card != null)
            {
                currentTooltipManager.OnCardHoverEnter(card);
                
                // 툴팁이 표시된 후 고정
                StartCoroutine(FixTooltipAfterShow());
            }
            
            // 시각적 표시 업데이트
            UpdateFixedTooltipVisual(true);
            
            GameLogger.LogInfo($"툴팁 고정: {card?.GetCardName()}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁이 표시된 후 고정하는 코루틴입니다.
        /// </summary>
        private System.Collections.IEnumerator FixTooltipAfterShow()
        {
            // 툴팁이 표시될 때까지 대기
            yield return new WaitForSeconds(0.1f);
            
            // 툴팁 매니저에서 현재 툴팁을 가져와서 고정
            var currentTooltipManager = GetTooltipManager();
            if (currentTooltipManager != null && currentTooltipManager.CurrentTooltip != null)
            {
                currentTooltipManager.CurrentTooltip.FixTooltip();
                GameLogger.LogInfo("[SkillCardUI] 툴팁 위치 고정 완료", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 고정된 툴팁을 해제합니다.
        /// </summary>
        private void ReleaseFixedTooltip()
        {
            isTooltipFixed = false;
            
            var currentTooltipManager = GetTooltipManager();
            if (currentTooltipManager != null && currentTooltipManager.CurrentTooltip != null)
            {
                // 툴팁 고정 해제
                currentTooltipManager.CurrentTooltip.UnfixTooltip();
                currentTooltipManager.ForceHideTooltip();
            }
            
            // 시각적 표시 업데이트
            UpdateFixedTooltipVisual(false);
            
            GameLogger.LogInfo("툴팁 고정 해제", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 외부에서 툴팁 고정을 해제할 수 있도록 하는 공개 메서드입니다.
        /// </summary>
        public void ForceReleaseTooltip()
        {
            if (isTooltipFixed)
            {
                ReleaseFixedTooltip();
            }
        }

        /// <summary>
        /// 툴팁 고정 상태의 시각적 표시를 업데이트합니다.
        /// </summary>
        /// <param name="isFixed">고정 상태 여부</param>
        private void UpdateFixedTooltipVisual(bool isFixed)
        {
            if (fixedTooltipBorder != null)
            {
                fixedTooltipBorder.gameObject.SetActive(isFixed);
                
                if (isFixed)
                {
                    fixedTooltipBorder.color = fixedTooltipColor;
                    
                    // 테두리 두께 설정 (RectTransform의 크기 조정)
                    var rectTransform = fixedTooltipBorder.rectTransform;
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(fixedTooltipBorderWidth, fixedTooltipBorderWidth);
                    }
                }
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
    }
}