using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.UI
{
    /// <summary>
    /// 아이템 툴팁을 표시하는 UI 컴포넌트입니다.
    /// 아이템 이름, 설명, 효과 등을 표시합니다.
    /// </summary>
    public class ItemTooltip : MonoBehaviour
    {
        #region Serialized Fields

        [Header("툴팁 배경")]
        [Tooltip("툴팁 배경 이미지")]
        [SerializeField] private Image backgroundImage;

        [Header("아이템 헤더")]
        [Tooltip("아이템 아이콘")]
        [SerializeField] private Image itemIconImage;
        
        [Tooltip("아이템 이름 텍스트")]
        [SerializeField] private TextMeshProUGUI itemNameText;

        [Header("아이템 정보")]
        [Tooltip("아이템 설명 텍스트")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인 시간")]
        [SerializeField] private float fadeInDuration = 0.2f;
        
        [Tooltip("페이드 아웃 시간")]
        [SerializeField] private float fadeOutDuration = 0.15f;
        
        [Tooltip("애니메이션 이징")]
        [SerializeField] private Ease fadeEase = Ease.OutQuad;

        #endregion

        #region Private Fields

        private ActiveItemDefinition currentItem;
        private PassiveItemDefinition currentPassiveItem;
        private int currentEnhancementLevel = 1;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 초기 상태 설정
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 툴팁에 아이템 정보를 표시합니다.
        /// </summary>
        /// <param name="item">표시할 아이템</param>
        public void Show(ActiveItemDefinition item)
        {
            if (item == null)
            {
                GameLogger.LogWarning("[ItemTooltip] 아이템이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            // 오브젝트를 활성화하여 보이도록 함
            gameObject.SetActive(true);

            currentItem = item;
            currentPassiveItem = null;
            currentEnhancementLevel = 1;
            UpdateTooltipContent();
            
            // 페이드 인 애니메이션
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeEase);
            
            GameLogger.LogInfo($"[ItemTooltip] 툴팁 표시: {item.DisplayName}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁에 패시브 아이템 정보를 표시합니다.
        /// </summary>
        /// <param name="item">표시할 패시브 아이템</param>
        /// <param name="enhancementLevel">강화 단계</param>
        public void Show(PassiveItemDefinition item, int enhancementLevel = 1)
        {
            if (item == null)
            {
                GameLogger.LogWarning("[ItemTooltip] 패시브 아이템이 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            // 오브젝트를 활성화하여 보이도록 함
            gameObject.SetActive(true);

            currentPassiveItem = item;
            currentEnhancementLevel = Mathf.Clamp(enhancementLevel, 1, 3);
            currentItem = null;
            UpdatePassiveItemTooltipContent();
            
            // 페이드 인 애니메이션
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeEase);
            
            GameLogger.LogInfo($"[ItemTooltip] 패시브 아이템 툴팁 표시: {item.DisplayName}, 강화 단계: {currentEnhancementLevel}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        public void Hide()
        {
            if (gameObject == null) return;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, fadeOutDuration)
                .SetEase(fadeEase)
                .OnComplete(() =>
                {
                    if (gameObject != null)
                    {
                        gameObject.SetActive(false);
                    }
                });
            
            GameLogger.LogInfo("[ItemTooltip] 툴팁 숨김", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 툴팁 내용을 업데이트합니다.
        /// </summary>
        private void UpdateTooltipContent()
        {
            if (currentItem == null) return;

            // 아이템 이름
            if (itemNameText != null)
            {
                itemNameText.text = currentItem.DisplayName;
            }

            // 아이템 아이콘
            if (itemIconImage != null && currentItem.Icon != null)
            {
                itemIconImage.sprite = currentItem.Icon;
                itemIconImage.enabled = true;
            }
            else if (itemIconImage != null)
            {
                itemIconImage.enabled = false;
            }

            // 아이템 설명 (효과 포함)
            UpdateEffects();
        }

        /// <summary>
        /// 패시브 아이템 툴팁 내용을 업데이트합니다.
        /// </summary>
        private void UpdatePassiveItemTooltipContent()
        {
            if (currentPassiveItem == null) return;

            // 아이템 이름 (강화 단계 표시 - 별(★) 사용)
            if (itemNameText != null)
            {
                string name = currentPassiveItem.DisplayName;
                if (currentEnhancementLevel > 0)
                {
                    name += $" {new string('★', Mathf.Clamp(currentEnhancementLevel, 1, Game.ItemSystem.Constants.ItemConstants.MAX_ENHANCEMENT_LEVEL))}";
                }
                itemNameText.text = name;
            }

            // 아이템 아이콘
            if (itemIconImage != null && currentPassiveItem.Icon != null)
            {
                itemIconImage.sprite = currentPassiveItem.Icon;
                itemIconImage.enabled = true;
            }
            else if (itemIconImage != null)
            {
                itemIconImage.enabled = false;
            }

            // 패시브 아이템 설명
            UpdatePassiveItemDescription();
        }

        /// <summary>
        /// 패시브 아이템 설명을 업데이트합니다.
        /// </summary>
        private void UpdatePassiveItemDescription()
        {
            if (currentPassiveItem == null || descriptionText == null) return;

            var builder = new System.Text.StringBuilder();

            // 강화 단계별 누적 보너스 계산
            int totalBonus = 0;
            if (currentPassiveItem.EnhancementIncrements.Length > 0)
            {
                for (int i = 0; i < currentEnhancementLevel && i < currentPassiveItem.EnhancementIncrements.Length; i++)
                {
                    totalBonus += currentPassiveItem.EnhancementIncrements[i];
                }
            }

            // 보너스 타입에 따라 설명 생성
            if (currentPassiveItem.IsSkillDamageBonus)
            {
                if (currentPassiveItem.TargetSkill != null)
                {
                    string skillName = string.IsNullOrEmpty(currentPassiveItem.TargetSkill.displayNameKO) 
                        ? currentPassiveItem.TargetSkill.displayName 
                        : currentPassiveItem.TargetSkill.displayNameKO;
                    builder.Append($"{skillName}의 데미지가 {totalBonus}만큼 영구적으로 증가합니다.");
                }
                else
                {
                    builder.Append($"스킬 데미지가 {totalBonus}만큼 영구적으로 증가합니다.");
                }
            }
            else if (currentPassiveItem.IsPlayerHealthBonus)
            {
                builder.Append($"플레이어의 최대 체력이 {totalBonus}만큼 영구적으로 증가합니다.");
            }

            // 강화 단계 정보 추가
            if (currentEnhancementLevel < currentPassiveItem.MaxEnhancementLevel)
            {
                builder.Append($"\n\n현재 강화 단계: {currentEnhancementLevel}/{currentPassiveItem.MaxEnhancementLevel}");
                if (currentEnhancementLevel < currentPassiveItem.EnhancementIncrements.Length)
                {
                    int nextBonus = currentPassiveItem.EnhancementIncrements[currentEnhancementLevel];
                    builder.Append($"\n다음 강화 시 추가 보너스: +{nextBonus}");
                }
            }
            else
            {
                builder.Append($"\n\n최대 강화 단계 달성 ({currentEnhancementLevel}/{currentPassiveItem.MaxEnhancementLevel})");
            }

            descriptionText.text = builder.ToString();
        }

        /// <summary>
        /// 아이템 효과를 업데이트합니다.
        /// </summary>
        private void UpdateEffects()
        {
            if (currentItem == null || currentItem.effectConfiguration == null)
                return;

            var config = currentItem.effectConfiguration;
            var effectTextList = new System.Collections.Generic.List<string>();

            // 효과들 분석
            if (config.effects != null && config.effects.Count > 0)
            {
                foreach (var effectConfig in config.effects)
                {
                    if (effectConfig.effectSO != null)
                    {
                        string effectDescription = GetEffectDescription(effectConfig);
                        if (!string.IsNullOrEmpty(effectDescription))
                        {
                            effectTextList.Add(effectDescription);
                        }
                    }
                }
            }

            // descriptionText 업데이트 (효과 설명만 표시)
            if (descriptionText != null)
            {
                if (effectTextList.Count > 0)
                {
                    var builder = new System.Text.StringBuilder();
                    
                    foreach (var effectText in effectTextList)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append("\n\n");
                        }
                        builder.Append(effectText);
                    }
                    
                    descriptionText.text = builder.ToString();
                }
                else
                {
                    // 효과가 없으면 비워둠
                    descriptionText.text = string.Empty;
                }
            }
        }

        /// <summary>
        /// 효과 설명을 생성합니다.
        /// </summary>
        private string GetEffectDescription(Game.ItemSystem.Data.ItemEffectConfig effectConfig)
        {
            if (effectConfig.effectSO == null)
                return null;

            var effectSO = effectConfig.effectSO;

            // 각 효과 타입별로 직접 처리
            if (effectSO is Game.ItemSystem.Effect.HealEffectSO)
                return GetHealEffectDescription(effectConfig);
            
            if (effectSO is Game.ItemSystem.Effect.AttackBuffEffectSO)
                return GetAttackBuffEffectDescription(effectConfig);
            
            if (effectSO is Game.ItemSystem.Effect.ClownPotionEffectSO)
                return GetClownPotionEffectDescription(effectConfig);
            
            if (effectSO is Game.ItemSystem.Effect.ReviveEffectSO)
                return "사망 시 해당 아이템을 자동으로 사용하여 부활합니다.";
            
            if (effectSO is Game.ItemSystem.Effect.TimeStopEffectSO)
                return "적의 행동을 1턴 동안 정지시킵니다.";
            
            if (effectSO is Game.ItemSystem.Effect.RerollEffectSO)
                return "플레이어의 손패를 재생성합니다.";
            
            if (effectSO is Game.ItemSystem.Effect.DiceOfFateEffectSO)
                return "다음 턴에 사용할 적의 스킬카드를 적의 덱에서 랜덤으로 재생성합니다.";
            
            if (effectSO is Game.ItemSystem.Effect.ShieldBreakerEffectSO)
                return "적의 가드 효과를 제거합니다.";

            return null;
        }

        /// <summary>
        /// 힐 효과 설명을 생성합니다.
        /// </summary>
        private string GetHealEffectDescription(Game.ItemSystem.Data.ItemEffectConfig effectConfig)
        {
            if (effectConfig.useCustomSettings && effectConfig.customSettings is Game.ItemSystem.Data.HealEffectCustomSettings healSettings)
            {
                return $"체력을 {healSettings.healAmount}만큼 회복시킵니다.";
            }
            return "체력을 회복시킵니다.";
        }

        /// <summary>
        /// 공격력 버프 효과 설명을 생성합니다.
        /// </summary>
        private string GetAttackBuffEffectDescription(Game.ItemSystem.Data.ItemEffectConfig effectConfig)
        {
            if (effectConfig.useCustomSettings && effectConfig.customSettings is Game.ItemSystem.Data.AttackBuffEffectCustomSettings buffSettings)
            {
                return $"공격력을 {buffSettings.buffAmount}만큼 {buffSettings.duration}턴 동안 증가시킵니다.";
            }
            return "공격력을 증가시킵니다.";
        }

        /// <summary>
        /// 광대 물약 효과 설명을 생성합니다.
        /// </summary>
        private string GetClownPotionEffectDescription(Game.ItemSystem.Data.ItemEffectConfig effectConfig)
        {
            if (effectConfig.useCustomSettings && effectConfig.customSettings is Game.ItemSystem.Data.ClownPotionEffectCustomSettings clownSettings)
            {
                int healChancePercent = Mathf.RoundToInt(clownSettings.healChance * 100);
                
                return $"50% 확률로 체력을 {clownSettings.healAmount} 회복시키거나 {clownSettings.damageAmount} 잃어버립니다.";
            }
            return "랜덤하게 체력을 회복시키거나 데미지를 받습니다.";
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }
        }

        #endregion
    }
}

