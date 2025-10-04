using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.Effect
{
    [CreateAssetMenu(menuName = "Game/Character/Effect/Summon Effect")]
    public class SummonEffect : ScriptableObject, ICharacterEffect
    {
        [Header("소환 설정")]
        [Tooltip("소환이 발동되는 체력 비율 (0.5 = 50%)")]
        [Range(0f, 1f)]
        [SerializeField] private float healthThreshold = 0.5f;

        [Tooltip("소환할 적 캐릭터 데이터")]
        [SerializeField] private EnemyCharacterData summonTarget;

        private bool hasTriggered = false;
        public event System.Action<EnemyCharacterData, int> OnSummonTriggered;

        public string GetEffectName() => "소환";

        public string GetDescription() =>
            $"체력이 {healthThreshold:P0} 이하가 되면 {summonTarget?.DisplayName ?? "???"}를 소환합니다.";

        public void Initialize(ICharacter character)
        {
            hasTriggered = false;
            GameLogger.LogInfo($"[SummonEffect] {character.GetCharacterName()}에 소환 효과 초기화", GameLogger.LogCategory.Character);
        }

        public void OnHealthChanged(ICharacter character, int previousHP, int currentHP)
        {
            if (hasTriggered) return;
            if (summonTarget == null) return;

            int maxHP = character.GetMaxHP();
            float currentRatio = (float)currentHP / maxHP;
            float previousRatio = (float)previousHP / maxHP;

            if (previousRatio > healthThreshold && currentRatio <= healthThreshold && currentHP > 0)
            {
                hasTriggered = true;
                GameLogger.LogInfo($"[SummonEffect] {character.GetCharacterName()} 소환 발동! 현재 체력: {currentHP}, 대상: {summonTarget.DisplayName}", GameLogger.LogCategory.Character);
                OnSummonTriggered?.Invoke(summonTarget, currentHP);
            }
        }

        public void OnDeath(ICharacter character)
        {
        }

        public void Cleanup(ICharacter character)
        {
            hasTriggered = false;
        }

        public void ResetTrigger()
        {
            hasTriggered = false;
        }
    }
}
