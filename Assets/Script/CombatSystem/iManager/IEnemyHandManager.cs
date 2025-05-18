using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.IManager
{
    /// <summary>
    /// 적의 스킬 핸드를 관리하는 매니저 인터페이스입니다.
    /// 슬롯 기반 카드 정보와 UI 처리를 포함합니다.
    /// </summary>
    public interface IEnemyHandManager
    {
        void Initialize(IEnemyCharacter enemy);
        void GenerateInitialHand();
        void AdvanceSlots();
        void ClearAllSlots();
        void ClearAllUI();
        ISkillCard GetSlotCard(SkillCardSlotPosition position);
        ISkillCard GetCardForCombat();
        ISkillCardUI GetCardUI(int index);
    }

}