namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 플레이어 캐릭터의 타입을 나타냅니다.
    /// 각 타입마다 고유한 스킬카드 특색과 리소스 시스템을 가집니다.
    /// </summary>
    public enum PlayerCharacterType
    {
        /// <summary>
        /// 검사 - 근접 공격 특화, 리소스 없음
        /// </summary>
        Sword,
        
        /// <summary>
        /// 궁수 - 원거리 공격 특화, 화살 리소스
        /// </summary>
        Bow,
        
        /// <summary>
        /// 마법사 - 마법 공격 특화, 마나 리소스
        /// </summary>
        Staff
    }
}
