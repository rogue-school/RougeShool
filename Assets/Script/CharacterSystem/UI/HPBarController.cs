using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 플레이어 캐릭터의 HP 바를 관리하는 컨트롤러입니다.
    /// 체력 변화에 따라 HP 바의 채움 비율과 색상을 업데이트합니다.
    /// </summary>
    public class HPBarController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("HP Bar Components")]
        [Tooltip("HP 바 배경 (테두리)")]
        [SerializeField] private Image hpBarBackground;
        
        [Tooltip("HP 바 채움 부분")]
        [SerializeField] private Image hpBarFill;
        
        [Tooltip("HP 바 장식 테두리")]
        [SerializeField] private Image hpBarBorder;
        
        [Tooltip("HP 텍스트 (선택사항)")]
        [SerializeField] private TextMeshProUGUI hpText;
        
        [Tooltip("HP 바 글로우 효과")]
        [SerializeField] private Image hpBarGlow;

        [Header("HP Bar Settings")]
        [Tooltip("풀피일 때의 색상")]
        [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.8f, 0.2f, 1f); // 녹색
        
        [Tooltip("중간 체력일 때의 색상")]
        [SerializeField] private Color halfHealthColor = new Color(1f, 0.8f, 0.2f, 1f); // 노란색
        
        [Tooltip("낮은 체력일 때의 색상")]
        [SerializeField] private Color lowHealthColor = new Color(0.8f, 0.2f, 0.2f, 1f); // 빨간색
        
        [Tooltip("HP 바 변화 애니메이션 속도")]
        [SerializeField] private float animationSpeed = 2f;

        [Header("표시 모드")]
        [Tooltip("플레이어/적 표시 모드")]
        [SerializeField] private DisplayMode displayMode = DisplayMode.Player;
        
        [Tooltip("숫자 표기 사용(적 일반은 비권장)")]
        [SerializeField] private bool showNumbers = true;
        
        [Header("Visual Effects")]
        [Tooltip("글로우 효과 활성화")]
        [SerializeField] private bool enableGlowEffect = true;
        
        [Tooltip("글로우 효과 강도")]
        [SerializeField] private float glowIntensity = 1.5f;
        
        [Tooltip("글로우 효과 색상")]
        [SerializeField] private Color glowColor = new Color(1f, 1f, 1f, 0.3f);
        
        [Tooltip("테두리 강조 효과")]
        [SerializeField] private bool enableBorderHighlight = true;

        [Header("버프/디버프 아이콘 (적 캐릭터용)")]
        [Tooltip("버프/디버프 아이콘들을 담을 부모 오브젝트")]
        [SerializeField] private Transform buffDebuffParent;
        
        [Tooltip("버프/디버프 아이콘 프리팹")]
        [SerializeField] private GameObject buffDebuffIconPrefab;

        #endregion

        #region Private Fields

        private ICharacter targetCharacter;
        private float targetFillAmount;
        private bool isAnimating = false;
        private Color originalGlowColor;
        private Color originalBorderColor;

        // 이벤트 구독 여부
        private bool isSubscribed = false;
        
        // 버프/디버프 아이콘 관리
        private System.Collections.Generic.Dictionary<string, GameObject> activeBuffDebuffIcons = new();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 원본 색상 저장
            if (hpBarGlow != null)
                originalGlowColor = hpBarGlow.color;
            if (hpBarBorder != null)
                originalBorderColor = hpBarBorder.color;

            // 이미지가 없거나 타입이 잘못된 경우 자동 구성(스프라이트 없이도 색상만으로 동작)
            ConfigureHpImagesForFallback();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            // HP 바 애니메이션 처리
            if (isAnimating && hpBarFill != null)
            {
                hpBarFill.fillAmount = Mathf.Lerp(hpBarFill.fillAmount, targetFillAmount, 
                    animationSpeed * Time.deltaTime);
                
                // 애니메이션 완료 체크
                if (Mathf.Abs(hpBarFill.fillAmount - targetFillAmount) < 0.01f)
                {
                    hpBarFill.fillAmount = targetFillAmount;
                    isAnimating = false;
                }
            }
            
            // 글로우 효과 애니메이션
            if (enableGlowEffect && hpBarGlow != null)
            {
                UpdateGlowEffect();
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 어떤 캐릭터(ICharacter)와도 연결하여 HP 바를 초기화합니다.
        /// </summary>
        /// <param name="character">연결할 캐릭터</param>
        public void Initialize(ICharacter character)
        {
            Unsubscribe();
            targetCharacter = character;

            if (targetCharacter == null)
            {
                Debug.LogWarning("[HPBarController] Initialize(ICharacter) - character가 null입니다.");
                return;
            }

            Subscribe();
            UpdateHPBar();
        }

        /// <summary>
        /// 기존 호환: PlayerCharacter 전용 초기화.
        /// </summary>
        public void Initialize(PlayerCharacter character)
        {
            Initialize((ICharacter)character);
        }

        /// <summary>
        /// 적 캐릭터 전용 초기화 (DisplayMode를 EnemyNormal으로 설정).
        /// </summary>
        public void Initialize(EnemyCharacter character)
        {
            if (character == null)
            {
                Debug.LogWarning("[HPBarController] Initialize(EnemyCharacter) - character가 null입니다.");
                return;
            }
            
            // 적 캐릭터용 표시 모드 설정
            displayMode = DisplayMode.EnemyNormal;
            showNumbers = false; // 적은 숫자 표시하지 않음
            
            Initialize((ICharacter)character);
        }

        #endregion

        #region HP 바 업데이트

        /// <summary>
        /// HP 바를 현재 체력에 맞게 업데이트합니다.
        /// </summary>
        public void UpdateHPBar()
        {
            if (targetCharacter == null || hpBarFill == null) return;

            int currentHP = targetCharacter.GetCurrentHP();
            int maxHP = targetCharacter.GetMaxHP();
            
            if (maxHP <= 0) return;

            float hpRatio = (float)currentHP / maxHP;
            
            // HP 바 채움 비율 설정 (애니메이션과 함께)
            targetFillAmount = hpRatio;
            isAnimating = true;

            // HP 비율에 따른 색상 변화
            Color targetColor = GetHealthColor(hpRatio);
            hpBarFill.color = targetColor;
            
            // 시각적 효과 업데이트
            UpdateVisualEffects(hpRatio, targetColor);

            // HP 텍스트 업데이트 (선택사항)
            UpdateHPText(currentHP, maxHP);
        }

        /// <summary>
        /// 스프라이트가 없어도 동작하도록 기본 스프라이트/타입을 설정합니다.
        /// </summary>
        private void ConfigureHpImagesForFallback()
        {
            EnsureSpriteIfMissing(hpBarBackground);
            if (hpBarFill != null)
            {
                EnsureSpriteIfMissing(hpBarFill);
                if (hpBarFill.type != Image.Type.Filled)
                {
                    hpBarFill.type = Image.Type.Filled;
                    hpBarFill.fillMethod = Image.FillMethod.Horizontal;
                    hpBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                }
            }
        }

        /// <summary>
        /// 이미지에 스프라이트가 없으면 Unity 기본 UISprite로 채웁니다.
        /// </summary>
        private void EnsureSpriteIfMissing(Image img)
        {
            if (img == null) return;
            if (img.sprite == null)
            {
                // 환경 의존성 제거: 1x1 단색 스프라이트를 즉석 생성
                var tex = Texture2D.whiteTexture;
                var rect = new Rect(0, 0, 1, 1);
                var pivot = new Vector2(0.5f, 0.5f);
                var generated = Sprite.Create(tex, rect, pivot, 1f);
                img.sprite = generated;
            }
        }

        /// <summary>
        /// HP 비율에 따른 색상을 반환합니다.
        /// </summary>
        /// <param name="hpRatio">HP 비율 (0.0 ~ 1.0)</param>
        /// <returns>해당하는 색상</returns>
        private Color GetHealthColor(float hpRatio)
        {
            if (hpRatio > 0.6f)
            {
                // 풀피 ~ 중간 체력: 녹색에서 노란색으로
                return Color.Lerp(halfHealthColor, fullHealthColor, (hpRatio - 0.6f) / 0.4f);
            }
            else if (hpRatio > 0.3f)
            {
                // 중간 체력 ~ 낮은 체력: 노란색에서 빨간색으로
                return Color.Lerp(lowHealthColor, halfHealthColor, (hpRatio - 0.3f) / 0.3f);
            }
            else
            {
                // 낮은 체력: 빨간색
                return lowHealthColor;
            }
        }

        /// <summary>
        /// HP 텍스트를 업데이트합니다.
        /// </summary>
        /// <param name="currentHP">현재 체력</param>
        /// <param name="maxHP">최대 체력</param>
        private void UpdateHPText(int currentHP, int maxHP)
        {
            if (hpText == null) return;
            if (!showNumbers && displayMode != DisplayMode.Player)
            {
                hpText.text = string.Empty;
                return;
            }
            hpText.text = $"{currentHP}/{maxHP}";
            // 텍스트 색상도 HP 상태에 따라 변경
            hpText.color = GetHealthColor((float)currentHP / maxHP);
        }

        /// <summary>
        /// 시각적 효과들을 업데이트합니다.
        /// </summary>
        /// <param name="hpRatio">HP 비율</param>
        /// <param name="healthColor">현재 체력 색상</param>
        private void UpdateVisualEffects(float hpRatio, Color healthColor)
        {
            // 글로우 효과 업데이트
            if (enableGlowEffect && hpBarGlow != null)
            {
                Color glowColorWithAlpha = new Color(healthColor.r, healthColor.g, healthColor.b, glowColor.a);
                hpBarGlow.color = glowColorWithAlpha;
            }
            
            // 테두리 강조 효과
            if (enableBorderHighlight && hpBarBorder != null)
            {
                // HP가 낮을 때 테두리를 더 밝게
                float borderIntensity = Mathf.Lerp(1.5f, 1f, hpRatio);
                Color borderColor = new Color(originalBorderColor.r * borderIntensity, 
                                            originalBorderColor.g * borderIntensity, 
                                            originalBorderColor.b * borderIntensity, 
                                            originalBorderColor.a);
                hpBarBorder.color = borderColor;
            }
        }

        /// <summary>
        /// 글로우 효과를 애니메이션합니다.
        /// </summary>
        private void UpdateGlowEffect()
        {
            if (hpBarGlow == null) return;
            
            // 부드러운 글로우 펄스 효과
            float pulse = Mathf.Sin(Time.time * 2f) * 0.1f + 0.9f;
            Color currentGlowColor = hpBarGlow.color;
            currentGlowColor.a = glowColor.a * pulse * glowIntensity;
            hpBarGlow.color = currentGlowColor;
        }

        #endregion

        #region 이벤트 처리

        /// <summary>
        /// 체력이 변경될 때 호출되는 메서드
        /// </summary>
        public void OnHealthChanged()
        {
            UpdateHPBar();
        }

        /// <summary>
        /// HP 바를 즉시 업데이트합니다 (애니메이션 없이)
        /// </summary>
        public void UpdateHPBarImmediate()
        {
            if (targetCharacter == null || hpBarFill == null) return;

            int currentHP = targetCharacter.GetCurrentHP();
            int maxHP = targetCharacter.GetMaxHP();
            
            if (maxHP <= 0) return;

            float hpRatio = (float)currentHP / maxHP;
            
            // 즉시 업데이트 (애니메이션 없이)
            hpBarFill.fillAmount = hpRatio;
            targetFillAmount = hpRatio;
            isAnimating = false;

            // 색상 업데이트
            Color healthColor = GetHealthColor(hpRatio);
            hpBarFill.color = healthColor;
            
            // 시각적 효과 업데이트
            UpdateVisualEffects(hpRatio, healthColor);
            
            // 텍스트 업데이트
            UpdateHPText(currentHP, maxHP);
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// HP 바 설정을 업데이트합니다.
        /// </summary>
        /// <param name="fullColor">풀피 색상</param>
        /// <param name="halfColor">중간 체력 색상</param>
        /// <param name="lowColor">낮은 체력 색상</param>
        /// <param name="speed">애니메이션 속도</param>
        public void UpdateSettings(Color fullColor, Color halfColor, Color lowColor, float speed)
        {
            fullHealthColor = fullColor;
            halfHealthColor = halfColor;
            lowHealthColor = lowColor;
            animationSpeed = speed;
        }

        /// <summary>
        /// HP 바를 숨기거나 보이게 합니다.
        /// </summary>
        /// <param name="visible">보이기 여부</param>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        #endregion

        #region 이벤트 구독/해제

        private void Subscribe()
        {
            if (targetCharacter == null || isSubscribed) return;
            targetCharacter.OnHPChanged += OnTargetHpChanged;
            
            // 버프/디버프 이벤트도 구독 (적 캐릭터용)
            if (targetCharacter is EnemyCharacter enemyCharacter)
            {
                enemyCharacter.OnBuffsChanged += OnBuffsChangedHandler;
            }
            
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (targetCharacter == null || !isSubscribed) return;
            targetCharacter.OnHPChanged -= OnTargetHpChanged;
            
            // 버프/디버프 이벤트 구독 해제
            if (targetCharacter is EnemyCharacter enemyCharacter)
            {
                enemyCharacter.OnBuffsChanged -= OnBuffsChangedHandler;
            }
            
            isSubscribed = false;
        }

        private void OnTargetHpChanged(int current, int max)
        {
            UpdateHPBar();
        }
        
        private void OnBuffsChangedHandler(System.Collections.Generic.IReadOnlyList<Game.SkillCardSystem.Interface.IPerTurnEffect> effects)
        {
            UpdateBuffDebuffIcons(effects);
        }

        #endregion

        #region 버프/디버프 시스템

        /// <summary>
        /// 버프/디버프 아이콘을 추가합니다.
        /// </summary>
        /// <param name="effectId">효과 ID</param>
        /// <param name="iconSprite">아이콘 스프라이트</param>
        /// <param name="isBuff">버프 여부 (true: 버프, false: 디버프)</param>
        /// <param name="duration">지속 시간 (초, -1이면 영구)</param>
        public void AddBuffDebuffIcon(string effectId, Sprite iconSprite, bool isBuff, float duration = -1f)
        {
            if (buffDebuffParent == null || buffDebuffIconPrefab == null) return;

            // 이미 같은 효과가 있으면 제거
            RemoveBuffDebuffIcon(effectId);

            // 새 아이콘 생성
            GameObject iconObj = Instantiate(buffDebuffIconPrefab, buffDebuffParent);
            var iconImage = iconObj.GetComponent<Image>();
            var iconText = iconObj.GetComponentInChildren<TextMeshProUGUI>();

            if (iconImage != null)
                iconImage.sprite = iconSprite;
            
            if (iconText != null && duration > 0)
                iconText.text = duration.ToString("F0");

            // 색상 설정 (버프: 파란색, 디버프: 빨간색)
            if (iconImage != null)
                iconImage.color = isBuff ? Color.blue : Color.red;

            // 딕셔너리에 저장
            activeBuffDebuffIcons[effectId] = iconObj;

            GameLogger.LogInfo($"[HPBarController] {(isBuff ? "버프" : "디버프")} 아이콘 추가: {effectId}", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 버프/디버프 아이콘을 제거합니다.
        /// </summary>
        /// <param name="effectId">효과 ID</param>
        public void RemoveBuffDebuffIcon(string effectId)
        {
            if (activeBuffDebuffIcons.TryGetValue(effectId, out GameObject iconObj))
            {
                Destroy(iconObj);
                activeBuffDebuffIcons.Remove(effectId);
                GameLogger.LogInfo($"[HPBarController] 버프/디버프 아이콘 제거: {effectId}", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 모든 버프/디버프 아이콘을 제거합니다.
        /// </summary>
        public void ClearAllBuffDebuffIcons()
        {
            foreach (var icon in activeBuffDebuffIcons.Values)
            {
                if (icon != null)
                    Destroy(icon);
            }
            activeBuffDebuffIcons.Clear();
            GameLogger.LogInfo("[HPBarController] 모든 버프/디버프 아이콘 제거", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 버프/디버프 효과 목록을 업데이트합니다.
        /// </summary>
        /// <param name="effects">효과 목록</param>
        public void UpdateBuffDebuffIcons(System.Collections.Generic.IReadOnlyList<Game.SkillCardSystem.Interface.IPerTurnEffect> effects)
        {
            if (buffDebuffParent == null) return;
            
            // 모두 제거 후 다시 구성(간단/안전)
            foreach (Transform child in buffDebuffParent)
            {
                if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
            }

            foreach (var e in effects)
            {
                if (e.Icon == null)
                {
                    GameLogger.LogWarning("[HPBarController] 효과 아이콘이 비어 있습니다. SO에 Sprite가 지정되었는지 확인하세요.", GameLogger.LogCategory.UI);
                }
                var slotObj = Instantiate(buffDebuffIconPrefab, buffDebuffParent);
                var view = slotObj.GetComponent<BuffDebuffSlotView>();
                if (view != null)
                {
                    view.SetData(e.Icon, e.RemainingTurns);
                }
                else
                {
                    // 최소 폴백: Image에 직접 아이콘만 지정
                    var img = slotObj.GetComponent<Image>();
                    if (img != null) img.sprite = e.Icon;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// HP 바 표시 모드
    /// </summary>
    public enum DisplayMode
    {
        Player,
        EnemyNormal,
        EnemyBoss
    }
}
