using Game.Effect;

namespace Game.Interface
{
    public interface ICharacter
    {
        string GetName();
        int GetHP();
        int GetCurrentHP();
        int GetMaxHP();
        void TakeDamage(int amount);
        void Heal(int amount);
        void RegisterPerTurnEffect(IPerTurnEffect effect);
        void ProcessTurnEffects();
        bool IsDead(); 
    }
}
