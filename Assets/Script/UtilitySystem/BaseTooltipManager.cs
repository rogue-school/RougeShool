using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Game.ItemSystem.Constants;
using System.Collections;
using System;

namespace Game.UtilitySystem
{
    /// <summary>
    /// 모든 툴팁 매니저의 공통 기능을 제공하는 베이스 클래스입니다.
    /// 제네릭을 사용하여 타입 안전성을 보장합니다.
    /// </summary>
    /// <typeparam name="TTooltip">툴팁 UI 컴포넌트 타입</typeparam>
    public abstract class BaseTooltipManager<TTooltip> : MonoBehaviour, ICoreSystemInitializable
        where TTooltip : MonoBehaviour
    {
        #region Serialized Fields

        [Header("툴팁 설정")]
        [Tooltip("툴팁 프리팹")]
        [SerializeField] protected TTooltip tooltipPrefab;
        
        [Header("Addressables 설정")]
        [Tooltip("Addressables에서 프리팹을 로드할 키 (Inspector에서 할당되지 않은 경우)")]
        [SerializeField] protected string addressablesKey;

        #endregion

        #region Protected Fields

        /// <summary>
        /// 현재 활성화된 툴팁 인스턴스
        /// </summary>
        protected TTooltip currentTooltip;

        /// <summary>
        /// 툴팁 표시 지연 시간
        /// </summary>
        protected float showDelay;

        /// <summary>
        /// 툴팁 숨김 지연 시간
        /// </summary>
        protected float hideDelay;

        /// <summary>
        /// 툴팁 표시 코루틴
        /// </summary>
        protected Coroutine showTooltipCoroutine;

        /// <summary>
        /// 툴팁 숨김 코루틴
        /// </summary>
        protected Coroutine hideTooltipCoroutine;

        /// <summary>
        /// 툴팁 표시 중 여부
        /// </summary>
        protected bool isShowingTooltip;

        /// <summary>
        /// 툴팁 숨김 중 여부
        /// </summary>
        protected bool isHidingTooltip;

        /// <summary>
        /// 이벤트 시스템
        /// </summary>
        protected EventSystem eventSystem;

        #endregion

        #region Public Properties

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized { get; protected set; } = false;

        /// <summary>
        /// 현재 활성화된 툴팁 인스턴스
        /// </summary>
        public TTooltip CurrentTooltip => currentTooltip;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeComponents();
        }

        protected virtual void Update()
        {
            if (!IsInitialized) return;
            UpdateTooltipPosition();
        }

        protected virtual void OnDestroy()
        {
            CleanupCoroutines();
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
        public virtual IEnumerator Initialize()
        {
            yield return InitializeTooltipSystem();
            IsInitialized = true;
        }

        /// <summary>
        /// 초기화 실패 시 호출됩니다.
        /// </summary>
        public virtual void OnInitializationFailed()
        {
            GameLogger.LogError($"{GetType().Name} 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 컴포넌트들을 초기화합니다.
        /// </summary>
        protected virtual void InitializeComponents()
        {
            showDelay = ItemConstants.TOOLTIP_SHOW_DELAY;
            hideDelay = ItemConstants.TOOLTIP_HIDE_DELAY;
            
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                GameLogger.LogError($"[{GetType().Name}] EventSystem을 찾을 수 없습니다", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 툴팁 시스템을 초기화합니다.
        /// </summary>
        protected virtual IEnumerator InitializeTooltipSystem()
        {
            // Inspector에서 할당되지 않았으면 Addressables에서 로드
            if (tooltipPrefab == null && !string.IsNullOrEmpty(addressablesKey))
            {
                GameLogger.LogInfo($"[{GetType().Name}] Inspector에서 프리팹이 할당되지 않았습니다. Addressables에서 로드를 시도합니다.", GameLogger.LogCategory.UI);
                
                var handle = Addressables.LoadAssetAsync<TTooltip>(addressablesKey);
                yield return handle;
                
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    tooltipPrefab = handle.Result;
                    GameLogger.LogInfo($"[{GetType().Name}] Addressables에서 툴팁 프리팹 로드 성공", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogError($"[{GetType().Name}] 툴팁 프리팹을 찾을 수 없습니다. Addressables에 '{addressablesKey}' 키로 프리팹을 등록하거나 Inspector에서 할당해주세요.", GameLogger.LogCategory.Error);
                    yield break;
                }
            }

            if (tooltipPrefab == null)
            {
                GameLogger.LogError($"[{GetType().Name}] 툴팁 프리팹이 null입니다. Inspector에서 할당하거나 Addressables 키를 설정해주세요.", GameLogger.LogCategory.Error);
                yield break;
            }

            yield return null;
        }

        #endregion

        #region Coroutine Management

        /// <summary>
        /// 코루틴을 정리합니다.
        /// </summary>
        protected virtual void CleanupCoroutines()
        {
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
        }

        /// <summary>
        /// 툴팁 표시 코루틴을 시작합니다.
        /// </summary>
        protected virtual void StartShowTooltipCoroutine()
        {
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
            }
            showTooltipCoroutine = StartCoroutine(ShowTooltipCoroutine());
        }

        /// <summary>
        /// 툴팁 숨김 코루틴을 시작합니다.
        /// </summary>
        protected virtual void StartHideTooltipCoroutine()
        {
            if (hideTooltipCoroutine != null)
            {
                StopCoroutine(hideTooltipCoroutine);
            }
            hideTooltipCoroutine = StartCoroutine(HideTooltipCoroutine());
        }

        /// <summary>
        /// 툴팁 표시 코루틴
        /// </summary>
        protected virtual IEnumerator ShowTooltipCoroutine()
        {
            if (isShowingTooltip) yield break;
            
            isShowingTooltip = true;
            yield return new WaitForSeconds(showDelay);
            
            if (ShouldShowTooltip())
            {
                ShowTooltip();
            }
            
            isShowingTooltip = false;
            showTooltipCoroutine = null;
        }

        /// <summary>
        /// 툴팁 숨김 코루틴
        /// </summary>
        protected virtual IEnumerator HideTooltipCoroutine()
        {
            if (isHidingTooltip) yield break;
            
            isHidingTooltip = true;
            yield return new WaitForSeconds(hideDelay);
            
            if (ShouldHideTooltip())
            {
                HideTooltip();
            }
            
            isHidingTooltip = false;
            hideTooltipCoroutine = null;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// 툴팁을 표시해야 하는지 확인합니다.
        /// </summary>
        protected abstract bool ShouldShowTooltip();

        /// <summary>
        /// 툴팁을 숨겨야 하는지 확인합니다.
        /// </summary>
        protected abstract bool ShouldHideTooltip();

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        protected abstract void ShowTooltip();

        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        protected abstract void HideTooltip();

        /// <summary>
        /// 툴팁 위치를 업데이트합니다.
        /// </summary>
        protected abstract void UpdateTooltipPosition();

        /// <summary>
        /// 다른 툴팁 매니저의 툴팁을 숨깁니다.
        /// </summary>
        protected abstract void HideOtherTooltips();

        /// <summary>
        /// 툴팁 인스턴스를 생성합니다.
        /// </summary>
        protected abstract void CreateTooltipInstance();

        #endregion

        #region Public Methods

        /// <summary>
        /// 툴팁을 강제로 숨깁니다.
        /// </summary>
        public virtual void ForceHideTooltip()
        {
            CleanupCoroutines();
            HideTooltip();
        }

        #endregion
    }
}

