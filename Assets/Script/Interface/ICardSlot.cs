namespace Game.Cards
{
    public interface ICardSlot
    {
        void SetCard(PlayerCardData card);
        PlayerCardData GetCard();
        void Clear();
        bool HasCard();
    }
}
