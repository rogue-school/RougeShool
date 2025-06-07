namespace Game.SkillCardSystem.Interface
{
    public interface ISkillCardUI
    {
        void SetCard(ISkillCard card);
        ISkillCard GetCard();

        /// <summary>
        /// 현재 카드의 쿨타임 정보를 기반으로 UI를 갱신합니다.
        /// </summary>
        void UpdateCoolTimeDisplay();
    }
}
