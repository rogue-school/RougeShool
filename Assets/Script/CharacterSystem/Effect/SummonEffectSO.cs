using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.UI;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.Effect
{
    /// <summary>
    /// 체력 임계값에 도달하면 다른 적을 소환하는 이펙트입니다.
    /// CharacterEffectEntry를 통해 개별 캐릭터마다 다른 설정(임계값, 소환 대상)을 적용할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SummonEffect", menuName = "Game/Character/Effect/Summon Effect")]
    public class SummonEffectSO : CharacterEffectSO
    {
        [Header("기본 설정 (커스텀 설정 미사용 시)")]
        [Tooltip("기본 소환 발동 체력 비율 (0.5 = 50%)")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultHealthThreshold = 0.5f;

        [Tooltip("기본 소환 대상")]
        [SerializeField] private EnemyCharacterData defaultSummonTarget;

        private bool hasTriggered = false;
        private float activeHealthThreshold;
        private EnemyCharacterData activeSummonTarget;

        public event System.Action<EnemyCharacterData, int> OnSummonTriggered;

        /// <summary>
        /// 커스텀 설정을 적용하여 초기화합니다.
        /// </summary>
        public void InitializeWithCustomSettings(ICharacter character, CharacterEffectCustomSettings customSettings)
        {
            hasTriggered = false;

            activeHealthThreshold = customSettings.healthThreshold;
            activeSummonTarget = customSettings.summonTarget;

            GameLogger.LogInfo($"[SummonEffectSO] {character.GetCharacterName()} 커스텀 설정 적용 - 대상: {activeSummonTarget?.DisplayName}, 임계값: {activeHealthThreshold:P0}", GameLogger.LogCategory.Character);
        }

        public override string GetDescription()
        {
            var target = activeSummonTarget ?? defaultSummonTarget;
            var threshold = activeSummonTarget != null ? activeHealthThreshold : defaultHealthThreshold;

            if (target != null)
                return $"체력이 {threshold:P0} 이하가 되면 {target.DisplayName}를 소환합니다.";
            else
                return $"체력이 {threshold:P0} 이하가 되면 소환합니다.";
        }

        public override void Initialize(ICharacter character)
        {
            hasTriggered = false;
            activeHealthThreshold = defaultHealthThreshold;
            activeSummonTarget = defaultSummonTarget;

            GameLogger.LogInfo($"[SummonEffectSO] {character.GetCharacterName()} 기본 설정으로 초기화 - 대상: {activeSummonTarget?.DisplayName}, 임계값: {activeHealthThreshold:P0}", GameLogger.LogCategory.Character);
        }

        public override void OnHealthChanged(ICharacter character, int previousHP, int currentHP)
        {
            if (hasTriggered) return;

            if (activeSummonTarget == null)
            {
                GameLogger.LogWarning($"[SummonEffectSO] {character.GetCharacterName()} - 소환 대상이 설정되지 않았습니다.", GameLogger.LogCategory.Character);
                return;
            }

            int maxHP = character.GetMaxHP();
            float currentRatio = (float)currentHP / maxHP;
            float previousRatio = (float)previousHP / maxHP;

            if (previousRatio > activeHealthThreshold && currentRatio <= activeHealthThreshold && currentHP > 0 && !hasTriggered)
            {
                hasTriggered = true;
                GameLogger.LogInfo($"[SummonEffectSO] {character.GetCharacterName()} 소환 발동! 현재 체력: {currentHP}/{maxHP} ({currentRatio:P0}), 대상: {activeSummonTarget.DisplayName}", GameLogger.LogCategory.Character);
                
                // UI 알림 표시
                ShowSummonNotification(activeSummonTarget);
                
                // 즉시 소환 트리거 (CombatStateMachine에서 즉시 감지하여 처리)
                OnSummonTriggered?.Invoke(activeSummonTarget, currentHP);
            }
            else if (hasTriggered)
            {
                GameLogger.LogInfo($"[SummonEffectSO] {character.GetCharacterName()} 소환 이펙트는 이미 발동됨 - 무시", GameLogger.LogCategory.Character);
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

        public EnemyCharacterData GetSummonTarget() => activeSummonTarget ?? defaultSummonTarget;
        public float GetHealthThreshold() => activeSummonTarget != null ? activeHealthThreshold : defaultHealthThreshold;

        /// <summary>
        /// 소환 알림을 UI 패널에 표시합니다.
        /// </summary>
        /// <param name="summonTarget">소환 대상</param>
        private void ShowSummonNotification(EnemyCharacterData summonTarget)
        {
            if (summonTarget == null)
            {
                GameLogger.LogWarning("[SummonEffectSO] 소환 대상이 null입니다 - 알림 표시 건너뜀", GameLogger.LogCategory.UI);
                return;
            }

            // EffectNotificationPanel 찾기 (비활성화된 오브젝트도 포함)
            EffectNotificationPanel notificationPanel = UnityEngine.Object.FindFirstObjectByType<EffectNotificationPanel>(FindObjectsInactive.Include);
            if (notificationPanel != null)
            {
                // 이펙트 이름만 사용
                string effectName = GetEffectName();

                notificationPanel.ShowNotification(effectName);
                GameLogger.LogInfo($"[SummonEffectSO] 소환 알림 표시: {effectName}", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[SummonEffectSO] EffectNotificationPanel을 찾을 수 없습니다 - 알림 표시 건너뜀", GameLogger.LogCategory.UI);
            }
        }
    }
}
