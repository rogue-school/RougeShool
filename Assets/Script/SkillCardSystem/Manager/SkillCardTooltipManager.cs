using UnityEngine;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Zenject;
using System.Collections;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 스킬 카드 툴팁을 관리하는 전역 매니저입니다.
    /// CoreSystem에 등록되어 모든 씬에서 사용 가능합니다.
    /// </summary>
    public class SkillCardTooltipManager : MonoBehaviour, ICoreSystemInitializable
    {
        #region Serialized Fields

        [Header("툴팁 설정")]
        [Tooltip("툴팁 프리팹")]
        [SerializeField] private SkillCardTooltip tooltipPrefab;
        
        [Tooltip("툴팁 표시 지연 시간 (초)")]
        [SerializeField] private float showDelay = 0.1f;
        
        [Tooltip("툴팁 숨김 지연 시간 (초)")]
        [SerializeField] private float hideDelay = 0.1f;

        [Header("캔버스 설정")]
        [Tooltip("캔버스 Sort Order (자동 생성 시 사용)")]
        [SerializeField] private int canvasSortOrder = 1000;
        
        [Tooltip("캔버스 이름 (자동 검색 시 사용)")]
        [SerializeField] private string canvasName = "TooltipCanvas";

        #endregion

        #region Private Fields

        private SkillCardTooltip currentTooltip;
        private Canvas tooltipCanvas;
        private ISkillCard hoveredCard;
        private Vector2 lastMousePosition;

        private float showTimer;
        private float hideTimer;
        private bool isShowingTooltip;
        private bool isHidingTooltip;

        private Camera uiCamera;
        private EventSystem eventSystem;

        private System.Collections.Generic.Dictionary<ISkillCard, RectTransform> cardUICache = new();

        #endregion

        #region Public Properties

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            UpdateMousePosition();
            UpdateTooltipTimers();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 현재 활성화된 툴팁 인스턴스를 반환합니다.
        /// </summary>
        public SkillCardTooltip CurrentTooltip => currentTooltip;

        #endregion

        #region ICoreSystemInitializable

        /// <summary>
        /// 시스템을 초기화합니다.
        /// </summary>
        public System.Collections.IEnumerator Initialize()
        {
            // GameLogger.LogInfo($"[SkillCardTooltipManager] 초기화 시작 - 현재 상태: IsInitialized={IsInitialized}", GameLogger.LogCategory.UI);
            
            yield return InitializeTooltipSystem();
            
            IsInitialized = true;
            // GameLogger.LogInfo($"[SkillCardTooltipManager] 초기화 완료 - IsInitialized={IsInitialized}, currentTooltip={currentTooltip != null}", GameLogger.LogCategory.UI);
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
                GameLogger.LogError("[SkillCardTooltipManager] EventSystem을 찾을 수 없습니다", GameLogger.LogCategory.Error);
                return;
            }

            // UI 카메라 설정 (캔버스는 자동 설정됨)
            if (tooltipCanvas != null)
            {
                uiCamera = tooltipCanvas.worldCamera;
            }
        }

        /// <summary>
        /// 툴팁 시스템을 초기화합니다.
        /// </summary>
        private System.Collections.IEnumerator InitializeTooltipSystem()
        {
            // 프리팹이 할당되지 않았으면 자동으로 찾기
            if (tooltipPrefab == null)
            {
                FindTooltipPrefab();
            }

            if (tooltipPrefab == null)
            {
                GameLogger.LogError("[SkillCardTooltipManager] 툴팁 프리팹을 찾을 수 없습니다. Resources 폴더에 SkillCardTooltip 프리팹을 배치해주세요.", GameLogger.LogCategory.Error);
                yield break;
            }

            // 툴팁 캔버스 자동 설정
            SetupTooltipCanvas();

            // 툴팁 인스턴스 생성
            CreateTooltipInstance();

            yield return null;
        }

        /// <summary>
        /// 툴팁 프리팹을 자동으로 찾습니다.
        /// </summary>
        private void FindTooltipPrefab()
        {
            // Resources 폴더에서 프리팹 찾기
            tooltipPrefab = Resources.Load<SkillCardTooltip>("SkillCardTooltip");
            
            if (tooltipPrefab != null)
            {
                // GameLogger.LogInfo("Resources에서 SkillCardTooltip 프리팹을 찾았습니다", GameLogger.LogCategory.UI);
                return;
            }

            // 씬에서 기존 인스턴스 찾기
            var existingTooltip = FindFirstObjectByType<SkillCardTooltip>();
            if (existingTooltip != null)
            {
                // 기존 인스턴스를 프리팹으로 사용 (개발 중 임시 방법)
                GameLogger.LogWarning("기존 SkillCardTooltip 인스턴스를 프리팹으로 사용합니다. 프리팹을 Resources에 배치하는 것을 권장합니다.", GameLogger.LogCategory.UI);
                tooltipPrefab = existingTooltip;
                return;
            }

            GameLogger.LogError("SkillCardTooltip 프리팹을 찾을 수 없습니다. 다음 중 하나를 수행해주세요:\n" +
                              "1. Resources/SkillCardTooltip.prefab 파일 생성\n" +
                              "2. Inspector에서 Tooltip Prefab 필드에 직접 할당", GameLogger.LogCategory.Error);
        }

        /// <summary>
        /// 툴팁 캔버스를 자동으로 설정합니다.
        /// </summary>
        private void SetupTooltipCanvas()
        {
            // 현재 씬의 UI 캔버스를 찾기 (Screen Space - Overlay 우선)
            tooltipCanvas = FindCurrentSceneUICanvas();
            
            if (tooltipCanvas == null)
            {
                // UI 캔버스가 없으면 자동 생성
                CreateTooltipCanvas();
            }
            else
            {
                // 기존 캔버스 설정 업데이트
                ConfigureCanvas(tooltipCanvas);
                // GameLogger.LogInfo($"현재 씬의 UI 캔버스 사용: {tooltipCanvas.name}", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 현재 씬의 UI 캔버스를 찾습니다.
        /// </summary>
        /// <returns>찾은 UI 캔버스 또는 null</returns>
        private Canvas FindCurrentSceneUICanvas()
        {
            // Screen Space - Overlay 캔버스 우선 검색
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            foreach (Canvas canvas in allCanvases)
            {
                // DontDestroyOnLoad 오브젝트가 아닌 현재 씬의 캔버스만 검색
                if (canvas.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    // Screen Space - Overlay 캔버스 우선
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        // GameLogger.LogInfo($"Screen Space - Overlay 캔버스 발견: {canvas.name}", GameLogger.LogCategory.UI);
                        return canvas;
                    }
                }
            }
            
            // Screen Space - Overlay가 없으면 다른 캔버스 검색
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    // GameLogger.LogInfo($"현재 씬의 캔버스 발견: {canvas.name} (RenderMode: {canvas.renderMode})", GameLogger.LogCategory.UI);
                    return canvas;
                }
            }
            
            GameLogger.LogWarning("현재 씬에서 UI 캔버스를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            return null;
        }

        /// <summary>
        /// 툴팁 캔버스를 생성합니다.
        /// 카드와 같은 캔버스에 생성하여 레이어링 문제를 방지합니다.
        /// </summary>
        private void CreateTooltipCanvas()
        {
            // 기존 캔버스가 있으면 제거
            if (tooltipCanvas != null)
            {
                DestroyImmediate(tooltipCanvas.gameObject);
                tooltipCanvas = null;
            }

            // 현재 씬의 메인 캔버스를 찾아서 사용
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null)
            {
                // 메인 캔버스의 자식으로 툴팁 캔버스 생성
                GameObject canvasObject = new GameObject(canvasName);
                tooltipCanvas = canvasObject.AddComponent<Canvas>();
                canvasObject.transform.SetParent(mainCanvas.transform, false);
                
                // 캔버스 설정 (메인 캔버스보다 높은 Sort Order)
                ConfigureCanvas(tooltipCanvas);
                
                // GameLogger.LogInfo($"메인 캔버스의 자식으로 툴팁 캔버스 생성: {canvasName}", GameLogger.LogCategory.UI);
            }
            else
            {
                // 메인 캔버스가 없으면 독립적으로 생성
                GameObject canvasObject = new GameObject(canvasName);
                tooltipCanvas = canvasObject.AddComponent<Canvas>();
                canvasObject.transform.SetParent(null, false);
                
                ConfigureCanvas(tooltipCanvas);
                
                // GameLogger.LogInfo($"독립적인 툴팁 캔버스 생성: {canvasName}", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 캔버스를 설정합니다.
        /// </summary>
        /// <param name="canvas">설정할 캔버스</param>
        private void ConfigureCanvas(Canvas canvas)
        {
            // 캔버스 설정
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvasSortOrder;
            
            // CanvasScaler 추가
            var scaler = canvas.gameObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            }
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // GraphicRaycaster 추가
            var raycaster = canvas.gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // GameLogger.LogInfo($"캔버스 설정 완료: Sort Order {canvasSortOrder}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁 인스턴스를 생성합니다.
        /// 각 카드마다 독립적인 툴팁을 생성합니다.
        /// </summary>
        private void CreateTooltipInstance()
        {
            // GameLogger.LogInfo($"[SkillCardTooltipManager] CreateTooltipInstance 시작 - currentTooltip: {currentTooltip != null}, tooltipPrefab: {tooltipPrefab != null}, tooltipCanvas: {tooltipCanvas != null}", GameLogger.LogCategory.UI);
            
            // 기존 툴팁이 있으면 제거 (단일 인스턴스 방식에서 변경)
            if (currentTooltip != null)
            {
                // GameLogger.LogInfo("[SkillCardTooltipManager] 기존 툴팁 제거", GameLogger.LogCategory.UI);
                DestroyImmediate(currentTooltip.gameObject);
                currentTooltip = null;
            }
            
            if (tooltipPrefab == null)
            {
                GameLogger.LogError("[SkillCardTooltipManager] tooltipPrefab이 null입니다", GameLogger.LogCategory.Error);
                return;
            }
            
            if (tooltipCanvas == null)
            {
                GameLogger.LogError("[SkillCardTooltipManager] tooltipCanvas가 null입니다", GameLogger.LogCategory.Error);
                return;
            }

            try
            {
                // 툴팁을 비활성화 상태로 생성 (필요할 때만 활성화)
                currentTooltip = Instantiate(tooltipPrefab, tooltipCanvas.transform);
                if (currentTooltip == null)
                {
                    GameLogger.LogError("[SkillCardTooltipManager] 툴팁 인스턴스 생성 실패", GameLogger.LogCategory.Error);
                    return;
                }

                // 초기에는 비활성화
                currentTooltip.gameObject.SetActive(false);

                // GameLogger.LogInfo("툴팁 인스턴스 생성 완료 (비활성화 상태)", GameLogger.LogCategory.UI);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SkillCardTooltipManager] 툴팁 인스턴스 생성 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 카드 UI를 등록합니다.
        /// </summary>
        /// <param name="card">카드 데이터</param>
        /// <param name="cardTransform">카드 UI의 RectTransform</param>
        public void RegisterCardUI(ISkillCard card, RectTransform cardTransform)
        {
            if (card == null || cardTransform == null) return;
            cardUICache[card] = cardTransform;
        }

        /// <summary>
        /// 카드 UI 등록을 해제합니다.
        /// </summary>
        /// <param name="card">카드 데이터</param>
        public void UnregisterCardUI(ISkillCard card)
        {
            if (card == null) return;
            cardUICache.Remove(card);
        }

        /// <summary>
        /// 카드에 마우스가 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="card">호버된 카드</param>
        public void OnCardHoverEnter(ISkillCard card)
        {
            if (card == null) return;

            // 초기화가 완료되지 않았다면 강제로 초기화 시도
            if (!IsInitialized)
            {
                GameLogger.LogWarning("[툴팁] 초기화 안됨 - 강제 초기화 시도", GameLogger.LogCategory.UI);
                StartCoroutine(ForceInitialize());
                return;
            }

            if (currentTooltip == null) return;

            hoveredCard = card;
            isHidingTooltip = false;
            hideTimer = 0f;

            if (!isShowingTooltip)
            {
                isShowingTooltip = true;
                showTimer = 0f;
            }
        }

        /// <summary>
        /// 카드에서 마우스가 이탈했을 때 호출됩니다.
        /// </summary>
        public void OnCardHoverExit()
        {
            if (hoveredCard == null) return;

            // 고정된 툴팁이 있으면 호버 종료 시에도 툴팁을 유지
            if (currentTooltip != null && currentTooltip.IsFixed)
            {
                hoveredCard = null;
                isShowingTooltip = false;
                showTimer = 0f;
                return;
            }
            
            hoveredCard = null;
            isShowingTooltip = false;
            showTimer = 0f;

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
            if (currentTooltip != null)
            {
                currentTooltip.HideTooltip();
            }

            hoveredCard = null;
            isShowingTooltip = false;
            isHidingTooltip = false;
            showTimer = 0f;
            hideTimer = 0f;
        }

        /// <summary>
        /// 강제로 초기화를 수행합니다.
        /// </summary>
        private System.Collections.IEnumerator ForceInitialize()
        {
            // GameLogger.LogInfo("[SkillCardTooltipManager] 강제 초기화 시작", GameLogger.LogCategory.UI);
            
            yield return InitializeTooltipSystem();
            
            IsInitialized = true;
            // GameLogger.LogInfo($"[SkillCardTooltipManager] 강제 초기화 완료 - IsInitialized={IsInitialized}, currentTooltip={currentTooltip != null}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁 시스템 상태를 디버깅합니다.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugTooltipSystem()
        {
            // GameLogger.LogInfo($"[SkillCardTooltipManager] 디버그 정보:", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - IsInitialized: {IsInitialized}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - tooltipPrefab: {tooltipPrefab != null}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - tooltipCanvas: {tooltipCanvas != null}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - currentTooltip: {currentTooltip != null}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - hoveredCard: {hoveredCard?.GetCardName()}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - isShowingTooltip: {isShowingTooltip}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - isHidingTooltip: {isHidingTooltip}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - showTimer: {showTimer:F2}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - hideTimer: {hideTimer:F2}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - showDelay: {showDelay}", GameLogger.LogCategory.UI);
            // GameLogger.LogInfo($"  - hideDelay: {hideDelay}", GameLogger.LogCategory.UI);
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// 마우스 위치를 업데이트합니다.
        /// </summary>
        private void UpdateMousePosition()
        {
            // EventSystem이 없어도 마우스 위치는 업데이트
            lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// 툴팁 타이머들을 업데이트합니다.
        /// </summary>
        private void UpdateTooltipTimers()
        {
            // 툴팁 표시 타이머
            if (isShowingTooltip && hoveredCard != null)
            {
                showTimer += Time.deltaTime;
                if (showTimer >= showDelay)
                {
                    // GameLogger.LogInfo($"[SkillCardTooltipManager] 툴팁 표시 타이머 완료 - showTimer: {showTimer:F2}, showDelay: {showDelay}", GameLogger.LogCategory.UI);
                    ShowTooltip();
                    isShowingTooltip = false;
                }
            }

            // 툴팁 숨김 타이머
            if (isHidingTooltip)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideDelay)
                {
                    // GameLogger.LogInfo($"[SkillCardTooltipManager] 툴팁 숨김 타이머 완료 - hideTimer: {hideTimer:F2}, hideDelay: {hideDelay}", GameLogger.LogCategory.UI);
                    HideTooltip();
                    isHidingTooltip = false;
                }
            }

            // 툴팁 위치 업데이트 (고정된 툴팁이 아닌 경우에만)
            if (currentTooltip != null && currentTooltip.gameObject.activeInHierarchy && !currentTooltip.IsFixed)
            {
                // 스킬카드 위치를 가져와서 툴팁 위치 업데이트
                Vector2 cardPosition = GetCurrentCardPosition();
                if (cardPosition != Vector2.zero)
                {
                    currentTooltip.UpdatePosition(cardPosition);
                }
            }
        }

        /// <summary>
        /// 현재 호버된 카드의 위치를 가져옵니다.
        /// 캐시된 RectTransform에서 위치 정보를 가져옵니다.
        /// </summary>
        /// <returns>카드의 스크린 위치</returns>
        private Vector2 GetCurrentCardPosition()
        {
            if (hoveredCard == null) return Vector2.zero;

            if (cardUICache.TryGetValue(hoveredCard, out RectTransform cardRect))
            {
                if (cardRect != null)
                {
                    Vector2 screenPosition;

                    if (tooltipCanvas != null && tooltipCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        screenPosition = cardRect.position;
                    }
                    else
                    {
                        screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, cardRect.position);
                    }

                    return screenPosition;
                }
                else
                {
                    cardUICache.Remove(hoveredCard);
                    GameLogger.LogWarning("[SkillCardTooltipManager] 캐시된 RectTransform이 파괴됨. 캐시에서 제거", GameLogger.LogCategory.UI);
                }
            }
            else
            {
                GameLogger.LogWarning($"[SkillCardTooltipManager] 카드 UI가 캐시에 등록되지 않음: {hoveredCard.GetCardName()}", GameLogger.LogCategory.UI);
            }

            return Vector2.zero;
        }

        #endregion

        #region Tooltip Control

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        public void ShowTooltip()
        {
            // GameLogger.LogInfo($"[SkillCardTooltipManager] ShowTooltip 호출됨 - currentTooltip: {currentTooltip != null}, hoveredCard: {hoveredCard?.GetCardName()}", GameLogger.LogCategory.UI);
            
            if (currentTooltip == null)
            {
                GameLogger.LogWarning("[SkillCardTooltipManager] currentTooltip이 null입니다.", GameLogger.LogCategory.UI);
                return;
            }
            
            if (hoveredCard == null)
            {
                GameLogger.LogWarning("[SkillCardTooltipManager] hoveredCard가 null입니다.", GameLogger.LogCategory.UI);
                return;
            }

            try
            {
                // 툴팁 활성화
                if (!currentTooltip.gameObject.activeInHierarchy)
                {
                    currentTooltip.gameObject.SetActive(true);
                }

                // 스킬카드 위치를 가져와서 툴팁 표시
                Vector2 cardPosition = GetCurrentCardPosition();
                if (cardPosition != Vector2.zero)
                {
                    currentTooltip.ShowTooltip(hoveredCard, cardPosition);
                    // GameLogger.LogInfo($"툴팁 표시: {hoveredCard.GetCardName()} at {cardPosition}", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[SkillCardTooltipManager] 스킬카드 위치를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SkillCardTooltipManager] 툴팁 표시 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        public void HideTooltip()
        {
            // GameLogger.LogInfo($"[SkillCardTooltipManager] HideTooltip 호출됨 - currentTooltip: {currentTooltip != null}", GameLogger.LogCategory.UI);
            
            if (currentTooltip == null) return;

            try
            {
                // 고정된 툴팁이 아닌 경우에만 숨김
                if (!currentTooltip.IsFixed)
                {
                    currentTooltip.HideTooltip();
                    // GameLogger.LogInfo("툴팁 숨김 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    // GameLogger.LogInfo("툴팁이 고정되어 있어 숨기지 않음", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[SkillCardTooltipManager] 툴팁 숨김 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (currentTooltip != null)
            {
                Destroy(currentTooltip.gameObject);
            }
        }

        /// <summary>
        /// 씬 전환 시 툴팁 캔버스를 정리합니다.
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                CleanupTooltipCanvas();
            }
        }

        /// <summary>
        /// 툴팁 캔버스를 정리합니다.
        /// </summary>
        private void CleanupTooltipCanvas()
        {
            if (tooltipCanvas != null && tooltipCanvas.gameObject.scene.name != "DontDestroyOnLoad")
            {
                // GameLogger.LogInfo($"씬 전환으로 인한 툴팁 캔버스 정리: {tooltipCanvas.name}", GameLogger.LogCategory.UI);
                
                if (currentTooltip != null)
                {
                    Destroy(currentTooltip.gameObject);
                    currentTooltip = null;
                }
                
                tooltipCanvas = null;
            }
        }

        #endregion
    }
}
