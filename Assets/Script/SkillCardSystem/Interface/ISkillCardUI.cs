namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드 UI를 제어하는 인터페이스입니다.
    /// 카드와 연결하고 시각적 정보를 갱신합니다.
    /// </summary>
    public interface ISkillCardUI
    {
        /// <summary>
        /// 이 UI에 표시할 스킬 카드를 설정합니다.
        /// </summary>
        /// <param name="card">연결할 카드</param>
        void SetCard(ISkillCard card);

        /// <summary>
        /// 현재 UI에 설정된 카드를 반환합니다.
        /// </summary>
        /// <returns>연결된 카드 객체</returns>
        ISkillCard GetCard();

    }
}
