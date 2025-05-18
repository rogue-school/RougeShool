using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Runtime;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 플레이어 캐릭터를 제어하기 위한 인터페이스입니다.
    /// </summary>
    public interface IPlayerCharacter : ICharacter
    {
        /// <summary>
        /// 현재 캐릭터의 방어 상태를 설정합니다.
        /// </summary>
        void SetGuarded(bool isGuarded);

        /// <summary>
        /// 현재 캐릭터가 방어 중인지 여부를 반환합니다.
        /// </summary>
        bool IsGuarded();

        /// <summary>
        /// 마지막으로 사용한 스킬 카드를 저장합니다.
        /// </summary>
        void SetLastUsedCard(PlayerSkillCardRuntime card);

        /// <summary>
        /// 마지막으로 사용한 스킬 카드를 반환합니다.
        /// </summary>
        PlayerSkillCardRuntime GetLastUsedCard();

        /// <summary>
        /// 특정 스킬 카드를 핸드로 복귀시킵니다.
        /// </summary>
        void RestoreCardToHand(PlayerSkillCardRuntime card);

        /// <summary>
        /// 플레이어의 캐릭터 데이터 (덱 등 포함).
        /// </summary>
        PlayerCharacterData Data { get; }
    }
}
