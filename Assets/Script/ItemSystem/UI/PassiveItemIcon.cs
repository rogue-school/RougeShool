using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Data;

namespace Game.ItemSystem.UI
{
    /// <summary>
    /// 패시브 아이템 아이콘을 관리하는 컴포넌트입니다.
    /// 아이템 아이콘과 강화 단계 숫자를 표시합니다.
    /// </summary>
    public class PassiveItemIcon : MonoBehaviour
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
        private int enhancementLevel = 1;
        private PassiveItemDefinition itemDefinition;

        private Tween fadeTween;
        private Tween scaleTween;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeIcon();
        }

        private void OnDestroy()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
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
        /// <param name="enhancementLevel">강화 단계 (1-3)</param>
        public void SetupIcon(PassiveItemDefinition itemDefinition, int enhancementLevel = 1)
        {
            if (itemDefinition == null)
            {
                GameLogger.LogError("[PassiveItemIcon] 패시브 아이템 정의가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            this.itemDefinition = itemDefinition;
            this.itemId = itemDefinition.ItemId;
            this.enhancementLevel = Mathf.Clamp(enhancementLevel, 1, 3);

            if (iconImage != null && itemDefinition.Icon != null)
                iconImage.sprite = itemDefinition.Icon;

            UpdateEnhancementLevelText();
            SetIconColor();

            FadeIn();

            GameLogger.LogInfo($"[PassiveItemIcon] 패시브 아이템 아이콘 설정: {itemDefinition.DisplayName}, 강화 단계: {enhancementLevel}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 강화 단계 텍스트를 업데이트합니다.
        /// </summary>
        private void UpdateEnhancementLevelText()
        {
            if (enhancementLevelText != null)
            {
                enhancementLevelText.text = enhancementLevel.ToString();
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
        /// <param name="newLevel">새로운 강화 단계 (1-3)</param>
        public void UpdateEnhancementLevel(int newLevel)
        {
            enhancementLevel = Mathf.Clamp(newLevel, 1, 3);
            UpdateEnhancementLevelText();
            GameLogger.LogInfo($"[PassiveItemIcon] 강화 단계 업데이트: {itemId} → {enhancementLevel}", GameLogger.LogCategory.UI);
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
            scaleTween?.Kill();
            scaleTween = transform.DOScale(hoverScale, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);
        }

        /// <summary>
        /// 마우스 호버 종료 시 호출됩니다.
        /// </summary>
        public void OnMouseExit()
        {
            scaleTween?.Kill();
            scaleTween = transform.DOScale(1f, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);
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
        /// <returns>강화 단계 (1-3)</returns>
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

