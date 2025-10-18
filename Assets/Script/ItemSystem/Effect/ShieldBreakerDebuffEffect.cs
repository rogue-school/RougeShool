using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 실드 브레이커 디버프 효과입니다.
    /// 적의 방어/반격 효과를 무시할 수 있게 해줍니다.
    /// </summary>
    public class ShieldBreakerDebuffEffect : IPerTurnEffect
    {
        /// <summary>남은 턴 수</summary>
        public int RemainingTurns { get; private set; }
        
        /// <summary>UI 아이콘</summary>
        public Sprite Icon { get; private set; }
        
        /// <summary>효과 만료 여부</summary>
        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>
        /// 실드 브레이커 디버프 효과를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">UI 아이콘</param>
        public ShieldBreakerDebuffEffect(int duration, Sprite icon = null)
        {
            RemainingTurns = duration;
            Icon = icon;
        }

        /// <summary>
        /// 턴 시작 시 효과를 처리합니다.
        /// 실드 브레이커는 턴마다 특별한 동작이 없고, 지속 시간만 감소합니다.
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;

            // TurnManager를 통해 현재 턴 확인
            var turnManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
            if (turnManager == null) return;

            // 자신의 턴에만 지속 시간 감소
            bool shouldDecrement = target.IsPlayerControlled()
                ? turnManager.IsPlayerTurn()
                : turnManager.IsEnemyTurn();

            if (shouldDecrement)
            {
                RemainingTurns--;
                Game.CoreSystem.Utility.GameLogger.LogInfo(
                    $"[ShieldBreakerDebuffEffect] {target.GetCharacterName()} 실드 브레이커 디버프 턴 감소 (남은 턴: {RemainingTurns})",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 실드 브레이커 효과가 활성화되어 있는지 확인합니다.
        /// </summary>
        /// <returns>효과 활성화 여부</returns>
        public bool IsShieldBreakerActive()
        {
            return !IsExpired;
        }
    }
}
