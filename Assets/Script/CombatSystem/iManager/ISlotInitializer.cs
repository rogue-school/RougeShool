namespace Game.IManager
{
    /// <summary>
    /// 슬롯 자동 바인딩을 수행하는 초기화 인터페이스입니다.
    /// </summary>
    public interface ISlotInitializer
    {
        /// <summary>
        /// 모든 슬롯에 대해 자동 바인딩을 수행합니다.
        /// 슬롯 UI와 슬롯 레지스트리를 연결하거나,
        /// 카드 드래그/드롭 기능과 상호작용을 준비합니다.
        /// </summary>
        void AutoBindAllSlots();
    }
}
