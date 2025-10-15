using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 개별 액티브 아이템 UI를 관리하는 컴포넌트입니다.
    /// 아이템 아이콘, 이름, 설명을 표시하는 역할만 담당합니다.
    /// </summary>
    public class ActiveItemUI : MonoBehaviour
    {
        #region UI 참조

        [Header("아이템 UI 구성 요소")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemDescription;

        #endregion

        #region 상태

        private ActiveItemDefinition currentItem;
        private int slotIndex = -1;

        #endregion

        #region 이벤트

        /// <summary>
        /// 아이템이 클릭되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<int> OnItemClicked;

        #endregion

        #region Unity 생명주기

        private void Start()
        {
            InitializeItemUI();
            SetupButtonEvent();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 아이템 UI를 초기화합니다.
        /// </summary>
        private void InitializeItemUI()
        {
            // 아이템 아이콘이 할당되지 않았으면 자동으로 찾기
            if (itemIcon == null)
            {
                itemIcon = GetComponent<Image>();
                if (itemIcon == null)
                {
                    GameLogger.LogWarning("[ActiveItemUI] Image 컴포넌트를 찾을 수 없습니다!", GameLogger.LogCategory.UI);
                }
            }

            GameLogger.LogInfo("[ActiveItemUI] 아이템 UI 초기화 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 버튼 이벤트를 설정합니다.
        /// </summary>
        private void SetupButtonEvent()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
                GameLogger.LogInfo("[ActiveItemUI] 버튼 이벤트 설정 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[ActiveItemUI] Button 컴포넌트를 찾을 수 없습니다!", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region 아이템 설정

        /// <summary>
        /// 슬롯 인덱스를 설정합니다.
        /// </summary>
        /// <param name="index">슬롯 인덱스</param>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

        /// <summary>
        /// 아이템을 설정합니다.
        /// </summary>
        /// <param name="item">설정할 아이템</param>
        public void SetItem(ActiveItemDefinition item)
        {
            currentItem = item;
            UpdateItemUI();
        }

        /// <summary>
        /// 슬롯을 빈 상태로 설정합니다.
        /// </summary>
        public void SetEmpty()
        {
            currentItem = null;
            UpdateItemUI();
        }

        /// <summary>
        /// 아이템 UI를 업데이트합니다.
        /// </summary>
        private void UpdateItemUI()
        {
            if (currentItem == null)
            {
                // 빈 슬롯 상태
                if (itemIcon != null)
                {
                    itemIcon.sprite = null;
                    itemIcon.color = Color.gray;
                }

                if (itemName != null)
                {
                    itemName.text = "";
                }

                if (itemDescription != null)
                {
                    itemDescription.text = "";
                }
                
                GameLogger.LogInfo($"[ActiveItemUI] 슬롯 {slotIndex} 빈 상태로 설정", GameLogger.LogCategory.UI);
                return;
            }

            // 아이템 아이콘 업데이트
            if (itemIcon != null)
            {
                itemIcon.sprite = currentItem.Icon;
                itemIcon.color = Color.white;
            }

            // 아이템 이름 업데이트
            if (itemName != null)
            {
                itemName.text = currentItem.DisplayName;
            }

            // 아이템 설명 업데이트
            if (itemDescription != null)
            {
                itemDescription.text = currentItem.Description;
            }
            
            GameLogger.LogInfo($"[ActiveItemUI] 아이템 UI 업데이트: {currentItem.DisplayName} @ 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 버튼이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void OnButtonClicked()
        {
            OnItemClicked?.Invoke(slotIndex);
            GameLogger.LogInfo($"[ActiveItemUI] 아이템 클릭: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 현재 아이템을 반환합니다.
        /// </summary>
        /// <returns>현재 아이템 또는 null</returns>
        public ActiveItemDefinition GetCurrentItem()
        {
            return currentItem;
        }

        /// <summary>
        /// 아이템이 설정되어 있는지 확인합니다.
        /// </summary>
        /// <returns>아이템이 있으면 true</returns>
        public bool HasItem()
        {
            return currentItem != null;
        }

        /// <summary>
        /// 슬롯 인덱스를 반환합니다.
        /// </summary>
        /// <returns>슬롯 인덱스</returns>
        public int GetSlotIndex()
        {
            return slotIndex;
        }

        /// <summary>
        /// 아이템 정보를 반환합니다.
        /// </summary>
        /// <returns>아이템 정보 문자열</returns>
        public string GetItemInfo()
        {
            if (currentItem == null)
                return "[빈 아이템]";

            return $"{currentItem.DisplayName}\n{currentItem.Description}";
        }

        #endregion
    }
}
