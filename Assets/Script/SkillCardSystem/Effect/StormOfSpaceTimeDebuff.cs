using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시공의 폭풍 디버프 효과 클래스입니다.
    /// 적에게 적용되며, 플레이어에게 3번의 공격 기회를 주는 기믹입니다.
    /// 적의 턴이 끝날 때마다 턴 수가 감소합니다.
    /// 분신처럼 추가 체력을 제공하며, 추가 체력이 먼저 소모됩니다.
    /// </summary>
    public class StormOfSpaceTimeDebuff : OwnTurnEffectBase
    {
        /// <summary>
        /// TurnManager 캐싱 (성능 최적화)
        /// </summary>
        private static Game.CombatSystem.Manager.TurnManager _cachedTurnManager;

        /// <summary>
        /// 목표 데미지 수치
        /// </summary>
        public int TargetDamage { get; private set; }

        /// <summary>
        /// 현재 누적된 데미지
        /// </summary>
        public int AccumulatedDamage { get; private set; }

        /// <summary>
        /// 시공의 폭풍 추가 체력 (분신처럼 먼저 소모되는 체력)
        /// </summary>
        public int StormHP { get; private set; }

        /// <summary>
        /// 목표 달성 여부
        /// </summary>
        public bool IsTargetAchieved => AccumulatedDamage >= TargetDamage;

        /// <summary>
        /// 시공의 폭풍 디버프를 생성합니다.
        /// </summary>
        /// <param name="targetDamage">목표 데미지 수치 (추가 체력으로도 사용됨)</param>
        /// <param name="duration">지속 턴 수 (기본값: 3)</param>
        /// <param name="icon">UI 아이콘</param>
        public StormOfSpaceTimeDebuff(int targetDamage, int duration = 3, Sprite icon = null) : base(duration, icon)
        {
            TargetDamage = targetDamage;
            AccumulatedDamage = 0;
            StormHP = targetDamage; // 추가 체력은 목표 데미지와 동일하게 설정
        }

        /// <summary>
        /// 시공의 폭풍 추가 체력을 설정합니다.
        /// </summary>
        /// <param name="value">설정할 추가 체력 값</param>
        public void SetStormHP(int value)
        {
            StormHP = Mathf.Max(0, value);
        }

        /// <summary>
        /// 데미지를 누적합니다.
        /// </summary>
        /// <param name="damage">추가할 데미지</param>
        public void AddDamage(int damage)
        {
            if (damage > 0)
            {
                AccumulatedDamage += damage;
                Game.CoreSystem.Utility.GameLogger.LogInfo(
                    $"[StormOfSpaceTimeDebuff] 데미지 누적: {AccumulatedDamage}/{TargetDamage} (+{damage})",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 턴 시작 시 효과를 처리합니다.
        /// 시공의 폭풍은 OnTurnStart에서 자동으로 턴을 감소시키지 않습니다.
        /// 대신 적이 시공의 폭풍 카드를 실행한 후 StormOfSpaceTimeEffectCommand에서 직접 턴을 감소시킵니다.
        /// </summary>
        /// <param name="target">시공의 폭풍 디버프가 적용된 캐릭터 (적)</param>
        public override void OnTurnStart(ICharacter target)
        {
            // 시공의 폭풍은 카드 실행 후에 턴을 감소시키므로 여기서는 아무것도 하지 않음
        }

        /// <summary>
        /// 턴 수를 수동으로 감소시킵니다.
        /// StormOfSpaceTimeEffectCommand에서 적이 시공의 폭풍 카드를 실행한 후 호출됩니다.
        /// </summary>
        /// <param name="target">시공의 폭풍 디버프가 적용된 캐릭터</param>
        public void DecrementTurn(ICharacter target)
        {
            if (target == null) return;

            RemainingTurns--;
            OnTurnDecrement(target);
            LogTurnDecrement(target);
            
            // 현재 턴수 디버그 로그
            Game.CoreSystem.Utility.GameLogger.LogInfo(
                $"[StormOfSpaceTimeDebuff] 현재 남은 턴수: {RemainingTurns}턴 (목표 데미지: {TargetDamage}, 누적 데미지: {AccumulatedDamage}/{TargetDamage})",
                Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 턴 감소 시 목표 달성 여부를 확인하고, 턴이 0이 되었을 때 페널티를 적용합니다.
        /// </summary>
        /// <param name="target">시공의 폭풍 디버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            // 목표 달성 시: 버프를 만료시켜 효과가 없도록 함
            if (IsTargetAchieved)
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo(
                    $"[StormOfSpaceTimeDebuff] {target.GetCharacterName()} 목표 달성! ({AccumulatedDamage}/{TargetDamage}) - 버프 만료",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                Expire(); // 버프 만료하여 효과 없음
                return;
            }

            // 턴이 0 이하가 되었을 때 목표 미달성 시 페널티 적용
            if (RemainingTurns <= 0)
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo(
                    $"[StormOfSpaceTimeDebuff] {target.GetCharacterName()} 턴 종료 - 목표 달성 여부: {IsTargetAchieved} ({AccumulatedDamage}/{TargetDamage})",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);

                // 목표 미달성 시 페널티 적용 (플레이어에게 데미지 - 가드 및 모든 방어 효과 무시)
                if (!IsTargetAchieved)
                {
                    int remainingDamage = TargetDamage - AccumulatedDamage;
                    if (remainingDamage > 0)
                    {
                        // 플레이어 찾기
                        var playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                        var playerCharacter = playerManager?.GetCharacter();
                        if (playerCharacter != null)
                        {
                            Game.CoreSystem.Utility.GameLogger.LogWarning(
                                $"[StormOfSpaceTimeDebuff] 목표 미달성! 플레이어에게 남은 데미지 {remainingDamage} 적용 (가드 무시) - 목표: {TargetDamage}, 누적: {AccumulatedDamage}",
                                Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                            // TakeDamageIgnoreGuard를 사용하여 가드 및 모든 방어 효과를 무시하고 데미지 적용
                            playerCharacter.TakeDamageIgnoreGuard(remainingDamage);
                        }
                        else
                        {
                            Game.CoreSystem.Utility.GameLogger.LogError(
                                "[StormOfSpaceTimeDebuff] 플레이어 캐릭터를 찾을 수 없습니다.",
                                Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                        }
                    }
                    else
                    {
                        Game.CoreSystem.Utility.GameLogger.LogInfo(
                            "[StormOfSpaceTimeDebuff] 목표 미달성이지만 남은 데미지가 0입니다.",
                            Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                    }
                }
                else
                {
                    Game.CoreSystem.Utility.GameLogger.LogInfo(
                        "[StormOfSpaceTimeDebuff] 목표 달성! 페널티 없음",
                        Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                }

                // 버프 만료 (턴 소진)
                Expire();
                Game.CoreSystem.Utility.GameLogger.LogInfo(
                    "[StormOfSpaceTimeDebuff] 시공의 폭풍 버프 만료 (턴 소진)",
                    Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 시공의 폭풍 버프가 만료되었는지 확인합니다.
        /// 추가 체력이 0 이하면 만료된 것으로 간주합니다.
        /// </summary>
        public override bool IsExpired => StormHP <= 0 || RemainingTurns <= 0;
    }
}

