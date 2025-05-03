namespace Game.Units
{
    /// <summary>
    /// 유닛이 가져야 할 최소 기능을 정의한 인터페이스입니다.
    /// </summary>
    public interface IUnit
    {
        void TakeDamage(int amount);
        int GetCurrentHP();
    }
}
