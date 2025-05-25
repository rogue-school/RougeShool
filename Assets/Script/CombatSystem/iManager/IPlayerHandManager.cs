using System.Collections.Generic;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.IManager
{
    /// <summary>
    /// 플레이어의 스킬 카드 핸드 슬롯을 제어하는 매니저 인터페이스입니다.
    /// 쿨타임 감소 및 카드 복귀 등의 전투 흐름 기능을 포함합니다.
    /// </summary>
    public interface IPlayerHandManager
    {
        /// <summary>전투 시작 시 필요한 의존성을 주입합니다.</summary>
        void Inject(IPlayerCharacter player, ISlotRegistry slotRegistry, ICombatTurnManager turnManager);

        void Initialize();
        void GenerateInitialHand();
        void TickCoolTime();
        void RestoreCardToHand(PlayerSkillCardRuntime card);
        void RestoreCardToHand(ISkillCard card); // 오버로드 추가
        void EnableCardInteraction(bool isEnabled);
        void EnableInput(bool isEnabled);
        void ClearAll();

        /// <summary>현재 핸드 슬롯들을 반환합니다.</summary>
        IEnumerable<IHandCardSlot> GetAllHandSlots();

        /// <summary>특정 슬롯에 등록된 스킬 카드를 반환합니다.</summary>
        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);

        /// <summary>특정 슬롯에 등록된 스킬 카드 UI를 반환합니다.</summary>
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);
    }
}
