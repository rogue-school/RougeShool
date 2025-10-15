using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 보상 슬롯 UI를 관리하는 컨트롤러입니다.
    /// 개별 아이템 슬롯의 표시와 선택을 처리합니다.
    /// </summary>
    public class RewardSlotUIController : MonoBehaviour
    {
        #region UI 참조

        [Header("슬롯 UI 구성 요소")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private Button slotButton;

        [Header("슬롯 설정")]
        [SerializeField] private Color normalColor = Color.white;

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
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 슬롯을 초기화합니다.
        /// </summary>
        private void InitializeSlot()
        {
            // 슬롯 버튼 이벤트 연결
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotClicked);
            }

            GameLogger.LogInfo("[RewardSlotUI] 슬롯 초기화 완료", GameLogger.LogCategory.UI);
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

            currentItem = item;
            slotIndex = index;

            // UI 업데이트
            UpdateSlotUI();

            GameLogger.LogInfo($"[RewardSlotUI] 슬롯 설정 완료: {item.DisplayName} (인덱스: {index})", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 슬롯 UI를 업데이트합니다.
        /// </summary>
        private void UpdateSlotUI()
        {
            if (currentItem == null) return;

            // 아이템 아이콘 설정
            if (itemIcon != null)
            {
                itemIcon.sprite = currentItem.Icon;
                itemIcon.color = normalColor;
            }

            // 아이템 이름 설정
            if (itemNameText != null)
            {
                itemNameText.text = currentItem.DisplayName;
            }

            // 아이템 설명 설정
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = currentItem.Description;
            }
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
            isInteractable = interactable;
            
            if (slotButton != null)
            {
                slotButton.interactable = interactable;
            }

            // 비활성화 시 색상 변경
            if (itemIcon != null)
            {
                itemIcon.color = interactable ? normalColor : Color.gray;
            }

            GameLogger.LogInfo($"[RewardSlotUI] 슬롯 상호작용 상태 변경: {interactable}", GameLogger.LogCategory.UI);
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

            // UI 초기화
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.color = normalColor;
            }

            if (itemNameText != null)
            {
                itemNameText.text = "";
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = "";
            }

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
