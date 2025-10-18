using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 공격력 증가 버프 효과입니다.
    /// 플레이어의 공격력에 일정 수치를 추가합니다.
    /// </summary>
    public class AttackPowerBuffEffect : IPerTurnEffect
    {
        /// <summary>버프로 증가하는 공격력 수치</summary>
        public int AttackPowerBonus { get; private set; }
        
        /// <summary>남은 턴 수</summary>
        public int RemainingTurns { get; private set; }
        
        /// <summary>UI 아이콘</summary>
        public Sprite Icon { get; private set; }
        
        /// <summary>효과 만료 여부</summary>
        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>
        /// 공격력 버프 효과를 생성합니다.
        /// </summary>
        /// <param name="attackPowerBonus">공격력 보너스</param>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">UI 아이콘</param>
        public AttackPowerBuffEffect(int attackPowerBonus, int duration, Sprite icon = null)
        {
            AttackPowerBonus = attackPowerBonus;
            RemainingTurns = duration;
            Icon = icon;
        }

        /// <summary>
        /// 턴 시작 시 효과를 처리합니다.
        /// 공격력 버프는 턴마다 특별한 동작이 없고, 지속 시간만 감소합니다.
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
                    $"[AttackPowerBuffEffect] {target.GetCharacterName()} 공격력 버프 턴 감소 (남은 턴: {RemainingTurns})",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 현재 공격력 보너스를 반환합니다.
        /// </summary>
        /// <returns>공격력 보너스</returns>
        public int GetAttackPowerBonus()
        {
            return AttackPowerBonus;
        }
    }
}
