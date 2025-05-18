namespace Game.SkillCardSystem.Interface
{
    public interface ISkillCardUI
    {
        void SetCard(ISkillCard card);
        ISkillCard GetCard();
    }
}