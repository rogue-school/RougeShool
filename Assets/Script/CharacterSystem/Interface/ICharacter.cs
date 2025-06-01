using Game.SkillCardSystem.Interface;

namespace Game.CharacterSystem.Interface
{
    public interface ICharacter
    {
        string GetCharacterName();
        int GetHP();
        int GetCurrentHP();
        int GetMaxHP();
        void TakeDamage(int amount);
        void Heal(int amount);
        void RegisterPerTurnEffect(IPerTurnEffect effect);
        void ProcessTurnEffects();
        bool IsDead();

        void SetGuarded(bool isGuarded);
        bool IsGuarded();
        void GainGuard(int amount);

        /// <summary>
        /// 플레이어 조작 여부를 반환 (PlayerCharacter → true, EnemyCharacter → false)
        /// </summary>
        bool IsPlayerControlled(); // 추가됨
    }
}
