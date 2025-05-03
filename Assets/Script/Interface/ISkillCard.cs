using Game.Cards;

namespace Game.Interface
{
    /// <summary>
    /// 스킬 카드가 공통으로 구현해야 하는 인터페이스입니다.
    /// </summary>
    public interface ISkillCard
    {
        string GetName();
        string GetDescription();
        UnityEngine.Sprite GetArtwork();
        ICardEffect CreateEffect();
    }
}
