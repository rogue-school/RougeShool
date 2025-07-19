namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 전투 턴 중 발생한 특정 이벤트를 추적하는 컨텍스트입니다.
    /// 현재는 적 처치 여부만 추적합니다.
    /// </summary>
    public class TurnContext
    {
        /// <summary>
        /// 이번 턴 동안 적이 처치되었는지 여부를 나타냅니다.
        /// </summary>
        public bool WasEnemyDefeated { get; private set; }

        /// <summary>
        /// 적이 처치되었음을 표시합니다.
        /// 외부 시스템에서 이 메서드를 호출하여 상태를 설정해야 합니다.
        /// </summary>
        public void MarkEnemyDefeated() => WasEnemyDefeated = true;

        /// <summary>
        /// 이번 턴에 핸드카드 소멸 애니메이션이 실행되었는지 여부를 나타냅니다.
        /// </summary>
        public bool WasHandCardsVanishedThisTurn { get; private set; }

        /// <summary>
        /// 이번 턴에 핸드카드 소멸 애니메이션이 실행되었음을 표시합니다.
        /// </summary>
        public void MarkHandCardsVanished() => WasHandCardsVanishedThisTurn = true;

        /// <summary>
        /// 새로운 턴을 시작하기 전에 호출하여 상태를 초기화합니다.
        /// </summary>
        public void Reset() {
            WasEnemyDefeated = false;
            WasHandCardsVanishedThisTurn = false;
        }
    }
}
