using Game.Effect;

namespace Game.Interface
{
    public interface ICharacter
    {
        string GetName();
        int GetHP();
        int GetMaxHP();
        void TakeDamage(int amount);
        void Heal(int amount);
        void RegisterPerTurnEffect(IPerTurnEffect effect);
        void ProcessTurnEffects();
        bool IsDead(); 
    }
}
