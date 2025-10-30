using UnityEngine;
using UnityEngine.EventSystems;
using Game.ItemSystem.Data;
using Game.ItemSystem.UI;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Game.SkillCardSystem.Manager;
using Game.CharacterSystem.Manager;

namespace Game.ItemSystem.Manager
{
    /// <summary>
    /// 아이템 툴팁을 관리하는 전역 매니저입니다.
    /// CoreSystem에 등록되어 모든 씬에서 사용 가능합니다.
    /// </summary>
    public class ItemTooltipManager : MonoBehaviour, ICoreSystemInitializable
    {
        #region Serialized Fields

        [Header("툴팁 설정")]
        [Tooltip("툴팁 프리팹 (ItemTooltip 컴포넌트 포함)")]
        [SerializeField] private GameObject tooltipPrefab;
        
        [Tooltip("툴팁 표시 지연 시간 (초)")]
        [SerializeField] private float showDelay = 0.1f;
        
        [Tooltip("툴팁 숨김 지연 시간 (초)")]
        [SerializeField] private float hideDelay = 0.1f;

        [Header("정렬 간격 설정")]
        [Tooltip("아이템과 툴팁 사이의 가로 간격(px)")]
        [SerializeField] private float alignPaddingX = 12f;
        [Tooltip("아이템 하단과 툴팁 하단의 세로 오프셋(px). 0이면 하단 정렬")]
        [SerializeField] private float alignPaddingY = 0f;

        #endregion

        #region Private Fields

        private ItemTooltip currentTooltip;
        private RectTransform tooltipLayer;
        private ActiveItemDefinition hoveredItem;
        private RectTransform currentTargetRect;
        private bool pendingShow;
        private ActiveItemDefinition pendingItem;

        private float showTimer;
        private float hideTimer;
        private bool isShowingTooltip;
        private bool isHidingTooltip;
        private bool isPinned; // 팝업 등으로 고정된 동안 숨기지 않음
        private ActiveItemDefinition pinnedItem;
        private RectTransform pinnedRect;

        private EventSystem eventSystem;
        private System.Collections.Generic.Dictionary<ActiveItemDefinition, RectTransform> itemUICache = new();

        #endregion

        #region Public Properties

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// 현재 활성화된 툴팁 인스턴스
        /// </summary>
        public ItemTooltip CurrentTooltip => currentTooltip;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            UpdateTooltipTimers();
        }

        private void OnDestroy()
        {
            if (currentTooltip != null && currentTooltip.gameObject != null)
            {
                Destroy(currentTooltip.gameObject);
            }
        }

        #endregion

        #region ICoreSystemInitializable

        /// <summary>
        /// 시스템을 초기화합니다.
        /// </summary>
        public IEnumerator Initialize()
        {
            yield return InitializeTooltipSystem();
            IsInitialized = true;
            GameLogger.LogInfo("[ItemTooltipManager] 초기화 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 초기화 실패 시 호출됩니다.
        /// </summary>
        public void OnInitializationFailed()
        {
            GameLogger.LogError($"{GetType().Name} 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                GameLogger.LogError("[ItemTooltipManager] EventSystem을 찾을 수 없습니다", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 툴팁 시스템을 초기화합니다.
        /// </summary>
        private IEnumerator InitializeTooltipSystem()
        {
            GameLogger.LogInfo("[ItemTooltipManager] 툴팁 시스템 초기화 시작", GameLogger.LogCategory.UI);
            
            if (tooltipPrefab == null)
            {
                GameLogger.LogWarning("[ItemTooltipManager] 툴팁 프리팹이 설정되지 않았습니다. Inspector에서 설정해주세요.", GameLogger.LogCategory.UI);
            }

            GameLogger.LogInfo("[ItemTooltipManager] 툴팁 시스템 초기화 완료", GameLogger.LogCategory.UI);
            yield return null;
        }

        /// <summary>
        /// 툴팁 인스턴스를 생성합니다.
        /// </summary>
        private void CreateTooltipInstance()
        {
            if (currentTooltip != null)
            {
                Destroy(currentTooltip);
                currentTooltip = null;
            }
            
            if (tooltipPrefab == null)
            {
                GameLogger.LogError("[ItemTooltipManager] tooltipPrefab이 null입니다", GameLogger.LogCategory.Error);
                return;
            }
            
            try
            {
                Transform parentForTooltip = GetCanvasOfCurrentTarget()?.transform;
                
                if (parentForTooltip == null)
                {
                    GameLogger.LogWarning("[ItemTooltipManager] 툴팁 부모를 찾지 못했습니다 (대상 캔버스 없음) – 표시를 건너뜁니다", GameLogger.LogCategory.UI);
                    return;
                }

                var tooltipInstance = Instantiate(tooltipPrefab, parentForTooltip);
                if (tooltipInstance == null)
                {
                    GameLogger.LogError("[ItemTooltipManager] 툴팁 인스턴스 생성 실패", GameLogger.LogCategory.Error);
                    return;
                }
                
                // 초기에는 비활성화하고 캔버스 최상단으로 정렬
                tooltipInstance.gameObject.SetActive(false);
                tooltipInstance.transform.SetAsLastSibling();

                currentTooltip = tooltipInstance.GetComponent<ItemTooltip>();
                if (currentTooltip == null)
                {
                    GameLogger.LogError("[ItemTooltipManager] ItemTooltip 컴포넌트를 찾을 수 없습니다", GameLogger.LogCategory.Error);
                    Destroy(tooltipInstance);
                    return;
                }

                tooltipInstance.SetActive(false);
                tooltipInstance.transform.SetAsLastSibling();

                GameLogger.LogInfo("[ItemTooltipManager] 툴팁 인스턴스 생성 완료", GameLogger.LogCategory.UI);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[ItemTooltipManager] 툴팁 인스턴스 생성 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 아이템 UI를 등록합니다.
        /// </summary>
        /// <param name="item">아이템 데이터</param>
        /// <param name="itemTransform">아이템 UI의 RectTransform</param>
        public void RegisterItemUI(ActiveItemDefinition item, RectTransform itemTransform)
        {
            if (item == null || itemTransform == null) return;
            itemUICache[item] = itemTransform;
        }

        /// <summary>
        /// 아이템 UI 등록을 해제합니다.
        /// </summary>
        /// <param name="item">아이템 데이터</param>
        public void UnregisterItemUI(ActiveItemDefinition item)
        {
            if (item == null) return;
            itemUICache.Remove(item);
        }

        /// <summary>
        /// 아이템에 마우스가 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="item">호버된 아이템</param>
        public void OnItemHoverEnter(ActiveItemDefinition item)
        {
            OnItemHoverEnter(item, null);
        }

        /// <summary>
        /// 아이템에 마우스가 진입했을 때 호출됩니다. (호버 소스 Rect 우선)
        /// </summary>
        /// <param name="item">호버된 아이템</param>
        /// <param name="sourceRect">호버를 발생시킨 UI의 RectTransform</param>
        public void OnItemHoverEnter(ActiveItemDefinition item, RectTransform sourceRect)
        {
            if (item == null) return;

            // 팝업으로 고정된 동안에는 다른 아이템으로 전환하지 않음
            if (isPinned)
            {
                return;
            }

            if (!IsInitialized)
            {
                GameLogger.LogInfo("[ItemTooltipManager] 초기화 안됨 - 즉시 초기화 시도", GameLogger.LogCategory.UI);
                pendingItem = item;
                pendingShow = true;
                StartCoroutine(ForceInitialize());
                return;
            }

            hoveredItem = item;
            // 호버를 발생시킨 소스 Rect를 우선 사용
            currentTargetRect = sourceRect;
            if (currentTargetRect == null)
            {
                // 폴백: 캐시된 Rect 사용
                if (itemUICache.TryGetValue(item, out var rt) && rt != null)
                {
                    currentTargetRect = rt;
                }
            }

            isHidingTooltip = false;
            hideTimer = 0f;

            if (!isShowingTooltip)
            {
                isShowingTooltip = true;
                showTimer = 0f;
            }
        }

        /// <summary>
        /// 아이템에서 마우스가 이탈했을 때 호출됩니다.
        /// </summary>
        public void OnItemHoverExit()
        {
            if (hoveredItem == null) return;

            if (isPinned)
            {
                // 고정 상태에서는 숨김 타이머를 시작하지 않고 상태만 초기화
                hoveredItem = null;
                isShowingTooltip = false;
                showTimer = 0f;
                return;
            }

            hoveredItem = null;
            isShowingTooltip = false;
            showTimer = 0f;

            pendingItem = null;
            pendingShow = false;

            if (!isHidingTooltip)
            {
                isHidingTooltip = true;
                hideTimer = 0f;
            }
        }

        /// <summary>
        /// 툴팁을 강제로 숨깁니다.
        /// </summary>
        public void ForceHideTooltip()
        {
            if (currentTooltip != null && currentTooltip.gameObject != null)
            {
                currentTooltip.Hide();
            }

            hoveredItem = null;
            isShowingTooltip = false;
            isHidingTooltip = false;
            showTimer = 0f;
            hideTimer = 0f;
            isPinned = false;
            pinnedItem = null;
            pinnedRect = null;
        }

        /// <summary>
        /// 다른 툴팁 매니저의 툴팁을 숨깁니다.
        /// </summary>
        private void HideOtherTooltips()
        {
            // 스킬카드 툴팁 숨김
            var skillCardTooltipManager = Object.FindFirstObjectByType<SkillCardTooltipManager>();
            if (skillCardTooltipManager != null)
            {
                skillCardTooltipManager.ForceHideTooltip();
                GameLogger.LogInfo("[ItemTooltipManager] 다른 툴팁 숨김 (SkillCardTooltipManager)", GameLogger.LogCategory.UI);
            }

            // 버프/디버프 툴팁 숨김
            var buffDebuffTooltipManager = Object.FindFirstObjectByType<BuffDebuffTooltipManager>();
            if (buffDebuffTooltipManager != null)
            {
                buffDebuffTooltipManager.ForceHideTooltip();
                GameLogger.LogInfo("[ItemTooltipManager] 다른 툴팁 숨김 (BuffDebuffTooltipManager)", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 강제로 초기화를 수행합니다.
        /// </summary>
        private IEnumerator ForceInitialize()
        {
            GameLogger.LogInfo("[ItemTooltipManager] 강제 초기화 시작", GameLogger.LogCategory.UI);
            
            yield return InitializeTooltipSystem();
            
            if (tooltipPrefab != null)
            {
                IsInitialized = true;
                GameLogger.LogInfo($"[ItemTooltipManager] 강제 초기화 완료", GameLogger.LogCategory.UI);

                if (pendingShow && pendingItem != null)
                {
                    try
                    {
                        hoveredItem = pendingItem;
                        pendingItem = null;
                        pendingShow = false;

                        isHidingTooltip = false;
                        hideTimer = 0f;
                        isShowingTooltip = false;
                        showTimer = 0f;

                        currentTargetRect = null;
                        if (hoveredItem != null && itemUICache.TryGetValue(hoveredItem, out var cachedRt) && cachedRt != null)
                        {
                            currentTargetRect = cachedRt;
                        }

                        ShowTooltip();
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogError($"[ItemTooltipManager] 초기화 직후 표시 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
                    }
                }
            }
            else
            {
                GameLogger.LogError("[ItemTooltipManager] 강제 초기화 실패 - 필수 컴포넌트 누락", GameLogger.LogCategory.Error);
                IsInitialized = false;
            }
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// 툴팁 타이머들을 업데이트합니다.
        /// </summary>
        private void UpdateTooltipTimers()
        {
            if (isShowingTooltip && hoveredItem != null)
            {
                showTimer += Time.deltaTime;
                if (showTimer >= showDelay)
                {
                    ShowTooltip();
                    isShowingTooltip = false;
                }
            }

            if (isHidingTooltip)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideDelay)
                {
                    HideTooltip();
                    isHidingTooltip = false;
                }
            }

            if (currentTooltip != null && currentTooltip.gameObject != null && currentTooltip.gameObject.activeInHierarchy)
            {
                Vector2 itemPosition = GetCurrentItemPosition();
                if (itemPosition != Vector2.zero)
                {
                    UpdateTooltipPosition(itemPosition);
                }
            }
        }

        /// <summary>
        /// 현재 호버된 아이템의 위치를 가져옵니다.
        /// </summary>
        private Vector2 GetCurrentItemPosition()
        {
            if (isPinned && pinnedRect != null)
            {
                // 고정된 Rect 기준으로 위치 계산
                var sourceCanvas = pinnedRect.GetComponentInParent<Canvas>();
                Camera cam = null;
                if (sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    cam = sourceCanvas.worldCamera;
                }

                Vector3[] cornersPinned = new Vector3[4];
                pinnedRect.GetWorldCorners(cornersPinned);
                Vector3 bottomLeftPinned = cornersPinned[0];
                return RectTransformUtility.WorldToScreenPoint(cam, bottomLeftPinned);
            }

            if (hoveredItem == null) return Vector2.zero;

            RectTransform itemRect = currentTargetRect;
            if (itemRect == null)
            {
                itemUICache.TryGetValue(hoveredItem, out itemRect);
            }
            
            if (itemRect != null)
            {
                var sourceCanvas = itemRect.GetComponentInParent<Canvas>();
                Camera cam = null;
                if (sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    cam = sourceCanvas.worldCamera;
                }

                Vector3[] corners = new Vector3[4];
                itemRect.GetWorldCorners(corners);
                Vector3 bottomLeftWorld = corners[0];
                Vector2 screenBL = RectTransformUtility.WorldToScreenPoint(cam, bottomLeftWorld);
                return screenBL;
            }

            return Vector2.zero;
        }

        private Canvas GetCanvasOfCurrentTarget()
        {
            if (currentTargetRect == null) return null;
            return currentTargetRect.GetComponentInParent<Canvas>();
        }

        #endregion

        #region Tooltip Control

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        public void ShowTooltip()
        {
            // 다른 툴팁 매니저의 툴팁 숨김 (중복 방지)
            HideOtherTooltips();

            if (currentTargetRect == null && hoveredItem != null)
            {
                if (itemUICache.TryGetValue(hoveredItem, out var cachedRt) && cachedRt != null)
                {
                    currentTargetRect = cachedRt;
                }
            }

            // 대상 캔버스를 찾지 못하면 표시를 건너뜁니다 (보상 패널 등 전환 시 안전 가드)
            var targetCanvas = GetCanvasOfCurrentTarget();
            if (targetCanvas == null)
            {
                GameLogger.LogWarning("[ItemTooltipManager] 대상 캔버스를 찾을 수 없습니다. 툴팁 표시를 건너뜁니다", GameLogger.LogCategory.UI);
                return;
            }

            if (currentTooltip == null)
            {
                CreateTooltipInstance();
                if (currentTooltip == null) return;
            }
            else
            {
                var canvas = targetCanvas;
                if (canvas != null && currentTooltip.transform.parent != canvas.transform)
                {
                    currentTooltip.transform.SetParent(canvas.transform, false);
                }
                // 항상 최상단으로 이동 (RewardPanel 같은 다른 UI 뒤에 숨지 않도록)
                currentTooltip.transform.SetAsLastSibling();
            }
            
            // 툴팁이 항상 최상단에 렌더링되도록 보장
            if (currentTooltip != null && currentTooltip.transform.parent != null)
            {
                currentTooltip.transform.SetAsLastSibling();
            }
            
            if (hoveredItem == null)
            {
                GameLogger.LogWarning("[ItemTooltipManager] hoveredItem이 null입니다.", GameLogger.LogCategory.UI);
                return;
            }

            try
            {
                Vector2 itemPosition = GetCurrentItemPosition();
                if (itemPosition != Vector2.zero)
                {
                    // 툴팁 표시
                    currentTooltip.Show(hoveredItem);
                    
                    // 위치 업데이트 (Show 이후에 호출하여 오브젝트가 활성화된 후 위치 설정)
                    UpdateTooltipPosition(itemPosition);
                    
                    GameLogger.LogInfo($"툴팁 표시: {hoveredItem.DisplayName}", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[ItemTooltipManager] 아이템 위치를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[ItemTooltipManager] 툴팁 표시 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        public void HideTooltip()
        {
            if (currentTooltip == null) return;

            try
            {
                if (isPinned)
                {
                    GameLogger.LogInfo("[ItemTooltipManager] 툴팁 고정 상태 - 숨기지 않음", GameLogger.LogCategory.UI);
                    return;
                }
                currentTooltip.Hide();
                GameLogger.LogInfo("툴팁 숨김 완료", GameLogger.LogCategory.UI);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[ItemTooltipManager] 툴팁 숨김 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 툴팁을 고정합니다. (팝업이 열리는 동안 유지)
        /// </summary>
        public void PinTooltip()
        {
            isPinned = true;
            pinnedItem = hoveredItem;
            pinnedRect = currentTargetRect;
        }

        /// <summary>
        /// 툴팁 고정을 해제합니다.
        /// </summary>
        public void UnpinTooltip()
        {
            isPinned = false;
            pinnedItem = null;
            pinnedRect = null;
        }

        /// <summary>
        /// 특정 아이템/Rect 기준으로 툴팁을 고정합니다.
        /// </summary>
        public void PinTooltip(ActiveItemDefinition item, RectTransform rect)
        {
            isPinned = true;
            pinnedItem = item;
            pinnedRect = rect;
            hoveredItem = item;
            currentTargetRect = rect;
        }

        /// <summary>
        /// 툴팁 위치를 업데이트합니다.
        /// </summary>
        private void UpdateTooltipPosition(Vector2 itemPosition)
        {
            if (currentTooltip == null) return;

            var rectTransform = currentTooltip.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            // 스크린 좌표를 캔버스 로컬 좌표로 변환
            var targetParent = rectTransform.parent as RectTransform;
            if (targetParent == null) return;

            // 툴팁이 속한 캔버스의 카메라 확인
            Camera cameraToUse = null;
            var parentCanvas = rectTransform.GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cameraToUse = parentCanvas.worldCamera;
            }

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetParent,
                itemPosition,
                cameraToUse,
                out localPoint))
            {
                // SkillCard과 동일한 정책: 우측 우선, 좌우 폴백, 하단 정렬
                rectTransform.pivot = new Vector2(0f, 0f);

                var canvasRect = targetParent.rect;
                var tooltipRect = rectTransform.rect;

                float tooltipWidth = Mathf.Abs(tooltipRect.width);

                // 아이템 폭 계산 (기본값 100)
                float itemWidth = 100f;
                if (currentTargetRect != null)
                {
                    itemWidth = Mathf.Abs(currentTargetRect.rect.width);
                }

                float itemRightEdge = localPoint.x + itemWidth;

                float canvasLeft = canvasRect.xMin;
                float canvasRight = canvasRect.xMax;

                float rightSpace = canvasRight - itemRightEdge;
                float leftSpace = localPoint.x - canvasLeft;

                float requiredWidth = tooltipWidth + alignPaddingX;
                bool canShowRight = rightSpace >= requiredWidth;
                bool canShowLeft = leftSpace >= requiredWidth;

                Vector2 tooltipPosition = localPoint;

                if (canShowRight)
                {
                    tooltipPosition.x = itemRightEdge + alignPaddingX;
                }
                else if (canShowLeft)
                {
                    tooltipPosition.x = localPoint.x - alignPaddingX - tooltipWidth;
                }
                else
                {
                    // 양쪽 모두 부족하면 중앙 쪽으로 클램프
                    tooltipPosition.x = Mathf.Clamp(localPoint.x, canvasRect.xMin, canvasRect.xMax - tooltipWidth);
                }

                // 하단 정렬 유지 + 세로 오프셋
                tooltipPosition.y = localPoint.y + alignPaddingY;

                // 다른 UI 위에 표시
                rectTransform.SetAsLastSibling();

                // 좌우 경계 클램프
                tooltipPosition = ClampToScreenBounds(tooltipPosition, targetParent);

                rectTransform.localPosition = tooltipPosition;
            }
        }

        /// <summary>
        /// 아이템 기준으로 툴팁 위치를 계산합니다.
        /// </summary>
        /// <param name="itemLocalPoint">아이템의 로컬 좌표</param>
        /// <returns>툴팁의 로컬 좌표</returns>
        private Vector2 CalculateTooltipPositionRelativeToItem(Vector2 itemLocalPoint)
        {
            // 더 이상 사용하지 않음 (SkillCard 정책으로 대체)
            return itemLocalPoint;
        }

        /// <summary>
        /// 위치를 화면 경계 내로 제한합니다.
        /// </summary>
        private Vector2 ClampToScreenBounds(Vector2 position, RectTransform parentRect)
        {
            if (parentRect == null || currentTooltip == null) return position;

            var canvasRect = parentRect.rect;
            var tooltipRect = currentTooltip.GetComponent<RectTransform>().rect;

            // 좌우 경계만 클램프 (하단 정렬 유지, pivot.x=0)
            float minX = canvasRect.xMin;
            float maxX = canvasRect.xMax - tooltipRect.width;

            if (position.x < minX)
            {
                position.x = minX;
            }
            else if (position.x > maxX)
            {
                position.x = maxX;
            }

            return position;
        }

        #endregion
    }
}


