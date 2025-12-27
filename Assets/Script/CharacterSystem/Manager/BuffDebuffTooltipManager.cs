using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.UI;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Game.ItemSystem.Constants;
using Zenject;
using System.Collections;
using System.Threading.Tasks;

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
        
        // 툴팁 지연 시간은 ItemConstants에서 관리 (코드로 제어)
        private float showDelay;
        private float hideDelay;

        #endregion

        #region Private Fields

        private BuffDebuffTooltip currentTooltip;
        private IPerTurnEffect hoveredEffect;
        private IPerTurnEffect pendingEffect; // 초기화 대기 중 첫 호버 효과 저장
        private bool pendingShow; // 초기화 완료 즉시 표시 플래그
        private Vector3 pendingShowPosition; // 초기화 대기 중 첫 호버 위치 저장
        private RectTransform pendingTargetRect; // 초기화 대기 중 첫 호버 대상 RectTransform 저장
		private RectTransform currentTargetRect; // 현재 호버 대상 슬롯의 RectTransform

        private Coroutine showTooltipCoroutine;
        private Coroutine hideTooltipCoroutine;
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
            
            // 모든 씬에서 사용 가능하도록 DontDestroyOnLoad 설정
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            if (!IsInitialized) return;

            // 대상 유효성 검사
            ValidateTarget();

            // 툴팁이 표시 중이면 지속적으로 위치 업데이트 (실시간 보간)
            if (currentTooltip != null && currentTooltip.gameObject != null && currentTooltip.gameObject.activeInHierarchy)
            {
                Vector2 effectPosition = GetCurrentEffectPosition();
                if (effectPosition != Vector2.zero)
                {
                    currentTooltip.UpdatePosition(effectPosition);
                }
            }
        }

        #endregion

        #region ICoreSystemInitializable

        /// <summary>
        /// 시스템을 초기화합니다
        /// </summary>
        /// <returns>초기화 코루틴</returns>
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
            
            // 코루틴 정리
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            if (hideTooltipCoroutine != null)
            {
                StopCoroutine(hideTooltipCoroutine);
                hideTooltipCoroutine = null;
            }
            
            // 툴팁 숨김
            HideTooltip();
            
            // 툴팁 인스턴스 정리
            if (currentTooltip != null)
            {
                DestroyImmediate(currentTooltip.gameObject);
                currentTooltip = null;
            }

            isShowingTooltip = false;
            isHidingTooltip = false;
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
            // 툴팁 지연 시간을 상수에서 초기화
            showDelay = ItemConstants.TOOLTIP_SHOW_DELAY;
            hideDelay = ItemConstants.TOOLTIP_HIDE_DELAY;
            
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
                yield return StartCoroutine(FindTooltipPrefab());
            }

            if (tooltipPrefab == null)
            {
                GameLogger.LogError("[BuffDebuffTooltipManager] 툴팁 프리팹을 찾을 수 없습니다. Resources 폴더에 BuffDebuffTooltip 프리팹을 배치해주세요.", GameLogger.LogCategory.Error);
                yield break;
            }

			// 초기화 완료 확인 (인스턴스는 첫 표시 시점에 생성)
			IsInitialized = true;
            GameLogger.LogInfo("[BuffDebuffTooltipManager] 툴팁 시스템 초기화 완료", GameLogger.LogCategory.UI);
            
            // 대기 중이던 첫 호버 효과가 있으면 즉시 표시
            if (pendingShow && pendingEffect != null)
            {
                GameLogger.LogInfo("[BuffDebuffTooltipManager] 대기 중이던 효과 툴팁 즉시 표시", GameLogger.LogCategory.UI);
                // 현재 상태를 저장하고 초기화 전 상태 복원
                var savedEffect = hoveredEffect;
                var savedRect = currentTargetRect;
                hoveredEffect = pendingEffect;
                currentTargetRect = pendingTargetRect;
                // 위치를 계산하여 ShowTooltip 호출
                Vector2 effectPosition = GetCurrentEffectPosition();
                if (effectPosition == Vector2.zero && pendingShowPosition != Vector3.zero)
                {
                    effectPosition = pendingShowPosition;
                }
                if (effectPosition != Vector2.zero)
                {
                    ShowBuffDebuffTooltip(pendingEffect, pendingShowPosition);
                }
                // 상태 복원
                hoveredEffect = savedEffect;
                currentTargetRect = savedRect;
                pendingEffect = null;
                pendingShow = false;
                pendingShowPosition = Vector3.zero;
                pendingTargetRect = null;
            }
        }

        /// <summary>
        /// 툴팁 프리팹을 자동으로 찾습니다.
        /// </summary>
        private System.Collections.IEnumerator FindTooltipPrefab()
        {
            var handle = Addressables.LoadAssetAsync<BuffDebuffTooltip>("BuffDebuffTooltip");
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                tooltipPrefab = handle.Result;
                GameLogger.LogInfo("[BuffDebuffTooltipManager] 툴팁 프리팹 자동 검색 성공", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[BuffDebuffTooltipManager] 툴팁 프리팹 자동 검색 실패", GameLogger.LogCategory.UI);
                if (handle.OperationException != null)
                {
                    GameLogger.LogError($"[BuffDebuffTooltipManager] Addressables 로드 오류: {handle.OperationException.Message}", GameLogger.LogCategory.Error);
                }
            }
        }

        /// <summary>
        /// 현재 대상의 캔버스를 가져옵니다.
        /// </summary>
        /// <returns>대상의 캔버스 (없으면 null)</returns>
        private Canvas GetCanvasOfCurrentTarget()
        {
            if (currentTargetRect == null) return null;
            return currentTargetRect.GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// 툴팁 인스턴스를 생성합니다.
        /// SkillCardTooltipManager, ItemTooltipManager와 동일한 방식으로 대상의 캔버스에 생성합니다.
        /// </summary>
		private void CreateTooltipInstance()
        {
            // 기존 툴팁이 있으면 제거
            if (currentTooltip != null)
            {
                DestroyImmediate(currentTooltip.gameObject);
                currentTooltip = null;
            }
            
			if (tooltipPrefab == null)
            {
                GameLogger.LogError("[BuffDebuffTooltipManager] tooltipPrefab이 null입니다", GameLogger.LogCategory.Error);
                return;
            }

            try
            {
                // SkillCardTooltipManager, ItemTooltipManager와 동일: 대상의 캔버스에 생성
                Transform parentForTooltip = GetCanvasOfCurrentTarget()?.transform;
                
                if (parentForTooltip == null)
                {
                    GameLogger.LogWarning("[BuffDebuffTooltipManager] 툴팁 부모를 찾지 못했습니다 (대상 캔버스 없음) – 표시를 건너뜁니다", GameLogger.LogCategory.UI);
                    return;
                }

				currentTooltip = Instantiate(tooltipPrefab, parentForTooltip);
                if (currentTooltip == null)
                {
                    GameLogger.LogError("[BuffDebuffTooltipManager] 툴팁 인스턴스 생성 실패", GameLogger.LogCategory.Error);
                    return;
                }

                // 초기에는 비활성화하고 캔버스 최상단으로 정렬
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
			// targetRect가 전달되지 않았거나 null이면 찾기
			if (currentTargetRect == null)
			{
				currentTargetRect = FindSlotRectForEffect(effect);
			}

            // 초기화가 완료되지 않았다면 즉시 초기화 시도 후 첫 호버를 기억하여 바로 표시
            if (!IsInitialized)
            {
                GameLogger.LogInfo("[BuffDebuffTooltipManager] 초기화 안됨 - 즉시 초기화 시도 (첫 호버 효과 보존)", GameLogger.LogCategory.UI);
                pendingEffect = effect;
                pendingShow = true;
                pendingShowPosition = position;
                pendingTargetRect = currentTargetRect;
                StartCoroutine(ForceInitialize());
                return;
            }

            hoveredEffect = effect;
            
            // 숨김 코루틴 취소
            if (hideTooltipCoroutine != null)
            {
                StopCoroutine(hideTooltipCoroutine);
                hideTooltipCoroutine = null;
            }
            isHidingTooltip = false;

            // 표시 코루틴 시작 (이미 실행 중이면 재시작하지 않음)
            if (!isShowingTooltip)
            {
                isShowingTooltip = true;
                if (showTooltipCoroutine != null)
                {
                    StopCoroutine(showTooltipCoroutine);
                }
                showTooltipCoroutine = StartCoroutine(ShowTooltipCoroutine());
            }
        }

        /// <summary>
        /// 효과에서 마우스가 이탈했을 때 호출됩니다
        /// </summary>
        public void OnEffectHoverExit()
        {
            if (hoveredEffect == null) return;
            
            hoveredEffect = null;
            
            // 표시 코루틴 취소
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            isShowingTooltip = false;

            // 초기화 대기 중이던 첫 호버도 해제
            pendingEffect = null;
            pendingShow = false;
            pendingShowPosition = Vector2.zero;
            pendingTargetRect = null;

            // 숨김 코루틴 시작
            if (!isHidingTooltip)
            {
                isHidingTooltip = true;
                if (hideTooltipCoroutine != null)
                {
                    StopCoroutine(hideTooltipCoroutine);
                }
                hideTooltipCoroutine = StartCoroutine(HideTooltipCoroutine());
            }
        }

        /// <summary>
        /// 버프/디버프 툴팁을 표시합니다.
        /// </summary>
        /// <param name="effect">표시할 효과</param>
        /// <param name="position">슬롯 위치</param>
        /// <param name="targetRect">대상 슬롯의 RectTransform (선택적, 전달되면 우선 사용)</param>
        public void ShowBuffDebuffTooltip(IPerTurnEffect effect, Vector3 position, RectTransform targetRect = null)
        {
            if (targetRect != null)
            {
                currentTargetRect = targetRect;
            }
            OnEffectHoverEnter(effect, position);
        }

        /// <summary>
        /// 버프/디버프 툴팁을 숨깁니다
        /// </summary>
        public void HideBuffDebuffTooltip()
        {
            OnEffectHoverExit();
        }

        #endregion

        #region Timer Management (Coroutine-based)

        /// <summary>
        /// 툴팁 표시 타이머 코루틴입니다.
        /// </summary>
        private IEnumerator ShowTooltipCoroutine()
        {
            yield return new WaitForSeconds(showDelay);
            
            // 코루틴이 취소되지 않았고 여전히 표시해야 하는 경우에만 실행
            if (isShowingTooltip && hoveredEffect != null)
            {
                ShowTooltip();
            }
            
            isShowingTooltip = false;
            showTooltipCoroutine = null;
        }

        /// <summary>
        /// 툴팁 숨김 타이머 코루틴입니다.
        /// </summary>
        private IEnumerator HideTooltipCoroutine()
        {
            yield return new WaitForSeconds(hideDelay);
            
            // 코루틴이 취소되지 않았고 여전히 숨겨야 하는 경우에만 실행
            if (isHidingTooltip)
            {
                HideTooltip();
            }
            
            isHidingTooltip = false;
            hideTooltipCoroutine = null;
        }

        /// <summary>
        /// 대상 유효성 검사를 수행합니다.
        /// </summary>
        private void ValidateTarget()
        {
            if (currentTooltip == null || currentTooltip.gameObject == null || !currentTooltip.gameObject.activeInHierarchy)
                return;

            bool targetValid = currentTargetRect != null && currentTargetRect && currentTargetRect.gameObject.activeInHierarchy;
            if (!targetValid)
            {
                ForceHideTooltip();
                return;
            }

            if (hoveredEffect != null && hoveredEffect.IsExpired)
            {
                ForceHideTooltip();
            }
        }

        /// <summary>
        /// 툴팁을 표시합니다
        /// </summary>
        public void ShowTooltip()
        {
            GameLogger.LogInfo($"[BuffDebuffTooltipManager] ShowTooltip 호출됨 - currentTooltip: {currentTooltip != null}, hoveredEffect: {hoveredEffect?.GetType().Name}", GameLogger.LogCategory.UI);
            
            // 대상 캔버스를 찾지 못하면 표시를 건너뜁니다
            var targetCanvas = GetCanvasOfCurrentTarget();
            if (targetCanvas == null)
            {
                GameLogger.LogWarning("[BuffDebuffTooltipManager] 대상 캔버스를 찾을 수 없습니다. 툴팁 표시를 건너뜁니다", GameLogger.LogCategory.UI);
                return;
            }
            
			if (currentTooltip == null)
            {
				CreateTooltipInstance();
				if (currentTooltip == null) return;
            }
            else
            {
                // 기존 인스턴스를 캔버스 최상위로 이동 (다른 UI 요소 위에 표시)
                var canvas = targetCanvas;
                if (canvas != null && currentTooltip.transform.parent != canvas.transform)
                {
                    currentTooltip.transform.SetParent(canvas.transform, false);
                }
                // 항상 최상단으로 이동 (다른 UI 뒤에 숨지 않도록)
                currentTooltip.transform.SetAsLastSibling();
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
                    // currentTargetRect를 툴팁에 전달하여 정확한 위치 계산
                    currentTooltip.ShowTooltip(hoveredEffect, effectPosition, currentTargetRect);
                    
                    // 툴팁이 항상 최상단에 렌더링되도록 보장
                    if (currentTooltip.transform.parent != null)
                    {
                        currentTooltip.transform.SetAsLastSibling();
                    }
                    
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
        /// 툴팁을 숨깁니다
        /// </summary>
        public void HideTooltip()
        {
            if (currentTooltip != null)
            {
                currentTooltip.HideTooltip();
            }
        }

        /// <summary>
        /// 툴팁을 강제로 숨깁니다
        /// </summary>
        public void ForceHideTooltip()
        {
            if (currentTooltip != null)
            {
                currentTooltip.HideTooltip();
                // 툴팁을 완전히 비활성화하여 다음 프레임 체크 방지
                if (currentTooltip.gameObject != null)
                {
                    currentTooltip.gameObject.SetActive(false);
                }
            }

            hoveredEffect = null;
            currentTargetRect = null;
            
            // 코루틴 정리
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            if (hideTooltipCoroutine != null)
            {
                StopCoroutine(hideTooltipCoroutine);
                hideTooltipCoroutine = null;
            }
            
            isShowingTooltip = false;
            isHidingTooltip = false;
        }

        /// <summary>
        /// 현재 효과의 위치를 가져옵니다. (좌하단 기준 스크린 좌표)
        /// </summary>
        /// <returns>효과 위치 (스크린 좌표)</returns>
        private Vector2 GetCurrentEffectPosition()
        {
            if (hoveredEffect == null) return Vector2.zero;

            // currentTargetRect가 있으면 직접 사용
            RectTransform slotRect = currentTargetRect;
            if (slotRect == null)
            {
                // 현재 호버된 효과의 슬롯을 찾아서 위치 반환
                var slotViews = Object.FindObjectsByType<BuffDebuffSlotView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var slotView in slotViews)
                {
                    if (slotView != null && slotView.CurrentEffect == hoveredEffect)
                    {
                        slotRect = slotView.transform as RectTransform;
                        if (slotRect != null)
                            break;
                    }
                }
            }

            if (slotRect != null)
            {
                // 슬롯이 속한 캔버스와 카메라를 확인
                var sourceCanvas = slotRect.GetComponentInParent<Canvas>();
                Camera cam = null;
                if (sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    cam = sourceCanvas.worldCamera;
                }

                // 슬롯의 왼쪽-아래 모서리를 계산: GetWorldCorners 사용 (하단 정렬용)
                Vector3[] corners = new Vector3[4];
                slotRect.GetWorldCorners(corners); // 0:BL, 1:TL, 2:TR, 3:BR
                Vector3 bottomLeftWorld = corners[0]; // BL = Bottom Left
                Vector2 screenBL = RectTransformUtility.WorldToScreenPoint(cam, bottomLeftWorld);
                return screenBL;
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
