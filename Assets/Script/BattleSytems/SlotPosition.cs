namespace Game.Battle
{
    /// <summary>
    /// 전투 슬롯의 위치를 정의하는 열거형입니다.
    /// </summary>
    public enum SlotPosition
    {
        FRONT,      // 선공 슬롯
        BACK,       // 후공 슬롯
        SUPPORT,    // 서포트 슬롯 (향후 확장 가능)
        UNKNOWN     // 정의되지 않은 위치
    }
}