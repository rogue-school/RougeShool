using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Game.ItemSystem.Data;
using Game.ItemSystem.Utility;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Manager;

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
        
        #endregion
        
        #region 툴팁 관련
        
        private ItemTooltipManager tooltipManager;
        private RectTransform rectTransform;
        
        #endregion

        #region 상태

        private ActiveItemDefinition currentItem;
        private int slotIndex = -1;
        private bool isInteractable = true;

        #endregion

        #region 이벤트

        /// <summary>
        /// 슬롯이 선택되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<ActiveItemDefinition, int> OnSlotSelected;

        #endregion

        #region Unity 생명주기

        private void Start()
        {
            InitializeSlot();
            FindTooltipManager();
        }
        
        private void OnDestroy()
        {
            UnregisterFromTooltipManager();
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
            tooltipManager = Object.FindFirstObjectByType<ItemTooltipManager>();
            if (tooltipManager == null)
            {
                GameLogger.LogWarning("[RewardSlotUI] ItemTooltipManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 툴팁 매니저에 등록합니다.
        /// </summary>
        private void RegisterToTooltipManager()
        {
            if (tooltipManager == null || currentItem == null || rectTransform == null)
                return;

            tooltipManager.RegisterItemUI(currentItem, rectTransform);
        }
        
        /// <summary>
        /// 툴팁 매니저 등록을 해제합니다.
        /// </summary>
        private void UnregisterFromTooltipManager()
        {
            if (tooltipManager == null || currentItem == null)
                return;

            tooltipManager.UnregisterItemUI(currentItem);
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
            slotIndex = index;

            // UI 업데이트
            UpdateSlotUI();
            
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
        private void UpdateSlotUI()
        {
            if (currentItem == null) return;

            // UIUpdateHelper를 사용하여 슬롯 UI 업데이트
            UIUpdateHelper.UpdateItemSlotUI(itemIcon, itemNameText, itemDescriptionText, currentItem, normalColor);
        }

        #endregion

        #region 상호작용

        /// <summary>
        /// 슬롯이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void OnSlotClicked()
        {
            if (currentItem == null)
            {
                GameLogger.LogWarning("[RewardSlotUI] 현재 아이템이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            if (!isInteractable)
            {
                GameLogger.LogWarning("[RewardSlotUI] 슬롯이 비활성화 상태입니다", GameLogger.LogCategory.UI);
                return;
            }

            // 이벤트 발생 (아이템과 인덱스 전달)
            OnSlotSelected?.Invoke(currentItem, slotIndex);
            GameLogger.LogInfo($"[RewardSlotUI] 아이템 선택됨: {currentItem.DisplayName} (인덱스: {slotIndex})", GameLogger.LogCategory.UI);
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
            if (currentItem == null || tooltipManager == null)
                return;
            
            tooltipManager.OnItemHoverEnter(currentItem);
        }
        
        /// <summary>
        /// 포인터가 오브젝트를 벗어났을 때 호출됩니다.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
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
