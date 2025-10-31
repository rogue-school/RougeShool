using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.UI.Mappers;

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
        
        // Border 제거: 프리팹/코드에서 사용하지 않음
        // [SerializeField] private Image borderImage;

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

        [Header("동작 옵션")]
        [Tooltip("카드 데이터 기반 자동 설명을 사용합니다 (수동 설명이 없을 때 권장)")]
        [SerializeField] private bool useAutoDescription = true;

        [Tooltip("수동 설명(definition.description)이 있을 경우 자동 설명보다 우선합니다")]
        [SerializeField] private bool preferManualDescription = true;

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인 시간")]
        [SerializeField] private float fadeInDuration = 0.1f; // 더 빠른 표시
        
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

        [Tooltip("서브 툴팁 스택 간격(px)")]
        [SerializeField] private float subTooltipStackSpacing = 12f;

        [Tooltip("서브 툴팁 최대 스택 수(0이면 무제한)")]
        [SerializeField] private int subTooltipMaxStack = 4;

        [Header("디버그 옵션")]
        [Tooltip("런타임에 배경/보더 설정을 상세 출력하고 이상 시 폴백을 적용합니다")]
        [SerializeField] private bool enableVisualDebugValidation = false;

		[Header("배치 옵션")]

        [Tooltip("상세 로깅 출력 여부")]
        [SerializeField] private bool verboseLogging = false;

        [Header("레이아웃 옵션")]
        [Tooltip("최소 높이(px) - 0이면 텍스트 내용에 맞게 자동 조절")]
        [SerializeField] private float minLayoutHeight = 0f;

        [Tooltip("선호 폭(px) - 고정 너비로 설정")]
        [SerializeField] private float preferredLayoutWidth = 300f;

		[Header("정렬 옵션")]
		[Tooltip("카드와 툴팁 사이의 가로 간격(px). 겹치지 않도록 충분한 간격")]
		[SerializeField] private float alignPaddingX = 10f;

		[Tooltip("카드 아래 모서리에 대한 세로 밀착 간격(px). 0이면 완전 밀착")]
		[SerializeField] private float alignPaddingY = 0f;

		// 내부 캐시
		private RectTransform _contentRoot;
		private LayoutElement _layoutElement;
		private ContentSizeFitter _contentSizeFitter;
        private int _lastAttackPowerStack = -1;

		private static Transform FindChildByName(Transform parent, string name)
		{
			if (parent == null || string.IsNullOrEmpty(name)) return null;
			for (int i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i);
				if (child.name == name) return child;
			}
			return null;
		}

		private void EnsurePrefabStructure()
		{
			// Background/Border 자동 바인딩 시도
			if (backgroundImage == null)
			{
				var bgTr = FindChildByName(transform, "Background");
				if (bgTr != null) backgroundImage = bgTr.GetComponent<Image>();
			}
            // Border 자동 바인딩 제거

			// ContentRoot 확보(없으면 생성)
			var existing = FindChildByName(transform, "ContentRoot");
			if (existing == null)
			{
				var go = new GameObject("ContentRoot", typeof(RectTransform));
				var rt = go.GetComponent<RectTransform>();
				rt.SetParent(transform, false);
				rt.anchorMin = new Vector2(0f, 0f);
				rt.anchorMax = new Vector2(1f, 1f);
				rt.offsetMin = Vector2.zero;
				rt.offsetMax = Vector2.zero;
				_contentRoot = rt;
			}
			else
			{
				_contentRoot = existing as RectTransform;
			}

			// ContentRoot 레이아웃 보장
			var vlg = _contentRoot.GetComponent<VerticalLayoutGroup>();
			if (vlg == null) vlg = _contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
			vlg.padding = new RectOffset(12, 12, 10, 10);
			vlg.spacing = 6f;
			vlg.childControlHeight = true;
			vlg.childForceExpandHeight = false;

			var crFitter = _contentRoot.GetComponent<ContentSizeFitter>();
			if (crFitter == null) crFitter = _contentRoot.gameObject.AddComponent<ContentSizeFitter>();
			crFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			crFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			// 루트 레이아웃 보장
			_layoutElement = GetComponent<LayoutElement>();
			if (_layoutElement == null) _layoutElement = gameObject.AddComponent<LayoutElement>();
			if (minLayoutHeight > 0f && _layoutElement.minHeight < minLayoutHeight) _layoutElement.minHeight = minLayoutHeight;
			if (preferredLayoutWidth > 0f) _layoutElement.preferredWidth = preferredLayoutWidth;

			_contentSizeFitter = GetComponent<ContentSizeFitter>();
			if (_contentSizeFitter == null) _contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
			_contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			_contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			// Background/Border RectTransform 스트레치 보정
			if (backgroundImage != null)
			{
				var bgRt = backgroundImage.rectTransform;
				bgRt.anchorMin = new Vector2(0f, 0f);
				bgRt.anchorMax = new Vector2(1f, 1f);
				bgRt.offsetMin = Vector2.zero;
				bgRt.offsetMax = Vector2.zero;
			}
            // Border 스트레치 보정 제거

			if (enableVisualDebugValidation)
			{
				GameLogger.LogInfo("[SkillCardTooltip] Prefab 구조 검증/보정 완료", GameLogger.LogCategory.UI);
			}
		}

        #endregion

        #region Private Fields

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        
        private Tween fadeTween;
        private bool isVisible = false;
        private ISkillCard currentCard;
        private RectTransform currentCardRectTransform; // 현재 카드의 RectTransform (크기 계산용)
        
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
        private readonly System.Collections.Generic.List<GameObject> stackedSubTooltips = new System.Collections.Generic.List<GameObject>();
        private readonly System.Collections.Generic.Dictionary<EffectData, GameObject> effectToSubTooltip = new System.Collections.Generic.Dictionary<EffectData, GameObject>();
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
			EnsurePrefabStructure();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            rectTransform = GetComponent<RectTransform>();
            
            // 레이아웃 컴포넌트 확보: 폭은 프리팹의 LayoutElement가 주도, 높이는 ContentSizeFitter로 자동
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }
            contentSizeFitter = GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 최소/선호 레이아웃 값 적용(프리팹 값 보존 우선)
            if (minLayoutHeight > 0f && layoutElement.minHeight < minLayoutHeight)
            {
                layoutElement.minHeight = minLayoutHeight;
            }
            if (preferredLayoutWidth > 0f)
            {
                layoutElement.preferredWidth = preferredLayoutWidth;
            }

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
        /// <param name="cardRectTransform">카드의 RectTransform (크기 계산용)</param>
        public void ShowTooltip(ISkillCard card, Vector2 cardPosition, RectTransform cardRectTransform = null)
        {
            if (verboseLogging)
            {
                GameLogger.LogInfo($"[SkillCardTooltip] ShowTooltip 호출됨 - card: {card?.GetCardName()}, cardPosition: {cardPosition}, isVisible: {isVisible}", GameLogger.LogCategory.UI);
            }
            
            if (card == null)
            {
                GameLogger.LogWarning("[SkillCardTooltip] 표시할 카드가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            currentCard = card;
            currentCardRectTransform = cardRectTransform; // 카드 RectTransform 저장
            UpdateTooltipContent(card);
            // 스택 기반 효과 변화 감지를 위해 현재 스택을 기록
            _lastAttackPowerStack = (card is Game.SkillCardSystem.Interface.IAttackPowerStackProvider sp) ? sp.GetAttackPowerStack() : -1;
            
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
                if (verboseLogging)
                {
                    GameLogger.LogInfo($"[SkillCardTooltip] 툴팁 페이드 인 시작: {currentCard?.GetCardName()}", GameLogger.LogCategory.UI);
                }
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
                HideSubTooltip();
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
                // 스택 변화가 있으면 콘텐츠 갱신
                if (currentCard is Game.SkillCardSystem.Interface.IAttackPowerStackProvider sp)
                {
                    int cur = sp.GetAttackPowerStack();
                    if (cur != _lastAttackPowerStack)
                    {
                        _lastAttackPowerStack = cur;
                        UpdateTooltipContent(currentCard);
                    }
                }
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

                GameLogger.LogInfo($"[EffectItemComponent] OnPointerEnter: {currentEffectData.name}", GameLogger.LogCategory.UI);
                
                // 이름 텍스트 강조
                if (nameText != null)
                {
                    nameText.color = highlightNameColor;
                }

                // 서브 툴팁 표시 (메인 툴팁의 오른쪽에 정렬)
                parentTooltip.ShowSubTooltip(currentEffectData, eventData.position);
            }

            /// <summary>
            /// 마우스가 효과 아이템에서 이탈했을 때 호출됩니다.
            /// </summary>
            /// <param name="eventData">포인터 이벤트 데이터</param>
            public void OnPointerExit(PointerEventData eventData)
            {
                if (currentEffectData == null || parentTooltip == null) return;

                GameLogger.LogInfo($"[EffectItemComponent] OnPointerExit: {currentEffectData.name}", GameLogger.LogCategory.UI);
                
                // 이름 텍스트 원래 색상으로 복원
                if (nameText != null)
                {
                    nameText.color = defaultNameColor;
                }

                // 서브 툴팁 숨김 (개별)
                parentTooltip.HideSubTooltip(currentEffectData);
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

            // 카드 헤더(아이콘/이름)만 갱신, 타입 비활성 처리
            UpdateCardHeader(definition);
            // 설명: 수동 설명 우선, 없으면 자동 설명
            if (descriptionText != null)
            {
                string manual = definition.description;
                if (preferManualDescription && !string.IsNullOrWhiteSpace(manual))
                {
                    descriptionText.text = manual.Trim();
                }
                else if (useAutoDescription)
                {
                    // 스택 기반 매퍼 사용 (실제 적용 데미지 표시)
                    int currentStacks = 0;
                    if (card is Game.SkillCardSystem.Interface.IAttackPowerStackProvider stackProvider)
                    {
                        currentStacks = stackProvider.GetAttackPowerStack();
                    }
                    
                    // 플레이어 캐릭터 가져오기 (공격력 물약 버프 확인용)
                    Game.CharacterSystem.Interface.ICharacter playerCharacter = null;
                    if (card != null && card.IsFromPlayer())
                    {
                        // 플레이어 카드인 경우에만 공격력 물약 버프 확인
                        var playerManager = FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                        if (playerManager != null)
                        {
                            var character = playerManager.GetCharacter();
                            if (character != null && character.IsPlayerControlled())
                            {
                                playerCharacter = character;
                            }
                        }
                    }
                    
                    var model = SkillCardTooltipMapper.FromWithStacks(definition, currentStacks, playerCharacter);
                    descriptionText.text = model?.DescriptionRichText ?? string.Empty;
                }
                else
                {
                    descriptionText.text = string.Empty;
                }
            }

            // 통계 UI는 사용하지 않으므로 비활성화
            if (damageText != null) damageText.gameObject.SetActive(false);
            if (damageIconImage != null) damageIconImage.gameObject.SetActive(false);
            if (resourceCostText != null) resourceCostText.gameObject.SetActive(false);
            if (resourceIconImage != null) resourceIconImage.gameObject.SetActive(false);

			// 효과 정보 업데이트 (동적 생성). 없으면 컨테이너 숨김
			UpdateEffectsInfo(definition);
            if (effectsContainer != null)
            {
                bool hasAnyEffect = HasAnyEffect(definition);
                effectsContainer.gameObject.SetActive(hasAnyEffect);
            }

			// 레이아웃 강제 갱신으로 0 높이 방지 및 후속 위치 계산 안정화
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
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
                bool hadSprite = backgroundImage.sprite != null;
                if (enableVisualDebugValidation)
                {
                    GameLogger.LogInfo($"[TooltipDBG] Background sprite={(hadSprite ? backgroundImage.sprite.name : "<none>")}, type={backgroundImage.type}, colorA={backgroundImage.color.a}", GameLogger.LogCategory.UI);
                }

                if (!hadSprite)
                {
                    // 프리팹에 스프라이트가 없으면 기본 이미지 생성
                    CreateDefaultBackgroundSprite();
                    backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    backgroundImage.gameObject.SetActive(true);
                    if (enableVisualDebugValidation)
                        GameLogger.LogInfo("[TooltipDBG] Background fallback: created 1x1 sprite", GameLogger.LogCategory.UI);
                }
                else
                {
                    // 통합 스프라이트(배경+보더)를 배경으로 쓰는 경우: FillCenter 유지
                    backgroundImage.type = Image.Type.Sliced;
                    if (backgroundImage.color.a < 0.05f)
                    {
                        backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                        if (enableVisualDebugValidation)
                            GameLogger.LogInfo("[TooltipDBG] Background alpha was ~0 → set to 0.95", GameLogger.LogCategory.UI);
                    }
                    backgroundImage.gameObject.SetActive(true);
                }
            }

            // Border 설정 로직 제거

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
        // Border 생성 로직 제거


        /// <summary>
        /// 카드 헤더를 업데이트합니다. (아이콘 + 이름 + 타입)
        /// 프리팹 구조를 존중하며 텍스트와 이미지만 업데이트합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        private void UpdateCardHeader(SkillCardDefinition definition)
        {
            if (definition == null) return;
            if (cardIconImage != null)
            {
                cardIconImage.sprite = definition.artwork;
                cardIconImage.gameObject.SetActive(definition.artwork != null);
            }
            if (cardNameText != null)
            {
                var title = string.IsNullOrEmpty(definition.displayNameKO) ? definition.displayName : definition.displayNameKO;
                cardNameText.text = title;
            }
            // 타입 텍스트는 사용하지 않음
            if (cardTypeText != null)
            {
                cardTypeText.gameObject.SetActive(false);
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

            // 새로운 효과 아이템들 생성(효과가 없으면 컨테이너 비활성)
            var effects = GetCardEffects(definition);
            if (effects.Count == 0)
            {
                effectsContainer.gameObject.SetActive(false);
            }
            else
            {
                effectsContainer.gameObject.SetActive(true);
                foreach (var effect in effects)
                {
                    CreateEffectItem(effect);
                }
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

            // 자원 코스트 표시 (소모)
            if (config.hasResource && config.resourceConfig != null && config.resourceConfig.cost > 0)
            {
                string resourceName = "자원";
                var pm = FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                if (pm != null && !string.IsNullOrEmpty(pm.ResourceName))
                    resourceName = pm.ResourceName;

                effects.Add(new EffectData
                {
                    name = resourceName,
                    description = $"{resourceName} 소모: {config.resourceConfig.cost}",
                    iconColor = Color.red,
                    effectType = EffectType.Special
                });
            }

            // 데미지 효과 (스택 기반 실제 데미지 표시)
            if (config.hasDamage)
            {
                int currentStacks = 0;
                if (currentCard is Game.SkillCardSystem.Interface.IAttackPowerStackProvider stackProvider)
                {
                    currentStacks = stackProvider.GetAttackPowerStack();
                }
                
                // 공격력 물약 버프 계산 (플레이어 카드만)
                int attackPotionBonus = 0;
                if (currentCard != null && currentCard.IsFromPlayer())
                {
                    var playerManager = FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                    if (playerManager != null)
                    {
                        var character = playerManager.GetCharacter();
                        if (character != null && character.IsPlayerControlled())
                        {
                            var buffs = character.GetBuffs();
                            foreach (var effect in buffs)
                            {
                                if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                                {
                                    attackPotionBonus += attackBuff.GetAttackPowerBonus();
                                }
                            }
                        }
                    }
                }
                
                // 강화 보너스 (패시브 성급)
                int enhancementBonus = 0;
                if (currentCard != null && currentCard.IsFromPlayer())
                {
                    // ItemService 조회 (주입이 없을 수 있어 1회 안전 조회)
                    var service = FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                    if (service != null)
                    {
                        string skillId = definition.displayName;
                        enhancementBonus = service.GetSkillDamageBonus(skillId);
                    }
                }

                var baseDmg = config.damageConfig.baseDamage;
                var actualDmg = baseDmg + currentStacks + attackPotionBonus + enhancementBonus; // 선형 합산
                
                string description;
                if (currentStacks > 0 && attackPotionBonus > 0 && enhancementBonus > 0)
                {
                    description = $"현재 데미지: {actualDmg} (기본 {baseDmg} + 스택 {currentStacks} + 물약 {attackPotionBonus} + 강화 {enhancementBonus})";
                }
                else if (currentStacks > 0 && enhancementBonus > 0)
                {
                    description = $"현재 데미지: {actualDmg} (기본 {baseDmg} + 스택 {currentStacks} + 강화 {enhancementBonus})";
                }
                else if (attackPotionBonus > 0 && enhancementBonus > 0)
                {
                    description = $"현재 데미지: {actualDmg} (기본 {baseDmg} + 물약 {attackPotionBonus} + 강화 {enhancementBonus})";
                }
                else if (enhancementBonus > 0)
                {
                    description = $"현재 데미지: {actualDmg} (기본 {baseDmg} + 강화 {enhancementBonus})";
                }
                else if (currentStacks > 0)
                {
                    description = $"현재 데미지: {actualDmg} (기본 {baseDmg} + 스택 {currentStacks})";
                }
                else if (attackPotionBonus > 0)
                {
                    description = $"현재 데미지: {actualDmg} (기본 {baseDmg} + 물약 {attackPotionBonus})";
                }
                else
                {
                    description = $"기본 데미지: {actualDmg}";
                }
                
                effects.Add(new EffectData
                {
                    name = "데미지",
                    description = description,
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

                        // 자원 효과: 플레이어 자원 이름(예: 화살/마나)로 표기
                        if (customSettings.resourceDelta != 0)
                        {
                            string resourceText = customSettings.resourceDelta > 0 ? "획득" : "소모";
                            string resourceName = "자원";
                            // 플레이어 카드인 경우 PlayerManager에서 자원 이름 조회
                            if (currentCard != null && currentCard.IsFromPlayer())
                            {
                                var pm = FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                                if (pm != null && !string.IsNullOrEmpty(pm.ResourceName))
                                {
                                    resourceName = pm.ResourceName;
                                }
                                else
                                {
                                    // 데이터 폴백
                                    var ch = pm?.GetCharacter();
                                    if (ch != null && ch.CharacterData is Game.CharacterSystem.Data.PlayerCharacterData pcd && !string.IsNullOrEmpty(pcd.ResourceName))
                                    {
                                        resourceName = pcd.ResourceName;
                                    }
                                }
                            }

                            effects.Add(new EffectData
                            {
                                name = resourceName,
                                description = $"{resourceName} {resourceText}: {Mathf.Abs(customSettings.resourceDelta)}",
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
            // 툴팁이 속한 캔버스의 카메라 사용(Overlay면 null)
            Camera cameraToUse = null;
            var parentCanvas = rectTransform.GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cameraToUse = parentCanvas.worldCamera;
            }
            var targetParent = rectTransform.parent as RectTransform;
            if (targetParent == null) targetParent = rectTransform; // 방어적 처리

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetParent, // 부모 RectTransform 기준 로컬 좌표로 변환
                cardPosition,
                cameraToUse,
                out cardLocalPoint);

            // 스킬카드 기준으로 툴팁 위치 계산
            Vector2 tooltipPosition = CalculateTooltipPositionRelativeToCard(cardLocalPoint);

            // 화면 경계 내로 제한
            tooltipPosition = ClampToScreenBounds(tooltipPosition, targetParent);

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

            // 툴팁의 실제 크기
            float tooltipWidth = Mathf.Abs(tooltipRect.width);
            float tooltipHeight = Mathf.Abs(tooltipRect.height);

            // 요구: 카드 오른쪽에 붙고, 카드 하단과 툴팁 하단이 일자 → Pivot을 좌하단(0,0)
            rectTransform.pivot = new Vector2(0f, 0f);
            Vector2 tooltipPivot = rectTransform.pivot;

            // 캔버스의 실제 경계 (Canvas 중앙이 0,0인 좌표계)
            float canvasLeft = canvasRect.xMin;
            float canvasRight = canvasRect.xMax;
            float canvasTop = canvasRect.yMax;
            float canvasBottom = canvasRect.yMin;

            // cardLocalPoint는 카드 좌하단 포인트이므로 카드 폭을 고려하여 우측 경계 계산
            float cardWidth = 100f; // 기본값
            if (currentCardRectTransform != null)
            {
                cardWidth = Mathf.Abs(currentCardRectTransform.rect.width);
            }
            float cardRightEdge = cardLocalPoint.x + cardWidth;
            
            float rightSpace = canvasRight - cardRightEdge;
            float leftSpace = cardLocalPoint.x - canvasLeft;

            // 툴팁이 들어갈 공간이 있는지 확인 (간격 포함)
            float tooltipRequiredWidth = tooltipWidth + alignPaddingX;
            bool canShowRight = rightSpace >= tooltipRequiredWidth;
            bool canShowLeft = leftSpace >= tooltipRequiredWidth;

            Vector2 tooltipPosition = cardLocalPoint;

            // 수평 위치 결정 (오른쪽 우선, 부족 시 왼쪽 폴백)
            if (canShowRight)
            {
                // 카드 우측 경계에서 간격을 두고 배치
                tooltipPosition.x = cardRightEdge + alignPaddingX;
            }
            else if (canShowLeft)
            {
                // 좌측 폴백: 카드 좌측에 간격을 두고 배치
                tooltipPosition.x = cardLocalPoint.x - alignPaddingX - tooltipWidth;
            }
            else
            {
                // 양쪽 모두 부족하면 화면 중앙 쪽으로 보정 (하단 정렬 유지)
                var parentRect = rectTransform.parent as RectTransform;
                tooltipPosition.x = Mathf.Clamp(cardLocalPoint.x, parentRect.rect.xMin, parentRect.rect.xMax - tooltipWidth);
            }

            // 수직 정렬: 카드 하단과 툴팁 하단을 일치 (Pivot.y=0)
            tooltipPosition.y = cardLocalPoint.y + alignPaddingY;

            // 툴팁을 최상단으로 이동 (다른 UI 요소 위에 표시)
            rectTransform.SetAsLastSibling();

            // 수직 경계는 요청사항에 따라 하단 정렬을 유지하고 변경하지 않습니다

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
        private Vector2 ClampToScreenBounds(Vector2 position, RectTransform targetParent)
        {
            if (rectTransform == null) return position;

            var parentRectTransform = targetParent != null ? targetParent : (rectTransform.parent as RectTransform);
            if (parentRectTransform == null) parentRectTransform = rectTransform;

            var canvasRect = parentRectTransform.rect;
            var tooltipRect = rectTransform.rect;

            // 좌우 경계만 클램프 (하단 정렬 유지)
            float minX = canvasRect.xMin;
            float maxX = canvasRect.xMax;

            float tooltipLeft = position.x; // pivot.x = 0
            float tooltipRight = position.x + tooltipRect.width;

            if (tooltipRight > maxX)
            {
                position.x = maxX - tooltipRect.width;
            }
            if (position.x < minX)
            {
                position.x = minX;
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
                    if (verboseLogging)
                    {
                        GameLogger.LogInfo("툴팁 표시 완료", GameLogger.LogCategory.UI);
                    }
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
            subTooltipCoroutine = ShowSubTooltipDelayed(effectData, AlignPositionToRightEdge(triggerPosition));
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

            // 서브 툴팁 숨김 (스택 포함)
            if (currentSubTooltip != null)
            {
                Destroy(currentSubTooltip);
                currentSubTooltip = null;
            }
            if (stackedSubTooltips.Count > 0)
            {
                foreach (var go in stackedSubTooltips)
                {
                    if (go != null) Destroy(go);
                }
                stackedSubTooltips.Clear();
            }
            if (effectToSubTooltip.Count > 0)
            {
                effectToSubTooltip.Clear();
            }
            
            currentHoveredEffect = null;
        }

        /// <summary>
        /// 특정 효과의 서브 툴팁만 숨깁니다.
        /// </summary>
        public void HideSubTooltip(EffectData effect)
        {
            if (effect == null) return;
            if (effectToSubTooltip.TryGetValue(effect, out var go) && go != null)
            {
                stackedSubTooltips.Remove(go);
                Destroy(go);
            }
            effectToSubTooltip.Remove(effect);
            // 현재 표시 참조 정리
            if (currentSubTooltip == go) currentSubTooltip = null;
            RepositionSubTooltipStack();
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

            // 서브 툴팁 생성 (스택)
            var newSub = Instantiate(subTooltipPrefab, rectTransform.parent);
            stackedSubTooltips.Add(newSub);
            currentSubTooltip = newSub;
            effectToSubTooltip[effectData] = newSub;

            // 최대 스택 초과 시 가장 오래된 것 제거
            if (subTooltipMaxStack > 0 && stackedSubTooltips.Count > subTooltipMaxStack)
            {
                var oldest = stackedSubTooltips[0];
                stackedSubTooltips.RemoveAt(0);
                // dict에서 해당 항목 제거
                EffectData keyToRemove = null;
                foreach (var kv in effectToSubTooltip)
                {
                    if (kv.Value == oldest) { keyToRemove = kv.Key; break; }
                }
                if (keyToRemove != null) effectToSubTooltip.Remove(keyToRemove);
                if (oldest != null) Destroy(oldest);
            }
            
            // 서브 툴팁 컴포넌트 설정
            var subTooltipComponent = newSub.GetComponent<SubTooltipComponent>();
            if (subTooltipComponent != null)
            {
                // 모델 기반 채움 후 표시
                var model = Game.SkillCardSystem.UI.Mappers.EffectTooltipMapper.From(effectData);
                subTooltipComponent.SetupSubTooltip(effectData, triggerPosition);
                TooltipBuilder.BuildSub(model, subTooltipComponent);
            }
            
            // 스택 정렬(우상단에서 아래로)
            RepositionSubTooltipStack();
            GameLogger.LogInfo($"[SkillCardTooltip] 서브 툴팁 생성 완료: {effectData.name}", GameLogger.LogCategory.UI);
        }

        private void RepositionSubTooltipStack()
        {
            if (rectTransform == null || stackedSubTooltips.Count == 0) return;
            // 메인 툴팁 우상단 로컬 포인트 계산
            var r = rectTransform.rect;
            Vector3 localTopRight = new Vector3(r.width * (1f - rectTransform.pivot.x), r.height * (1f - rectTransform.pivot.y), 0f);
            Vector3 worldTopRight = rectTransform.TransformPoint(localTopRight);
            Vector2 baseScreen = RectTransformUtility.WorldToScreenPoint(null, worldTopRight);

            float yOffset = 0f;
            for (int i = 0; i < stackedSubTooltips.Count; i++)
            {
                var go = stackedSubTooltips[i];
                if (go == null) continue;
                var comp = go.GetComponent<SubTooltipComponent>();
                if (comp == null) continue;

                // 각 서브 툴팁을 메인 우상단에서 아래로 간격 배치
                var screen = new Vector2(baseScreen.x + 20f, baseScreen.y - yOffset);
                comp.UpdatePosition(screen);

                // 다음 아이템 간격 계산: 현재 툴팁 높이를 이용
                var rt = go.GetComponent<RectTransform>();
                float h = rt != null ? Mathf.Abs(rt.rect.height) : 60f;
                yOffset += h + subTooltipStackSpacing;
            }
        }
        /// <summary>
        /// 메인 툴팁의 오른쪽 가장자리 기준으로 서브 툴팁 시작 위치를 정렬합니다.
        /// </summary>
        private Vector2 AlignPositionToRightEdge(Vector2 eventScreenPos)
        {
            if (rectTransform == null) return eventScreenPos;

            // 메인 툴팁의 우측 가장자리 스크린 좌표를 계산
            var parentRect = rectTransform.parent as RectTransform;
            if (parentRect == null) return eventScreenPos;

            var r = rectTransform.rect;
            var p = rectTransform.pivot;
            // 메인 툴팁의 우상단 기준으로 정렬
            Vector3 worldTopRight = rectTransform.TransformPoint(new Vector3(r.width * (1f - p.x), r.height * (1f - p.y), 0f));
            Vector2 rightEdgeScreen = RectTransformUtility.WorldToScreenPoint(null, worldTopRight);

            // 약간의 오프셋을 더해 서브 툴팁이 겹치지 않게 함
            rightEdgeScreen.x += 20f;

            // 수직 정렬: 메인 툴팁 상단 경계에 맞춤
            // rightEdgeScreen.y는 이미 상단 경계를 가리킴

            return rightEdgeScreen;
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
            
            // Border 제거: 서브 툴팁에서도 사용하지 않음
            // [SerializeField] private Image borderImage;

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

                // Border 설정 제거

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
            // Border 생성 제거

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
            public void UpdatePosition(Vector2 triggerPosition)
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

        /// <summary>
        /// 자동 설명을 생성합니다 (가드/출혈/데미지 조합).
        /// </summary>
        private string BuildAutoDescription(SkillCardDefinition definition)
        {
            if (definition?.configuration == null) return "";
            var config = definition.configuration;

            // 직접 데미지
            int baseDamage = 0;
            if (config.hasDamage && config.damageConfig != null)
            {
                baseDamage = Mathf.Max(0, config.damageConfig.baseDamage);
            }

            // 효과 스캔 (커스텀 설정 우선)
            int bleedAmount = 0; int bleedDuration = 0; int guardDuration = 0;
            if (config.hasEffects && config.effects != null)
            {
                foreach (var e in config.effects)
                {
                    if (e == null) continue;
                    var cs = (e.useCustomSettings ? e.customSettings : null);
                    if (cs != null)
                    {
                        // 출혈
                        if (cs.bleedAmount > 0 && cs.bleedDuration > 0)
                        {
                            bleedAmount = cs.bleedAmount;
                            bleedDuration = cs.bleedDuration;
                        }
                        // 가드
                        if (cs.guardDuration > 0)
                        {
                            guardDuration = cs.guardDuration;
                        }
                    }
                }
            }

            // 문구 규칙
            // 가드만 존재
            if (guardDuration > 0 && bleedDuration == 0 && baseDamage == 0)
            {
                return $"{guardDuration}턴 동안 가드 효과를 부여합니다.";
            }

            // 출혈만 존재 (직접 데미지 없음)
            if (bleedDuration > 0 && bleedAmount > 0 && baseDamage == 0)
            {
                return $"{bleedDuration}턴 동안 {bleedAmount}의 데미지를 줍니다.";
            }

            // 출혈 + 직접 데미지
            if (bleedDuration > 0 && bleedAmount > 0 && baseDamage > 0)
            {
                return $"{baseDamage}의 데미지를 줍니다. {bleedDuration}턴 동안 {bleedAmount}의 데미지를 주는 출혈 이펙트를 부여합니다.";
            }

            // 직접 데미지만 존재
            if (baseDamage > 0)
            {
                return $"{baseDamage}의 데미지를 줍니다.";
            }

            // 가드만 (수치 미상) 혹은 기타
            if (guardDuration > 0)
            {
                return "가드 효과를 부여합니다.";
            }

            // 효과 없음 → 기본 문구
            return "기본 효과를 사용합니다.";
        }

        private bool HasAnyEffect(SkillCardDefinition definition)
        {
            var config = definition?.configuration;
            return config != null && config.hasEffects && config.effects != null && config.effects.Count > 0;
        }
    }
}
