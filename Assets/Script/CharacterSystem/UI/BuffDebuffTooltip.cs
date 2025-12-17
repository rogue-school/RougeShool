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
        /// <param name="targetRect">대상 슬롯의 RectTransform (선택적)</param>
        public void ShowTooltip(IPerTurnEffect effect, Vector2 slotPosition, RectTransform targetRect = null)
        {
            if (effect == null)
            {
                GameLogger.LogWarning("[BuffDebuffTooltip] 표시할 효과가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            currentEffect = effect;
            currentTargetRect = targetRect;
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
            UpdatePosition(slotPosition);

            if (!isVisible)
            {
                FadeIn();
            }
            else
            {
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

            // 턴 정보는 사용하지 않음
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
        /// 턴 정보는 사용하지 않습니다.
        /// </summary>
        /// <param name="effect">효과</param>
        private void UpdateTurnInfo(IPerTurnEffect effect)
        {
            // 턴 정보 텍스트 비활성화
            if (remainingTurnsText != null)
            {
                remainingTurnsText.gameObject.SetActive(false);
            }

            // 턴 정보 컨테이너 비활성화
            if (turnInfoContainer != null)
            {
                turnInfoContainer.gameObject.SetActive(false);
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

            // 액티브 아이템 효과인 경우 아이템 이름을 우선 표시
            if (effect is Game.ItemSystem.Interface.IItemPerTurnEffect)
            {
                string itemName = GetItemNameFromEffect(effect);
                if (!string.IsNullOrEmpty(itemName))
                {
                    return itemName;
                }
            }

            // 스킬 카드 효과인 경우 원본 SO의 effectName을 우선 표시
            string effectSOName = GetEffectNameFromSO(effect);
            if (!string.IsNullOrEmpty(effectSOName))
            {
                return effectSOName;
            }

            string effectTypeName = effect.GetType().Name;
            
            switch (effectTypeName)
            {
                case "BleedEffect":
                    return "출혈";
                case "StunEffect":
                case "StunDebuff":
                    return "기절";
                case "GuardBuff":
                    return "가드";
                case "CounterBuff":
                    return "반격";
                case "HealEffect":
                    return "치유";
                case "AttackPowerBuffEffect":
                    return "공격력 증가";
                default:
                    return effectTypeName.Replace("Effect", "").Replace("Buff", "");
            }
        }

        /// <summary>
        /// 효과에서 원본 SkillCardEffectSO의 effectName을 가져옵니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>효과 SO 이름 (없으면 null)</returns>
        private string GetEffectNameFromSO(IPerTurnEffect effect)
        {
            if (effect == null) return null;

            try
            {
                var effectType = effect.GetType();
                
                // 리플렉션으로 SourceEffectSO, EffectSO 등의 필드/프로퍼티 찾기
                var fieldNames = new[] { "SourceEffectSO", "EffectSO", "sourceEffectSO", "effectSO", "SourceEffectName", "EffectName", "sourceEffectName", "effectName" };
                
                foreach (var fieldName in fieldNames)
                {
                    // 필드 확인
                    var field = effectType.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var value = field.GetValue(effect);
                        if (value is Game.SkillCardSystem.Effect.SkillCardEffectSO effectSO)
                        {
                            string name = effectSO.GetEffectName();
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                return name;
                            }
                        }
                        else if (value is string nameStr && !string.IsNullOrEmpty(nameStr))
                        {
                            return nameStr;
                        }
                    }
                    
                    // 프로퍼티 확인
                    var property = effectType.GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (property != null && property.CanRead)
                    {
                        var value = property.GetValue(effect);
                        if (value is Game.SkillCardSystem.Effect.SkillCardEffectSO effectSO)
                        {
                            string name = effectSO.GetEffectName();
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                return name;
                            }
                        }
                        else if (value is string nameStr && !string.IsNullOrEmpty(nameStr))
                        {
                            return nameStr;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[BuffDebuffTooltip] 효과 SO 이름 추출 중 오류: {ex.Message}", GameLogger.LogCategory.UI);
            }

            return null;
        }

        /// <summary>
        /// 효과에서 원본 아이템 이름을 가져옵니다 (리플렉션 사용).
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>아이템 이름 (없으면 null)</returns>
        private string GetItemNameFromEffect(IPerTurnEffect effect)
        {
            if (effect == null) return null;

            try
            {
                var effectType = effect.GetType();
                
                // 리플렉션으로 SourceItemName, ItemName, ActiveItemName 등의 필드/프로퍼티 찾기
                var fieldNames = new[] { "SourceItemName", "ItemName", "ActiveItemName", "sourceItemName", "itemName", "activeItemName" };
                
                foreach (var fieldName in fieldNames)
                {
                    var field = effectType.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var value = field.GetValue(effect) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                    
                    var property = effectType.GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (property != null && property.CanRead)
                    {
                        var value = property.GetValue(effect) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[BuffDebuffTooltip] 아이템 이름 추출 중 오류: {ex.Message}", GameLogger.LogCategory.UI);
            }

            return null;
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
            int remainingTurns = effect.RemainingTurns;
            
            // 리플렉션으로 데미지/치유량 등 수치 값 가져오기
            // BleedEffect의 경우 "amount" 필드 (소문자)를 찾아야 함
            int damageValue = GetEffectValue(effect, new[] { "amount", "Amount", "Value", "DamagePerTurn", "Damage", "BleedAmount" });
            int healValue = GetEffectValue(effect, new[] { "healAmount", "HealPerTurn", "HealAmount" });
            
            switch (effectTypeName)
            {
                case "BleedEffect":
                    if (damageValue > 0)
                        return $"매 턴이 시작할때마다 피해를 입힙니다.\n{damageValue}의 피해를 입히며 {remainingTurns}턴이 남았습니다.";
                    else
                        return $"매 턴이 시작할때마다 피해를 입힙니다.\n피해를 입히며 {remainingTurns}턴이 남았습니다.";
                        
                case "StunEffect":
                case "StunDebuff":
                    return $"매 턴이 시작할때마다 작동합니다.\n효과가 {remainingTurns}턴이 남았습니다.";
                    
                case "GuardBuff":
                    return $"받는 모든 데미지와 상태이상을 무효화합니다.\n자신의 턴이 시작할 때마다 턴 수가 감소하며 {remainingTurns}턴이 남았습니다.";
                    
                case "CounterBuff":
                    return $"공격을 받으면 반격합니다.\n자신의 턴이 시작할 때마다 턴 수가 감소하며 {remainingTurns}턴이 남았습니다.";
                    
                case "HealEffect":
                    if (healValue > 0)
                        return $"매 턴이 시작할때마다 작동합니다.\n{healValue}의 체력을 회복하며 {remainingTurns}턴이 남았습니다.";
                    else
                        return $"매 턴이 시작할때마다 작동합니다.\n체력을 회복하며 {remainingTurns}턴이 남았습니다.";

                case "AttackPowerBuffEffect":
                {
                    int bonus = GetEffectValue(effect, new[] { "AttackPowerBonus" });
                    if (bonus > 0)
                    {
                        return $"공격력이 {bonus} 증가한 상태가 {remainingTurns}턴 동안 유지됩니다.";
                    }
                    return $"공격력이 증가한 상태가 {remainingTurns}턴 동안 유지됩니다.";
                }
                        
                default:
                    return $"효과가 {remainingTurns}턴 동안 유지됩니다.";
            }
        }
        
        /// <summary>
        /// 리플렉션을 사용하여 효과의 수치 값을 가져옵니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <param name="propertyNames">찾을 프로퍼티 이름 배열</param>
        /// <returns>찾은 수치 값 (없으면 0)</returns>
        private int GetEffectValue(IPerTurnEffect effect, string[] propertyNames)
        {
            if (effect == null) return 0;
            
            var effectType = effect.GetType();
            
            foreach (var propName in propertyNames)
            {
                var prop = effectType.GetProperty(propName, 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (prop != null && prop.CanRead)
                {
                    var value = prop.GetValue(effect);
                    if (value is int intValue)
                        return intValue;
                    if (value is float floatValue)
                        return Mathf.RoundToInt(floatValue);
                }
                
                // 프로퍼티가 없으면 필드 찾기
                var field = effectType.GetField(propName, 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    var value = field.GetValue(effect);
                    if (value is int intValue)
                        return intValue;
                    if (value is float floatValue)
                        return Mathf.RoundToInt(floatValue);
                }
            }
            
            return 0;
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
        /// <param name="slotPosition">슬롯의 위치 (스크린 좌표)</param>
        public void UpdatePosition(Vector2 slotPosition)
        {
            if (rectTransform == null) return;
            
            // 툴팁이 활성화되지 않았으면 위치만 설정 (표시되지 않음)
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            // 슬롯 위치를 캔버스 로컬 좌표로 변환
            var targetParent = rectTransform.parent as RectTransform;
            if (targetParent == null) return;

            // 툴팁이 속한 캔버스의 카메라 확인
            Camera cameraToUse = null;
            var parentCanvas = rectTransform.GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cameraToUse = parentCanvas.worldCamera;
            }

            Vector2 slotLocalPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetParent,
                slotPosition,
                cameraToUse,
                out slotLocalPoint))
            {
                return; // 변환 실패
            }

            // 슬롯 기준으로 툴팁 위치 계산
            Vector2 tooltipPosition = CalculateTooltipPositionRelativeToSlot(slotLocalPoint);

            // 화면 경계 내로 제한
            tooltipPosition = ClampToScreenBounds(tooltipPosition, targetParent);

            rectTransform.localPosition = tooltipPosition;
            
            // 다른 UI 위에 표시
            rectTransform.SetAsLastSibling();
        }

        /// <summary>
        /// 슬롯 기준으로 툴팁 위치를 계산합니다. (SkillCardTooltip과 동일한 정책)
        /// </summary>
        /// <param name="slotLocalPoint">슬롯의 로컬 좌표 (좌하단 기준)</param>
        /// <returns>툴팁의 로컬 좌표</returns>
        private Vector2 CalculateTooltipPositionRelativeToSlot(Vector2 slotLocalPoint)
        {
            if (rectTransform == null) return slotLocalPoint;

            var canvasRect = rectTransform.parent.GetComponent<RectTransform>().rect;
            var tooltipRect = rectTransform.rect;

            // 툴팁의 실제 크기
            float tooltipWidth = Mathf.Abs(tooltipRect.width);
            float tooltipHeight = Mathf.Abs(tooltipRect.height);

            // SkillCardTooltip과 동일: 좌하단 pivot 사용
            rectTransform.pivot = new Vector2(0f, 0f);
            Vector2 tooltipPivot = rectTransform.pivot;

            // 슬롯 크기 계산 (currentTargetRect가 있으면 사용, 없으면 기본값)
            float slotWidth = 50f;
            if (currentTargetRect != null)
            {
                slotWidth = Mathf.Abs(currentTargetRect.rect.width);
            }

            // 캔버스의 실제 경계
            float canvasLeft = canvasRect.xMin;
            float canvasRight = canvasRect.xMax;
            float canvasTop = canvasRect.yMax;
            float canvasBottom = canvasRect.yMin;

            // 슬롯 우측 경계 계산 (slotLocalPoint는 슬롯 좌하단)
            float slotRightEdge = slotLocalPoint.x + slotWidth;
            
            // 슬롯 위치 기준 여유 공간 계산
            float rightSpace = canvasRight - slotRightEdge;
            float leftSpace = slotLocalPoint.x - canvasLeft;

            // 툴팁이 들어갈 공간이 있는지 확인 (간격 포함)
            float tooltipRequiredWidth = tooltipWidth + slotOffsetX;
            bool canShowRight = rightSpace >= tooltipRequiredWidth;
            bool canShowLeft = leftSpace >= tooltipRequiredWidth;

            Vector2 tooltipPosition = slotLocalPoint;

            // 수평 위치 결정 (오른쪽 우선, 부족 시 왼쪽 폴백)
            if (canShowRight)
            {
                // 슬롯 우측 경계에서 간격을 두고 배치
                tooltipPosition.x = slotRightEdge + slotOffsetX;
            }
            else if (canShowLeft)
            {
                // 좌측 폴백: 슬롯 좌측에 간격을 두고 배치
                tooltipPosition.x = slotLocalPoint.x - slotOffsetX - tooltipWidth;
            }
            else
            {
                // 양쪽 모두 부족하면 중앙 쪽으로 클램프
                tooltipPosition.x = Mathf.Clamp(slotLocalPoint.x, canvasLeft, canvasRight - tooltipWidth);
            }

            // 수직 위치: 슬롯 하단과 맞춤 + 오프셋
            tooltipPosition.y = slotLocalPoint.y + slotOffsetY;

            // 수직 경계 체크 및 조정
            float tooltipTop = tooltipPosition.y + tooltipHeight;
            float tooltipBottom = tooltipPosition.y;

            if (tooltipTop > canvasTop)
            {
                tooltipPosition.y = canvasTop - tooltipHeight;
            }
            else if (tooltipBottom < canvasBottom)
            {
                tooltipPosition.y = canvasBottom;
            }

            return tooltipPosition;
        }
        
        private RectTransform currentTargetRect;

        /// <summary>
        /// 위치를 화면 경계 내로 제한합니다.
        /// </summary>
        /// <param name="position">원본 위치</param>
        /// <param name="targetParent">대상 부모 RectTransform</param>
        /// <returns>제한된 위치</returns>
        private Vector2 ClampToScreenBounds(Vector2 position, RectTransform targetParent)
        {
            if (rectTransform == null || targetParent == null) return position;

            var canvasRect = targetParent.rect;
            var tooltipRect = rectTransform.rect;

            // X축 제한
            float tooltipWidth = Mathf.Abs(tooltipRect.width);
            float tooltipHeight = Mathf.Abs(tooltipRect.height);
            
            if (position.x + tooltipWidth > canvasRect.xMax)
            {
                position.x = canvasRect.xMax - tooltipWidth;
            }
            if (position.x < canvasRect.xMin)
            {
                position.x = canvasRect.xMin;
            }

            // Y축 제한
            if (position.y + tooltipHeight > canvasRect.yMax)
            {
                position.y = canvasRect.yMax - tooltipHeight;
            }
            if (position.y < canvasRect.yMin)
            {
                position.y = canvasRect.yMin;
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
