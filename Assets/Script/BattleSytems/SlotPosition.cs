namespace Game.Battle
{
    /// <summary>
    /// 카드가 배치될 슬롯의 위치를 나타냅니다.
    /// 확장 시 Middle, Reserve 등 추가 가능
    /// </summary>
    public enum SlotPosition
    {
        Front = 0, // 선공 슬롯
        Back = 1   // 후공 슬롯
    }
}
