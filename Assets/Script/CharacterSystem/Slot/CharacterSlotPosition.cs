namespace Game.CharacterSystem.Slot
{
    /// <summary>
    /// 전투 화면에서 캐릭터가 배치되는 슬롯 위치를 나타냅니다.
    /// 이 위치는 캐릭터의 시각적 배치 및 전투 로직 처리에 사용됩니다.
    /// </summary>
    public enum CharacterSlotPosition
    {
        /// <summary>
        /// 플레이어 캐릭터가 고정으로 배치되는 위치입니다.
        /// </summary>
        Player,

        /// <summary>
        /// 적 캐릭터가 고정으로 배치되는 위치입니다.
        /// </summary>
        Enemy
    }
}
