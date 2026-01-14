namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드 자체에 누적되는 공격력 스택 제공자 인터페이스입니다.
    /// </summary>
    public interface IAttackPowerStackProvider
    {
        /// <summary>현재 카드의 추가 공격력 스택(0~max)을 반환합니다.</summary>
        int GetAttackPowerStack();

        /// <summary>스택을 1 증가시킵니다. 최댓값을 초과하지 않습니다.</summary>
        /// <param name="max">최대 스택</param>
        void IncrementAttackPowerStack(int max);
    }
}


