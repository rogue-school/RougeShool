namespace Game.Slots
{
    /// <summary>
    /// 슬롯의 용도 - 어떤 목적으로 사용하는 슬롯인지 구분합니다.
    /// </summary>
    public enum SlotRole
    {
        CharacterSpawn, // 캐릭터를 배치하기 위한 슬롯
        CardDrop,       // 카드가 드롭되는 전투 슬롯
        UIOnly          // UI 목적의 슬롯 (드래그 불가 등)
    }
}
