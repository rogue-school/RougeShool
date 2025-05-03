namespace Game.Interface
{
    /// <summary>
    /// 전투 캐릭터가 가져야 할 최소 기능 정의
    /// 카드 효과 등 외부 시스템에서 사용됨
    /// </summary>
    public interface ICharacter
    {
        void TakeDamage(int amount);
        int GetCurrentHP();
        int GetMaxHP();
    }
}
