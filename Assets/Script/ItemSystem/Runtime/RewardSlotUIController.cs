using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Game.ItemSystem.Data;
using Game.ItemSystem.Utility;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Manager;
using Game.ItemSystem.Interface;
using DG.Tweening;
using Zenject;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 보상 슬롯 UI를 관리하는 컨트롤러입니다.
    /// 개별 아이템 슬롯의 표시와 선택을 처리합니다.
    /// </summary>
    public class RewardSlotUIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region UI 참조

        [Header("슬롯 UI 구성 요소")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private Button slotButton;

        [Header("슬롯 설정")]
        [SerializeField] private Color normalColor = Color.white;
        
        [Header("툴팁용 Image (자동 할당)")]
        [SerializeField] private Image backgroundImage;

        [Header("호버 효과 설정")]
        [Tooltip("호버 시 스케일")]
        [SerializeField] private float hoverScale = 1.2f;
        
        #endregion
        
        #region 툴팁 관련
        
        private ItemTooltipManager tooltipManager;
        private RectTransform rectTransform;

        // 호버 효과 관련
        private Tween scaleTween;
        
        #endregion

        #region 의존성

        [Inject(Optional = true)] private IItemService _itemService;

        #endregion

        #region 상태

        private ActiveItemDefinition currentItem;
        private PassiveItemDefinition currentPassive;
        private int slotIndex = -1;
        private bool isInteractable = true;
        private bool isPassiveSlot = false;

        #endregion

        #region 이벤트

        /// <summary>
        /// 슬롯이 선택되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<ActiveItemDefinition, int> OnSlotSelected;
        public event System.Action<PassiveItemDefinition, int> OnPassiveSlotSelected;

        #endregion

        #region Unity 생명주기

        private void Start()
        {
            InitializeSlot();
            FindTooltipManager();
        }
        
        private void OnDisable()
        {
            scaleTween?.Kill();
        }
        
        private void OnDestroy()
        {
            UnregisterFromTooltipManager();
            scaleTween?.Kill();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 슬롯을 초기화합니다.
        /// </summary>
        private void InitializeSlot()
        {
            // RectTransform 캐시
            rectTransform = GetComponent<RectTransform>();
            
            // 배경 Image 찾기 (툴팁용)
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
                if (backgroundImage != null)
                {
                    // raycastTarget 활성화해야 포인터 이벤트 감지
                    backgroundImage.raycastTarget = true;
                    GameLogger.LogInfo("[RewardSlotUI] 배경 Image 찾음 및 raycast 활성화", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[RewardSlotUI] Image 컴포넌트를 찾을 수 없습니다", GameLogger.LogCategory.UI);
                }
            }
            
            // 슬롯 버튼 이벤트 연결
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotClicked);
            }

            GameLogger.LogInfo("[RewardSlotUI] 슬롯 초기화 완료", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 툴팁 매니저를 찾습니다.
        /// </summary>
        private void FindTooltipManager()
        {
            tooltipManager = ItemTooltipManager.Instance;
            if (tooltipManager == null)
            {
                GameLogger.LogWarning("[RewardSlotUI] ItemTooltipManager 인스턴스를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 툴팁 매니저에 등록합니다.
        /// </summary>
        private void RegisterToTooltipManager()
        {
            if (tooltipManager == null || rectTransform == null)
                return;

            if (currentItem != null)
            {
                tooltipManager.RegisterItemUI(currentItem, rectTransform);
            }
            else if (currentPassive != null)
            {
                tooltipManager.RegisterPassiveItemUI(currentPassive, rectTransform);
            }
        }
        
        /// <summary>
        /// 툴팁 매니저 등록을 해제합니다.
        /// </summary>
        private void UnregisterFromTooltipManager()
        {
            if (tooltipManager == null)
                return;

            if (currentItem != null)
            {
                tooltipManager.UnregisterItemUI(currentItem);
            }
            else if (currentPassive != null)
            {
                tooltipManager.UnregisterPassiveItemUI(currentPassive);
            }
        }

        #endregion

        #region 슬롯 설정

        /// <summary>
        /// 슬롯에 아이템을 설정합니다.
        /// </summary>
        /// <param name="item">설정할 아이템</param>
        /// <param name="index">슬롯 인덱스</param>
        public void SetupSlot(ActiveItemDefinition item, int index)
        {
            if (item == null)
            {
                GameLogger.LogWarning("[RewardSlotUI] 설정할 아이템이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            // 기존 아이템 등록 해제
            if (currentItem != null)
            {
                UnregisterFromTooltipManager();
            }
            
            currentItem = item;
            currentPassive = null;
            isPassiveSlot = false;
            slotIndex = index;

            // UI 업데이트
            UpdateSlotUIActive();
            
            // 툴팁 매니저에 등록
            RegisterToTooltipManager();

            GameLogger.LogInfo($"[RewardSlotUI] 슬롯 설정 완료: {item.DisplayName} (인덱스: {index})", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 슬롯의 인덱스만 변경합니다.
        /// </summary>
        /// <param name="index">새로운 인덱스</param>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
            GameLogger.LogInfo($"[RewardSlotUI] 슬롯 인덱스 변경: {index}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 슬롯 UI를 업데이트합니다.
        /// </summary>
        private void UpdateSlotUIActive()
        {
            if (currentItem == null) return;

            // UIUpdateHelper를 사용하여 슬롯 UI 업데이트
            UIUpdateHelper.UpdateItemSlotUI(itemIcon, itemNameText, itemDescriptionText, currentItem, normalColor);
        }

        /// <summary>
        /// 패시브 슬롯 설정
        /// </summary>
        public void SetupSlot(PassiveItemDefinition item, int index)
        {
            if (item == null)
            {
                GameLogger.LogWarning("[RewardSlotUI] 설정할 패시브 아이템이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            if (currentItem != null)
            {
                UnregisterFromTooltipManager();
            }

            currentItem = null;
            currentPassive = item;
            isPassiveSlot = true;
            slotIndex = index;

            // UI 업데이트 (수동 - 공통 헬퍼는 액티브 전용)
            if (itemIcon != null)
            {
                itemIcon.sprite = item.Icon;
                itemIcon.color = normalColor;
            }
            if (itemNameText != null) itemNameText.text = item.DisplayName;
            if (itemDescriptionText != null) itemDescriptionText.text = item.Description;

            // 툴팁 매니저에 등록
            RegisterToTooltipManager();

            GameLogger.LogInfo($"[RewardSlotUI] 패시브 슬롯 설정 완료: {item.DisplayName} (인덱스: {index})", GameLogger.LogCategory.UI);
        }

        #endregion

        #region 상호작용

        /// <summary>
        /// 슬롯이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void OnSlotClicked()
        {
            if (!isPassiveSlot && currentItem == null)
            {
                GameLogger.LogWarning("[RewardSlotUI] 현재 아이템이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            if (!isInteractable)
            {
                GameLogger.LogWarning("[RewardSlotUI] 슬롯이 비활성화 상태입니다", GameLogger.LogCategory.UI);
                return;
            }

            if (isPassiveSlot)
            {
                OnPassiveSlotSelected?.Invoke(currentPassive, slotIndex);
                GameLogger.LogInfo($"[RewardSlotUI] 패시브 아이템 선택됨: {currentPassive?.DisplayName ?? "null"} (인덱스: {slotIndex})", GameLogger.LogCategory.UI);
            }
            else
            {
                OnSlotSelected?.Invoke(currentItem, slotIndex);
                GameLogger.LogInfo($"[RewardSlotUI] 아이템 선택됨: {currentItem.DisplayName} (인덱스: {slotIndex})", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 슬롯의 상호작용 가능 여부를 설정합니다.
        /// </summary>
        /// <param name="interactable">상호작용 가능 여부</param>
        public void SetInteractable(bool interactable)
        {
            GameLogger.LogInfo($"[RewardSlotUI] SetInteractable({interactable}) 호출 - item: {currentItem?.DisplayName ?? "null"}, button: {slotButton?.name ?? "null"}", GameLogger.LogCategory.UI);
            
            isInteractable = interactable;
            
            if (slotButton != null)
            {
                bool oldValue = slotButton.interactable;
                slotButton.interactable = interactable;
                GameLogger.LogInfo($"[RewardSlotUI] 버튼 interactable 변경: {oldValue} → {slotButton.interactable}", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[RewardSlotUI] slotButton이 null입니다!", GameLogger.LogCategory.UI);
            }

            // 비활성화 시 색상 변경
            if (itemIcon != null)
            {
                itemIcon.color = interactable ? normalColor : Color.gray;
                GameLogger.LogInfo($"[RewardSlotUI] 아이콘 색상 변경: {(interactable ? "normal" : "gray")}", GameLogger.LogCategory.UI);
            }

            GameLogger.LogInfo($"[RewardSlotUI] 슬롯 상호작용 상태 변경 완료: {interactable}", GameLogger.LogCategory.UI);
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 슬롯을 리셋합니다.
        /// </summary>
        public void ResetSlot()
        {
            currentItem = null;
            slotIndex = -1;
            isInteractable = true;

            // UIUpdateHelper를 사용하여 슬롯 리셋
            UIUpdateHelper.SetEmptySlot(itemIcon, itemNameText, itemDescriptionText, normalColor);

            if (slotButton != null)
            {
                slotButton.interactable = true;
            }
        }

        /// <summary>
        /// 현재 아이템을 반환합니다.
        /// </summary>
        public ActiveItemDefinition GetCurrentItem()
        {
            return currentItem;
        }

        public PassiveItemDefinition GetCurrentPassive() => currentPassive;

        /// <summary>
        /// 슬롯 인덱스를 반환합니다.
        /// </summary>
        public int GetSlotIndex()
        {
            return slotIndex;
        }

        /// <summary>
        /// 슬롯이 상호작용 가능한지 확인합니다.
        /// </summary>
        public bool IsInteractable => isInteractable;

        #endregion
        
        #region 툴팁 호버 이벤트
        
        /// <summary>
        /// 포인터가 오브젝트에 진입했을 때 호출됩니다.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 호버 확대 효과
            scaleTween?.Kill();
            scaleTween = transform.DOScale(hoverScale, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);

            if (tooltipManager == null)
                return;
            
            // 보상창 슬롯의 RectTransform을 소스로 명시 전달하여 인벤토리 슬롯과 구분
            if (currentItem != null)
            {
                tooltipManager.OnItemHoverEnter(currentItem, rectTransform);
            }
            else if (currentPassive != null)
            {
                // 보상창에서는 실제 강화 레벨을 조회 (아직 선택하지 않았으면 0)
                int enhancementLevel = 0;
                if (_itemService != null)
                {
                    string skillId = GetPassiveItemSkillId(currentPassive);
                    if (!string.IsNullOrEmpty(skillId))
                    {
                        enhancementLevel = _itemService.GetSkillEnhancementLevel(skillId);
                    }
                }
                // 보상창 컨텍스트로 표시
                tooltipManager.OnPassiveItemHoverEnter(currentPassive, rectTransform, enhancementLevel, isRewardPanel: true);
            }
        }

        /// <summary>
        /// 패시브 아이템의 스킬 ID를 가져옵니다.
        /// </summary>
        private string GetPassiveItemSkillId(PassiveItemDefinition item)
        {
            if (item == null) return null;

            if (item.IsPlayerHealthBonus)
            {
                var itemKey = !string.IsNullOrEmpty(item.ItemId) ? item.ItemId : System.Guid.NewGuid().ToString();
                return $"__PLAYER_HP__:{itemKey}";
            }
            else if (item.TargetSkill != null)
            {
                return !string.IsNullOrEmpty(item.TargetSkill.displayName) 
                    ? item.TargetSkill.displayName 
                    : item.TargetSkill.cardId;
            }

            return null;
        }
        
        /// <summary>
        /// 포인터가 오브젝트를 벗어났을 때 호출됩니다.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // 호버 확대 효과 해제
            scaleTween?.Kill();
            scaleTween = transform.DOScale(1f, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);

            if (tooltipManager == null)
                return;
            
            tooltipManager.OnItemHoverExit();
        }
        
        #endregion

        #region 디버그

        /// <summary>
        /// 현재 슬롯 상태를 출력합니다.
        /// </summary>
        [ContextMenu("슬롯 상태 출력")]
        public void PrintSlotStatus()
        {
            string itemName = currentItem?.DisplayName ?? "없음";
            GameLogger.LogInfo($"[RewardSlotUI] 아이템: {itemName}, 인덱스: {slotIndex}, 상호작용: {isInteractable}", GameLogger.LogCategory.UI);
        }

        #endregion
    }
}
