using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.ItemSystem.Data;
using Game.ItemSystem.Utility;
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

    #endregion

        #region 상태

        [SerializeField] private ActiveItemDefinition currentItem;
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
                    // Image 컴포넌트가 없으면 자동으로 추가
                    itemIcon = gameObject.AddComponent<Image>();
                    GameLogger.LogInfo("[ActiveItemUI] Image 컴포넌트를 자동으로 추가했습니다", GameLogger.LogCategory.UI);
                }
            }

            // Button의 Target Graphic을 Image로 설정
            var button = GetComponent<Button>();
            if (button != null && button.targetGraphic != itemIcon)
            {
                button.targetGraphic = itemIcon;
                GameLogger.LogInfo("[ActiveItemUI] Button의 Target Graphic을 Image로 설정했습니다", GameLogger.LogCategory.UI);
            }

            GameLogger.LogInfo("[ActiveItemUI] 아이템 UI 초기화 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 버튼 이벤트를 설정합니다.
        /// </summary>
        private void SetupButtonEvent()
        {
            var button = GetComponent<Button>();
            if (button == null)
            {
                // Button 컴포넌트가 없으면 자동으로 추가
                button = gameObject.AddComponent<Button>();
                GameLogger.LogInfo("[ActiveItemUI] Button 컴포넌트를 자동으로 추가했습니다", GameLogger.LogCategory.UI);
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
                GameLogger.LogInfo("[ActiveItemUI] 버튼 이벤트 설정 완료", GameLogger.LogCategory.UI);
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
                // UIUpdateHelper를 사용하여 빈 슬롯 설정
                UIUpdateHelper.SetEmptySlot(itemIcon, null, null);
                GameLogger.LogInfo($"[ActiveItemUI] 슬롯 {slotIndex} 빈 상태로 설정", GameLogger.LogCategory.UI);
                return;
            }

            // UIUpdateHelper를 사용하여 아이템 UI 업데이트
            UIUpdateHelper.UpdateItemSlotUI(itemIcon, null, null, currentItem);
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
        /// 아이템 정보를 반환합니다. (툴팁용)
        /// </summary>
        /// <returns>아이템 정보 문자열</returns>
        public string GetItemInfo()
        {
            if (currentItem == null)
                return "[빈 아이템]";

            return $"{currentItem.DisplayName}\n{currentItem.Description}";
        }

        /// <summary>
        /// 현재 아이템의 ActiveItemDefinition을 반환합니다.
        /// </summary>
        /// <returns>ActiveItemDefinition 또는 null</returns>
        public ActiveItemDefinition GetItemDefinition()
        {
            return currentItem;
        }

        /// <summary>
        /// 아이템의 효과 정보를 반환합니다.
        /// </summary>
        /// <returns>효과 정보 문자열</returns>
        public string GetEffectInfo()
        {
            if (currentItem == null)
                return "효과 없음";

            // UIUpdateHelper를 사용하여 효과 정보 생성
            return UIUpdateHelper.GenerateItemInfo(currentItem, true);
        }

        /// <summary>
        /// 아이템의 연출 정보를 반환합니다.
        /// </summary>
        /// <returns>연출 정보 문자열</returns>
        public string GetPresentationInfo()
        {
            if (currentItem == null)
                return "연출 없음";

            var presentationInfo = new System.Text.StringBuilder();
            presentationInfo.AppendLine("연출:");
            
            if (currentItem.Presentation.sfxClip != null)
            {
                presentationInfo.AppendLine($"- 사운드: {currentItem.Presentation.sfxClip.name}");
            }
            else
            {
                presentationInfo.AppendLine("- 사운드: 없음");
            }
            
            if (currentItem.Presentation.visualEffectPrefab != null)
            {
                presentationInfo.AppendLine($"- 이펙트: {currentItem.Presentation.visualEffectPrefab.name}");
            }
            else
            {
                presentationInfo.AppendLine("- 이펙트: 없음");
            }
            
            return presentationInfo.ToString();
        }

        /// <summary>
        /// 아이템의 모든 정보를 반환합니다. (디버그용)
        /// </summary>
        /// <returns>완전한 아이템 정보 문자열</returns>
        public string GetFullItemInfo()
        {
            if (currentItem == null)
                return "[빈 아이템]";

            var fullInfo = new System.Text.StringBuilder();
            fullInfo.AppendLine($"=== {currentItem.DisplayName} ===");
            fullInfo.AppendLine($"ID: {currentItem.ItemId}");
            fullInfo.AppendLine($"설명: {currentItem.Description}");
            fullInfo.AppendLine($"타입: {currentItem.Type}");
            fullInfo.AppendLine($"아이콘: {(currentItem.Icon != null ? currentItem.Icon.name : "없음")}");
            fullInfo.AppendLine();
            fullInfo.Append(GetEffectInfo());
            fullInfo.AppendLine();
            fullInfo.Append(GetPresentationInfo());
            
            return fullInfo.ToString();
        }

        #endregion
    }
}
