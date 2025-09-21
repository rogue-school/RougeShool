namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 게임 초기화 처리를 위한 통합 인터페이스입니다.
    /// 핸드 슬롯과 전투 슬롯의 초기화를 모두 담당합니다.
    /// </summary>
    public interface IHandInitializer
    {
        /// <summary>
        /// 플레이어 및 적의 핸드 슬롯을 초기화합니다.
        /// 슬롯 생성, 정렬, 초기 카드 배치 등의 작업을 포함할 수 있습니다.
        /// </summary>
        void SetupHands();

        /// <summary>
        /// 모든 슬롯에 대해 자동 바인딩을 수행합니다.
        /// 슬롯 UI와 슬롯 레지스트리를 연결하거나,
        /// 카드 드래그/드롭 기능과 상호작용을 준비합니다.
        /// </summary>
        void AutoBindAllSlots();
    }
}
