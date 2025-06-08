namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 중 발생하는 이벤트나 정보를 기록하는 로그 서비스 인터페이스입니다.
    /// UI 출력, 디버깅, 리플레이 등을 위한 로그 기록용으로 사용됩니다.
    /// </summary>
    public interface ICombatLogService
    {
        /// <summary>
        /// 전투 로그 메시지를 기록합니다.
        /// 메시지는 콘솔 출력, UI 표시, 파일 저장 등 다양한 방식으로 활용될 수 있습니다.
        /// </summary>
        /// <param name="message">기록할 로그 메시지</param>
        void Log(string message);
    }
}
