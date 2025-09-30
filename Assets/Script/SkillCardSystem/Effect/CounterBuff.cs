using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 반격 버프: 1턴 동안 받는 피해의 절반만 받고, 나머지 절반을 공격자에게 반사.
    /// 또한 버프 지속 중에는 적의 상태이상 효과를 무효화합니다.
    /// </summary>
    public class CounterBuff : IStatusEffectBuff
    {
        public int RemainingTurns { get; private set; }
        public Sprite Icon { get; private set; }
        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>
        /// 최근 공격자를 추적하기 위한 참조. DamageEffectCommand에서 설정합니다.
        /// </summary>
        public ICharacter LastAttacker { get; set; }

        public CounterBuff(int duration = 1, Sprite icon = null)
        {
            RemainingTurns = duration;
            Icon = icon;
        }

        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;
            // 가드와 동일한 턴 소모 규칙: 자기 진영의 턴에만 감소
            var turnManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
            if (target.IsPlayerControlled())
            {
                if (turnManager != null && turnManager.IsPlayerTurn())
                {
                    RemainingTurns--;
                }
            }
            else
            {
                if (turnManager != null && turnManager.IsEnemyTurn())
                {
                    RemainingTurns--;
                }
            }
        }
    }
}


