using Game.CoreSystem.Utility;
using Game.ItemSystem.Data;
using Game.ItemSystem.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 개별 액티브 아이템 UI를 관리하는 컴포넌트입니다.
    /// 아이템 아이콘, 이름, 설명을 표시하는 역할만 담당합니다.
    /// </summary>
    public class ActiveItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region UI 참조

        [Header("아이템 UI 구성 요소")]
        [SerializeField] private Image itemIcon;

        [Header("액션 팝업 프리팹")]
        [SerializeField] private GameObject actionPopupPrefab;

        #endregion

        #region 상태

        [SerializeField] private ActiveItemDefinition currentItem;
        private int slotIndex = -1;
        private ActionPopupUI currentPopup;

        // 의존성 주입
        [Inject(Optional = true)] private Game.CombatSystem.Manager.TurnManager turnManager;
        
        // 툴팁 매니저
        private Game.ItemSystem.Manager.ItemTooltipManager tooltipManager;
        private RectTransform rectTransform;

        #endregion

        #region 이벤트

        /// <summary>
        /// 아이템이 클릭되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<int> OnItemClicked;

        /// <summary>
        /// 사용 버튼이 클릭되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<int> OnUseButtonClicked;

        /// <summary>
        /// 버리기 버튼이 클릭되었을 때 발생하는 이벤트
        /// </summary>
        public event System.Action<int> OnDiscardButtonClicked;

        #endregion

        #region Unity 생명주기

        private void Start()
        {
            GameLogger.LogInfo($"[ActiveItemUI] Start() 호출됨 - GameObject: {gameObject.name}", GameLogger.LogCategory.UI);

            // RectTransform 캐시
            rectTransform = GetComponent<RectTransform>();

            // 툴팁 매니저 찾기
            FindTooltipManager();

            // 디버깅: 컴포넌트 상태 확인
            var image = GetComponent<Image>();
            var button = GetComponent<Button>();
            var canvasGroup = GetComponent<CanvasGroup>();

            GameLogger.LogInfo($"[ActiveItemUI] 컴포넌트 상태 - Image: {image != null}, Button: {button != null}, CanvasGroup: {canvasGroup != null}", GameLogger.LogCategory.UI);

            if (image != null)
            {
                GameLogger.LogInfo($"[ActiveItemUI] Image 상태 - RaycastTarget: {image.raycastTarget}, Enabled: {image.enabled}", GameLogger.LogCategory.UI);
            }

            if (canvasGroup != null)
            {
                GameLogger.LogInfo($"[ActiveItemUI] CanvasGroup 상태 - Interactable: {canvasGroup.interactable}, BlocksRaycasts: {canvasGroup.blocksRaycasts}", GameLogger.LogCategory.UI);
            }

            InitializeItemUI();
            SetupButtonEvent();
            RegisterToTooltipManager();
            GameLogger.LogInfo($"[ActiveItemUI] Start() 완료 - GameObject: {gameObject.name}", GameLogger.LogCategory.UI);
        }
        
        private void OnDestroy()
        {
            UnregisterFromTooltipManager();
        }
        
        /// <summary>
        /// 툴팁 매니저를 찾습니다.
        /// </summary>
        private void FindTooltipManager()
        {
            tooltipManager = FindFirstObjectByType<Game.ItemSystem.Manager.ItemTooltipManager>();
            if (tooltipManager != null)
            {
                GameLogger.LogInfo("[ActiveItemUI] ItemTooltipManager 찾기 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[ActiveItemUI] ItemTooltipManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 툴팁 매니저에 아이템을 등록합니다.
        /// </summary>
        private void RegisterToTooltipManager()
        {
            if (tooltipManager != null && currentItem != null && rectTransform != null)
            {
                tooltipManager.RegisterItemUI(currentItem, rectTransform);
                GameLogger.LogInfo($"[ActiveItemUI] 툴팁 매니저에 등록: {currentItem.DisplayName}", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 툴팁 매니저에서 아이템 등록을 해제합니다.
        /// </summary>
        private void UnregisterFromTooltipManager()
        {
            if (tooltipManager != null && currentItem != null)
            {
                tooltipManager.UnregisterItemUI(currentItem);
                GameLogger.LogInfo($"[ActiveItemUI] 툴팁 매니저에서 등록 해제: {currentItem.DisplayName}", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 아이템 UI를 초기화합니다.
        /// </summary>
        private void InitializeItemUI()
        {
            // 아이템 아이콘이 할당되지 않았으면 자식에서 찾기
            if (itemIcon == null)
            {
                // 먼저 자식 Button에서 Image 찾기
                var buttonChild = transform.Find("Button");
                if (buttonChild != null)
                {
                    itemIcon = buttonChild.GetComponent<Image>();
                    GameLogger.LogInfo("[ActiveItemUI] 자식 Button에서 Image 컴포넌트를 찾았습니다", GameLogger.LogCategory.UI);
                }

                // 자식에서 못 찾으면 자신에게서 찾기
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
        /// 클릭 이벤트를 설정합니다. (Image + IPointerClickHandler 사용)
        /// </summary>
        private void SetupButtonEvent()
        {
            // Image 컴포넌트가 Raycast Target을 활성화해야 클릭 감지 가능
            if (itemIcon != null)
            {
                itemIcon.raycastTarget = true;
                GameLogger.LogInfo("[ActiveItemUI] Image의 Raycast Target 활성화", GameLogger.LogCategory.UI);
            }

            // 자식 Button의 Button 컴포넌트 제거 (Image만 사용)
            var childButton = transform.Find("Button")?.GetComponent<Button>();
            if (childButton != null)
            {
                GameLogger.LogInfo("[ActiveItemUI] 자식 Button 컴포넌트 제거 - Image만 사용", GameLogger.LogCategory.UI);
                DestroyImmediate(childButton);
            }

            GameLogger.LogInfo("[ActiveItemUI] IPointerClickHandler 설정 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 액션 팝업을 생성하고 표시합니다.
        /// </summary>
        /// <param name="allowUse">사용 버튼 활성화 여부</param>
        private void ShowActionPopup(bool allowUse = true)
        {
            // 기존 팝업이 있으면 제거
            CloseActionPopup();

            if (actionPopupPrefab == null)
            {
                GameLogger.LogError("[ActiveItemUI] actionPopupPrefab이 설정되지 않았습니다!", GameLogger.LogCategory.UI);
                return;
            }

            if (currentItem == null)
            {
                GameLogger.LogWarning("[ActiveItemUI] 현재 아이템이 null입니다!", GameLogger.LogCategory.UI);
                return;
            }

            // 팝업 생성 (초기 부모는 슬롯이지만, 툴팁이 있으면 동일 캔버스로 이동)
            GameObject popupInstance = Instantiate(actionPopupPrefab, transform);
            currentPopup = popupInstance.GetComponent<ActionPopupUI>();

            if (currentPopup == null)
            {
                GameLogger.LogError("[ActiveItemUI] ActionPopupUI 컴포넌트를 찾을 수 없습니다!", GameLogger.LogCategory.UI);
                Destroy(popupInstance);
                return;
            }

            // 레이어 정렬: 슬롯/팝업을 맨 앞으로 올려 가림 방지
            transform.SetAsLastSibling();
            currentPopup.transform.SetAsLastSibling();

            // 팝업 전용 Canvas 설정(상단 렌더링 보장)
            var popupCanvas = currentPopup.GetComponent<Canvas>();
            if (popupCanvas == null)
            {
                popupCanvas = currentPopup.gameObject.AddComponent<Canvas>();
            }
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 1000; // 인벤토리 내 최상단 보장

            if (currentPopup.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                currentPopup.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // 먼저 툴팁을 현재 슬롯 기준으로 고정하고 즉시 표시 (아래 팝업 위치 계산에서 필요)
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (tooltipManager != null)
            {
                tooltipManager.PinTooltip(currentItem, rectTransform);
                tooltipManager.ShowTooltip();
            }

            // 팝업 위치 설정: 기본은 슬롯 위쪽, 가능하면 툴팁 오른편에 정렬(툴팁 활성 보장 후 계산)
            Vector2 popupPosition = rectTransform.anchoredPosition + Vector2.up * 60f; // 기본값

            var itemTooltip = tooltipManager != null ? tooltipManager.CurrentTooltip : null;
            if (itemTooltip != null && itemTooltip.gameObject.activeInHierarchy)
            {
                var tooltipRT = itemTooltip.GetComponent<RectTransform>();
                var tooltipParentRT = tooltipRT != null ? tooltipRT.parent as RectTransform : null;
                if (tooltipRT != null && tooltipParentRT != null)
                {
                    // 팝업 부모를 툴팁과 동일한 캔버스로 이동
                    currentPopup.transform.SetParent(tooltipParentRT, false);

                    // 툴팁 좌/우 하단(World) 코너를 동일 부모 로컬로 변환
                    Vector3[] corners = new Vector3[4];
                    tooltipRT.GetWorldCorners(corners); // 0:BL, 1:TL, 2:TR, 3:BR
                    Vector3 tooltipBLWorld = corners[0];
                    Vector3 tooltipBRWorld = corners[3];

                    var canvas = tooltipParentRT.GetComponentInParent<Canvas>();
                    Camera cam = null;
                    if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    {
                        cam = canvas.worldCamera;
                    }

                    Vector2 brScreen = RectTransformUtility.WorldToScreenPoint(cam, tooltipBRWorld);
                    Vector2 blScreen = RectTransformUtility.WorldToScreenPoint(cam, tooltipBLWorld);

                    Vector2 brLocal, blLocal;
                    bool gotBR = RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParentRT, brScreen, cam, out brLocal);
                    bool gotBL = RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParentRT, blScreen, cam, out blLocal);
                    if (gotBR && gotBL)
                    {
                        // 팝업 Rect 정보
                        var popupRT = currentPopup.GetComponent<RectTransform>();
                        if (popupRT != null)
                        {
                            // 좌하 피벗으로 정렬해 겹침 방지
                            popupRT.pivot = new Vector2(0f, 0f);

                            float tooltipRight = brLocal.x;
                            float tooltipLeft = blLocal.x;
                            float popupWidth = Mathf.Abs(popupRT.rect.width);
                            const float popupOffsetX = 12f;

                            var parentRect = tooltipParentRT.rect;
                            bool canShowRight = (parentRect.xMax - tooltipRight) >= (popupWidth + popupOffsetX);

                            float targetX = canShowRight
                                ? tooltipRight + popupOffsetX
                                : tooltipLeft - popupOffsetX - popupWidth;

                            // 하단 정렬(y는 툴팁 하단과 동일)
                            float targetY = brLocal.y;

                            // 좌우 경계 클램프
                            float minX = parentRect.xMin;
                            float maxX = parentRect.xMax - popupWidth;
                            if (targetX < minX) targetX = minX;
                            else if (targetX > maxX) targetX = maxX;

                            popupPosition = new Vector2(targetX, targetY);
                        }
                    }
                }
            }

            // 팝업 설정 (최종 위치 반영)
            currentPopup.SetupPopup(slotIndex, currentItem, popupPosition, allowUse);

            // 이벤트 연결
            currentPopup.OnUseButtonClicked += HandleUseButtonClicked;
            currentPopup.OnDiscardButtonClicked += HandleDiscardButtonClicked;
            currentPopup.OnPopupClosed += HandlePopupClosed;

            GameLogger.LogInfo($"[ActiveItemUI] 액션 팝업 표시: {currentItem.DisplayName} @ 슬롯 {slotIndex} (사용 허용: {allowUse})", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 액션 팝업을 닫습니다.
        /// </summary>
        private void CloseActionPopup()
        {
            if (currentPopup != null)
            {
                // 이벤트 해제
                currentPopup.OnUseButtonClicked -= HandleUseButtonClicked;
                currentPopup.OnDiscardButtonClicked -= HandleDiscardButtonClicked;
                currentPopup.OnPopupClosed -= HandlePopupClosed;

                // 팝업 제거
                Destroy(currentPopup.gameObject);
                currentPopup = null;

                // 외부/내부 어떤 경로로든 팝업을 닫을 때 툴팁도 함께 종료하되,
                // 현재 팝업의 아이템에 고정된 경우에만 닫도록 조건부 처리
                if (tooltipManager != null)
                {
                    tooltipManager.ForceHideIfPinnedTo(currentItem);
                    tooltipManager.UnpinTooltip();
                }

                GameLogger.LogInfo($"[ActiveItemUI] 액션 팝업 닫힘: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 사용 버튼이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void HandleUseButtonClicked(int slotIndex)
        {
            // 아이템 툴팁 강제 숨김 (현재 아이템에 고정된 경우에만)
            if (tooltipManager != null)
            {
                tooltipManager.ForceHideIfPinnedTo(currentItem);
                GameLogger.LogInfo("[ActiveItemUI] 사용 버튼 클릭 - 아이템 툴팁 숨김", GameLogger.LogCategory.UI);
            }

            OnUseButtonClicked?.Invoke(slotIndex);
            GameLogger.LogInfo($"[ActiveItemUI] 사용 버튼 클릭: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 버리기 버튼이 클릭되었을 때 호출됩니다.
        /// </summary>
        private void HandleDiscardButtonClicked(int slotIndex)
        {
            // 아이템 툴팁 강제 숨김 (현재 아이템에 고정된 경우에만)
            if (tooltipManager != null)
            {
                tooltipManager.ForceHideIfPinnedTo(currentItem);
                GameLogger.LogInfo("[ActiveItemUI] 버리기 버튼 클릭 - 아이템 툴팁 숨김", GameLogger.LogCategory.UI);
            }

            OnDiscardButtonClicked?.Invoke(slotIndex);
            GameLogger.LogInfo($"[ActiveItemUI] 버리기 버튼 클릭: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 팝업이 닫혔을 때 호출됩니다.
        /// </summary>
        private void HandlePopupClosed()
        {
            currentPopup = null;
            // 팝업이 닫히면 툴팁 고정 해제
            if (tooltipManager != null)
            {
                tooltipManager.UnpinTooltip();
                // 클릭으로 고정된 툴팁도 함께 닫기
                tooltipManager.ForceHideTooltip();
            }
            GameLogger.LogInfo($"[ActiveItemUI] 팝업 닫힘 이벤트 처리: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
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
            // 기존 아이템 등록 해제
            if (currentItem != null)
            {
                UnregisterFromTooltipManager();
            }
            
            currentItem = item;
            UpdateItemUI();
            
            // 새 아이템 등록
            RegisterToTooltipManager();
        }

        /// <summary>
        /// 슬롯을 빈 상태로 설정합니다.
        /// </summary>
        public void SetEmpty()
        {
            UnregisterFromTooltipManager();
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
        /// 포인터 클릭 이벤트를 처리합니다. (IPointerClickHandler 구현)
        /// 플레이어 턴에만 팝업을 표시합니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 좌클릭만 처리
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 아이템이 없으면 무시
                if (currentItem == null)
                {
                    GameLogger.LogInfo($"[ActiveItemUI] 슬롯 {slotIndex}에 아이템이 없습니다", GameLogger.LogCategory.UI);
                    return;
                }

                // 다른 슬롯의 팝업이 열려 있으면: 먼저 모두 닫고 다음 프레임에 현재 아이템을 정확히 열기
                var openPopups = FindObjectsByType<ActionPopupUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                bool hasOtherOpen = false;
                for (int i = 0; i < openPopups.Length; i++)
                {
                    if (openPopups[i] != null && openPopups[i].GetSlotIndex() != slotIndex)
                    {
                        hasOtherOpen = true;
                        break;
                    }
                }
                if (hasOtherOpen)
                {
                    var panel = FindFirstObjectByType<InventoryPanelController>();
                    if (panel != null)
                    {
                        // 다른 슬롯 전환 시: 먼저 툴팁을 현재 슬롯으로 재고정하여
                        // 이전 슬롯 팝업 정리 과정에서 새 툴팁이 닫히지 않게 보호
                        if (tooltipManager != null)
                        {
                            tooltipManager.PinTooltip(currentItem, GetComponent<RectTransform>());
                            tooltipManager.ShowTooltip();
                        }

                        // 그런 다음 다른 슬롯의 팝업만 닫기
                        // (툴팁은 유지, 다음 프레임에 팝업을 정상적으로 연다)
                        panel.CloseAllPopupsOnly();
                    }
                    StartCoroutine(OpenAfterFrame());
                    return;
                }

                // 동일 아이템(동일 슬롯) 토글: 팝업이 열려 있으면 빈공간 클릭처럼 팝업/툴팁 모두 닫고 종료
                if (currentPopup != null && currentPopup.gameObject != null && currentPopup.gameObject.activeInHierarchy)
                {
                    if (currentPopup.GetSlotIndex() == slotIndex)
                    {
                        CloseActionPopup();
                        return;
                    }
                }

                // 이벤트 발송
                if (OnItemClicked != null)
                {
                    OnItemClicked.Invoke(slotIndex);
                }

                // 액션 팝업 표시 (적턴에도 팝업은 열림, 버튼 활성화는 팝업 내부에서 처리)
                bool isPlayerTurn = IsPlayerTurn();
                if (isPlayerTurn)
                {
                    GameLogger.LogInfo($"[ActiveItemUI] ✅ 플레이어 턴 - 팝업 표시: {currentItem.DisplayName}", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogInfo($"[ActiveItemUI] ⚠️ 적 턴 - 팝업 표시 (사용 불가, 버리기만 가능): {currentItem.DisplayName}", GameLogger.LogCategory.UI);
                }
                ShowActionPopup(isPlayerTurn);
            }
        }

        private System.Collections.IEnumerator OpenAfterFrame()
        {
            // 다음 프레임에서 글로벌 닫기 억제 후 정상 오픈
            yield return null;
            var panel = FindFirstObjectByType<InventoryPanelController>();
            if (panel != null)
            {
                panel.SuppressGlobalCloseOneFrame();
            }
            OnItemClicked?.Invoke(slotIndex);
            ShowActionPopup(IsPlayerTurn());
        }

        /// <summary>
        /// 현재 플레이어 턴인지 확인합니다.
        /// 턴 상태와 전투 상태를 모두 확인하여 완전한 플레이어 턴에서만 사용 가능하도록 합니다.
        /// </summary>
        /// <returns>완전한 플레이어 턴이면 true, 아니면 false</returns>
        private bool IsPlayerTurn()
        {
            // 1단계: TurnManager 턴 상태 확인
            bool isTurnPlayerTurn = false;
            if (turnManager != null)
            {
                isTurnPlayerTurn = turnManager.IsPlayerTurn();
            }
            else
            {
                // TurnManager가 없으면 씬에서 직접 찾기
                var foundTurnManager = FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (foundTurnManager != null)
                {
                    isTurnPlayerTurn = foundTurnManager.IsPlayerTurn();
                }
                else
                {
                    // TurnManager를 찾을 수 없으면 안전하게 false 반환 (아이템 사용 차단)
                    GameLogger.LogWarning("[ActiveItemUI] TurnManager를 찾을 수 없습니다. 아이템 사용을 차단합니다.", GameLogger.LogCategory.UI);
                    return false;
                }
            }

            // 2단계: CombatStateMachine 전투 상태 확인
            var combatStateMachine = FindFirstObjectByType<Game.CombatSystem.State.CombatStateMachine>();
            if (combatStateMachine == null)
            {
                GameLogger.LogWarning("[ActiveItemUI] CombatStateMachine을 찾을 수 없습니다. 아이템 사용을 차단합니다.", GameLogger.LogCategory.UI);
                return false;
            }

            var currentState = combatStateMachine.GetCurrentState();
            if (currentState == null)
            {
                GameLogger.LogWarning("[ActiveItemUI] 현재 전투 상태가 없습니다. 아이템 사용을 차단합니다.", GameLogger.LogCategory.UI);
                return false;
            }

            // 3단계: 완전한 플레이어 턴 상태인지 확인
            bool isCompletePlayerTurn = isTurnPlayerTurn && 
                                       currentState is Game.CombatSystem.State.PlayerTurnState &&
                                       currentState.AllowPlayerCardDrag;

            if (!isCompletePlayerTurn)
            {
                GameLogger.LogInfo($"[ActiveItemUI] 아이템 사용 불가 - 턴상태: {isTurnPlayerTurn}, 전투상태: {currentState.StateName}, 드래그허용: {currentState.AllowPlayerCardDrag}", GameLogger.LogCategory.UI);
            }

            return isCompletePlayerTurn;
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
        /// 액션 팝업을 닫습니다. (외부에서 호출 가능)
        /// </summary>
        public void CloseActionPopupExternal()
        {
            CloseActionPopup();
        }

        /// <summary>
        /// 현재 팝업이 열려있는지 확인합니다.
        /// </summary>
        /// <returns>팝업이 열려있으면 true, 아니면 false</returns>
        public bool HasOpenPopup()
        {
            return currentPopup != null && currentPopup.gameObject.activeInHierarchy;
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

        #region 툴팁 호버 이벤트

        /// <summary>
        /// 포인터가 오브젝트에 진입했을 때 호출됩니다.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem == null || tooltipManager == null)
                return;

            // 인벤토리 슬롯에서도 자신의 RectTransform을 명시 전달
            tooltipManager.OnItemHoverEnter(currentItem, rectTransform);
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
    }
}
