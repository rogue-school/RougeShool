using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Data;
using Game.ItemSystem.Manager;
using Zenject;

namespace Game.ItemSystem.UI
{
    /// <summary>
    /// 패시브 아이템 아이콘을 관리하는 컴포넌트입니다.
    /// 아이템 아이콘과 강화 단계 숫자를 표시합니다.
    /// </summary>
    public class PassiveItemIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Serialized Fields

        [Header("UI Components")]
        [Tooltip("아이콘 이미지")]
        [SerializeField] private Image iconImage;
        
        [Tooltip("강화 단계 텍스트")]
        [SerializeField] private TextMeshProUGUI enhancementLevelText;
        
        [Tooltip("아이콘 배경")]
        [SerializeField] private Image backgroundImage;
        
        [Tooltip("아이콘 테두리")]
        [SerializeField] private Image borderImage;

        [Header("색상 설정")]
        [Tooltip("패시브 아이템 아이콘 색상")]
        [SerializeField] private Color passiveItemColor = new Color(1f, 0.84f, 0f); // 금색
        
        [Tooltip("기본 아이콘 색상")]
        [SerializeField] private Color defaultColor = Color.white;

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인/아웃 속도")]
        [SerializeField] private float fadeSpeed = 2f;
        
        [Tooltip("호버 시 스케일")]
        [SerializeField] private float hoverScale = 1.2f;

        #endregion

        #region Private Fields

        private string itemId;
        private int enhancementLevel = 0; // 0 = 강화 안됨, 1-3 = 강화 단계
        private PassiveItemDefinition itemDefinition;

        private Tween fadeTween;
        private Tween scaleTween;

        [Inject(Optional = true)]
        private ItemTooltipManager tooltipManager;
        private RectTransform rectTransform;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeIcon();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            // tooltipManager는 DI로 주입받음
            if (tooltipManager == null)
            {
                GameLogger.LogWarning("[PassiveItemIcon] ItemTooltipManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// tooltipManager가 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsureTooltipManagerInjected()
        {
            if (tooltipManager != null) return;

            try
            {
                // 1. ProjectContext를 통해 Container에 접근하여 주입 시도
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    projectContext.Container.Inject(this);
                    if (tooltipManager != null)
                    {
                        GameLogger.LogInfo("[PassiveItemIcon] ItemTooltipManager 주입 완료 (ProjectContext)", GameLogger.LogCategory.UI);
                        return;
                    }
                }

                // 2. SceneContextRegistry를 통해 현재 씬의 Container에 접근하여 주입 시도
                try
                {
                    var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                    var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                    if (sceneContainer != null)
                    {
                        sceneContainer.Inject(this);
                        if (tooltipManager != null)
                        {
                            GameLogger.LogInfo("[PassiveItemIcon] ItemTooltipManager 주입 완료 (SceneContext)", GameLogger.LogCategory.UI);
                            return;
                        }
                    }
                }
                catch
                {
                    // SceneContextRegistry를 찾을 수 없거나 씬 컨테이너가 없는 경우 무시
                }

                // 3. 직접 찾아서 할당 (최후의 수단)
                var foundManager = UnityEngine.Object.FindFirstObjectByType<ItemTooltipManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    tooltipManager = foundManager;
                    GameLogger.LogInfo("[PassiveItemIcon] ItemTooltipManager 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[PassiveItemIcon] ItemTooltipManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.UI);
            }
        }

        private void OnDisable()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = null;
            scaleTween = null;
        }

        private void OnDestroy()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = null;
            scaleTween = null;

            // 툴팁 매니저에서 등록 해제
            if (tooltipManager != null && itemDefinition != null)
            {
                tooltipManager.UnregisterPassiveItemUI(itemDefinition);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 아이콘 초기화
        /// </summary>
        private void InitializeIcon()
        {
            if (iconImage != null)
                iconImage.color = defaultColor;
            
            if (enhancementLevelText != null)
                enhancementLevelText.text = "";
            
            SetAlpha(0f);
        }

        /// <summary>
        /// 패시브 아이템 아이콘을 설정합니다.
        /// </summary>
        /// <param name="itemDefinition">패시브 아이템 정의</param>
        /// <param name="enhancementLevel">강화 단계 (0-3, 0 = 강화 안됨)</param>
        public void SetupIcon(PassiveItemDefinition itemDefinition, int enhancementLevel = 0)
        {
            if (itemDefinition == null)
            {
                GameLogger.LogError("[PassiveItemIcon] 패시브 아이템 정의가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            this.itemDefinition = itemDefinition;
            this.itemId = itemDefinition.ItemId;
            this.enhancementLevel = Mathf.Clamp(enhancementLevel, 0, Game.ItemSystem.Constants.ItemConstants.MAX_ENHANCEMENT_LEVEL);

            if (iconImage != null && itemDefinition.Icon != null)
                iconImage.sprite = itemDefinition.Icon;

            UpdateEnhancementLevelText();
            SetIconColor();

            FadeIn();

            // tooltipManager는 DI로 주입받음

            if (tooltipManager != null && rectTransform != null)
            {
                tooltipManager.RegisterPassiveItemUI(itemDefinition, rectTransform);
            }
        }

        /// <summary>
        /// 강화 단계 텍스트를 업데이트합니다.
        /// </summary>
        private void UpdateEnhancementLevelText()
        {
            if (enhancementLevelText != null)
            {
                // 0강일 때는 텍스트를 표시하지 않음
                if (enhancementLevel > 0)
                {
                    enhancementLevelText.text = enhancementLevel.ToString();
                }
                else
                {
                    enhancementLevelText.text = "";
                }
            }
        }

        /// <summary>
        /// 아이콘 색상을 설정합니다.
        /// </summary>
        private void SetIconColor()
        {
            if (iconImage == null) return;

            iconImage.color = passiveItemColor;

            if (backgroundImage != null)
            {
                Color bgColor = passiveItemColor;
                bgColor.a = 0.3f;
                backgroundImage.color = bgColor;
            }

            if (borderImage != null)
                borderImage.color = passiveItemColor;
        }

        /// <summary>
        /// 강화 단계를 업데이트합니다.
        /// </summary>
        /// <param name="newLevel">새로운 강화 단계 (0-3)</param>
        public void UpdateEnhancementLevel(int newLevel)
        {
            enhancementLevel = Mathf.Clamp(newLevel, 0, Game.ItemSystem.Constants.ItemConstants.MAX_ENHANCEMENT_LEVEL);
            UpdateEnhancementLevelText();
        }

        #endregion

        #region 애니메이션

        /// <summary>
        /// 페이드 인 애니메이션을 실행합니다.
        /// </summary>
        private void FadeIn()
        {
            fadeTween?.Kill();
            fadeTween = DOTween.To(() => GetAlpha(), x => SetAlpha(x), 1f, 1f / fadeSpeed)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);
        }

        /// <summary>
        /// 페이드 아웃 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="onComplete">완료 시 콜백</param>
        public void FadeOut(System.Action onComplete = null)
        {
            fadeTween?.Kill();
            fadeTween = DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0f, 1f / fadeSpeed)
                .SetEase(Ease.InQuad)
                .SetAutoKill(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 현재 알파값을 반환합니다.
        /// </summary>
        /// <returns>알파값</returns>
        private float GetAlpha()
        {
            if (iconImage != null)
                return iconImage.color.a;
            return 1f;
        }

        /// <summary>
        /// 알파값을 설정합니다.
        /// </summary>
        /// <param name="alpha">알파값</param>
        private void SetAlpha(float alpha)
        {
            if (iconImage != null)
            {
                Color color = iconImage.color;
                color.a = alpha;
                iconImage.color = color;
            }

            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = alpha * 0.3f;
                backgroundImage.color = color;
            }

            if (borderImage != null)
            {
                Color color = borderImage.color;
                color.a = alpha;
                borderImage.color = color;
            }

            if (enhancementLevelText != null)
            {
                Color color = enhancementLevelText.color;
                color.a = alpha;
                enhancementLevelText.color = color;
            }
        }

        #endregion

        #region 마우스 이벤트

        /// <summary>
        /// 마우스 호버 시 호출됩니다.
        /// </summary>
        public void OnMouseEnter()
        {
            Game.UtilitySystem.HoverEffectHelper.PlayHoverScaleWithCleanup(
                ref scaleTween,
                transform,
                hoverScale,
                0.2f);
        }

        /// <summary>
        /// 마우스 호버 종료 시 호출됩니다.
        /// </summary>
        public void OnMouseExit()
        {
            Game.UtilitySystem.HoverEffectHelper.ResetScaleWithCleanup(
                ref scaleTween,
                transform,
                0.2f);
        }

        /// <summary>
        /// 포인터가 UI 요소에 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter();

            if (itemDefinition == null)
                return;

            // tooltipManager가 null이면 찾기 시도
            EnsureTooltipManagerInjected();

            // 툴팁 표시
            if (tooltipManager != null)
            {
                tooltipManager.OnPassiveItemHoverEnter(itemDefinition, rectTransform, enhancementLevel);
            }
        }

        /// <summary>
        /// 포인터가 UI 요소에서 이탈했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit();

            // 툴팁 숨김
            if (tooltipManager != null)
            {
                tooltipManager.OnItemHoverExit();
            }
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 아이템 ID를 반환합니다.
        /// </summary>
        /// <returns>아이템 ID</returns>
        public string GetItemId()
        {
            return itemId;
        }

        /// <summary>
        /// 현재 강화 단계를 반환합니다.
        /// </summary>
        /// <returns>강화 단계 (0-3)</returns>
        public int GetEnhancementLevel()
        {
            return enhancementLevel;
        }

        /// <summary>
        /// 패시브 아이템 정의를 반환합니다.
        /// </summary>
        /// <returns>패시브 아이템 정의</returns>
        public PassiveItemDefinition GetItemDefinition()
        {
            return itemDefinition;
        }

        #endregion
    }
}

