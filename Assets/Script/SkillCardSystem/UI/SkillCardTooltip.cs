using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 스킬 카드 툴팁을 표시하는 UI 컴포넌트입니다.
    /// 카드 정보, 효과, 데미지 등을 상세히 표시하며, 효과에 마우스 오버 시 상세 설명을 제공합니다.
    /// </summary>
    public class SkillCardTooltip : MonoBehaviour
    {
        #region Serialized Fields

        [Header("툴팁 배경")]
        [Tooltip("툴팁 배경 이미지")]
        [SerializeField] private Image backgroundImage;
        
        [Tooltip("툴팁 테두리 이미지")]
        [SerializeField] private Image borderImage;

        [Header("카드 헤더")]
        [Tooltip("카드 아이콘")]
        [SerializeField] private Image cardIconImage;
        
        [Tooltip("카드 이름 텍스트")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        
        [Tooltip("카드 타입 텍스트")]
        [SerializeField] private TextMeshProUGUI cardTypeText;

        [Header("카드 설명")]
        [Tooltip("카드 설명 텍스트")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("카드 통계")]
        [Tooltip("데미지 아이콘")]
        [SerializeField] private Image damageIconImage;
        
        [Tooltip("데미지 텍스트")]
        [SerializeField] private TextMeshProUGUI damageText;
        
        [Tooltip("소모 자원 아이콘")]
        [SerializeField] private Image resourceIconImage;
        
        [Tooltip("소모 자원 텍스트")]
        [SerializeField] private TextMeshProUGUI resourceCostText;

        [Header("효과 정보")]
        [Tooltip("효과 목록 컨테이너")]
        [SerializeField] private Transform effectsContainer;
        
        [Tooltip("효과 아이템 프리팹")]
        [SerializeField] private GameObject effectItemPrefab;

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인 시간")]
        [SerializeField] private float fadeInDuration = 0.2f;
        
        [Tooltip("페이드 아웃 시간")]
        [SerializeField] private float fadeOutDuration = 0.15f;
        
        [Tooltip("애니메이션 이징")]
        [SerializeField] private Ease fadeEase = Ease.OutQuad;

        [Header("위치 설정")]
        [Tooltip("마우스 오프셋 X")]
        [SerializeField] private float mouseOffsetX = 20f;
        
        [Tooltip("마우스 오프셋 Y")]
        [SerializeField] private float mouseOffsetY = -20f;

        [Header("서브 툴팁 설정")]
        [Tooltip("서브 툴팁 프리팹")]
        [SerializeField] private GameObject subTooltipPrefab;
        
        [Tooltip("서브 툴팁 표시 지연 시간")]
        [SerializeField] private float subTooltipDelay = 0.2f;

        #endregion

        #region Private Fields

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        
        private Tween fadeTween;
        private bool isVisible = false;
        private ISkillCard currentCard;
        
        // 툴팁 고정 관련
        private bool isFixed = false;
        private Vector2 fixedPosition;
        
        // 동적 크기 조절을 위한 컴포넌트들
        private ContentSizeFitter contentSizeFitter;
        private LayoutElement layoutElement;
        private VerticalLayoutGroup verticalLayoutGroup;
        private HorizontalLayoutGroup horizontalLayoutGroup;
        
        // 서브 툴팁 관련
        private GameObject currentSubTooltip;
        private EffectData currentHoveredEffect;
        private System.Collections.IEnumerator subTooltipCoroutine;

        #endregion

        #region Public Properties

        /// <summary>
        /// 툴팁이 고정되어 있는지 여부를 반환합니다.
        /// </summary>
        public bool IsFixed => isFixed;

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
        /// 독립적인 툴팁 캔버스를 사용하므로 부모 캔버스 참조를 제거합니다.
        /// </summary>
        private void InitializeComponents()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            rectTransform = GetComponent<RectTransform>();
            
            // 독립적인 툴팁 캔버스 사용
            
            GameLogger.LogInfo("[SkillCardTooltip] 독립적인 툴팁 캔버스 사용 - 부모 캔버스 참조 제거", GameLogger.LogCategory.UI);

            // 프리팹 구조를 존중하기 위해 레이아웃 컴포넌트 자동 추가 제거

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
        /// <param name="card">표시할 카드</param>
        /// <param name="cardPosition">스킬카드의 위치</param>
        public void ShowTooltip(ISkillCard card, Vector2 cardPosition)
        {
            GameLogger.LogInfo($"[SkillCardTooltip] ShowTooltip 호출됨 - card: {card?.GetCardName()}, cardPosition: {cardPosition}, isVisible: {isVisible}", GameLogger.LogCategory.UI);
            
            if (card == null)
            {
                GameLogger.LogWarning("[SkillCardTooltip] 표시할 카드가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            currentCard = card;
            UpdateTooltipContent(card);
            
            // Layout 시스템이 적용되도록 한 프레임 대기 후 위치 계산
            StartCoroutine(ShowTooltipWithLayout(cardPosition));
        }

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        /// <param name="cardPosition">스킬카드의 위치</param>
        private System.Collections.IEnumerator ShowTooltipWithLayout(Vector2 cardPosition)
        {
            // 프리팹 크기를 사용하므로 레이아웃 대기 불필요
            yield return null;

            // 툴팁 위치 계산
            UpdateTooltipPosition(cardPosition);

            if (!isVisible)
            {
                GameLogger.LogInfo($"[SkillCardTooltip] 툴팁 페이드 인 시작: {currentCard?.GetCardName()}", GameLogger.LogCategory.UI);
                FadeIn();
            }
            else
            {
                GameLogger.LogInfo("[SkillCardTooltip] 툴팁이 이미 표시 중입니다", GameLogger.LogCategory.UI);
            }
        }


        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        public void HideTooltip()
        {
            if (isVisible)
            {
                // 서브 툴팁도 함께 숨김 (임시 비활성화)
                // HideSubTooltip();
                FadeOut();
            }
        }

        /// <summary>
        /// 툴팁 위치를 업데이트합니다.
        /// </summary>
        /// <param name="mousePosition">마우스 위치</param>
        public void UpdatePosition(Vector2 mousePosition)
        {
            if (isVisible)
            {
                UpdateTooltipPosition(mousePosition);
            }
        }

        #endregion

        #region UI Components

        /// <summary>
        /// 효과 아이템 컴포넌트입니다.
        /// </summary>
        [System.Serializable]
        public class EffectItem
        {
            [Header("효과 아이콘")]
            [Tooltip("효과 아이콘 이미지")]
            public Image iconImage;
            
            [Tooltip("효과 이름 텍스트")]
            public TextMeshProUGUI nameText;
            
            [Tooltip("효과 설명 텍스트")]
            public TextMeshProUGUI descriptionText;
            
            [Tooltip("효과 색상")]
            public Color effectColor = Color.white;
        }

        #endregion

        #region Data Structures

        /// <summary>
        /// 효과 데이터 구조입니다.
        /// </summary>
        [System.Serializable]
        public class EffectData
        {
            public string name;
            public string description;
            public Color iconColor;
            public EffectType effectType;
        }

        /// <summary>
        /// 효과 타입 열거형
        /// </summary>
        public enum EffectType
        {
            Damage,     // 데미지
            Buff,       // 버프 (반격)
            Debuff,     // 디버프 (출혈, 스턴)
            Heal,       // 치유
            Shield,     // 방어 (가드)
            Special     // 특수 효과 (자원)
        }

        #endregion

        #region UI Components

        /// <summary>
        /// 효과 아이템 컴포넌트입니다.
        /// 호버 시 서브 툴팁을 표시합니다.
        /// </summary>
        public class EffectItemComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            [Header("효과 아이콘")]
            [SerializeField] private Image iconImage;
            
            [Header("효과 텍스트")]
            [SerializeField] private TextMeshProUGUI nameText;
            [SerializeField] private TextMeshProUGUI descriptionText;

            [Header("호버 효과")]
            [Tooltip("효과 이름의 기본 색상")]
            [SerializeField] private Color defaultNameColor = Color.white;
            
            [Tooltip("효과 이름의 강조 색상 (호버 시)")]
            [SerializeField] private Color highlightNameColor = Color.yellow;

            private EffectData currentEffectData;
            private SkillCardTooltip parentTooltip;

            /// <summary>
            /// 효과를 설정합니다.
            /// </summary>
            /// <param name="effectData">효과 데이터</param>
            /// <param name="tooltip">부모 툴팁 참조</param>
            public void SetupEffect(EffectData effectData, SkillCardTooltip tooltip)
            {
                currentEffectData = effectData;
                parentTooltip = tooltip;

                if (nameText != null)
                {
                    nameText.text = $"<b>{effectData.name}</b>"; // 볼드 처리
                    nameText.color = defaultNameColor;
                }
                
                if (descriptionText != null)
                {
                    descriptionText.text = effectData.description;
                }
                
                if (iconImage != null)
                {
                    iconImage.color = effectData.iconColor;
                }
            }

            /// <summary>
            /// 마우스가 효과 아이템에 진입했을 때 호출됩니다.
            /// </summary>
            /// <param name="eventData">포인터 이벤트 데이터</param>
            public void OnPointerEnter(PointerEventData eventData)
            {
                if (currentEffectData == null || parentTooltip == null) return;

                GameLogger.LogInfo($"[EffectItemComponent] OnPointerEnter: {currentEffectData.name} (서브 툴팁 비활성화됨)", GameLogger.LogCategory.UI);
                
                // 이름 텍스트 강조
                if (nameText != null)
                {
                    nameText.color = highlightNameColor;
                }

                // 서브 툴팁 표시 (임시 비활성화)
                // parentTooltip.ShowSubTooltip(currentEffectData, eventData.position);
            }

            /// <summary>
            /// 마우스가 효과 아이템에서 이탈했을 때 호출됩니다.
            /// </summary>
            /// <param name="eventData">포인터 이벤트 데이터</param>
            public void OnPointerExit(PointerEventData eventData)
            {
                if (currentEffectData == null || parentTooltip == null) return;

                GameLogger.LogInfo($"[EffectItemComponent] OnPointerExit: {currentEffectData.name} (서브 툴팁 비활성화됨)", GameLogger.LogCategory.UI);
                
                // 이름 텍스트 원래 색상으로 복원
                if (nameText != null)
                {
                    nameText.color = defaultNameColor;
                }

                // 서브 툴팁 숨김 (임시 비활성화)
                // parentTooltip.HideSubTooltip();
            }
        }

        #endregion

        #region Content Update

        /// <summary>
        /// 툴팁 내용을 업데이트합니다.
        /// </summary>
        /// <param name="card">카드 정보</param>
        private void UpdateTooltipContent(ISkillCard card)
        {
            if (card?.CardDefinition == null) return;

            var definition = card.CardDefinition;

            // 툴팁 배경 및 보더 설정
            SetupTooltipBackground();

            // 카드 헤더 업데이트 (아이콘 + 이름 + 타입)
            UpdateCardHeader(definition);

            // 카드 설명 업데이트
            UpdateCardDescription(definition);

            // 카드 통계 업데이트 (아이콘 + 텍스트)
            UpdateCardStats(card, definition);

            // 효과 정보 업데이트 (동적 생성)
            UpdateEffectsInfo(definition);
        }

        /// <summary>
        /// 툴팁 배경과 보더를 설정합니다.
        /// 프리팹의 설정을 최대한 존중하며, 런타임 설정만 적용합니다.
        /// </summary>
        private void SetupTooltipBackground()
        {
            // 배경 이미지 설정
            if (backgroundImage != null)
            {
                // 프리팹에 스프라이트가 있으면 그대로 사용
                if (backgroundImage.sprite != null)
                {
                    // 배경 색상 설정 (어두운 반투명) - 프리팹 이미지에 틴트 적용
                    backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    backgroundImage.gameObject.SetActive(true); // 활성화 확인
                    GameLogger.LogInfo("[SkillCardTooltip] 프리팹 배경 이미지 사용", GameLogger.LogCategory.UI);
                }
                else
                {
                    // 프리팹에 스프라이트가 없으면 기본 이미지 생성
                    CreateDefaultBackgroundSprite();
                    backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    backgroundImage.gameObject.SetActive(true);
                    GameLogger.LogInfo("[SkillCardTooltip] 기본 배경 이미지 생성", GameLogger.LogCategory.UI);
                }
            }

            // 보더 이미지 설정
            if (borderImage != null)
            {
                // 보더 활성화 (프리팹에서 비활성화되어 있을 수 있음)
                borderImage.gameObject.SetActive(true);

                // 프리팹에 스프라이트가 있으면 그대로 사용
                if (borderImage.sprite != null)
                {
                    // 보더 색상 설정 (금색 테두리)
                    borderImage.color = new Color(1f, 0.8f, 0.2f, 1f);

                    // 보더가 테두리만 보이도록 설정 (Fill Center 끄기)
                    if (borderImage.type == Image.Type.Sliced)
                    {
                        borderImage.fillCenter = false;
                        GameLogger.LogInfo("[SkillCardTooltip] 보더 Fill Center 비활성화 (테두리만 표시)", GameLogger.LogCategory.UI);
                    }
                    else
                    {
                        // Sliced가 아니면 Sliced로 변경
                        borderImage.type = Image.Type.Sliced;
                        borderImage.fillCenter = false;
                        GameLogger.LogInfo("[SkillCardTooltip] 보더를 Sliced 타입으로 변경 및 Fill Center 비활성화", GameLogger.LogCategory.UI);
                    }

                    GameLogger.LogInfo("[SkillCardTooltip] 프리팹 보더 이미지 사용", GameLogger.LogCategory.UI);
                }
                else
                {
                    // 프리팹에 스프라이트가 없으면 기본 이미지 생성
                    CreateDefaultBorderSprite();
                    borderImage.color = new Color(1f, 0.8f, 0.2f, 1f);
                    borderImage.type = Image.Type.Sliced;
                    borderImage.fillCenter = false;
                    GameLogger.LogInfo("[SkillCardTooltip] 기본 보더 이미지 생성", GameLogger.LogCategory.UI);
                }
            }

            GameLogger.LogInfo("[SkillCardTooltip] 툴팁 배경 및 보더 설정 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 기본 배경 스프라이트를 생성합니다.
        /// </summary>
        private void CreateDefaultBackgroundSprite()
        {
            if (backgroundImage == null) return;

            // 1x1 픽셀의 흰색 텍스처 생성
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // 스프라이트 생성 및 할당
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            backgroundImage.sprite = sprite;
            backgroundImage.type = Image.Type.Sliced; // 9-slice 스타일로 설정
        }

        /// <summary>
        /// 기본 보더 스프라이트를 생성합니다.
        /// </summary>
        private void CreateDefaultBorderSprite()
        {
            if (borderImage == null) return;

            // 1x1 픽셀의 흰색 텍스처 생성
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // 스프라이트 생성 및 할당
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            borderImage.sprite = sprite;
            borderImage.type = Image.Type.Sliced; // 9-slice 스타일로 설정
        }


        /// <summary>
        /// 카드 헤더를 업데이트합니다. (아이콘 + 이름 + 타입)
        /// 프리팹 구조를 존중하며 텍스트와 이미지만 업데이트합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        private void UpdateCardHeader(SkillCardDefinition definition)
        {
            // 카드 아이콘 설정
            if (cardIconImage != null)
            {
                // 실제 카드 아이콘이 있다면 설정, 없으면 기본 아이콘 생성
                if (definition.artwork != null)
                {
                    cardIconImage.sprite = definition.artwork;
                    cardIconImage.color = Color.white;
                }
                else
                {
                    // 기본 카드 아이콘 생성
                    CreateDefaultCardIcon();
                }
                cardIconImage.gameObject.SetActive(true);
            }

            // 카드 이름
            if (cardNameText != null)
            {
                string displayName = !string.IsNullOrEmpty(definition.displayNameKO)
                    ? definition.displayNameKO
                    : definition.displayName;
                cardNameText.text = displayName;
            }

            // 카드 타입
            if (cardTypeText != null)
            {
                string cardType = GetCardTypeString(definition);
                if (!string.IsNullOrEmpty(cardType))
                {
                    cardTypeText.text = cardType;
                    cardTypeText.gameObject.SetActive(true);
                }
                else
                {
                    cardTypeText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 기본 카드 아이콘을 생성합니다.
        /// </summary>
        private void CreateDefaultCardIcon()
        {
            if (cardIconImage == null) return;

            // 64x64 픽셀의 기본 카드 아이콘 생성
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
                    Color color = Color.Lerp(new Color(0.3f, 0.6f, 1f, 1f), new Color(0.1f, 0.3f, 0.8f, 1f), normalizedDistance);
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();

            // 스프라이트 생성 및 할당
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            cardIconImage.sprite = sprite;
            cardIconImage.color = Color.white;
        }

        /// <summary>
        /// 카드 설명을 업데이트합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        private void UpdateCardDescription(SkillCardDefinition definition)
        {
            if (descriptionText != null)
            {
                string description = definition.description ?? "";
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
            else
            {
                GameLogger.LogInfo("[SkillCardTooltip] descriptionText가 프리팹에 할당되지 않았습니다 - 선택적 컴포넌트", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 카드 통계를 업데이트합니다. (아이콘 + 텍스트)
        /// </summary>
        /// <param name="card">카드 정보</param>
        /// <param name="definition">카드 정의</param>
        private void UpdateCardStats(ISkillCard card, SkillCardDefinition definition)
        {
            // 데미지 정보 (아이콘 + 텍스트)
            if (damageText != null && damageIconImage != null)
            {
                if (definition.configuration?.hasDamage == true)
                {
                    int damage = definition.configuration.damageConfig.baseDamage;
                    damageText.text = $"데미지: {damage}";
                    damageText.gameObject.SetActive(true);
                    damageIconImage.gameObject.SetActive(true);
                }
                else
                {
                    damageText.gameObject.SetActive(false);
                    damageIconImage.gameObject.SetActive(false);
                }
            }

            // 소모 자원 정보 (아이콘 + 텍스트)
            if (resourceCostText != null && resourceIconImage != null)
            {
                int resourceCost = GetResourceCost(card);
                
                if (resourceCost > 0)
                {
                    resourceCostText.text = $"소모 자원: {resourceCost}";
                    resourceCostText.gameObject.SetActive(true);
                    resourceIconImage.gameObject.SetActive(true);
                }
                else
                {
                    resourceCostText.gameObject.SetActive(false);
                    resourceIconImage.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 효과 정보를 업데이트합니다. (동적 생성)
        /// </summary>
        /// <param name="definition">카드 정의</param>
        private void UpdateEffectsInfo(SkillCardDefinition definition)
        {
            if (effectsContainer == null)
            {
                GameLogger.LogInfo("[SkillCardTooltip] effectsContainer가 프리팹에 할당되지 않았습니다 - 선택적 컴포넌트", GameLogger.LogCategory.UI);
                return;
            }

            if (effectItemPrefab == null)
            {
                GameLogger.LogWarning("[SkillCardTooltip] effectItemPrefab이 프리팹에 할당되지 않았습니다", GameLogger.LogCategory.UI);
                return;
            }

            // 기존 효과 아이템들 제거
            foreach (Transform child in effectsContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            // 새로운 효과 아이템들 생성
            var effects = GetCardEffects(definition);
            foreach (var effect in effects)
            {
                CreateEffectItem(effect);
            }
        }

        /// <summary>
        /// 카드의 소모 자원을 가져옵니다.
        /// </summary>
        /// <param name="card">카드 정보</param>
        /// <returns>소모 자원</returns>
        private int GetResourceCost(ISkillCard card)
        {
            if (card?.CardDefinition?.configuration == null) return 0;

            // 실제 카드 데이터에서 소모 자원 정보를 가져옴
            // 현재는 기본값 0을 반환하지만, 실제 데이터 구조에 맞게 수정 가능
            var config = card.CardDefinition.configuration;
            
            // 예시: 실제 데이터 구조에 따라 수정
            // if (config.hasResourceCost)
            // {
            //     return config.resourceCostConfig.baseCost;
            // }
            
            // 현재는 모든 카드가 소모 자원이 없다고 가정
            return 0;
        }

        /// <summary>
        /// 카드의 효과 목록을 가져옵니다. (실제 게임 시스템 기반)
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <returns>효과 목록</returns>
        private System.Collections.Generic.List<EffectData> GetCardEffects(SkillCardDefinition definition)
        {
            var effects = new System.Collections.Generic.List<EffectData>();

            if (definition.configuration == null) return effects;

            var config = definition.configuration;

            // 데미지 효과
            if (config.hasDamage)
            {
                effects.Add(new EffectData
                {
                    name = "데미지",
                    description = $"기본 데미지: {config.damageConfig.baseDamage}",
                    iconColor = Color.red,
                    effectType = EffectType.Damage
                });
            }

            // 효과 목록에서 커스텀 설정 확인
            if (config.hasEffects && config.effects != null)
            {
                foreach (var effectConfig in config.effects)
                {
                    if (effectConfig.useCustomSettings && effectConfig.customSettings != null)
                    {
                        var customSettings = effectConfig.customSettings;

                        // 출혈 효과
                        if (customSettings.bleedAmount > 0)
                        {
                            effects.Add(new EffectData
                            {
                                name = "출혈",
                                description = $"출혈량: {customSettings.bleedAmount}, 지속: {customSettings.bleedDuration}턴",
                                iconColor = Color.red,
                                effectType = EffectType.Debuff
                            });
                        }

                        // 반격 효과
                        if (customSettings.counterDuration > 0)
                        {
                            effects.Add(new EffectData
                            {
                                name = "반격",
                                description = $"반격 지속: {customSettings.counterDuration}턴",
                                iconColor = Color.yellow,
                                effectType = EffectType.Buff
                            });
                        }

                        // 가드 효과
                        if (customSettings.guardDuration > 0)
                        {
                            effects.Add(new EffectData
                            {
                                name = "가드",
                                description = $"가드 지속: {customSettings.guardDuration}턴",
                                iconColor = Color.blue,
                                effectType = EffectType.Shield
                            });
                        }

                        // 스턴 효과
                        if (customSettings.stunDuration > 0)
                        {
                            effects.Add(new EffectData
                            {
                                name = "스턴",
                                description = $"스턴 지속: {customSettings.stunDuration}턴",
                                iconColor = Color.red,
                                effectType = EffectType.Debuff
                            });
                        }

                        // 치유 효과
                        if (customSettings.healAmount > 0)
                        {
                            effects.Add(new EffectData
                            {
                                name = "치유",
                                description = $"치유량: {customSettings.healAmount}",
                                iconColor = Color.green,
                                effectType = EffectType.Heal
                            });
                        }

                        // 자원 효과 (리소스 → 자원으로 용어 통일)
                        if (customSettings.resourceDelta != 0)
                        {
                            string resourceText = customSettings.resourceDelta > 0 ? "획득" : "소모";
                            effects.Add(new EffectData
                            {
                                name = "자원",
                                description = $"자원 {resourceText}: {Mathf.Abs(customSettings.resourceDelta)}",
                                iconColor = customSettings.resourceDelta > 0 ? Color.green : Color.red,
                                effectType = EffectType.Special
                            });
                        }
                    }
                }
            }

            return effects;
        }

        /// <summary>
        /// 효과 아이템을 생성합니다.
        /// </summary>
        /// <param name="effectData">효과 데이터</param>
        private void CreateEffectItem(EffectData effectData)
        {
            if (effectItemPrefab == null || effectsContainer == null) return;

            GameObject effectItem = Instantiate(effectItemPrefab, effectsContainer);
            
            // 효과 아이템 컴포넌트 설정
            var effectItemComponent = effectItem.GetComponent<EffectItemComponent>();
            if (effectItemComponent != null)
            {
                effectItemComponent.SetupEffect(effectData, this); // 부모 툴팁 참조 전달
            }
        }

        /// <summary>
        /// 카드 타입 문자열을 반환합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <returns>카드 타입 문자열</returns>
        private string GetCardTypeString(SkillCardDefinition definition)
        {
            if (definition.configuration?.hasDamage == true)
                return "공격 카드";
            else if (definition.configuration?.hasEffects == true)
                return "효과 카드";
            else
                return "기본 카드";
        }

        #endregion

        #region Position Update

        /// <summary>
        /// 툴팁 위치를 스킬카드 기준으로 업데이트합니다.
        /// </summary>
        /// <param name="cardPosition">스킬카드의 위치</param>
        private void UpdateTooltipPosition(Vector2 cardPosition)
        {
            if (rectTransform == null) return;

            // 고정된 툴팁은 위치를 업데이트하지 않음
            if (isFixed)
            {
                GameLogger.LogInfo("[SkillCardTooltip] 툴팁이 고정되어 있어 위치 업데이트 무시", GameLogger.LogCategory.UI);
                return;
            }

            // 스킬카드 위치를 캔버스 로컬 좌표로 변환
            Vector2 cardLocalPoint;
            Camera cameraToUse = null; // ScreenSpaceOverlay 모드 사용
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform, // 부모 RectTransform 사용
                cardPosition,
                cameraToUse,
                out cardLocalPoint);

            // 스킬카드 기준으로 툴팁 위치 계산
            Vector2 tooltipPosition = CalculateTooltipPositionRelativeToCard(cardLocalPoint);

            // 화면 경계 내로 제한
            tooltipPosition = ClampToScreenBounds(tooltipPosition);

            rectTransform.localPosition = tooltipPosition;
        }

        /// <summary>
        /// 스킬카드 기준으로 툴팁 위치를 계산합니다.
        /// </summary>
        /// <param name="cardLocalPoint">스킬카드의 로컬 좌표</param>
        /// <returns>툴팁의 로컬 좌표</returns>
        private Vector2 CalculateTooltipPositionRelativeToCard(Vector2 cardLocalPoint)
        {
            if (rectTransform == null) return cardLocalPoint;

            var canvasRect = rectTransform.parent.GetComponent<RectTransform>().rect;
            var tooltipRect = rectTransform.rect;

            // 툴팁의 실제 크기 (프리팹 크기: 600x600)
            float tooltipWidth = Mathf.Abs(tooltipRect.width);
            float tooltipHeight = Mathf.Abs(tooltipRect.height);

            // 툴팁의 Pivot 고려 (프리팹 기본값: 0.5, 0.5)
            Vector2 tooltipPivot = rectTransform.pivot;

            // 스킬카드 크기 (실제 카드 크기, 기본값 150)
            float cardWidth = 150f;

            // 캔버스의 실제 경계 (Canvas 중앙이 0,0인 좌표계)
            float canvasLeft = canvasRect.xMin;
            float canvasRight = canvasRect.xMax;
            float canvasTop = canvasRect.yMax;
            float canvasBottom = canvasRect.yMin;

            // 카드 위치 기준 여유 공간 계산
            float rightSpace = canvasRight - (cardLocalPoint.x + cardWidth * 0.5f);
            float leftSpace = (cardLocalPoint.x - cardWidth * 0.5f) - canvasLeft;

            // 툴팁이 들어갈 공간이 있는지 확인
            float tooltipRequiredWidth = tooltipWidth * (1f - tooltipPivot.x) + mouseOffsetX;
            bool canShowRight = rightSpace >= tooltipRequiredWidth;
            bool canShowLeft = leftSpace >= tooltipRequiredWidth;

            Vector2 tooltipPosition = cardLocalPoint;

            // 수평 위치 결정 (오른쪽 우선)
            if (canShowRight)
            {
                tooltipPosition.x = cardLocalPoint.x + (cardWidth * 0.5f) + mouseOffsetX + (tooltipWidth * tooltipPivot.x);
            }
            else if (canShowLeft)
            {
                tooltipPosition.x = cardLocalPoint.x - (cardWidth * 0.5f) - mouseOffsetX - (tooltipWidth * (1f - tooltipPivot.x));
            }
            else
            {
                tooltipPosition.x = cardLocalPoint.x + cardWidth * 0.5f + 10f;
            }

            // 수직 위치는 카드 중심과 맞춤
            tooltipPosition.y = cardLocalPoint.y;

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
        /// 화면 경계를 고려한 스마트 오프셋을 적용합니다.
        /// </summary>
        /// <param name="localPoint">로컬 좌표</param>
        /// <param name="mousePosition">마우스 위치</param>
        /// <returns>오프셋이 적용된 좌표</returns>
        private Vector2 ApplySmartOffset(Vector2 localPoint, Vector2 mousePosition)
        {
            if (rectTransform == null) return localPoint;

            var canvasRect = rectTransform.parent.GetComponent<RectTransform>().rect;
            var tooltipRect = rectTransform.rect;

            // 툴팁의 실제 크기 계산 (Layout 시스템이 적용된 후)
            float tooltipWidth = tooltipRect.width;
            float tooltipHeight = tooltipRect.height;

            // 화면 경계에서의 여유 공간 계산
            float rightSpace = canvasRect.width - localPoint.x;
            float leftSpace = localPoint.x;
            float topSpace = canvasRect.height - localPoint.y;
            float bottomSpace = localPoint.y;

            // 툴팁을 오른쪽에 표시할 수 있는지 확인
            bool canShowRight = rightSpace >= tooltipWidth + mouseOffsetX;
            // 툴팁을 왼쪽에 표시할 수 있는지 확인
            bool canShowLeft = leftSpace >= tooltipWidth + mouseOffsetX;

            // 툴팁을 위쪽에 표시할 수 있는지 확인
            bool canShowTop = topSpace >= tooltipHeight + Mathf.Abs(mouseOffsetY);
            // 툴팁을 아래쪽에 표시할 수 있는지 확인
            bool canShowBottom = bottomSpace >= tooltipHeight + Mathf.Abs(mouseOffsetY);

            // 수평 위치 결정 (오른쪽 우선, 공간이 부족하면 왼쪽)
            if (canShowRight)
            {
                localPoint.x += mouseOffsetX; // 오른쪽으로 오프셋
            }
            else if (canShowLeft)
            {
                localPoint.x -= tooltipWidth + mouseOffsetX; // 왼쪽으로 오프셋
            }
            else
            {
                // 양쪽 모두 공간이 부족하면 화면 중앙에 배치
                localPoint.x = canvasRect.width * 0.5f - tooltipWidth * 0.5f;
            }

            // 수직 위치 결정 (위쪽 우선, 공간이 부족하면 아래쪽)
            if (canShowTop)
            {
                localPoint.y += Mathf.Abs(mouseOffsetY); // 위쪽으로 오프셋
            }
            else if (canShowBottom)
            {
                localPoint.y -= tooltipHeight + Mathf.Abs(mouseOffsetY); // 아래쪽으로 오프셋
            }
            else
            {
                // 양쪽 모두 공간이 부족하면 마우스 위치에 배치
                localPoint.y += mouseOffsetY;
            }

            // 디버그 로그는 위치가 크게 변경될 때만 출력
            Vector2 previousPosition = rectTransform.localPosition;
            if (Vector2.Distance(localPoint, previousPosition) > 50f)
            {
                GameLogger.LogInfo($"[SkillCardTooltip] 스마트 위치 계산 - 마우스: {mousePosition}, 툴팁 크기: {tooltipWidth}x{tooltipHeight}, 최종 위치: {localPoint}", GameLogger.LogCategory.UI);
            }

            return localPoint;
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
                position.x = canvasRect.width - tooltipRect.width - mouseOffsetX;
            }
            if (position.x < canvasRect.x)
            {
                position.x = canvasRect.x + mouseOffsetX;
            }

            // Y축 제한
            if (position.y - tooltipRect.height < canvasRect.y)
            {
                position.y = canvasRect.y + tooltipRect.height + mouseOffsetY;
            }
            if (position.y > canvasRect.height)
            {
                position.y = canvasRect.height - mouseOffsetY;
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
                    GameLogger.LogInfo("툴팁 표시 완료", GameLogger.LogCategory.UI);
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
                    currentCard = null;
                    GameLogger.LogInfo("툴팁 숨김 완료", GameLogger.LogCategory.UI);
                });
        }

        #endregion

        #region Tooltip Fix/Unfix

        /// <summary>
        /// 툴팁을 현재 위치에 고정합니다.
        /// </summary>
        public void FixTooltip()
        {
            if (rectTransform == null) return;

            isFixed = true;
            fixedPosition = rectTransform.localPosition;
            
            GameLogger.LogInfo($"[SkillCardTooltip] 툴팁 고정됨 - 위치: {fixedPosition}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁 고정을 해제합니다.
        /// </summary>
        public void UnfixTooltip()
        {
            isFixed = false;
            fixedPosition = Vector2.zero;
            
            GameLogger.LogInfo("[SkillCardTooltip] 툴팁 고정 해제됨", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁 고정 상태를 토글합니다.
        /// </summary>
        public void ToggleTooltipFix()
        {
            if (isFixed)
            {
                UnfixTooltip();
            }
            else
            {
                FixTooltip();
            }
        }

        #endregion

        #region Sub Tooltip

        /// <summary>
        /// 서브 툴팁을 표시합니다.
        /// </summary>
        /// <param name="effectData">표시할 효과 데이터</param>
        /// <param name="triggerPosition">트리거된 UI 요소의 위치</param>
        public void ShowSubTooltip(EffectData effectData, Vector2 triggerPosition)
        {
            GameLogger.LogInfo($"[SkillCardTooltip] ShowSubTooltip 호출됨 - effect: {effectData.name}", GameLogger.LogCategory.UI);
            
            if (effectData == null)
            {
                GameLogger.LogWarning("[SkillCardTooltip] 표시할 효과 데이터가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            currentHoveredEffect = effectData;

            // 기존 서브 툴팁 숨김 코루틴 중지
            if (subTooltipCoroutine != null)
            {
                StopCoroutine(subTooltipCoroutine);
            }

            // 지연된 서브 툴팁 표시
            subTooltipCoroutine = ShowSubTooltipDelayed(effectData, triggerPosition);
            StartCoroutine(subTooltipCoroutine);
        }

        /// <summary>
        /// 서브 툴팁을 숨깁니다.
        /// </summary>
        public void HideSubTooltip()
        {
            GameLogger.LogInfo("[SkillCardTooltip] HideSubTooltip 호출됨", GameLogger.LogCategory.UI);
            
            // 기존 서브 툴팁 표시 코루틴 중지
            if (subTooltipCoroutine != null)
            {
                StopCoroutine(subTooltipCoroutine);
                subTooltipCoroutine = null;
            }

            // 서브 툴팁 숨김
            if (currentSubTooltip != null)
            {
                Destroy(currentSubTooltip);
                currentSubTooltip = null;
            }
            
            currentHoveredEffect = null;
        }

        /// <summary>
        /// 지연된 서브 툴팁 표시 코루틴
        /// </summary>
        private System.Collections.IEnumerator ShowSubTooltipDelayed(EffectData effectData, Vector2 triggerPosition)
        {
            yield return new WaitForSeconds(subTooltipDelay);

            if (currentHoveredEffect == effectData) // 여전히 같은 효과에 호버 중인지 확인
            {
                CreateSubTooltip(effectData, triggerPosition);
            }
            subTooltipCoroutine = null;
        }

        /// <summary>
        /// 서브 툴팁을 생성합니다.
        /// </summary>
        /// <param name="effectData">효과 데이터</param>
        /// <param name="triggerPosition">트리거된 UI 요소의 위치</param>
        private void CreateSubTooltip(EffectData effectData, Vector2 triggerPosition)
        {
            if (subTooltipPrefab == null) return;

            // 서브 툴팁 생성
            currentSubTooltip = Instantiate(subTooltipPrefab, rectTransform.parent);
            
            // 서브 툴팁 컴포넌트 설정
            var subTooltipComponent = currentSubTooltip.GetComponent<SubTooltipComponent>();
            if (subTooltipComponent != null)
            {
                subTooltipComponent.SetupSubTooltip(effectData, triggerPosition);
            }

            GameLogger.LogInfo($"[SkillCardTooltip] 서브 툴팁 생성 완료: {effectData.name}", GameLogger.LogCategory.UI);
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

        /// <summary>
        /// 서브 툴팁 컴포넌트입니다.
        /// 효과에 대한 상세 설명을 표시합니다.
        /// </summary>
        public class SubTooltipComponent : MonoBehaviour
        {
            [Header("서브 툴팁 배경")]
            [Tooltip("서브 툴팁 배경 이미지")]
            [SerializeField] private Image backgroundImage;
            
            [Tooltip("서브 툴팁 테두리 이미지")]
            [SerializeField] private Image borderImage;

            [Header("효과 상세 정보")]
            [Tooltip("효과 이름 텍스트")]
            [SerializeField] private TextMeshProUGUI effectNameText;
            
            [Tooltip("효과 상세 설명 텍스트")]
            [SerializeField] private TextMeshProUGUI effectDescriptionText;

            [Header("애니메이션 설정")]
            [Tooltip("페이드 인 시간")]
            [SerializeField] private float fadeInDuration = 0.15f;
            
            [Tooltip("페이드 아웃 시간")]
            [SerializeField] private float fadeOutDuration = 0.1f;
            
            [Tooltip("애니메이션 이징")]
            [SerializeField] private Ease fadeEase = Ease.OutQuad;

            [Header("위치 설정")]
            [Tooltip("메인 툴팁 오프셋 X")]
            [SerializeField] private float offsetX = 20f;
            
            [Tooltip("메인 툴팁 오프셋 Y")]
            [SerializeField] private float offsetY = 0f;

            private CanvasGroup canvasGroup;
            private RectTransform rectTransform;
            private Tween fadeTween;

            /// <summary>
            /// 서브 툴팁을 설정합니다.
            /// </summary>
            /// <param name="effectData">효과 데이터</param>
            /// <param name="triggerPosition">트리거된 UI 요소의 위치</param>
            public void SetupSubTooltip(EffectData effectData, Vector2 triggerPosition)
            {
                InitializeComponents();
                SetupSizeConstraints();
                SetupSubTooltipBackground();
                UpdateContent(effectData);
                UpdatePosition(triggerPosition);
                ShowTooltip();
            }

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
                // 독립적인 캔버스 사용

                // 초기 상태 설정
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            /// <summary>
            /// 서브 툴팁의 크기 제약을 설정합니다.
            /// 프리팹의 기존 설정을 존중하며 필요한 경우에만 추가합니다.
            /// </summary>
            private void SetupSizeConstraints()
            {
                if (rectTransform == null) return;

                // LayoutElement 확인/추가 (프리팹에 없을 경우에만)
                var layoutElement = GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = gameObject.AddComponent<LayoutElement>();
                    layoutElement.minHeight = 80f; // 최소 높이만 설정
                    GameLogger.LogInfo("[SubTooltipComponent] LayoutElement 추가됨", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogInfo("[SubTooltipComponent] 기존 LayoutElement 사용", GameLogger.LogCategory.UI);
                }

                // ContentSizeFitter 확인/추가 (프리팹에 없을 경우에만)
                var contentSizeFitter = GetComponent<ContentSizeFitter>();
                if (contentSizeFitter == null)
                {
                    contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    GameLogger.LogInfo("[SubTooltipComponent] ContentSizeFitter 추가됨", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogInfo("[SubTooltipComponent] 기존 ContentSizeFitter 사용", GameLogger.LogCategory.UI);
                }

                // VerticalLayoutGroup 확인/추가 (프리팹에 없을 경우에만)
                var verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
                if (verticalLayoutGroup == null)
                {
                    verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
                    verticalLayoutGroup.spacing = 4f;
                    verticalLayoutGroup.padding = new RectOffset(12, 12, 8, 8);
                    verticalLayoutGroup.childControlHeight = true;
                    verticalLayoutGroup.childControlWidth = false;
                    verticalLayoutGroup.childForceExpandHeight = false;
                    verticalLayoutGroup.childForceExpandWidth = false;
                    GameLogger.LogInfo("[SubTooltipComponent] VerticalLayoutGroup 추가됨", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogInfo("[SubTooltipComponent] 기존 VerticalLayoutGroup 사용", GameLogger.LogCategory.UI);
                }

                GameLogger.LogInfo("[SubTooltipComponent] 서브 툴팁 크기 제약 설정 완료", GameLogger.LogCategory.UI);
            }

            /// <summary>
            /// 서브 툴팁의 배경과 보더를 설정합니다.
            /// </summary>
            private void SetupSubTooltipBackground()
            {
                // 배경 이미지 설정
                if (backgroundImage != null)
                {
                    // 서브 툴팁 배경 색상 설정 (밝은 파란색 반투명)
                    backgroundImage.color = new Color(0.2f, 0.4f, 0.8f, 0.9f);
                    
                    // 배경 이미지가 할당되지 않은 경우 기본 색상으로 설정
                    if (backgroundImage.sprite == null)
                    {
                        CreateDefaultSubTooltipBackgroundSprite();
                    }
                }

                // 보더 이미지 설정
                if (borderImage != null)
                {
                    // 서브 툴팁 보더 색상 설정 (밝은 파란색 테두리)
                    borderImage.color = new Color(0.4f, 0.6f, 1f, 1f);
                    
                    // 보더 이미지가 할당되지 않은 경우 기본 색상으로 설정
                    if (borderImage.sprite == null)
                    {
                        CreateDefaultSubTooltipBorderSprite();
                    }
                }

                GameLogger.LogInfo("[SubTooltipComponent] 서브 툴팁 배경 및 보더 설정 완료", GameLogger.LogCategory.UI);
            }

            /// <summary>
            /// 서브 툴팁 기본 배경 스프라이트를 생성합니다.
            /// </summary>
            private void CreateDefaultSubTooltipBackgroundSprite()
            {
                if (backgroundImage == null) return;

                // 1x1 픽셀의 흰색 텍스처 생성
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                // 스프라이트 생성 및 할당
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                backgroundImage.sprite = sprite;
                backgroundImage.type = Image.Type.Sliced; // 9-slice 스타일로 설정
            }

            /// <summary>
            /// 서브 툴팁 기본 보더 스프라이트를 생성합니다.
            /// </summary>
            private void CreateDefaultSubTooltipBorderSprite()
            {
                if (borderImage == null) return;

                // 1x1 픽셀의 흰색 텍스처 생성
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                // 스프라이트 생성 및 할당
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                borderImage.sprite = sprite;
                borderImage.type = Image.Type.Sliced; // 9-slice 스타일로 설정
            }

            /// <summary>
            /// 서브 툴팁 내용을 업데이트합니다.
            /// </summary>
            /// <param name="effectData">효과 데이터</param>
            private void UpdateContent(EffectData effectData)
            {
                if (effectNameText != null)
                {
                    effectNameText.text = effectData.name;
                }
                
                if (effectDescriptionText != null)
                {
                    effectDescriptionText.text = GetDetailedDescription(effectData);
                }
            }

            /// <summary>
            /// 효과에 대한 상세 설명을 반환합니다.
            /// </summary>
            /// <param name="effectData">효과 데이터</param>
            /// <returns>상세 설명</returns>
            private string GetDetailedDescription(EffectData effectData)
            {
                // 기본 설명에 추가 정보를 포함
                string baseDescription = effectData.description;
                
                // 효과 타입별 추가 설명
                switch (effectData.effectType)
                {
                    case EffectType.Damage:
                        return $"{baseDescription}\n\n공격력에 따라 데미지가 증가합니다.";
                    case EffectType.Buff:
                        return $"{baseDescription}\n\n버프 효과로 전투에 유리한 상태를 제공합니다.";
                    case EffectType.Debuff:
                        return $"{baseDescription}\n\n디버프 효과로 적에게 불리한 상태를 부여합니다.";
                    case EffectType.Heal:
                        return $"{baseDescription}\n\n체력을 회복하여 생존력을 높입니다.";
                    case EffectType.Shield:
                        return $"{baseDescription}\n\n방어 효과로 데미지를 무효화합니다.";
                    case EffectType.Special:
                        return $"{baseDescription}\n\n특수 효과로 전략적 이점을 제공합니다.";
                    default:
                        return baseDescription;
                }
            }

            /// <summary>
            /// 서브 툴팁 위치를 업데이트합니다.
            /// </summary>
            /// <param name="triggerPosition">트리거된 UI 요소의 위치</param>
            private void UpdatePosition(Vector2 triggerPosition)
            {
                if (rectTransform == null) return;

                // 트리거 위치를 캔버스 로컬 좌표로 변환
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform, // 부모 RectTransform 사용
                    triggerPosition,
                    null, // ScreenSpaceOverlay 모드에서는 카메라 불필요
                    out localPoint);

                // 서브 툴팁을 메인 툴팁의 오른쪽에 배치
                Vector2 subTooltipPosition = localPoint + new Vector2(offsetX, offsetY);
                
                // 화면 경계 내로 제한
                subTooltipPosition = ClampToScreenBounds(subTooltipPosition);
                
                rectTransform.localPosition = subTooltipPosition;
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

                // 툴팁의 피벗이 중앙(0.5, 0.5)이라고 가정
                float minX = canvasRect.xMin + tooltipRect.width * rectTransform.pivot.x;
                float maxX = canvasRect.xMax - tooltipRect.width * (1 - rectTransform.pivot.x);
                float minY = canvasRect.yMin + tooltipRect.height * rectTransform.pivot.y;
                float maxY = canvasRect.yMax - tooltipRect.height * (1 - rectTransform.pivot.y);

                position.x = Mathf.Clamp(position.x, minX, maxX);
                position.y = Mathf.Clamp(position.y, minY, maxY);

                return position;
            }

            /// <summary>
            /// 서브 툴팁을 표시합니다.
            /// </summary>
            private void ShowTooltip()
            {
                if (fadeTween != null)
                {
                    fadeTween.Kill();
                }

                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                fadeTween = canvasGroup.DOFade(1f, fadeInDuration)
                    .SetEase(fadeEase)
                    .OnComplete(() => {
                        GameLogger.LogInfo("[SubTooltipComponent] 서브 툴팁 표시 완료", GameLogger.LogCategory.UI);
                    });
            }

            /// <summary>
            /// 서브 툴팁을 숨깁니다.
            /// </summary>
            public void HideTooltip()
            {
                if (fadeTween != null)
                {
                    fadeTween.Kill();
                }

                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                fadeTween = canvasGroup.DOFade(0f, fadeOutDuration)
                    .SetEase(fadeEase)
                    .OnComplete(() => {
                        if (Application.isPlaying)
                        {
                            Destroy(gameObject);
                        }
                    });
            }

            private void OnDestroy()
            {
                if (fadeTween != null)
                {
                    fadeTween.Kill();
                }
            }
        }
    }
}
