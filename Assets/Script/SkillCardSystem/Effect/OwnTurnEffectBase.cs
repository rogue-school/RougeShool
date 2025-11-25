using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 자신의 턴에만 지속 시간이 감소하는 효과의 기반 클래스입니다.
    /// GuardBuff, CounterBuff 등에서 공통 로직을 재사용합니다.
    /// </summary>
    public abstract class OwnTurnEffectBase : IStatusEffectBuff
    {
        /// <summary>남은 턴 수</summary>
        public int RemainingTurns { get; protected set; }

        /// <summary>UI 아이콘</summary>
        public Sprite Icon { get; protected set; }

        /// <summary>효과 만료 여부</summary>
        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>TurnManager 캐싱 (성능 최적화)</summary>
        private static Game.CombatSystem.Manager.TurnManager _cachedTurnManager;

        /// <summary>
        /// 효과를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">UI 아이콘</param>
        protected OwnTurnEffectBase(int duration, Sprite icon = null)
        {
            RemainingTurns = duration;
            Icon = icon;
        }

        /// <summary>
        /// 턴 시작 시 효과를 처리합니다.
        /// 자신의 턴에만 지속 시간이 감소합니다.
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;

            if (_cachedTurnManager == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogWarning(
                    $"[{GetType().Name}] TurnManager가 주입되지 않았습니다. 턴 감소를 건너뜁니다.",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                return;
            }

            // 자신의 턴에만 감소
            bool shouldDecrement = target.IsPlayerControlled()
                ? _cachedTurnManager.IsPlayerTurn()
                : _cachedTurnManager.IsEnemyTurn();

            if (shouldDecrement)
            {
                RemainingTurns--;
                OnTurnDecrement(target);
                LogTurnDecrement(target);
            }
        }

        /// <summary>
        /// 턴 감소 시 추가 동작을 수행합니다. (자식 클래스에서 구현)
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        protected abstract void OnTurnDecrement(ICharacter target);

        /// <summary>
        /// 턴 감소 로그를 출력합니다. (자식 클래스에서 오버라이드 가능)
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        protected virtual void LogTurnDecrement(ICharacter target)
        {
            Game.CoreSystem.Utility.GameLogger.LogInfo(
                $"[{GetType().Name}] {target.GetCharacterName()} 효과 턴 감소 (남은 턴: {RemainingTurns})",
                Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 남은 턴 수를 반환합니다.
        /// </summary>
        public int GetRemainingTurns() => RemainingTurns;

        /// <summary>
        /// 효과를 즉시 만료시킵니다.
        /// </summary>
        public void Expire()
        {
            RemainingTurns = 0;
        }
    }
}
