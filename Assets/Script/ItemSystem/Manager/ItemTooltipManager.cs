using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Game.ItemSystem.Data;
using Game.ItemSystem.UI;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Game.ItemSystem.Constants;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Game.SkillCardSystem.Manager;
using Game.CharacterSystem.Manager;
using Zenject;

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
        [Tooltip("툴팁 프리팹 Addressables 주소")]
        [SerializeField] private string tooltipPrefabAddress = "ItemTooltip.prefab";
        [Tooltip("툴팁 프리팹 (Inspector 할당용, Addressables 로드 실패 시 폴백)")]
        [SerializeField] private GameObject tooltipPrefab;
        
        // 툴팁 지연 시간은 ItemConstants에서 관리 (코드로 제어)
        private float showDelay;
        private float activeItemShowDelay; // 엑티브 아이템 전용 지연 시간
        private float hideDelay;

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
        private PassiveItemDefinition hoveredPassiveItem;
        private int hoveredPassiveItemEnhancementLevel = 0;
        private bool hoveredPassiveItemIsRewardPanel = false; // 보상창 컨텍스트인지 여부
        private RectTransform currentTargetRect;
        private bool pendingShow;
        private ActiveItemDefinition pendingItem;
        private PassiveItemDefinition pendingPassiveItem;
        private int pendingPassiveItemEnhancementLevel = 1;

        private Coroutine showTooltipCoroutine;
        private Coroutine hideTooltipCoroutine;
        private bool isShowingTooltip;
        private bool isHidingTooltip;
        private bool isPinned; // 팝업 등으로 고정된 동안 숨기지 않음
        private ActiveItemDefinition pinnedItem;
        private PassiveItemDefinition pinnedPassiveItem;
        private int pinnedPassiveItemEnhancementLevel = 1;
        private RectTransform pinnedRect;

        private EventSystem eventSystem;
        private System.Collections.Generic.Dictionary<ActiveItemDefinition, RectTransform> itemUICache = new();
        private System.Collections.Generic.Dictionary<PassiveItemDefinition, RectTransform> passiveItemUICache = new();
        
        private AsyncOperationHandle<GameObject> _loadOperation;
        
        [Inject(Optional = true)]
        private Game.ItemSystem.Service.ItemService itemService;
        
        [Inject(Optional = true)]
        private SkillCardTooltipManager skillCardTooltipManager;
        
        [Inject(Optional = true)]
        private BuffDebuffTooltipManager buffDebuffTooltipManager;

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
            
            // 모든 씬에서 사용 가능하도록 DontDestroyOnLoad 설정
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            if (!IsInitialized) return;

            // 고정 대상 유효성 검사 및 위치 업데이트
            ValidateTargetAndUpdatePosition();
        }

        private void OnDestroy()
        {
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
            
            // 이벤트 구독 해제
            if (itemService != null)
            {
                itemService.OnEnhancementUpgraded -= OnEnhancementUpgradedHandler;
            }

            // Addressables 리소스 해제
            if (_loadOperation.IsValid())
            {
                Addressables.Release(_loadOperation);
            }

            if (currentTooltip != null && currentTooltip.gameObject != null)
            {
                Destroy(currentTooltip.gameObject);
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
            // 툴팁 지연 시간을 상수에서 초기화
            showDelay = ItemConstants.TOOLTIP_SHOW_DELAY;
            activeItemShowDelay = ItemConstants.ACTIVE_ITEM_TOOLTIP_SHOW_DELAY;
            hideDelay = ItemConstants.TOOLTIP_HIDE_DELAY;
            
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
            
            // tooltipPrefab이 없으면 Addressables에서 로드 시도
            if (tooltipPrefab == null && !string.IsNullOrEmpty(tooltipPrefabAddress))
            {
                GameLogger.LogInfo($"[ItemTooltipManager] Addressables에서 툴팁 프리팹 로드 시도: {tooltipPrefabAddress}", GameLogger.LogCategory.UI);
                _loadOperation = Addressables.LoadAssetAsync<GameObject>(tooltipPrefabAddress);
                yield return _loadOperation;

                if (_loadOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    tooltipPrefab = _loadOperation.Result;
                    GameLogger.LogInfo($"[ItemTooltipManager] 툴팁 프리팹 로드 성공: {tooltipPrefab.name}", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogError($"[ItemTooltipManager] Addressables에서 툴팁 프리팹 로드 실패: {tooltipPrefabAddress} - {_loadOperation.OperationException?.Message}", GameLogger.LogCategory.Error);
                }
            }
            
            if (tooltipPrefab == null)
            {
                GameLogger.LogError("[ItemTooltipManager] 툴팁 프리팹이 할당되지 않았습니다. Inspector 또는 Addressables 설정을 확인해주세요.", GameLogger.LogCategory.Error);
                OnInitializationFailed();
                yield break;
            }

            // ItemService 이벤트 구독
            if (itemService != null)
            {
                itemService.OnEnhancementUpgraded += OnEnhancementUpgradedHandler;
                GameLogger.LogInfo("[ItemTooltipManager] ItemService 이벤트 구독 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[ItemTooltipManager] ItemService를 찾을 수 없습니다. 강화 레벨 업데이트가 작동하지 않을 수 있습니다.", GameLogger.LogCategory.UI);
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
        /// 패시브 아이템 UI를 등록합니다.
        /// </summary>
        /// <param name="item">패시브 아이템 데이터</param>
        /// <param name="itemTransform">아이템 UI의 RectTransform</param>
        public void RegisterPassiveItemUI(PassiveItemDefinition item, RectTransform itemTransform)
        {
            if (item == null || itemTransform == null) return;
            passiveItemUICache[item] = itemTransform;
        }

        /// <summary>
        /// 아이템 UI 등록을 해제합니다.
        /// </summary>
        /// <param name="item">아이템 데이터</param>
        public void UnregisterItemUI(ActiveItemDefinition item)
        {
            if (item == null) return;
            itemUICache.Remove(item);

            // 현재 표시/대기/고정 대상이 사라지면 즉시 숨김 처리
            if (hoveredItem == item || pendingItem == item || (isPinned && pinnedItem == item))
            {
                ForceHideTooltip();
            }
        }

        /// <summary>
        /// 패시브 아이템 UI 등록을 해제합니다.
        /// </summary>
        /// <param name="item">패시브 아이템 데이터</param>
        public void UnregisterPassiveItemUI(PassiveItemDefinition item)
        {
            if (item == null) return;
            passiveItemUICache.Remove(item);

            // 현재 표시/대기/고정 대상이 사라지면 즉시 숨김 처리
            if (hoveredPassiveItem == item || pendingPassiveItem == item || (isPinned && pinnedPassiveItem == item))
            {
                ForceHideTooltip();
            }
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

            // 다른 아이템으로 전환하는 경우 (이미 툴팁이 표시 중이고 다른 아이템인 경우)
            bool isSwitchingItems = hoveredItem != null && hoveredItem != item && currentTooltip != null && currentTooltip.gameObject != null && currentTooltip.gameObject.activeInHierarchy;

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

            // 숨김 코루틴 취소 (다른 슬롯으로 전환 시)
            if (hideTooltipCoroutine != null)
            {
                StopCoroutine(hideTooltipCoroutine);
                hideTooltipCoroutine = null;
            }
            isHidingTooltip = false;

            // 다른 아이템으로 전환하는 경우 즉시 표시
            if (isSwitchingItems)
            {
                // 표시 코루틴 취소
                if (showTooltipCoroutine != null)
                {
                    StopCoroutine(showTooltipCoroutine);
                    showTooltipCoroutine = null;
                }
                isShowingTooltip = false;
                ShowTooltip(); // 즉시 새 아이템 툴팁 표시
            }
            else if (!isShowingTooltip)
            {
                isShowingTooltip = true;
                if (showTooltipCoroutine != null)
                {
                    StopCoroutine(showTooltipCoroutine);
                }
                // 엑티브 아이템인 경우 더 긴 지연 시간 사용
                float currentDelay = hoveredItem != null ? activeItemShowDelay : showDelay;
                showTooltipCoroutine = StartCoroutine(ShowTooltipCoroutine(currentDelay));
            }
        }

        /// <summary>
        /// 아이템에서 마우스가 이탈했을 때 호출됩니다
        /// </summary>
        public void OnItemHoverExit()
        {
            if (hoveredItem == null && hoveredPassiveItem == null) return;

            if (isPinned)
            {
                // 고정 상태에서는 숨김 코루틴을 시작하지 않고 상태만 초기화
                hoveredItem = null;
                hoveredPassiveItem = null;
                if (showTooltipCoroutine != null)
                {
                    StopCoroutine(showTooltipCoroutine);
                    showTooltipCoroutine = null;
                }
                isShowingTooltip = false;
                return;
            }

            hoveredItem = null;
            hoveredPassiveItem = null;
            
            // 표시 코루틴 취소
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            isShowingTooltip = false;

            pendingItem = null;
            pendingPassiveItem = null;
            pendingShow = false;

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
        /// 패시브 아이템에 마우스가 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="item">호버된 패시브 아이템</param>
        /// <param name="enhancementLevel">강화 단계 (0-3, 0 = 강화 안됨)</param>
        /// <param name="isRewardPanel">보상창 컨텍스트인지 여부 (true = 보상창, false = 패시브 컨테이너)</param>
        public void OnPassiveItemHoverEnter(PassiveItemDefinition item, int enhancementLevel = 0, bool isRewardPanel = false)
        {
            OnPassiveItemHoverEnter(item, null, enhancementLevel, isRewardPanel);
        }

        /// <summary>
        /// 패시브 아이템에 마우스가 진입했을 때 호출됩니다. (호버 소스 Rect 우선)
        /// </summary>
        /// <param name="item">호버된 패시브 아이템</param>
        /// <param name="sourceRect">호버를 발생시킨 UI의 RectTransform</param>
        /// <param name="enhancementLevel">강화 단계 (0-3, 0 = 강화 안됨)</param>
        /// <param name="isRewardPanel">보상창 컨텍스트인지 여부 (true = 보상창, false = 패시브 컨테이너)</param>
        public void OnPassiveItemHoverEnter(PassiveItemDefinition item, RectTransform sourceRect, int enhancementLevel = 0, bool isRewardPanel = false)
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
                pendingPassiveItem = item;
                pendingPassiveItemEnhancementLevel = enhancementLevel;
                pendingShow = true;
                StartCoroutine(ForceInitialize());
                return;
            }

            // 다른 패시브 아이템으로 전환하는 경우 (이미 툴팁이 표시 중이고 다른 아이템인 경우)
            bool isSwitchingItems = hoveredPassiveItem != null && hoveredPassiveItem != item && currentTooltip != null && currentTooltip.gameObject != null && currentTooltip.gameObject.activeInHierarchy;

            hoveredPassiveItem = item;
            hoveredPassiveItemEnhancementLevel = Mathf.Clamp(enhancementLevel, 0, Game.ItemSystem.Constants.ItemConstants.MAX_ENHANCEMENT_LEVEL);
            hoveredPassiveItemIsRewardPanel = isRewardPanel;
            hoveredItem = null;
            // 호버를 발생시킨 소스 Rect를 우선 사용
            currentTargetRect = sourceRect;
            if (currentTargetRect == null)
            {
                // 폴백: 캐시된 Rect 사용
                if (passiveItemUICache.TryGetValue(item, out var rt) && rt != null)
                {
                    currentTargetRect = rt;
                }
            }

            // 숨김 코루틴 취소 (다른 슬롯으로 전환 시)
            if (hideTooltipCoroutine != null)
            {
                StopCoroutine(hideTooltipCoroutine);
                hideTooltipCoroutine = null;
            }
            isHidingTooltip = false;

            // 다른 패시브 아이템으로 전환하는 경우 즉시 표시
            if (isSwitchingItems)
            {
                // 표시 코루틴 취소
                if (showTooltipCoroutine != null)
                {
                    StopCoroutine(showTooltipCoroutine);
                    showTooltipCoroutine = null;
                }
                isShowingTooltip = false;
                ShowTooltip(); // 즉시 새 패시브 아이템 툴팁 표시
            }
            else if (!isShowingTooltip)
            {
                isShowingTooltip = true;
                if (showTooltipCoroutine != null)
                {
                    StopCoroutine(showTooltipCoroutine);
                }
                // 패시브 아이템은 기본 지연 시간 사용
                showTooltipCoroutine = StartCoroutine(ShowTooltipCoroutine(showDelay));
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
            hoveredPassiveItem = null;
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
            isPinned = false;
            pinnedItem = null;
            pinnedPassiveItem = null;
            pinnedPassiveItemEnhancementLevel = 1;
            pinnedRect = null;
        }

        /// <summary>
        /// 툴팁이 특정 아이템에 고정되어 있을 때만 숨깁니다.
        /// 다른 슬롯을 클릭해 새 툴팁을 띄운 직후, 이전 슬롯 정리 과정에서 새 툴팁이 닫히는 것을 방지합니다.
        /// </summary>
        /// <param name="item">검사할 아이템</param>
        public void ForceHideIfPinnedTo(ActiveItemDefinition item)
        {
            if (item == null) return;
            if (isPinned && pinnedItem == item)
            {
                ForceHideTooltip();
            }
        }

        /// <summary>
        /// 다른 툴팁 매니저의 툴팁을 숨깁니다.
        /// </summary>
        private void HideOtherTooltips()
        {
            // 스킬카드 툴팁 숨김
            if (skillCardTooltipManager != null)
            {
                skillCardTooltipManager.ForceHideTooltip();
                GameLogger.LogInfo("[ItemTooltipManager] 다른 툴팁 숨김 (SkillCardTooltipManager)", GameLogger.LogCategory.UI);
            }

            // 버프/디버프 툴팁 숨김
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

                        if (pendingShow)
                        {
                            try
                            {
                                if (pendingItem != null)
                                {
                                    hoveredItem = pendingItem;
                                    pendingItem = null;
                                    currentTargetRect = null;
                                    if (hoveredItem != null && itemUICache.TryGetValue(hoveredItem, out var cachedRt) && cachedRt != null)
                                    {
                                        currentTargetRect = cachedRt;
                                    }
                                }
                                else if (pendingPassiveItem != null)
                                {
                                    hoveredPassiveItem = pendingPassiveItem;
                                    hoveredPassiveItemEnhancementLevel = pendingPassiveItemEnhancementLevel;
                                    pendingPassiveItem = null;
                                    currentTargetRect = null;
                                    if (hoveredPassiveItem != null && passiveItemUICache.TryGetValue(hoveredPassiveItem, out var cachedRt) && cachedRt != null)
                                    {
                                        currentTargetRect = cachedRt;
                                    }
                                }

                                pendingShow = false;

                                // 코루틴 정리
                                if (hideTooltipCoroutine != null)
                                {
                                    StopCoroutine(hideTooltipCoroutine);
                                    hideTooltipCoroutine = null;
                                }
                                if (showTooltipCoroutine != null)
                                {
                                    StopCoroutine(showTooltipCoroutine);
                                    showTooltipCoroutine = null;
                                }
                                isHidingTooltip = false;
                                isShowingTooltip = false;

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

        #region Event Handlers

        /// <summary>
        /// 강화 레벨 업그레이드 이벤트 핸들러
        /// </summary>
        private void OnEnhancementUpgradedHandler(string skillId, int newLevel)
        {
            // 현재 표시 중인 패시브 아이템 툴팁이 있고, 해당 스킬과 관련된 아이템인지 확인
            if (currentTooltip != null && currentTooltip.gameObject != null && currentTooltip.gameObject.activeInHierarchy)
            {
                if (hoveredPassiveItem != null)
                {
                    // 패시브 아이템의 대상 스킬 ID 계산
                    string targetSkillId = GetPassiveItemSkillId(hoveredPassiveItem);

                    if (targetSkillId == skillId)
                    {
                        // 강화 레벨 업데이트
                        hoveredPassiveItemEnhancementLevel = newLevel;
                        currentTooltip.UpdateEnhancementLevel(newLevel);
                        GameLogger.LogInfo($"[ItemTooltipManager] 툴팁 강화 레벨 실시간 업데이트: {hoveredPassiveItem.DisplayName} → {newLevel}", GameLogger.LogCategory.UI);
                    }
                }
            }
        }

        /// <summary>
        /// 패시브 아이템의 스킬 ID를 가져옵니다.
        /// </summary>
        private string GetPassiveItemSkillId(PassiveItemDefinition item)
        {
            if (item == null) return null;

            if (item.IsPlayerHealthBonus)
            {
                // 공용 체력 보너스는 아이템 ID를 키에 포함
                var itemKey = !string.IsNullOrEmpty(item.ItemId) ? item.ItemId : Guid.NewGuid().ToString();
                return $"__PLAYER_HP__:{itemKey}";
            }
            else if (item.TargetSkill != null)
            {
                return !string.IsNullOrEmpty(item.TargetSkill.displayName) 
                    ? item.TargetSkill.displayName 
                    : item.TargetSkill.cardId;
            }

            return null;
        }

        /// <summary>
        /// 현재 표시 중인 패시브 아이템 툴팁의 강화 레벨을 업데이트합니다.
        /// 보상창에서 아이템을 선택했을 때 호출됩니다.
        /// </summary>
        /// <param name="item">업데이트할 패시브 아이템</param>
        public void RefreshPassiveItemTooltip(PassiveItemDefinition item)
        {
            if (item == null || itemService == null) return;

            // 현재 표시 중인 툴팁이 패시브 아이템인지 확인
            if (currentTooltip != null && currentTooltip.gameObject != null && currentTooltip.gameObject.activeInHierarchy)
            {
                if (hoveredPassiveItem == null) return;

                // 스킬 ID로 비교 (더 정확한 매칭)
                string selectedSkillId = GetPassiveItemSkillId(item);
                string hoveredSkillId = GetPassiveItemSkillId(hoveredPassiveItem);

                // 스킬 ID가 같으면 같은 아이템으로 간주
                if (!string.IsNullOrEmpty(selectedSkillId) && selectedSkillId == hoveredSkillId)
                {
                    // 코루틴으로 약간의 지연을 주어 AddPassiveItem이 완료된 후 업데이트
                    StartCoroutine(RefreshPassiveItemTooltipDelayed(item));
                }
            }
        }

        /// <summary>
        /// 지연된 패시브 아이템 툴팁 업데이트 코루틴
        /// </summary>
        private System.Collections.IEnumerator RefreshPassiveItemTooltipDelayed(PassiveItemDefinition item)
        {
            // 한 프레임 대기하여 AddPassiveItem이 완료되도록 함
            yield return null;

            if (item == null || itemService == null) yield break;

            // 현재 강화 레벨 조회
            string skillId = GetPassiveItemSkillId(item);
            if (!string.IsNullOrEmpty(skillId))
            {
                int currentLevel = itemService.GetSkillEnhancementLevel(skillId);
                // 강화 레벨이 변경되었으면 업데이트 (0에서 1로 증가하는 경우 포함)
                if (currentLevel != hoveredPassiveItemEnhancementLevel)
                {
                    hoveredPassiveItemEnhancementLevel = currentLevel;
                    if (currentTooltip != null && currentTooltip.gameObject != null && currentTooltip.gameObject.activeInHierarchy)
                    {
                        currentTooltip.UpdateEnhancementLevel(currentLevel);
                        GameLogger.LogInfo($"[ItemTooltipManager] 보상창 선택 후 툴팁 강화 레벨 업데이트: {item.DisplayName} → {currentLevel}", GameLogger.LogCategory.UI);
                    }
                }
            }
        }

        #endregion

        #region Timer Management (Coroutine-based)

        /// <summary>
        /// 툴팁 표시 타이머 코루틴입니다.
        /// </summary>
        /// <param name="delay">지연 시간 (엑티브 아이템인 경우 더 긴 시간 사용)</param>
        private IEnumerator ShowTooltipCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // 코루틴이 취소되지 않았고 여전히 표시해야 하는 경우에만 실행
            if (isShowingTooltip && (hoveredItem != null || hoveredPassiveItem != null))
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
        /// 대상 유효성 검사 및 위치 업데이트를 수행합니다.
        /// </summary>
        private void ValidateTargetAndUpdatePosition()
        {
            // 고정 대상 유효성 검사: 보상 선택 등으로 대상 슬롯/오브젝트가 파괴되면 즉시 숨김
            if (isPinned)
            {
                bool pinnedValid = pinnedRect != null && pinnedRect && pinnedRect.gameObject.activeInHierarchy;
                if (!pinnedValid)
                {
                    ForceHideTooltip();
                    return;
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

        #endregion

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

            RectTransform itemRect = currentTargetRect;
            
            if (hoveredItem != null)
            {
                if (itemRect == null)
                {
                    itemUICache.TryGetValue(hoveredItem, out itemRect);
                }
            }
            else if (hoveredPassiveItem != null)
            {
                if (itemRect == null)
                {
                    passiveItemUICache.TryGetValue(hoveredPassiveItem, out itemRect);
                }
            }
            else
            {
                return Vector2.zero;
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

        #region Tooltip Control

        /// <summary>
        /// 툴팁을 표시합니다
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
            
            try
            {
                Vector2 itemPosition = GetCurrentItemPosition();
                if (itemPosition != Vector2.zero)
                {
                    // 툴팁 표시 (액티브 아이템 또는 패시브 아이템)
                    if (hoveredItem != null)
                    {
                        currentTooltip.Show(hoveredItem);
                        GameLogger.LogInfo($"툴팁 표시: {hoveredItem.DisplayName}", GameLogger.LogCategory.UI);
                    }
                    else if (hoveredPassiveItem != null)
                    {
                        currentTooltip.Show(hoveredPassiveItem, hoveredPassiveItemEnhancementLevel, hoveredPassiveItemIsRewardPanel);
                        GameLogger.LogInfo($"패시브 아이템 툴팁 표시: {hoveredPassiveItem.DisplayName}, 강화 단계: {hoveredPassiveItemEnhancementLevel}, 보상창: {hoveredPassiveItemIsRewardPanel}", GameLogger.LogCategory.UI);
                    }
                    else
                    {
                        GameLogger.LogWarning("[ItemTooltipManager] hoveredItem과 hoveredPassiveItem이 모두 null입니다.", GameLogger.LogCategory.UI);
                        return;
                    }
                    
                    // 위치 업데이트 (Show 이후에 호출하여 오브젝트가 활성화된 후 위치 설정)
                    UpdateTooltipPosition(itemPosition);
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
        /// 툴팁을 숨깁니다
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
        /// 툴팁을 고정합니다 (팝업이 열리는 동안 유지)
        /// </summary>
        public void PinTooltip()
        {
            isPinned = true;
            pinnedItem = hoveredItem;
            pinnedPassiveItem = hoveredPassiveItem;
            pinnedPassiveItemEnhancementLevel = hoveredPassiveItemEnhancementLevel;
            pinnedRect = currentTargetRect;
        }

        /// <summary>
        /// 툴팁 고정을 해제합니다.
        /// </summary>
        public void UnpinTooltip()
        {
            isPinned = false;
            pinnedItem = null;
            pinnedPassiveItem = null;
            pinnedPassiveItemEnhancementLevel = 1;
            pinnedRect = null;
        }

        /// <summary>
        /// 특정 아이템/Rect 기준으로 툴팁을 고정합니다
        /// </summary>
        /// <param name="item">고정할 아이템 정의</param>
        /// <param name="rect">대상 RectTransform</param>
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
                // 패시브 아이템인지 확인
                bool isPassiveItem = hoveredPassiveItem != null;
                
                // 패시브 아이템은 상단 정렬, 액티브 아이템은 하단 정렬
                if (isPassiveItem)
                {
                    // 상단 정렬: pivot을 좌상단(0, 1)으로 설정
                    rectTransform.pivot = new Vector2(0f, 1f);
                }
                else
                {
                    // 하단 정렬: pivot을 좌하단(0, 0)으로 설정
                    rectTransform.pivot = new Vector2(0f, 0f);
                }

                var canvasRect = targetParent.rect;
                var tooltipRect = rectTransform.rect;

                float tooltipWidth = Mathf.Abs(tooltipRect.width);
                float tooltipHeight = Mathf.Abs(tooltipRect.height);

                // 아이템 폭 계산 (기본값 100)
                float itemWidth = 100f;
                if (currentTargetRect != null)
                {
                    itemWidth = Mathf.Abs(currentTargetRect.rect.width);
                }

                float itemRightEdge = localPoint.x + itemWidth;
                
                // 아이템 높이 계산
                float itemHeight = 100f;
                if (currentTargetRect != null)
                {
                    itemHeight = Mathf.Abs(currentTargetRect.rect.height);
                }

                float canvasLeft = canvasRect.xMin;
                float canvasRight = canvasRect.xMax;

                float rightSpace = canvasRight - itemRightEdge;
                float leftSpace = localPoint.x - canvasLeft;

                float requiredWidth = tooltipWidth + alignPaddingX;
                bool canShowRight = rightSpace >= requiredWidth;
                bool canShowLeft = leftSpace >= requiredWidth;

                Vector2 tooltipPosition = localPoint;

                // 수평 위치 결정 (우측 우선, 부족 시 좌측 폴백)
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

                // 수직 정렬: 패시브 아이템은 상단 정렬, 액티브 아이템은 하단 정렬
                if (isPassiveItem)
                {
                    // 상단 정렬: 아이템 상단과 툴팁 상단을 일치
                    tooltipPosition.y = localPoint.y + itemHeight + alignPaddingY;
                }
                else
                {
                    // 하단 정렬: 아이템 하단과 툴팁 하단을 일치
                    tooltipPosition.y = localPoint.y + alignPaddingY;
                }

                // 다른 UI 위에 표시
                rectTransform.SetAsLastSibling();

                // 좌우 경계 클램프
                tooltipPosition = ClampToScreenBounds(tooltipPosition, targetParent, isPassiveItem);

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
        private Vector2 ClampToScreenBounds(Vector2 position, RectTransform parentRect, bool isPassiveItem = false)
        {
            if (parentRect == null || currentTooltip == null) return position;

            var canvasRect = parentRect.rect;
            var tooltipRect = currentTooltip.GetComponent<RectTransform>().rect;

            // 좌우 경계 클램프
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

            // 패시브 아이템의 경우 상단 정렬이므로 상단 경계도 클램프
            if (isPassiveItem)
            {
                float maxY = canvasRect.yMax;
                float tooltipTop = position.y; // pivot.y = 1이므로 position.y가 상단
                if (tooltipTop > maxY)
                {
                    position.y = maxY;
                }
            }
            else
            {
                // 액티브 아이템은 하단 정렬이므로 하단 경계만 클램프 (기존 동작 유지)
            }

            return position;
        }

        #endregion
    }
}


