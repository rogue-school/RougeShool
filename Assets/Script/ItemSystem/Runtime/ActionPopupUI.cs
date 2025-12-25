using Game.CoreSystem.Utility;
using Game.ItemSystem.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.Manager.TurnManager turnManager;

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
            EnsureTurnManagerInjected();
            SubscribeToTurnChanges();
        }

        /// <summary>
        /// TurnManager가 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsureTurnManagerInjected()
        {
            if (turnManager != null) return;

            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    projectContext.Container.Inject(this);
                    if (turnManager != null)
                    {
                        GameLogger.LogInfo("[ActionPopupUI] TurnManager 주입 완료 (ProjectContext)", GameLogger.LogCategory.UI);
                        return;
                    }
                }

                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    turnManager = foundManager;
                    GameLogger.LogInfo("[ActionPopupUI] TurnManager 직접 찾기 완료", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[ActionPopupUI] TurnManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.UI);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromTurnChanges();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 팝업을 설정합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <param name="item">아이템 정보</param>
        /// <param name="targetPosition">팝업 표시 위치</param>
        /// <param name="allowUse">사용 버튼 활성화 여부</param>
        public void SetupPopup(int slotIndex, ActiveItemDefinition item, Vector2 targetPosition, bool allowUse = true)
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

            // 사용 버튼 활성화/비활성화
            if (useButton != null)
            {
                useButton.interactable = allowUse;
                GameLogger.LogInfo($"[ActionPopupUI] 사용 버튼 {(allowUse ? "활성화" : "비활성화")}", GameLogger.LogCategory.UI);
            }

            // 버리기 버튼은 항상 활성화
            if (discardButton != null)
            {
                discardButton.interactable = true;
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

        }

        #endregion

        #region 이벤트 처리

        /// <summary>
        /// 사용 버튼이 클릭되었을 때 호출됩니다.
        /// ItemService에서 턴 체크를 하므로 여기서는 생략합니다.
        /// </summary>
        private void HandleUseButtonClicked()
        {
            OnUseButtonClicked?.Invoke(slotIndex);
            ClosePopup();
        }

        /// <summary>
        /// 버리기 버튼이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void HandleDiscardButtonClicked()
        {
            OnDiscardButtonClicked?.Invoke(slotIndex);
            ClosePopup();
        }

        #endregion

        #region 턴 변경 감지

        /// <summary>
        /// 턴 변경 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeToTurnChanges()
        {
            // turnManager는 DI로 주입받음
            if (turnManager != null)
            {
                turnManager.OnTurnChanged += HandleTurnChanged;
                GameLogger.LogInfo("[ActionPopupUI] 턴 변경 이벤트 구독 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[ActionPopupUI] TurnManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 턴 변경 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromTurnChanges()
        {
            if (turnManager != null)
            {
                turnManager.OnTurnChanged -= HandleTurnChanged;
                GameLogger.LogInfo("[ActionPopupUI] 턴 변경 이벤트 구독 해제", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 턴이 변경되면 팝업을 자동으로 닫습니다.
        /// </summary>
        private void HandleTurnChanged(Game.CombatSystem.Manager.TurnManager.TurnType newTurn)
        {
            GameLogger.LogWarning($"[ActionPopupUI] ⚠️ 턴 변경 감지 ({newTurn}) - 팝업 강제 닫기", GameLogger.LogCategory.UI);
            ClosePopup();
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
