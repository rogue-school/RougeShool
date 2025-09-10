using System;

namespace Game.SkillCardSystem.Data
{
    /// <summary>
    /// 스킬 카드의 소유자 정책을 정의합니다.
    /// </summary>
    public enum OwnerPolicy
    {
        PlayerOnly = 0,
        EnemyOnly = 1,
        Shared = 2
    }
}


