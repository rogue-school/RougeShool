using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Interface;
using UnityEngine;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 아이템 효과의 베이스 클래스입니다.
    /// 턴 감소 정책에 따라 자동으로 턴을 관리합니다.
    /// </summary>
    public abstract class ItemEffectBase : IItemPerTurnEffect
    {
        /// <summary>남은 턴 수</summary>
        public int RemainingTurns { get; protected set; }

        /// <summary>UI 아이콘</summary>
        public Sprite Icon { get; protected set; }

        /// <summary>효과 만료 여부</summary>
        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>턴 감소 정책</summary>
        public ItemEffectTurnPolicy TurnPolicy { get; protected set; }

        /// <summary>원본 아이템 이름 (툴팁 표시용)</summary>
        public string SourceItemName { get; protected set; }

        /// <summary>TurnManager 캐싱 (성능 최적화)</summary>
        private static Game.CombatSystem.Manager.TurnManager _cachedTurnManager;
        private readonly Game.CombatSystem.Manager.TurnManager turnManager;

        /// <summary>
        /// 효과를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="turnPolicy">턴 감소 정책</param>
        /// <param name="icon">UI 아이콘</param>
        /// <param name="sourceItemName">원본 아이템 이름 (선택적)</param>
        /// <param name="turnManager">턴 매니저 (선택적)</param>
        protected ItemEffectBase(int duration, ItemEffectTurnPolicy turnPolicy, Sprite icon = null, string sourceItemName = null, Game.CombatSystem.Manager.TurnManager turnManager = null)
        {
            RemainingTurns = duration;
            TurnPolicy = turnPolicy;
            Icon = icon;
            SourceItemName = sourceItemName;
            this.turnManager = turnManager;
        }

        /// <summary>
        /// 턴 시작 시 효과를 처리합니다.
        /// 정책에 따라 턴을 감소시키고, 추가 동작을 수행합니다.
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;

            // TurnManager 사용 (주입받았으면 사용, 없으면 캐시 또는 찾기)
            var tm = turnManager ?? _cachedTurnManager;
            if (tm == null)
            {
                tm = Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (tm != null)
                {
                    _cachedTurnManager = tm; // 캐시
                }
            }

            if (tm == null)
            {
                GameLogger.LogWarning($"[{GetType().Name}] TurnManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return;
            }

            // 정책에 따라 턴 감소 여부 결정
            bool shouldDecrement = ShouldDecrementThisTurn(target, tm);

            if (shouldDecrement)
            {
                RemainingTurns--;
                OnTurnDecrement(target);
                LogTurnDecrement(target);
            }
        }

        /// <summary>
        /// 현재 턴에 턴 수를 감소시킬지 결정합니다.
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        /// <param name="turnManager">턴 매니저</param>
        /// <returns>감소 여부</returns>
        private bool ShouldDecrementThisTurn(ICharacter target, Game.CombatSystem.Manager.TurnManager turnManager)
        {
            bool isPlayerControlled = target.IsPlayerControlled();
            bool isPlayerTurn = turnManager.IsPlayerTurn();

            switch (TurnPolicy)
            {
                case ItemEffectTurnPolicy.Immediate:
                    // 즉시 소모 (턴 감소 안 함)
                    return false;

                case ItemEffectTurnPolicy.EveryTurn:
                    // 매 턴마다 감소
                    return true;

                case ItemEffectTurnPolicy.TargetTurnOnly:
                    // 대상의 턴에만 감소
                    return isPlayerControlled ? isPlayerTurn : !isPlayerTurn;

                case ItemEffectTurnPolicy.OpponentTurnOnly:
                    // 대상의 상대 턴에만 감소
                    return isPlayerControlled ? !isPlayerTurn : isPlayerTurn;

                default:
                    GameLogger.LogWarning($"[{GetType().Name}] 알 수 없는 턴 정책: {TurnPolicy}", GameLogger.LogCategory.Core);
                    return false;
            }
        }

        /// <summary>
        /// 턴 감소 시 추가 동작을 수행합니다. (자식 클래스에서 구현)
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        protected virtual void OnTurnDecrement(ICharacter target)
        {
            // 기본 구현은 비어 있음 - 자식 클래스에서 필요 시 오버라이드
        }

        /// <summary>
        /// 턴 감소 로그를 출력합니다. (자식 클래스에서 오버라이드 가능)
        /// </summary>
        /// <param name="target">효과가 적용된 캐릭터</param>
        protected virtual void LogTurnDecrement(ICharacter target)
        {
            GameLogger.LogInfo(
                $"[{GetType().Name}] {target.GetCharacterName()} 효과 턴 감소 (남은 턴: {RemainingTurns}, 정책: {TurnPolicy})",
                GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 효과를 즉시 만료시킵니다.
        /// </summary>
        public void Expire()
        {
            RemainingTurns = 0;
        }
    }
}
