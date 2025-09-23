using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 버프 효과 클래스입니다.
    /// 1턴 동안 모든 데미지와 상태이상을 차단합니다.
    /// </summary>
    public class GuardBuff : IPerTurnEffect
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
        /// </summary>
        /// <param name="target">가드 버프가 적용된 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;
            // 모든 효과 규칙: 적용된 턴에는 대기 → 다음 '자신의' 턴 시작 시 1틱 처리
            // 가드의 1틱 처리 = 해당 턴 전체를 보호하고, 턴 시작 시점에서 카운트만 감소
            // (보호는 외부 로직: IsGuarded 상태를 전투 시스템이 조회하여 처리)
            RemainingTurns--;
            // 남은 턴이 여전히 존재하면 그 턴 동안 보호 상태 유지
            // 0이 되면 이번 턴 시작 직후 만료 → 외부 시스템에서 더 이상 가드 판정 없음
            target.SetGuarded(!IsExpired);
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
