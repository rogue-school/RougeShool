namespace Game.SkillCardSystem.Data
{
    /// <summary>
    /// 슬롯의 용도를 정의합니다.
    /// 각 슬롯은 전투에서의 역할 또는 UI용도로 구분됩니다.
    /// </summary>
    public enum SlotRole
    {
        /// <summary>
        /// 캐릭터가 배치되는 슬롯입니다.
        /// </summary>
        CharacterSpawn,

        /// <summary>
        /// 전투 중 카드가 드롭되어 실행되는 슬롯입니다.
        /// </summary>
        CardDrop,

        /// <summary>
        /// UI 전용 슬롯으로, 카드 드래그나 실행과는 무관합니다.
        /// </summary>
        UIOnly
    }
}
