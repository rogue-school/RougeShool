using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;

namespace Game.CharacterSystem.Effect
{
    /// <summary>
    /// 특정 체력에 도달하면 스킬 카드를 발동시키는 이펙트입니다.
    /// CharacterEffectEntry를 통해 개별 캐릭터마다 다른 설정(임계값, 스킬 카드 ID)을 적용할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "TriggerSkillOnHealthEffect", menuName = "Game/Character/Effect/Trigger Skill On Health")]
    public class TriggerSkillOnHealthEffectSO : CharacterEffectSO
    {
        [Header("기본 설정 (커스텀 설정 미사용 시)")]
        [Tooltip("기본 스킬 발동 체력 임계값 (절대값, 예: 30)")]
        [SerializeField] private int defaultHealthThreshold = 30;

        [Tooltip("기본 발동할 스킬 카드 ID")]
        [SerializeField] private string defaultSkillCardId = "";

        [Tooltip("체력 비율 기반 사용 여부 (true면 비율, false면 절대값)")]
        [SerializeField] private bool useHealthRatio = false;

        private bool hasTriggered = false;
        private int activeHealthThreshold;
        private SkillCardDefinition activeSkillCardDefinition;
        private string activeSkillCardId;
        private bool activeUseHealthRatio;

        public event System.Action<string, int> OnSkillTriggered;
        public event System.Action<SkillCardDefinition, int> OnSkillDefinitionTriggered;

        /// <summary>
        /// 커스텀 설정을 적용하여 초기화합니다.
        /// </summary>
        public void InitializeWithCustomSettings(ICharacter character, CharacterEffectCustomSettings customSettings)
        {
            hasTriggered = false;

            activeHealthThreshold = customSettings.skillHealthThreshold;
            activeSkillCardDefinition = customSettings.skillCardDefinition;
            activeSkillCardId = customSettings.skillCardDefinition != null ? customSettings.skillCardDefinition.cardId : customSettings.skillCardId;
            activeUseHealthRatio = customSettings.useSkillHealthRatio;

            string cardInfo = activeSkillCardDefinition != null 
                ? $"카드 정의: {activeSkillCardDefinition.displayNameKO} (ID: {activeSkillCardDefinition.cardId})" 
                : $"카드 ID: {activeSkillCardId}";

            // 커스텀 설정 적용 완료
        }

        public override string GetDescription()
        {
            var cardId = string.IsNullOrEmpty(activeSkillCardId) ? defaultSkillCardId : activeSkillCardId;
            var threshold = string.IsNullOrEmpty(activeSkillCardId) ? defaultHealthThreshold : activeHealthThreshold;
            var useRatio = string.IsNullOrEmpty(activeSkillCardId) ? useHealthRatio : activeUseHealthRatio;

            if (useRatio)
                return $"체력이 {threshold}% 이하가 되면 스킬 카드를 발동합니다.";
            else
                return $"체력이 {threshold} 이하가 되면 스킬 카드를 발동합니다.";
        }

        public override void Initialize(ICharacter character)
        {
            hasTriggered = false;
            activeHealthThreshold = defaultHealthThreshold;
            activeSkillCardId = defaultSkillCardId;
            activeUseHealthRatio = useHealthRatio;

            GameLogger.LogInfo($"[TriggerSkillOnHealthEffectSO] {character.GetCharacterName()} 기본 설정으로 초기화 - 카드 ID: {activeSkillCardId}, 임계값: {activeHealthThreshold} ({(activeUseHealthRatio ? "비율" : "절대값")})", GameLogger.LogCategory.Character);
        }

        public override void OnHealthChanged(ICharacter character, int previousHP, int currentHP)
        {
            if (hasTriggered) return;

            string cardId = string.IsNullOrEmpty(activeSkillCardId) ? defaultSkillCardId : activeSkillCardId;
            if (string.IsNullOrEmpty(cardId))
            {
                GameLogger.LogWarning($"[TriggerSkillOnHealthEffectSO] {character.GetCharacterName()} - 스킬 카드 ID가 설정되지 않았습니다.", GameLogger.LogCategory.Character);
                return;
            }

            int threshold = string.IsNullOrEmpty(activeSkillCardId) ? defaultHealthThreshold : activeHealthThreshold;
            bool useRatio = string.IsNullOrEmpty(activeSkillCardId) ? useHealthRatio : activeUseHealthRatio;

            bool shouldTrigger = false;

            if (useRatio)
            {
                // 비율 기반 체크
                int maxHP = character.GetMaxHP();
                if (maxHP <= 0) return;

                float currentRatio = (float)currentHP / maxHP * 100f;
                float previousRatio = (float)previousHP / maxHP * 100f;
                float thresholdRatio = threshold;

                if (previousRatio > thresholdRatio && currentRatio <= thresholdRatio && currentHP > 0)
                {
                    shouldTrigger = true;
                }
            }
            else
            {
                // 절대값 기반 체크
                if (previousHP > threshold && currentHP <= threshold && currentHP > 0)
                {
                    shouldTrigger = true;
                }
            }

            if (shouldTrigger && !hasTriggered)
            {
                hasTriggered = true;
                int maxHP = character.GetMaxHP();
                
                // SkillCardDefinition이 있으면 우선 사용, 없으면 레거시 ID 사용
                if (activeSkillCardDefinition != null)
                {
                    // SkillCardDefinition 이벤트 발생
                    OnSkillDefinitionTriggered?.Invoke(activeSkillCardDefinition, currentHP);
                }
                else
                {
                    // 레거시: 카드 ID만 사용
                    if (useRatio)
                    {
                        float currentRatio = (float)currentHP / maxHP * 100f;
                        GameLogger.LogInfo($"[TriggerSkillOnHealthEffectSO] {character.GetCharacterName()} 스킬 발동! 현재 체력: {currentHP}/{maxHP} ({currentRatio:F1}%), 카드 ID: {cardId}", GameLogger.LogCategory.Character);
                    }
                    else
                    {
                        GameLogger.LogInfo($"[TriggerSkillOnHealthEffectSO] {character.GetCharacterName()} 스킬 발동! 현재 체력: {currentHP}/{maxHP}, 임계값: {threshold}, 카드 ID: {cardId}", GameLogger.LogCategory.Character);
                    }
                    
                    // 레거시 이벤트 발생
                    OnSkillTriggered?.Invoke(cardId, currentHP);
                }
            }
            else if (hasTriggered)
            {
                GameLogger.LogInfo($"[TriggerSkillOnHealthEffectSO] {character.GetCharacterName()} 스킬 이펙트는 이미 발동됨 - 무시", GameLogger.LogCategory.Character);
            }
        }

        public override void OnDeath(ICharacter character)
        {
        }

        public override void Cleanup(ICharacter character)
        {
            hasTriggered = false;
        }

        public void ResetTrigger()
        {
            hasTriggered = false;
        }

        public string GetSkillCardId() => string.IsNullOrEmpty(activeSkillCardId) ? defaultSkillCardId : activeSkillCardId;
        public int GetHealthThreshold() => string.IsNullOrEmpty(activeSkillCardId) ? defaultHealthThreshold : activeHealthThreshold;
        public bool GetUseHealthRatio() => string.IsNullOrEmpty(activeSkillCardId) ? useHealthRatio : activeUseHealthRatio;
    }
}

