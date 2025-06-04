using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Interface
{
    public interface IPlayerHandManager
    {
        void SetPlayer(IPlayerCharacter player);
        void GenerateInitialHand();
        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);
        void RestoreCardToHand(ISkillCard card);
        void RestoreCardToHand(ISkillCard card, SkillCardSlotPosition slot);
        void LogPlayerHandSlotStates();
        void EnableInput(bool enable);
        void ClearAll();

        /// <summary>
        /// 현재 핸드에 존재하는 모든 카드와 UI를 반환합니다.
        /// 쿨타임 처리 및 UI 갱신에 사용됩니다.
        /// </summary>
        IEnumerable<(ISkillCard card, ISkillCardUI ui)> GetAllHandCards();
    }
}
