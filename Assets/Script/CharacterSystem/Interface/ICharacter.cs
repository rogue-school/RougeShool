using Game.SkillCardSystem.Interface;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 전투에 참여하는 모든 캐릭터가 구현해야 할 인터페이스입니다.
    /// 체력 관리, 상태 처리, 효과 등록 등의 기능을 포함합니다.
    /// </summary>
    public interface ICharacter
    {
        /// <summary>
        /// 캐릭터의 이름을 반환합니다. (디버깅 및 UI용)
        /// </summary>
        /// <returns>캐릭터 이름 문자열</returns>
        string GetCharacterName();

        /// <summary>
        /// 캐릭터의 현재 체력을 반환합니다.
        /// </summary>
        /// <returns>현재 체력 수치</returns>
        int GetHP();

        /// <summary>
        /// 캐릭터의 현재 체력을 반환합니다.
        /// (GetHP와 동일하며 이름만 다름 — 세부 구분용)
        /// </summary>
        /// <returns>현재 체력 수치</returns>
        int GetCurrentHP();

        /// <summary>
        /// 캐릭터의 최대 체력을 반환합니다.
        /// </summary>
        /// <returns>최대 체력 수치</returns>
        int GetMaxHP();

        /// <summary>
        /// 지정된 데미지를 받아 체력을 감소시킵니다.
        /// </summary>
        /// <param name="amount">감소할 체력 수치</param>
        void TakeDamage(int amount);

        /// <summary>
        /// 지정된 수치만큼 체력을 회복합니다.
        /// </summary>
        /// <param name="amount">회복할 체력 수치</param>
        void Heal(int amount);

        /// <summary>
        /// 매 턴마다 자동으로 실행될 효과를 등록합니다.
        /// </summary>
        /// <param name="effect">등록할 턴 효과</param>
        void RegisterPerTurnEffect(IPerTurnEffect effect);

        /// <summary>
        /// 등록된 모든 턴 효과를 실행합니다.
        /// </summary>
        void ProcessTurnEffects();

        /// <summary>
        /// 캐릭터가 사망 상태인지 여부를 반환합니다.
        /// </summary>
        /// <returns>true = 사망, false = 생존</returns>
        bool IsDead();

        void SetGuarded(bool isGuarded);
        bool IsGuarded();

    }
}
