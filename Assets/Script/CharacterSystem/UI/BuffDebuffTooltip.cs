using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.UI.Mappers;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 버프/디버프 툴팁을 표시하는 UI 컴포넌트입니다.
    /// 현재 남은 턴수와 효과 정보를 상세히 표시합니다.
    /// </summary>
    public class BuffDebuffTooltip : MonoBehaviour
    {
        #region Serialized Fields

        [Header("툴팁 배경")]
        [Tooltip("툴팁 배경 이미지")]
        [SerializeField] private Image backgroundImage;
        
        [Tooltip("툴팁 테두리 이미지")]
        [SerializeField] private Image borderImage;

        [Header("효과 헤더")]
        [Tooltip("효과 아이콘")]
        [SerializeField] private Image effectIconImage;
        
        [Tooltip("효과 이름 텍스트")]
        [SerializeField] private TextMeshProUGUI effectNameText;
        
        [Tooltip("효과 타입 텍스트")]
        [SerializeField] private TextMeshProUGUI effectTypeText;

        [Header("효과 정보")]
        [Tooltip("효과 설명 텍스트")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("턴 정보")]
        [Tooltip("남은 턴 텍스트")]
        [SerializeField] private TextMeshProUGUI remainingTurnsText;
        
        [Tooltip("턴 정보 컨테이너")]
        [SerializeField] private Transform turnInfoContainer;

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인 시간")]
        [SerializeField] private float fadeInDuration = 0.2f;
        
        [Tooltip("페이드 아웃 시간")]
        [SerializeField] private float fadeOutDuration = 0.15f;
        
        [Tooltip("애니메이션 이징")]
        [SerializeField] private Ease fadeEase = Ease.OutQuad;

        [Header("위치 설정")]
        [Tooltip("슬롯 오프셋 X")]
        [SerializeField] private float slotOffsetX = 20f;
        
        [Tooltip("슬롯 오프셋 Y")]
        [SerializeField] private float slotOffsetY = -20f;

        #endregion

        #region Private Fields

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        
        private Tween fadeTween;
        private bool isVisible = false;
        private IPerTurnEffect currentEffect;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            HideTooltip();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            rectTransform = GetComponent<RectTransform>();

            // 초기 상태 설정
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        /// <param name="effect">표시할 효과</param>
        /// <param name="slotPosition">슬롯의 위치</param>
        public void ShowTooltip(IPerTurnEffect effect, Vector2 slotPosition)
        {
            GameLogger.LogInfo($"[BuffDebuffTooltip] ShowTooltip 호출됨 - effect: {effect?.GetType().Name}, slotPosition: {slotPosition}, isVisible: {isVisible}", GameLogger.LogCategory.UI);
            
            if (effect == null)
            {
                GameLogger.LogWarning("[BuffDebuffTooltip] 표시할 효과가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            currentEffect = effect;
            UpdateTooltipContent(effect);
            
            // Layout 시스템이 적용되도록 한 프레임 대기 후 위치 계산
            StartCoroutine(ShowTooltipWithLayout(slotPosition));
        }

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        /// <param name="slotPosition">슬롯의 위치</param>
        private System.Collections.IEnumerator ShowTooltipWithLayout(Vector2 slotPosition)
        {
            yield return null;

            // 툴팁 위치 계산
            UpdateTooltipPosition(slotPosition);

            if (!isVisible)
            {
                GameLogger.LogInfo($"[BuffDebuffTooltip] 툴팁 페이드 인 시작: {currentEffect?.GetType().Name}", GameLogger.LogCategory.UI);
                FadeIn();
            }
            else
            {
                GameLogger.LogInfo("[BuffDebuffTooltip] 툴팁이 이미 표시 중입니다", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        public void HideTooltip()
        {
            if (isVisible)
            {
                FadeOut();
            }
        }

        /// <summary>
        /// 툴팁 위치를 업데이트합니다.
        /// </summary>
        /// <param name="slotPosition">슬롯 위치</param>
        public void UpdatePosition(Vector2 slotPosition)
        {
            if (isVisible)
            {
                UpdateTooltipPosition(slotPosition);
            }
        }

        #endregion

        #region Content Update

        /// <summary>
        /// 툴팁 내용을 업데이트합니다.
        /// </summary>
        /// <param name="effect">효과 정보</param>
        private void UpdateTooltipContent(IPerTurnEffect effect)
        {
            if (effect == null) return;

            // 툴팁 배경 및 보더 설정
            SetupTooltipBackground();

            // 효과 헤더 업데이트 (아이콘 + 이름 + 타입)
            UpdateEffectHeader(effect);

            // 효과 설명 업데이트
            UpdateEffectDescription(effect);

            // 턴 정보 업데이트
            UpdateTurnInfo(effect);
        }

        /// <summary>
        /// 툴팁 배경과 보더를 설정합니다.
        /// </summary>
        private void SetupTooltipBackground()
        {
            // 배경 이미지 설정
            if (backgroundImage != null)
            {
                if (backgroundImage.sprite != null)
                {
                    // 배경 색상 설정 (어두운 반투명)
                    backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    backgroundImage.gameObject.SetActive(true);
                }
                else
                {
                    CreateDefaultBackgroundSprite();
                    backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    backgroundImage.gameObject.SetActive(true);
                }
            }

            // 보더 이미지 설정
            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(true);

                if (borderImage.sprite != null)
                {
                    // 보더 색상 설정 (금색 테두리)
                    borderImage.color = new Color(1f, 0.8f, 0.2f, 1f);

                    if (borderImage.type == Image.Type.Sliced)
                    {
                        borderImage.fillCenter = false;
                    }
                    else
                    {
                        borderImage.type = Image.Type.Sliced;
                        borderImage.fillCenter = false;
                    }
                }
                else
                {
                    CreateDefaultBorderSprite();
                    borderImage.color = new Color(1f, 0.8f, 0.2f, 1f);
                    borderImage.type = Image.Type.Sliced;
                    borderImage.fillCenter = false;
                }
            }
        }

        /// <summary>
        /// 기본 배경 스프라이트를 생성합니다.
        /// </summary>
        private void CreateDefaultBackgroundSprite()
        {
            if (backgroundImage == null) return;

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            backgroundImage.sprite = sprite;
            backgroundImage.type = Image.Type.Sliced;
        }

        /// <summary>
        /// 기본 보더 스프라이트를 생성합니다.
        /// </summary>
        private void CreateDefaultBorderSprite()
        {
            if (borderImage == null) return;

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            borderImage.sprite = sprite;
            borderImage.type = Image.Type.Sliced;
        }

        /// <summary>
        /// 효과 헤더를 업데이트합니다. (아이콘 + 이름 + 타입)
        /// </summary>
        /// <param name="effect">효과</param>
        private void UpdateEffectHeader(IPerTurnEffect effect)
        {
            // 효과 아이콘 설정
            if (effectIconImage != null)
            {
                if (effect.Icon != null)
                {
                    effectIconImage.sprite = effect.Icon;
                    effectIconImage.color = Color.white;
                }
                else
                {
                    CreateDefaultEffectIcon();
                }
                effectIconImage.gameObject.SetActive(true);
            }

            // 효과 이름
            if (effectNameText != null)
            {
                string displayName = GetEffectDisplayName(effect);
                effectNameText.text = displayName;
            }

            // 효과 타입
            if (effectTypeText != null)
            {
                string effectType = GetEffectTypeString(effect);
                if (!string.IsNullOrEmpty(effectType))
                {
                    effectTypeText.text = effectType;
                    effectTypeText.gameObject.SetActive(true);
                }
                else
                {
                    effectTypeText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 기본 효과 아이콘을 생성합니다.
        /// </summary>
        private void CreateDefaultEffectIcon()
        {
            if (effectIconImage == null) return;

            // 64x64 픽셀의 기본 효과 아이콘 생성
            Texture2D texture = new Texture2D(64, 64);
            
            // 그라데이션 배경 생성
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    float centerX = 32f;
                    float centerY = 32f;
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    float normalizedDistance = distance / 32f;
                    
                    // 중앙에서 바깥쪽으로 갈수록 어두워지는 그라데이션
                    Color color = Color.Lerp(new Color(0.8f, 0.4f, 0.2f, 1f), new Color(0.4f, 0.2f, 0.1f, 1f), normalizedDistance);
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            effectIconImage.sprite = sprite;
            effectIconImage.color = Color.white;
        }

        /// <summary>
        /// 효과 설명을 업데이트합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        private void UpdateEffectDescription(IPerTurnEffect effect)
        {
            if (descriptionText != null)
            {
                string description = GetEffectDescription(effect);
                if (!string.IsNullOrEmpty(description))
                {
                    descriptionText.text = description;
                    descriptionText.gameObject.SetActive(true);
                }
                else
                {
                    descriptionText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 턴 정보를 업데이트합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        private void UpdateTurnInfo(IPerTurnEffect effect)
        {
            if (remainingTurnsText != null)
            {
                int remainingTurns = effect.RemainingTurns;
                remainingTurnsText.text = $"남은 턴: {remainingTurns}";
                remainingTurnsText.gameObject.SetActive(true);
            }

            // 턴 정보 컨테이너가 있으면 추가 정보 표시
            if (turnInfoContainer != null)
            {
                UpdateTurnInfoContainer(effect);
            }
        }

        /// <summary>
        /// 턴 정보 컨테이너를 업데이트합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        private void UpdateTurnInfoContainer(IPerTurnEffect effect)
        {
            // 기존 턴 정보 아이템들 제거
            foreach (Transform child in turnInfoContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            // 효과별 추가 턴 정보 생성
            var turnInfoItems = GetTurnInfoItems(effect);
            foreach (var item in turnInfoItems)
            {
                CreateTurnInfoItem(item);
            }
        }

        /// <summary>
        /// 턴 정보 아이템을 생성합니다.
        /// </summary>
        /// <param name="item">턴 정보 아이템</param>
        private void CreateTurnInfoItem(TurnInfoItem item)
        {
            if (turnInfoContainer == null) return;

            // 간단한 텍스트 아이템 생성
            GameObject itemObj = new GameObject($"TurnInfo_{item.name}");
            itemObj.transform.SetParent(turnInfoContainer);

            var textComponent = itemObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = $"{item.name}: {item.value}";
            textComponent.fontSize = 14;
            textComponent.color = item.color;
        }

        #endregion

        #region Effect Information

        /// <summary>
        /// 효과의 표시 이름을 반환합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>표시 이름</returns>
        private string GetEffectDisplayName(IPerTurnEffect effect)
        {
            if (effect == null) return "알 수 없는 효과";

            string effectTypeName = effect.GetType().Name;
            
            switch (effectTypeName)
            {
                case "BleedEffect":
                    return "출혈";
                case "StunEffect":
                    return "스턴";
                case "GuardBuff":
                    return "가드";
                case "CounterBuff":
                    return "반격";
                case "HealEffect":
                    return "치유";
                default:
                    return effectTypeName.Replace("Effect", "").Replace("Buff", "");
            }
        }

        /// <summary>
        /// 효과 타입 문자열을 반환합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>효과 타입 문자열</returns>
        private string GetEffectTypeString(IPerTurnEffect effect)
        {
            if (effect == null) return "";

            string effectTypeName = effect.GetType().Name;
            
            if (effectTypeName.Contains("Buff"))
                return "버프";
            else if (effectTypeName.Contains("Effect"))
                return "디버프";
            else
                return "효과";
        }

        /// <summary>
        /// 효과 설명을 반환합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>효과 설명</returns>
        private string GetEffectDescription(IPerTurnEffect effect)
        {
            if (effect == null) return "";

            string effectTypeName = effect.GetType().Name;
            
            switch (effectTypeName)
            {
                case "BleedEffect":
                    return "매 턴마다 체력이 감소합니다.\n지속 시간이 끝날 때까지 계속됩니다.";
                case "StunEffect":
                    return "행동을 할 수 없는 상태입니다.\n턴을 건너뛰게 됩니다.";
                case "GuardBuff":
                    return "받는 데미지를 무효화합니다.\n지속 시간이 끝날 때까지 유지됩니다.";
                case "CounterBuff":
                    return "공격을 받으면 반격합니다.\n지속 시간이 끝날 때까지 유지됩니다.";
                case "HealEffect":
                    return "매 턴마다 체력이 회복됩니다.\n지속 시간이 끝날 때까지 계속됩니다.";
                default:
                    return "특수 효과가 적용되어 있습니다.\n지속 시간이 끝날 때까지 유지됩니다.";
            }
        }

        /// <summary>
        /// 턴 정보 아이템 목록을 반환합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>턴 정보 아이템 목록</returns>
        private System.Collections.Generic.List<TurnInfoItem> GetTurnInfoItems(IPerTurnEffect effect)
        {
            var items = new System.Collections.Generic.List<TurnInfoItem>();

            if (effect == null) return items;

            string effectTypeName = effect.GetType().Name;
            
            switch (effectTypeName)
            {
                case "BleedEffect":
                    items.Add(new TurnInfoItem { name = "데미지", value = "2", color = Color.red });
                    break;
                case "StunEffect":
                    items.Add(new TurnInfoItem { name = "효과", value = "행동 불가", color = Color.red });
                    break;
                case "GuardBuff":
                    items.Add(new TurnInfoItem { name = "효과", value = "데미지 무효", color = Color.blue });
                    break;
                case "CounterBuff":
                    items.Add(new TurnInfoItem { name = "효과", value = "반격", color = Color.yellow });
                    break;
                case "HealEffect":
                    items.Add(new TurnInfoItem { name = "회복량", value = "5", color = Color.green });
                    break;
            }

            // 공통 값-페어(남은 턴 등) 추가
            var model = PerTurnEffectTooltipMapper.From(effect);
            if (model.ExtraPairs != null)
            {
                foreach (var pair in model.ExtraPairs)
                {
                    items.Add(new TurnInfoItem { name = pair.key, value = pair.value, color = new Color(0.65f, 0.82f, 1f) });
                }
            }

            return items;
        }

        #endregion

        #region Position Update

        /// <summary>
        /// 툴팁 위치를 슬롯 기준으로 업데이트합니다.
        /// </summary>
        /// <param name="slotPosition">슬롯의 위치</param>
        private void UpdateTooltipPosition(Vector2 slotPosition)
        {
            if (rectTransform == null) return;

            // 슬롯 위치를 캔버스 로컬 좌표로 변환
            Vector2 slotLocalPoint;
            Camera cameraToUse = null; // ScreenSpaceOverlay 모드 사용
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform, // 부모 RectTransform 사용
                slotPosition,
                cameraToUse,
                out slotLocalPoint);

            // 슬롯 기준으로 툴팁 위치 계산
            Vector2 tooltipPosition = CalculateTooltipPositionRelativeToSlot(slotLocalPoint);

            // 화면 경계 내로 제한
            tooltipPosition = ClampToScreenBounds(tooltipPosition);

            rectTransform.localPosition = tooltipPosition;
        }

        /// <summary>
        /// 슬롯 기준으로 툴팁 위치를 계산합니다.
        /// </summary>
        /// <param name="slotLocalPoint">슬롯의 로컬 좌표</param>
        /// <returns>툴팁의 로컬 좌표</returns>
        private Vector2 CalculateTooltipPositionRelativeToSlot(Vector2 slotLocalPoint)
        {
            if (rectTransform == null) return slotLocalPoint;

            var canvasRect = rectTransform.parent.GetComponent<RectTransform>().rect;
            var tooltipRect = rectTransform.rect;

            // 툴팁의 실제 크기
            float tooltipWidth = Mathf.Abs(tooltipRect.width);
            float tooltipHeight = Mathf.Abs(tooltipRect.height);

            // 툴팁의 Pivot 고려
            Vector2 tooltipPivot = rectTransform.pivot;

            // 슬롯 크기 (기본값 50)
            float slotSize = 50f;

            // 캔버스의 실제 경계
            float canvasLeft = canvasRect.xMin;
            float canvasRight = canvasRect.xMax;
            float canvasTop = canvasRect.yMax;
            float canvasBottom = canvasRect.yMin;

            // 슬롯 위치 기준 여유 공간 계산
            float rightSpace = canvasRight - (slotLocalPoint.x + slotSize * 0.5f);
            float leftSpace = (slotLocalPoint.x - slotSize * 0.5f) - canvasLeft;

            // 툴팁이 들어갈 공간이 있는지 확인
            float tooltipRequiredWidth = tooltipWidth * (1f - tooltipPivot.x) + slotOffsetX;
            bool canShowRight = rightSpace >= tooltipRequiredWidth;
            bool canShowLeft = leftSpace >= tooltipRequiredWidth;

            Vector2 tooltipPosition = slotLocalPoint;

            // 수평 위치 결정 (오른쪽 우선)
            if (canShowRight)
            {
                tooltipPosition.x = slotLocalPoint.x + (slotSize * 0.5f) + slotOffsetX + (tooltipWidth * tooltipPivot.x);
            }
            else if (canShowLeft)
            {
                tooltipPosition.x = slotLocalPoint.x - (slotSize * 0.5f) - slotOffsetX - (tooltipWidth * (1f - tooltipPivot.x));
            }
            else
            {
                tooltipPosition.x = slotLocalPoint.x + slotSize * 0.5f + 10f;
            }

            // 수직 위치는 슬롯 중심과 맞춤
            tooltipPosition.y = slotLocalPoint.y;

            // 수직 경계 체크 및 조정
            float tooltipTop = tooltipPosition.y + tooltipHeight * (1f - tooltipPivot.y);
            float tooltipBottom = tooltipPosition.y - tooltipHeight * tooltipPivot.y;

            if (tooltipTop > canvasTop)
            {
                tooltipPosition.y = canvasTop - tooltipHeight * (1f - tooltipPivot.y);
            }
            else if (tooltipBottom < canvasBottom)
            {
                tooltipPosition.y = canvasBottom + tooltipHeight * tooltipPivot.y;
            }

            return tooltipPosition;
        }

        /// <summary>
        /// 위치를 화면 경계 내로 제한합니다.
        /// </summary>
        /// <param name="position">원본 위치</param>
        /// <returns>제한된 위치</returns>
        private Vector2 ClampToScreenBounds(Vector2 position)
        {
            if (rectTransform == null) return position;

            var canvasRect = rectTransform.parent.GetComponent<RectTransform>().rect;
            var tooltipRect = rectTransform.rect;

            // X축 제한
            if (position.x + tooltipRect.width > canvasRect.width)
            {
                position.x = canvasRect.width - tooltipRect.width - slotOffsetX;
            }
            if (position.x < canvasRect.x)
            {
                position.x = canvasRect.x + slotOffsetX;
            }

            // Y축 제한
            if (position.y - tooltipRect.height < canvasRect.y)
            {
                position.y = canvasRect.y + tooltipRect.height + slotOffsetY;
            }
            if (position.y > canvasRect.height)
            {
                position.y = canvasRect.height - slotOffsetY;
            }

            return position;
        }

        #endregion

        #region Animation

        /// <summary>
        /// 페이드 인 애니메이션을 실행합니다.
        /// </summary>
        private void FadeIn()
        {
            if (fadeTween != null)
            {
                fadeTween.Kill();
            }

            isVisible = true;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            fadeTween = canvasGroup.DOFade(1f, fadeInDuration)
                .SetEase(fadeEase)
                .OnComplete(() => {
                    GameLogger.LogInfo("버프/디버프 툴팁 표시 완료", GameLogger.LogCategory.UI);
                });
        }

        /// <summary>
        /// 페이드 아웃 애니메이션을 실행합니다.
        /// </summary>
        private void FadeOut()
        {
            if (fadeTween != null)
            {
                fadeTween.Kill();
            }

            isVisible = false;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            fadeTween = canvasGroup.DOFade(0f, fadeOutDuration)
                .SetEase(fadeEase)
                .OnComplete(() => {
                    currentEffect = null;
                    GameLogger.LogInfo("버프/디버프 툴팁 숨김 완료", GameLogger.LogCategory.UI);
                });
        }

        #endregion

        #region Data Structures

        /// <summary>
        /// 턴 정보 아이템 구조입니다.
        /// </summary>
        [System.Serializable]
        public class TurnInfoItem
        {
            public string name;
            public string value;
            public Color color;
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (fadeTween != null)
            {
                fadeTween.Kill();
            }
        }

        #endregion
    }
}
