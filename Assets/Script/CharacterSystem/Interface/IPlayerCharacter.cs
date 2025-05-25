using Game.CharacterSystem.Data;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 플레이어 캐릭터를 제어하기 위한 인터페이스입니다.
    /// </summary>
    public interface IPlayerCharacter : ICharacter
    {
        /// <summary>
        /// 마지막으로 사용한 스킬 카드를 저장합니다.
        /// </summary>
        void SetLastUsedCard(ISkillCard card);

        /// <summary>
        /// 마지막으로 사용한 스킬 카드를 반환합니다.
        /// </summary>
        ISkillCard GetLastUsedCard();

        /// <summary>
        /// 특정 스킬 카드를 핸드로 복귀시킵니다.
        /// </summary>
        void RestoreCardToHand(ISkillCard card);

        /// <summary>
        /// 플레이어의 캐릭터 데이터 (덱 등 포함).
        /// </summary>
        PlayerCharacterData Data { get; }

        /// <summary>
        /// 캐릭터가 생존 중인지 확인합니다.
        /// </summary>
        bool IsAlive();

        ISkillCard GetCardInHandSlot(SkillCardSlotPosition pos);
        ISkillCardUI GetCardUIInHandSlot(SkillCardSlotPosition pos);
        void InjectHandManager(IPlayerHandManager manager);
    }
}
