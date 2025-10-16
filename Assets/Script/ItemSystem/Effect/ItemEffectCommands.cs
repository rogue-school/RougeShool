using UnityEngine;
using Game.ItemSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Utility;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 체력 회복 효과 커맨드입니다.
    /// </summary>
    public class HealEffectCommand : BaseItemEffectCommand
    {
        private readonly int healAmount;

        public HealEffectCommand(int healAmount) : base("체력 회복")
        {
            this.healAmount = healAmount;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            int currentHP = context.User.GetCurrentHP();
            int maxHP = context.User.GetMaxHP();
            int actualHeal = Mathf.Min(healAmount, maxHP - currentHP);

            if (actualHeal > 0)
            {
                context.User.Heal(actualHeal);
                GameLogger.LogInfo($"체력 회복: {actualHeal} (현재: {currentHP + actualHeal}/{maxHP})", GameLogger.LogCategory.Core);
                return true;
            }
            else
            {
                GameLogger.LogInfo("체력이 이미 최대입니다", GameLogger.LogCategory.Core);
                return false;
            }
        }
    }

    /// <summary>
    /// 공격력 버프 효과 커맨드입니다.
    /// </summary>
    public class AttackBuffEffectCommand : BaseItemEffectCommand
    {
        private readonly int buffAmount;
        private readonly int duration;

        public AttackBuffEffectCommand(int buffAmount, int duration = 1) : base("공격력 버프")
        {
            this.buffAmount = buffAmount;
            this.duration = duration;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // TODO: 실제 버프 시스템과 연동
            GameLogger.LogInfo($"공격력 버프 적용: +{buffAmount} ({duration}턴)", GameLogger.LogCategory.Core);
            return true;
        }
    }


    /// <summary>
    /// 광대 물약 랜덤 효과 커맨드입니다.
    /// </summary>
    public class ClownPotionEffectCommand : IItemEffectCommand
    {
        private int healChance;
        private int healAmount;
        private int damageAmount;

        public ClownPotionEffectCommand(int healChance = 50, int healAmount = 5, int damageAmount = 5)
        {
            this.healChance = Mathf.Clamp(healChance, 0, 100);
            this.healAmount = healAmount;
            this.damageAmount = damageAmount;
        }

        public bool Execute(IItemUseContext context)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogError("광대 물약 실패: 사용자가 null이거나 사망 상태입니다", GameLogger.LogCategory.Core);
                return false;
            }

            bool isHeal = UnityEngine.Random.Range(0, 100) < healChance;
            
            if (isHeal)
            {
                context.User.Heal(healAmount);
                GameLogger.LogInfo($"광대 물약 효과: 체력 회복 +{healAmount} (확률: {healChance}%)", GameLogger.LogCategory.Core);
            }
            else
            {
                context.User.TakeDamage(damageAmount);
                GameLogger.LogInfo($"광대 물약 효과: 데미지 -{damageAmount} (확률: {100 - healChance}%)", GameLogger.LogCategory.Core);
            }
            
            return true;
        }
    }

    /// <summary>
    /// 부활 효과 커맨드입니다.
    /// </summary>
    public class ReviveEffectCommand : IItemEffectCommand
    {
        public bool Execute(IItemUseContext context)
        {
            if (context?.User == null)
            {
                GameLogger.LogError("부활 실패: 사용자가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            if (!context.User.IsDead())
            {
                GameLogger.LogInfo("사용자가 이미 살아있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 최대 체력으로 부활
            int maxHP = context.User.GetMaxHP();
            context.User.Heal(maxHP);
            
            // TODO: 디버프 제거 시스템과 연동
            GameLogger.LogInfo($"부활 완료: 체력 {maxHP}으로 회복, 모든 디버프 제거", GameLogger.LogCategory.Core);
            return true;
        }
    }

    /// <summary>
    /// 시간 정지 효과 커맨드입니다.
    /// 다음 턴에 사용될 적 카드를 봉인시킵니다.
    /// </summary>
    public class TimeStopEffectCommand : IItemEffectCommand
    {
        private int sealCount;

        public TimeStopEffectCommand(int sealCount = 1)
        {
            this.sealCount = Mathf.Max(1, sealCount);
        }

        public bool Execute(IItemUseContext context)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogError("시간 정지 실패: 사용자가 null이거나 사망 상태입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // TODO: 실제 시간 정지 시스템과 연동
            GameLogger.LogInfo($"시간 정지 효과: 다음 적 카드 {sealCount}장 봉인", GameLogger.LogCategory.Core);
            return true;
        }
    }

    /// <summary>
    /// 운명의 주사위 효과 커맨드입니다.
    /// 다음 턴 적이 사용 예정인 스킬을 무작위로 변경시킵니다.
    /// </summary>
    public class DiceOfFateEffectCommand : IItemEffectCommand
    {
        private int changeCount;

        public DiceOfFateEffectCommand(int changeCount = 1)
        {
            this.changeCount = Mathf.Max(1, changeCount);
        }

        public bool Execute(IItemUseContext context)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogError("운명의 주사위 실패: 사용자가 null이거나 사망 상태입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // TODO: 실제 운명의 주사위 시스템과 연동
            GameLogger.LogInfo($"운명의 주사위 효과: 다음 적 스킬 {changeCount}개를 무작위로 변경", GameLogger.LogCategory.Core);
            return true;
        }
    }

    /// <summary>
    /// 리롤 효과 커맨드입니다.
    /// </summary>
    public class RerollEffectCommand : IItemEffectCommand
    {
        private int rerollCount;

        public RerollEffectCommand(int rerollCount)
        {
            this.rerollCount = rerollCount;
        }

        public bool Execute(IItemUseContext context)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogError("리롤 실패: 사용자가 null이거나 사망 상태입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // TODO: 실제 리롤 시스템과 연동
            GameLogger.LogInfo($"리롤 효과: 카드 {rerollCount}장 다시 드로우", GameLogger.LogCategory.Core);
            return true;
        }
    }

    /// <summary>
    /// 실드 브레이커 효과 커맨드입니다.
    /// </summary>
    public class ShieldBreakerEffectCommand : IItemEffectCommand
    {
        private int duration;

        public ShieldBreakerEffectCommand(int duration = 1)
        {
            this.duration = duration;
        }

        public bool Execute(IItemUseContext context)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogError("실드 브레이커 실패: 사용자가 null이거나 사망 상태입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // TODO: 실제 실드 브레이커 시스템과 연동
            GameLogger.LogInfo($"실드 브레이커 효과: 방어/반격 무시 ({duration}턴)", GameLogger.LogCategory.Core);
            return true;
        }
    }
}

