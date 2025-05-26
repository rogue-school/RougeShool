using System.Collections.Generic;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;

namespace Game.IManager
{
    /// <summary>
    /// 플레이어의 스킬 카드 핸드를 제어하는 매니저 인터페이스입니다.
    /// </summary>
    public interface IPlayerHandManager
    {
        /// <summary>핸드 매니저 초기화에 필요한 의존성 주입</summary>
        void Inject(IPlayerCharacter player, IHandSlotRegistry slotRegistry, ICombatTurnManager turnManager);

        void Initialize();
        void GenerateInitialHand();
        void TickCoolTime();
        void RestoreCardToHand(PlayerSkillCardRuntime card);
        void RestoreCardToHand(ISkillCard card); // 오버로드

        void EnableCardInteraction(bool isEnabled);
        void EnableInput(bool isEnabled);
        void ClearAll();

        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);
        IEnumerable<IHandCardSlot> GetAllHandSlots();
    }
}
