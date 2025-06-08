namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 핸드 초기화 처리를 위한 인터페이스입니다.
    /// 전투 시작 시 핸드 슬롯을 구성하거나 카드를 배치할 때 사용됩니다.
    /// </summary>
    public interface IHandInitializer
    {
        /// <summary>
        /// 플레이어 및 적의 핸드 슬롯을 초기화합니다.
        /// 슬롯 생성, 정렬, 초기 카드 배치 등의 작업을 포함할 수 있습니다.
        /// </summary>
        void SetupHands();
    }
}
