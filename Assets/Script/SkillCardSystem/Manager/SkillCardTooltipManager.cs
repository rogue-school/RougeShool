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

        private float showTimer;
        private float hideTimer;
        private bool isShowingTooltip;
        private bool isHidingTooltip;

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
        }

        /// <summary>
        /// 툴팁 시스템을 초기화합니다.
        /// </summary>
        private System.Collections.IEnumerator InitializeTooltipSystem()
        {
            GameLogger.LogInfo("[SkillCardTooltipManager] 툴팁 시스템 초기화 시작 - 이펙트 상태 확인", GameLogger.LogCategory.UI);
            
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
            GameLogger.LogInfo("[SkillCardTooltipManager] 캔버스 설정 전 - 이펙트 상태 확인", GameLogger.LogCategory.UI);
            SetupTooltipCanvas();
            GameLogger.LogInfo("[SkillCardTooltipManager] 캔버스 설정 후 - 이펙트 상태 확인", GameLogger.LogCategory.UI);

            // 툴팁 인스턴스 생성
            GameLogger.LogInfo("[SkillCardTooltipManager] 툴팁 인스턴스 생성 전 - 이펙트 상태 확인", GameLogger.LogCategory.UI);
            CreateTooltipInstance();
            GameLogger.LogInfo("[SkillCardTooltipManager] 툴팁 인스턴스 생성 후 - 이펙트 상태 확인", GameLogger.LogCategory.UI);

            // 초기화 완료 확인
            if (currentTooltip != null && tooltipCanvas != null)
            {
                GameLogger.LogInfo("[SkillCardTooltipManager] 툴팁 시스템 초기화 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogError("[SkillCardTooltipManager] 툴팁 시스템 초기화 실패", GameLogger.LogCategory.Error);
            }

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
        /// 스테이지 캔버스에 영향을 주지 않도록 항상 독립적인 캔버스를 생성합니다.
        /// </summary>
        private void SetupTooltipCanvas()
        {
            // 항상 독립적인 툴팁 캔버스 생성 (스테이지 캔버스 보호)
            CreateTooltipCanvas();
            GameLogger.LogInfo("[SkillCardTooltipManager] 독립적인 툴팁 캔버스 생성 - 스테이지 캔버스 보호", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁 캔버스를 생성합니다.
        /// 스테이지 캔버스에 영향을 주지 않도록 독립적인 캔버스를 생성합니다.
        /// </summary>
        private void CreateTooltipCanvas()
        {
            // 기존 캔버스가 있으면 제거
            if (tooltipCanvas != null)
            {
                DestroyImmediate(tooltipCanvas.gameObject);
                tooltipCanvas = null;
            }

            // 독립적인 툴팁 캔버스 생성 (스테이지 캔버스와 분리)
            GameObject canvasObject = new GameObject(canvasName);
            tooltipCanvas = canvasObject.AddComponent<Canvas>();
            
            // 독립적인 캔버스로 설정 (부모 없이)
            canvasObject.transform.SetParent(null, false);
            
            // 캔버스 설정 (독립적인 렌더링)
            ConfigureCanvas(tooltipCanvas);
            
            GameLogger.LogInfo($"[SkillCardTooltipManager] 독립적인 툴팁 캔버스 생성: {canvasName} (스테이지 캔버스와 분리)", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 캔버스를 설정합니다.
        /// </summary>
        /// <param name="canvas">설정할 캔버스</param>
        private void ConfigureCanvas(Canvas canvas)
        {
            // 캔버스 설정 - 원래 ScreenSpaceOverlay로 복원
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

            // 초기화가 완료되지 않았다면 즉시 초기화 시도
            if (!IsInitialized)
            {
                GameLogger.LogInfo("[툴팁] 초기화 안됨 - 즉시 초기화 시도", GameLogger.LogCategory.UI);
                StartCoroutine(ForceInitialize());
                return;
            }

            if (currentTooltip == null) 
            {
                GameLogger.LogWarning("[툴팁] currentTooltip이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

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
        /// 이펙트 렌더링에 영향을 주지 않도록 최적화된 초기화를 수행합니다.
        /// </summary>
        private System.Collections.IEnumerator ForceInitialize()
        {
            GameLogger.LogInfo("[SkillCardTooltipManager] 강제 초기화 시작 - 이펙트 보호 모드", GameLogger.LogCategory.UI);
            
            // 이펙트 상태 확인 로그 추가
            GameLogger.LogInfo("[SkillCardTooltipManager] 강제 초기화 전 이펙트 상태 확인", GameLogger.LogCategory.UI);
            
            // 기존 초기화 로직 실행 (이펙트에 영향 주지 않도록)
            yield return InitializeTooltipSystem();
            
            // 초기화 완료 확인
            if (currentTooltip != null && tooltipCanvas != null)
            {
                IsInitialized = true;
                GameLogger.LogInfo($"[SkillCardTooltipManager] 강제 초기화 완료 - IsInitialized={IsInitialized}, currentTooltip={currentTooltip != null}", GameLogger.LogCategory.UI);
                GameLogger.LogInfo("[SkillCardTooltipManager] 강제 초기화 후 이펙트 상태 확인", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogError("[SkillCardTooltipManager] 강제 초기화 실패 - 필수 컴포넌트 누락", GameLogger.LogCategory.Error);
                IsInitialized = false;
            }
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
        /// 현재는 사용하지 않으므로 제거됨.
        /// </summary>
        private void UpdateMousePosition()
        {
            // 마우스 위치 추적 기능 제거됨 - 독립적인 캔버스 사용으로 불필요
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
                        // 독립적인 캔버스 사용 - ScreenSpaceOverlay 모드에서는 카메라 불필요
                        screenPosition = cardRect.position;
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
            GameLogger.LogInfo($"[SkillCardTooltipManager] ShowTooltip 호출됨 - currentTooltip: {currentTooltip != null}, hoveredCard: {hoveredCard?.GetCardName()}", GameLogger.LogCategory.UI);
            
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
                // 툴팁 활성화 (카메라 전환 없이)
                if (!currentTooltip.gameObject.activeInHierarchy)
                {
                    currentTooltip.gameObject.SetActive(true);
                }

                // 스킬카드 위치를 가져와서 툴팁 표시
                Vector2 cardPosition = GetCurrentCardPosition();
                if (cardPosition != Vector2.zero)
                {
                    currentTooltip.ShowTooltip(hoveredCard, cardPosition);
                    GameLogger.LogInfo($"툴팁 표시: {hoveredCard.GetCardName()} at {cardPosition}", GameLogger.LogCategory.UI);
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
            GameLogger.LogInfo($"[SkillCardTooltipManager] HideTooltip 호출됨 - currentTooltip: {currentTooltip != null}", GameLogger.LogCategory.UI);
            
            if (currentTooltip == null) return;

            try
            {
                // 고정된 툴팁이 아닌 경우에만 숨김
                if (!currentTooltip.IsFixed)
                {
                    currentTooltip.HideTooltip();
                    GameLogger.LogInfo("툴팁 숨김 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogInfo("툴팁이 고정되어 있어 숨기지 않음", GameLogger.LogCategory.UI);
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
