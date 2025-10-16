using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Data;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 아이템 사용/버리기 팝업 UI를 관리하는 컴포넌트입니다.
    /// 동적으로 생성되어 아이템 위에 표시됩니다.
    /// </summary>
    public class ActionPopupUI : MonoBehaviour
    {
        #region UI 참조

        [Header("팝업 구성 요소")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private TextMeshProUGUI useButtonText;
        [SerializeField] private TextMeshProUGUI discardButtonText;

        #endregion

        #region 상태

        private int slotIndex = -1;
        private ActiveItemDefinition currentItem;

        #endregion

        #region 이벤트

        /// <summary>
        /// 사용 버튼이 클릭되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<int> OnUseButtonClicked;

        /// <summary>
        /// 버리기 버튼이 클릭되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<int> OnDiscardButtonClicked;

        /// <summary>
        /// 팝업이 닫혔을 때 발생하는 이벤트
        /// </summary>
        public event System.Action OnPopupClosed;

        #endregion

        #region Unity 생명주기

        private void Start()
        {
            SetupButtons();
            GameLogger.LogInfo($"[ActionPopupUI] 팝업 초기화 완료 - 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 팝업을 설정합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <param name="item">아이템 정보</param>
        /// <param name="targetPosition">팝업 표시 위치</param>
        public void SetupPopup(int slotIndex, ActiveItemDefinition item, Vector2 targetPosition)
        {
            this.slotIndex = slotIndex;
            this.currentItem = item;

            // 버튼 텍스트 설정
            if (useButtonText != null)
            {
                useButtonText.text = "사용";
            }

            if (discardButtonText != null)
            {
                discardButtonText.text = "버리기";
            }

            // 팝업 위치 설정 (RectTransform 사용)
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = targetPosition;
            }
            else
            {
                transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            }

            // 팝업 활성화
            gameObject.SetActive(true);

            GameLogger.LogInfo($"[ActionPopupUI] 팝업 설정 완료: {item?.DisplayName} @ 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 버튼 이벤트를 설정합니다.
        /// </summary>
        private void SetupButtons()
        {
            if (useButton != null)
            {
                useButton.onClick.RemoveAllListeners();
                useButton.onClick.AddListener(HandleUseButtonClicked);
            }

            if (discardButton != null)
            {
                discardButton.onClick.RemoveAllListeners();
                discardButton.onClick.AddListener(HandleDiscardButtonClicked);
            }

            GameLogger.LogInfo("[ActionPopupUI] 버튼 이벤트 설정 완료", GameLogger.LogCategory.UI);
        }

        #endregion

        #region 이벤트 처리

        /// <summary>
        /// 사용 버튼이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void HandleUseButtonClicked()
        {
            OnUseButtonClicked?.Invoke(slotIndex);
            ClosePopup();
            GameLogger.LogInfo($"[ActionPopupUI] 사용 버튼 클릭: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 버리기 버튼이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void HandleDiscardButtonClicked()
        {
            OnDiscardButtonClicked?.Invoke(slotIndex);
            ClosePopup();
            GameLogger.LogInfo($"[ActionPopupUI] 버리기 버튼 클릭: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 팝업을 닫습니다.
        /// </summary>
        public void ClosePopup()
        {
            OnPopupClosed?.Invoke();
            Destroy(gameObject);
            GameLogger.LogInfo($"[ActionPopupUI] 팝업 닫힘: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 팝업 위치를 업데이트합니다.
        /// </summary>
        /// <param name="newPosition">새로운 위치</param>
        public void UpdatePosition(Vector2 newPosition)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = newPosition;
            }
            else
            {
                transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            }
        }

        /// <summary>
        /// 현재 슬롯 인덱스를 반환합니다.
        /// </summary>
        /// <returns>슬롯 인덱스</returns>
        public int GetSlotIndex()
        {
            return slotIndex;
        }

        #endregion
    }
}
