using Game.CharacterSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 운명의 실 디버프 효과 클래스입니다.
    /// 플레이어에게 적용되며, 해당 디버프가 있을 때 플레이어는 스킬을 원하는 대로 사용할 수 없습니다.
    /// 핸드에서 3장을 뽑고 2개를 제거한 후 나머지 1개를 전투 슬롯으로 이동시킵니다.
    /// 
    /// 턴 수 관리: 플레이어 턴에서 효과가 작동하고, 적 턴으로 넘어갈 때 턴 수가 차감됩니다.
    /// </summary>
    public class ThreadOfFateDebuff : OwnTurnEffectBase
    {
        /// <summary>
        /// TurnManager 캐싱 (성능 최적화)
        /// </summary>
        private static Game.CombatSystem.Manager.TurnManager _cachedTurnManager;

        /// <summary>
        /// 운명의 실 디버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        /// <param name="icon">UI 아이콘</param>
        public ThreadOfFateDebuff(int duration = 1, Sprite icon = null) : base(duration, icon)
        {
        }

        /// <summary>
        /// 턴 시작 시 효과를 처리합니다.
        /// 플레이어 턴에서는 턴 수를 차감하지 않고, 적 턴으로 넘어갈 때만 차감합니다.
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        public override void OnTurnStart(ICharacter target)
        {
            if (target == null) return;

            // TurnManager 찾기
            var tm = _cachedTurnManager;
            if (tm == null)
            {
                tm = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (tm != null)
                {
                    _cachedTurnManager = tm;
                }
            }

            if (tm == null) return;

            // 플레이어에게 적용된 경우, 적 턴일 때만 턴 수 차감
            // (플레이어 턴에서는 효과가 작동하므로 턴 수를 차감하지 않음)
            if (target.IsPlayerControlled() && tm.IsEnemyTurn())
            {
                RemainingTurns--;
                OnTurnDecrement(target);
                LogTurnDecrement(target);
            }
            // 적에게 적용된 경우는 일반적인 동작 (자신의 턴에 차감)
            else if (!target.IsPlayerControlled() && tm.IsEnemyTurn())
            {
                RemainingTurns--;
                OnTurnDecrement(target);
                LogTurnDecrement(target);
            }
        }

        /// <summary>
        /// 턴 감소 시 동작 (현재는 없음)
        /// </summary>
        /// <param name="target">운명의 실 디버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            // 턴 감소 시 특별한 동작 없음
        }
    }
}

