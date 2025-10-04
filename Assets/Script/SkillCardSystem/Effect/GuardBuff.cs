using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 버프 효과 클래스입니다.
    /// 1턴 동안 모든 데미지와 상태이상을 차단합니다.
    /// </summary>
    public class GuardBuff : IStatusEffectBuff
    {
        /// <summary>남은 턴 수</summary>
        public int RemainingTurns { get; private set; }

        /// <summary>UI 아이콘(없으면 null)</summary>
        public UnityEngine.Sprite Icon { get; private set; }

        /// <summary>가드 버프가 만료되었는지 여부</summary>
        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>
        /// 가드 버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        public GuardBuff(int duration = 1, UnityEngine.Sprite icon = null)
        {
            RemainingTurns = duration;
            Icon = icon;
        }

        /// <summary>
        /// 턴 시작 시 가드 버프를 처리합니다.
        /// 가드는 자신의 턴에만 턴 수가 감소합니다.
        /// </summary>
        /// <param name="target">가드 버프가 적용된 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;
            
            // 가드는 자신의 턴에만 턴 수 감소
            // 플레이어가 가드를 사용했다면, 플레이어 턴에만 감소
            // 적이 가드를 사용했다면, 적 턴에만 감소
            if (target.IsPlayerControlled())
            {
                // 플레이어 캐릭터: 플레이어 턴에만 감소
                var turnManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (turnManager != null && turnManager.IsPlayerTurn())
                {
                    RemainingTurns--;
                    target.SetGuarded(!IsExpired);
                    Game.CoreSystem.Utility.GameLogger.LogInfo($"[GuardBuff] {target.GetCharacterName()} 가드 버프 턴 감소 (남은 턴: {RemainingTurns})", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                // 적 캐릭터: 적 턴에만 감소
                var turnManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (turnManager != null && turnManager.IsEnemyTurn())
                {
                    RemainingTurns--;
                    target.SetGuarded(!IsExpired);
                    Game.CoreSystem.Utility.GameLogger.LogInfo($"[GuardBuff] {target.GetCharacterName()} 가드 버프 턴 감소 (남은 턴: {RemainingTurns})", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                }
            }
        }

        /// <summary>
        /// 가드 버프의 남은 턴 수를 반환합니다.
        /// </summary>
        /// <returns>남은 턴 수</returns>
        public int GetRemainingTurns()
        {
            return RemainingTurns;
        }

        /// <summary>
        /// 가드 버프를 즉시 만료시킵니다.
        /// </summary>
        public void Expire()
        {
            RemainingTurns = 0;
        }
    }
}
