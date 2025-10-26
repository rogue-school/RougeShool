using UnityEngine;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.UI;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Zenject;
using System.Collections;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 버프/디버프 툴팁을 관리하는 전역 매니저입니다.
    /// CoreSystem에 등록되어 모든 씬에서 사용 가능합니다.
    /// </summary>
    public class BuffDebuffTooltipManager : MonoBehaviour, ICoreSystemInitializable
    {
        #region Serialized Fields

        [Header("툴팁 설정")]
        [Tooltip("버프/디버프 툴팁 프리팹")]
        [SerializeField] private BuffDebuffTooltip tooltipPrefab;
        
        [Tooltip("툴팁 표시 지연 시간 (초)")]
        [SerializeField] private float showDelay = 0.2f;
        
        [Tooltip("툴팁 숨김 지연 시간 (초)")]
        [SerializeField] private float hideDelay = 0.1f;

        [Header("캔버스 설정")]
        [Tooltip("캔버스 Sort Order (자동 생성 시 사용)")]
        [SerializeField] private int canvasSortOrder = 1001;
        
        [Tooltip("캔버스 이름 (자동 검색 시 사용)")]
        [SerializeField] private string canvasName = "BuffDebuffTooltipCanvas";

        #endregion

        #region Private Fields

        private BuffDebuffTooltip currentTooltip;
        private Canvas tooltipCanvas;
        private IPerTurnEffect hoveredEffect;
        private IPerTurnEffect pendingEffect; // 초기화 대기 중 첫 호버 효과 저장
        private bool pendingShow; // 초기화 완료 즉시 표시 플래그
		private RectTransform currentTargetRect; // 현재 호버 대상 슬롯의 RectTransform

        private float showTimer;
        private float hideTimer;
        private bool isShowingTooltip;
        private bool isHidingTooltip;

        private EventSystem eventSystem;

        #endregion

        #region Public Properties

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized { get; private set; }

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

        #endregion

        #region ICoreSystemInitializable

        /// <summary>
        /// 시스템을 초기화합니다.
        /// </summary>
        public IEnumerator Initialize()
        {
            if (IsInitialized) yield break;

            GameLogger.LogInfo("[BuffDebuffTooltipManager] 초기화 시작", GameLogger.LogCategory.UI);
            yield return InitializeTooltipSystem();
        }

        /// <summary>
        /// 시스템을 정리합니다.
        /// </summary>
        public void Cleanup()
        {
            if (!IsInitialized) return;

            GameLogger.LogInfo("[BuffDebuffTooltipManager] 정리 시작", GameLogger.LogCategory.UI);
            
            // 툴팁 숨김
            HideTooltip();
            
            // 툴팁 인스턴스 정리
            if (currentTooltip != null)
            {
                DestroyImmediate(currentTooltip.gameObject);
                currentTooltip = null;
            }

            IsInitialized = false;
            GameLogger.LogInfo("[BuffDebuffTooltipManager] 정리 완료", GameLogger.LogCategory.UI);
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
                GameLogger.LogError("[BuffDebuffTooltipManager] EventSystem을 찾을 수 없습니다", GameLogger.LogCategory.Error);
                return;
            }
        }

        /// <summary>
        /// 툴팁 시스템을 초기화합니다.
        /// </summary>
        private System.Collections.IEnumerator InitializeTooltipSystem()
        {
            GameLogger.LogInfo("[BuffDebuffTooltipManager] 툴팁 시스템 초기화 시작", GameLogger.LogCategory.UI);
            
            // 프리팹이 할당되지 않았으면 자동으로 찾기
            if (tooltipPrefab == null)
            {
                FindTooltipPrefab();
            }

            if (tooltipPrefab == null)
            {
                GameLogger.LogError("[BuffDebuffTooltipManager] 툴팁 프리팹을 찾을 수 없습니다. Resources 폴더에 BuffDebuffTooltip 프리팹을 배치해주세요.", GameLogger.LogCategory.Error);
                yield break;
            }

            // 툴팁 캔버스 자동 설정
            SetupTooltipCanvas();


			// 초기화 완료 확인 (인스턴스는 첫 표시 시점에 생성)
			if (tooltipCanvas != null || true)
            {
                IsInitialized = true;
                GameLogger.LogInfo("[BuffDebuffTooltipManager] 툴팁 시스템 초기화 완료", GameLogger.LogCategory.UI);
                
                // 대기 중이던 첫 호버 효과가 있으면 즉시 표시
                if (pendingShow && pendingEffect != null)
                {
                    GameLogger.LogInfo("[BuffDebuffTooltipManager] 대기 중이던 효과 툴팁 즉시 표시", GameLogger.LogCategory.UI);
                    ShowTooltip();
                }
            }
            else
            {
                GameLogger.LogError("[BuffDebuffTooltipManager] 툴팁 시스템 초기화 실패", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 툴팁 프리팹을 자동으로 찾습니다.
        /// </summary>
        private void FindTooltipPrefab()
        {
            var prefab = Resources.Load<BuffDebuffTooltip>("BuffDebuffTooltip");
            if (prefab != null)
            {
                tooltipPrefab = prefab;
                GameLogger.LogInfo("[BuffDebuffTooltipManager] 툴팁 프리팹 자동 검색 성공", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[BuffDebuffTooltipManager] 툴팁 프리팹 자동 검색 실패", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 툴팁 캔버스를 설정합니다.
        /// </summary>
        private void SetupTooltipCanvas()
        {
            // 기존 캔버스 찾기
            tooltipCanvas = GameObject.Find(canvasName)?.GetComponent<Canvas>();
            
            if (tooltipCanvas == null)
            {
                // 캔버스가 없으면 새로 생성
                CreateTooltipCanvas();
            }
            else
            {
                GameLogger.LogInfo("[BuffDebuffTooltipManager] 기존 툴팁 캔버스 사용", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 툴팁 캔버스를 생성합니다.
        /// </summary>
        private void CreateTooltipCanvas()
        {
            GameObject canvasObj = new GameObject(canvasName);
            canvasObj.transform.SetParent(transform);
            
            tooltipCanvas = canvasObj.AddComponent<Canvas>();
            tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tooltipCanvas.sortingOrder = canvasSortOrder;
            
            // CanvasScaler 추가
            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            
            // GraphicRaycaster 추가
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            GameLogger.LogInfo("[BuffDebuffTooltipManager] 툴팁 캔버스 생성 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁 인스턴스를 생성합니다.
        /// </summary>
		private void CreateTooltipInstance()
        {
            // 기존 툴팁이 있으면 제거
            if (currentTooltip != null)
            {
                DestroyImmediate(currentTooltip.gameObject);
                currentTooltip = null;
            }
            
			if (tooltipPrefab == null) return;

            try
            {
				// 툴팁을 대상 슬롯의 자식으로 생성 (요청사항 반영)
				Transform parentForTooltip = currentTargetRect != null
					? (Transform)currentTargetRect
					: (tooltipCanvas != null ? tooltipCanvas.transform : null);
				if (parentForTooltip == null)
				{
					GameLogger.LogError("[BuffDebuffTooltipManager] 툴팁 부모를 찾지 못했습니다 (대상 슬롯/캔버스)", GameLogger.LogCategory.Error);
					return;
				}

				currentTooltip = Instantiate(tooltipPrefab, parentForTooltip);
                if (currentTooltip == null)
                {
                    GameLogger.LogError("[BuffDebuffTooltipManager] 툴팁 인스턴스 생성 실패", GameLogger.LogCategory.Error);
                    return;
                }

                // 초기에는 비활성화
                currentTooltip.gameObject.SetActive(false);
				currentTooltip.transform.SetAsLastSibling();

                GameLogger.LogInfo("[BuffDebuffTooltipManager] 툴팁 인스턴스 생성 완료", GameLogger.LogCategory.UI);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[BuffDebuffTooltipManager] 툴팁 인스턴스 생성 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 버프/디버프 효과에 마우스가 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="effect">호버된 효과</param>
        /// <param name="position">슬롯 위치</param>
		public void OnEffectHoverEnter(IPerTurnEffect effect, Vector3 position)
        {
            if (effect == null) return;

			// 현재 호버된 효과의 슬롯 RectTransform 캐시
			currentTargetRect = FindSlotRectForEffect(effect);

            // 초기화가 완료되지 않았다면 즉시 초기화 시도 후 첫 호버를 기억하여 바로 표시
            if (!IsInitialized)
            {
                GameLogger.LogInfo("[BuffDebuffTooltipManager] 초기화 안됨 - 즉시 초기화 시도 (첫 호버 효과 보존)", GameLogger.LogCategory.UI);
                pendingEffect = effect;
                pendingShow = true;
                StartCoroutine(ForceInitialize());
                return;
            }

            if (currentTooltip == null) 
            {
                GameLogger.LogWarning("[BuffDebuffTooltipManager] currentTooltip이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            hoveredEffect = effect;
            isHidingTooltip = false;
            hideTimer = 0f;

            if (!isShowingTooltip)
            {
                isShowingTooltip = true;
                showTimer = 0f;
            }
        }

        /// <summary>
        /// 효과에서 마우스가 이탈했을 때 호출됩니다.
        /// </summary>
        public void OnEffectHoverExit()
        {
            if (hoveredEffect == null) return;
            
            hoveredEffect = null;
            isShowingTooltip = false;
            showTimer = 0f;

            // 초기화 대기 중이던 첫 호버도 해제
            pendingEffect = null;
            pendingShow = false;

            if (!isHidingTooltip)
            {
                isHidingTooltip = true;
                hideTimer = 0f;
            }
        }

        /// <summary>
        /// 버프/디버프 툴팁을 표시합니다.
        /// </summary>
        /// <param name="effect">표시할 효과</param>
        /// <param name="position">슬롯 위치</param>
        public void ShowBuffDebuffTooltip(IPerTurnEffect effect, Vector3 position)
        {
            OnEffectHoverEnter(effect, position);
        }

        /// <summary>
        /// 버프/디버프 툴팁을 숨깁니다.
        /// </summary>
        public void HideBuffDebuffTooltip()
        {
            OnEffectHoverExit();
        }

        #endregion

        #region Timer Management

        /// <summary>
        /// 툴팁 타이머들을 업데이트합니다.
        /// </summary>
        private void UpdateTooltipTimers()
        {
            // 표시 타이머 업데이트
            if (isShowingTooltip)
            {
                showTimer += Time.deltaTime;
                if (showTimer >= showDelay)
                {
                    ShowTooltip();
                }
            }

            // 숨김 타이머 업데이트
            if (isHidingTooltip)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideDelay)
                {
                    HideTooltip();
                }
            }
        }

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        public void ShowTooltip()
        {
            GameLogger.LogInfo($"[BuffDebuffTooltipManager] ShowTooltip 호출됨 - currentTooltip: {currentTooltip != null}, hoveredEffect: {hoveredEffect?.GetType().Name}", GameLogger.LogCategory.UI);
            
			if (currentTooltip == null)
            {
				CreateTooltipInstance();
				if (currentTooltip == null) return;
            }
            
            if (hoveredEffect == null)
            {
                GameLogger.LogWarning("[BuffDebuffTooltipManager] hoveredEffect가 null입니다.", GameLogger.LogCategory.UI);
                return;
            }

            try
            {
				// 툴팁 활성화
				if (!currentTooltip.gameObject.activeInHierarchy)
				{
					currentTooltip.gameObject.SetActive(true);
				}

                // 효과 위치를 가져와서 툴팁 표시
                Vector2 effectPosition = GetCurrentEffectPosition();
                if (effectPosition != Vector2.zero)
                {
                    // SubTooltipModel에 남은 턴 등 값-페어를 추가하도록 유도하려면
                    // 현재 구조에서 SubTooltip 생성 시 SkillCardTooltip이 모델을 조립합니다.
                    currentTooltip.ShowTooltip(hoveredEffect, effectPosition);
                    GameLogger.LogInfo($"버프/디버프 툴팁 표시: {hoveredEffect.GetType().Name} at {effectPosition}", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[BuffDebuffTooltipManager] 효과 위치를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[BuffDebuffTooltipManager] 툴팁 표시 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        public void HideTooltip()
        {
            if (currentTooltip != null)
            {
                currentTooltip.HideTooltip();
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

            hoveredEffect = null;
            isShowingTooltip = false;
            isHidingTooltip = false;
            showTimer = 0f;
            hideTimer = 0f;
        }

        /// <summary>
        /// 현재 효과의 위치를 가져옵니다.
        /// </summary>
        /// <returns>효과 위치</returns>
        private Vector2 GetCurrentEffectPosition()
        {
            if (hoveredEffect == null) return Vector2.zero;

            // 현재 호버된 효과의 슬롯을 찾아서 위치 반환
            var slotViews = Object.FindObjectsByType<BuffDebuffSlotView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var slotView in slotViews)
            {
                if (slotView.CurrentEffect == hoveredEffect)
                {
                    return RectTransformUtility.WorldToScreenPoint(Camera.main, slotView.transform.position);
                }
            }

            return Vector2.zero;
        }

		/// <summary>
		/// 현재 효과를 표시 중인 슬롯의 RectTransform을 반환합니다.
		/// </summary>
		private RectTransform FindSlotRectForEffect(IPerTurnEffect effect)
		{
			if (effect == null) return null;
			var slotViews = Object.FindObjectsByType<BuffDebuffSlotView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (var slotView in slotViews)
			{
				if (slotView != null && slotView.CurrentEffect == effect)
				{
					return slotView.transform as RectTransform;
				}
			}
			return null;
		}

        #endregion

        #region Force Initialize

        /// <summary>
        /// 강제 초기화를 수행합니다.
        /// </summary>
        private System.Collections.IEnumerator ForceInitialize()
        {
            yield return StartCoroutine(InitializeTooltipSystem());
        }

        #endregion
    }
}
