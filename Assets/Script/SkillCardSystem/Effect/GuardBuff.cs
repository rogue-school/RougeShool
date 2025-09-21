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

        /// <summary>가드 버프가 만료되었는지 여부</summary>
        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>
        /// 가드 버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        public GuardBuff(int duration = 1)
        {
            RemainingTurns = duration;
        }

        /// <summary>
        /// 턴 시작 시 가드 버프를 처리합니다.
        /// </summary>
        /// <param name="target">가드 버프가 적용된 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;

            // 가드 버프가 활성화되어 있으면 가드 상태 설정
            if (RemainingTurns > 0)
            {
                target.SetGuarded(true);
            }

            // 턴 수 감소
            RemainingTurns--;

            // 만료된 경우 가드 상태 해제
            if (IsExpired)
            {
                target.SetGuarded(false);
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
