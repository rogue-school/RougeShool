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

        // TooltipLayer 제거: 캔버스 루트에 직접 붙입니다

        #endregion

        #region Private Fields

        private SkillCardTooltip currentTooltip;
        private RectTransform tooltipLayer; // 현재 선택된 캔버스의 루트 RectTransform
        private ISkillCard hoveredCard;
        private ISkillCard pendingCard; // 초기화 대기 중 첫 호버 카드 저장
        private bool pendingShow; // 초기화 완료 즉시 표시 플래그
        private RectTransform currentTargetRect; // 실제 대상 RectTransform

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

            // TooltipLayer는 카드의 실제 캔버스 기준으로 런타임에 보장합니다

            // 인스턴스는 첫 표시 시점에 생성 (대상 캔버스 보장)
            GameLogger.LogInfo("[SkillCardTooltipManager] 툴팁 인스턴스는 첫 표시 시 생성", GameLogger.LogCategory.UI);

			// 초기화 완료 확인 (툴팁은 첫 사용 시 생성)
			GameLogger.LogInfo("[SkillCardTooltipManager] 툴팁 시스템 초기화 완료", GameLogger.LogCategory.UI);

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

        private void SetupTooltipLayer()
        {
            var stageCanvas = GetPreferredCanvas();
            if (stageCanvas == null)
            {
                GameLogger.LogError("[SkillCardTooltipManager] Stage Canvas를 찾을 수 없습니다", GameLogger.LogCategory.Error);
                return;
            }
            tooltipLayer = stageCanvas.GetComponent<RectTransform>();
        }

        // 독립 캔버스 생성/설정 로직 제거 (스테이지 캔버스의 TooltipLayer 사용)

        /// <summary>
        /// 툴팁 인스턴스를 생성합니다.
        /// 각 카드마다 독립적인 툴팁을 생성합니다.
        /// </summary>
        private void CreateTooltipInstance()
        {
            // GameLogger.LogInfo($"[SkillCardTooltipManager] CreateTooltipInstance 시작 - currentTooltip: {currentTooltip != null}, tooltipPrefab: {tooltipPrefab != null}", GameLogger.LogCategory.UI);
            
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
            
            // tooltipLayer는 아래에서 선택된 Canvas의 루트를 사용하므로 선행 검사 불필요

            try
            {
                // 요청: 툴팁을 캔버스 최상위에 생성 (다른 UI 요소 위에 표시)
                Transform parentForTooltip = GetCanvasOfCurrentTarget()?.transform;

                if (parentForTooltip == null)
                {
                    GameLogger.LogError("[SkillCardTooltipManager] 툴팁 부모를 찾지 못했습니다 (캔버스)", GameLogger.LogCategory.Error);
                    return;
                }

                currentTooltip = Instantiate(tooltipPrefab, parentForTooltip);
                if (currentTooltip == null)
                {
                    GameLogger.LogError("[SkillCardTooltipManager] 툴팁 인스턴스 생성 실패", GameLogger.LogCategory.Error);
                    return;
                }

                // 초기에는 비활성화하고 캔버스 최상단으로 정렬
                currentTooltip.gameObject.SetActive(false);
                currentTooltip.transform.SetAsLastSibling();

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

            // 초기화가 완료되지 않았다면 즉시 초기화 시도 후 첫 호버를 기억하여 바로 표시
            if (!IsInitialized)
            {
                GameLogger.LogInfo("[툴팁] 초기화 안됨 - 즉시 초기화 시도 (첫 호버 카드 보존)", GameLogger.LogCategory.UI);
                pendingCard = card;
                pendingShow = true;
                StartCoroutine(ForceInitialize());
                return;
            }

			// 인스턴스는 ShowTooltip 시점에 생성되므로 여기서는 존재 여부를 강제하지 않습니다

            hoveredCard = card;
            // 실제 대상 RectTransform을 즉시 캐시(없으면 null)
            currentTargetRect = null;
            if (cardUICache.TryGetValue(card, out var rt) && rt != null)
            {
                currentTargetRect = rt;
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

            // 초기화 대기 중이던 첫 호버도 해제
            pendingCard = null;
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
			
			// 초기화 완료 확인 (인스턴스는 첫 표시 시 생성)
			if (tooltipPrefab != null)
            {
                IsInitialized = true;
                GameLogger.LogInfo($"[SkillCardTooltipManager] 강제 초기화 완료 - IsInitialized={IsInitialized}, currentTooltip={currentTooltip != null}", GameLogger.LogCategory.UI);
                GameLogger.LogInfo("[SkillCardTooltipManager] 강제 초기화 후 이펙트 상태 확인", GameLogger.LogCategory.UI);

                // 첫 호버 즉시 표시 처리
                if (pendingShow && pendingCard != null)
                {
                    try
                    {
                        hoveredCard = pendingCard;
                        pendingCard = null;
                        pendingShow = false;

                        isHidingTooltip = false;
                        hideTimer = 0f;
                        isShowingTooltip = false;
                        showTimer = 0f;

                        // 강제 초기화 이후에도 부모 RectTransform을 확실히 캐시
                        currentTargetRect = null;
                        if (hoveredCard != null && cardUICache.TryGetValue(hoveredCard, out var cachedRt) && cachedRt != null)
                        {
                            currentTargetRect = cachedRt;
                        }

                        // 표준 경로로 표시하여 인스턴스 생성/부모 배치 포함
                        ShowTooltip();
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogError($"[툴팁] 초기화 직후 표시 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
                    }
                }
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

            // 우선 현재 타깃 Rect를 사용하고, 없으면 캐시 조회
            RectTransform cardRect = currentTargetRect;
            if (cardRect == null)
            {
                cardUICache.TryGetValue(hoveredCard, out cardRect);
            }
            if (cardRect != null)
            {
                // 카드가 속한 캔버스와 카메라를 확인
                var sourceCanvas = cardRect.GetComponentInParent<Canvas>();
                Camera cam = null;
                if (sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    cam = sourceCanvas.worldCamera;
                }

                // 카드의 왼쪽-아래 모서리를 계산: GetWorldCorners 사용 (하단 정렬용)
                Vector3[] corners = new Vector3[4];
                cardRect.GetWorldCorners(corners); // 0:BL, 1:TL, 2:TR, 3:BR
                Vector3 bottomLeftWorld = corners[0]; // BL = Bottom Left
                Vector2 screenBL = RectTransformUtility.WorldToScreenPoint(cam, bottomLeftWorld);
                return screenBL;
            }
            GameLogger.LogWarning($"[SkillCardTooltipManager] 카드 UI Rect를 찾지 못함: {hoveredCard.GetCardName()}", GameLogger.LogCategory.UI);

            return Vector2.zero;
        }

        private Canvas GetCanvasOfCurrentTarget()
        {
            if (currentTargetRect != null)
            {
                var c = currentTargetRect.GetComponentInParent<Canvas>();
                if (c != null) return c;
            }
            // 폴백: 씬의 첫 Canvas
            return FindFirstObjectByType<Canvas>();
        }

        private Canvas GetPreferredCanvas()
        {
            // DontDestroyOnLoad에 있는 Canvas는 제외하고, 현재 씬의 최상위 Canvas를 선택
            var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Canvas best = null;
            foreach (var c in allCanvases)
            {
                if (c == null) continue;
                var sceneName = c.gameObject.scene.name;
                if (string.Equals(sceneName, "DontDestroyOnLoad")) continue;
                // 대상 Rect가 이 캔버스 하위라면 즉시 채택
                if (currentTargetRect != null && currentTargetRect.IsChildOf(c.transform))
                {
                    return c;
                }
                // 첫 유효 Canvas 기억
                if (best == null) best = c;
            }
            return best != null ? best : GetCanvasOfCurrentTarget();
        }

        // TooltipLayer 제거: 캔버스 루트 RectTransform을 사용하므로 별도 보장 메서드 불필요

        #endregion

        #region Tooltip Control

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        public void ShowTooltip()
        {
            GameLogger.LogInfo($"[SkillCardTooltipManager] ShowTooltip 호출됨 - currentTooltip: {currentTooltip != null}, hoveredCard: {hoveredCard?.GetCardName()}", GameLogger.LogCategory.UI);
            
            // 부모 확정을 위해 현재 타깃 RectTransform을 우선 보장
            if (hoveredCard != null && currentTargetRect == null)
            {
                if (cardUICache.TryGetValue(hoveredCard, out var cachedRt) && cachedRt != null)
                {
                    currentTargetRect = cachedRt;
                }
            }

            // 첫 표시 시점에 툴팁 생성 (대상 캔버스/부모 확정 후)
            if (currentTooltip == null)
            {
                CreateTooltipInstance();
                if (currentTooltip == null) return;
            }
            else
            {
                // 기존 인스턴스를 캔버스 최상위로 이동 (다른 UI 요소 위에 표시)
                var canvas = GetCanvasOfCurrentTarget();
                if (canvas != null && currentTooltip.transform.parent != canvas.transform)
                {
                    currentTooltip.transform.SetParent(canvas.transform, false);
                    currentTooltip.transform.SetAsLastSibling();
                }
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
                    currentTooltip.ShowTooltip(hoveredCard, cardPosition, currentTargetRect);
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
                // 동일 캔버스 사용: 별도 정리 불필요
            }
        }

        // 독립 캔버스 사용 제거로 정리 로직 삭제

        #endregion
    }
}
