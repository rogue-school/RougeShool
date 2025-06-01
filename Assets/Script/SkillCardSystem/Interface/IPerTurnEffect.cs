using Game.CharacterSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    public interface IPerTurnEffect
    {
        bool IsExpired { get; }
        void OnTurnStart(ICharacter target);
    }
}
