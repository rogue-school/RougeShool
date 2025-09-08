namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 플레이어 캐릭터의 리소스 관리를 위한 인터페이스입니다.
    /// </summary>
    public interface IPlayerResourceManager
    {
        /// <summary>
        /// 현재 리소스 양을 반환합니다.
        /// </summary>
        int CurrentResource { get; }

        /// <summary>
        /// 최대 리소스 양을 반환합니다.
        /// </summary>
        int MaxResource { get; }

        /// <summary>
        /// 리소스 이름을 반환합니다.
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// 리소스를 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 양</param>
        /// <returns>소모 성공 여부</returns>
        bool ConsumeResource(int amount);

        /// <summary>
        /// 리소스를 회복합니다.
        /// </summary>
        /// <param name="amount">회복할 양</param>
        void RestoreResource(int amount);

        /// <summary>
        /// 리소스를 최대치로 회복합니다.
        /// </summary>
        void RestoreToMax();

        /// <summary>
        /// 리소스가 충분한지 확인합니다.
        /// </summary>
        /// <param name="amount">필요한 양</param>
        /// <returns>충분한지 여부</returns>
        bool HasEnoughResource(int amount);
    }
}
