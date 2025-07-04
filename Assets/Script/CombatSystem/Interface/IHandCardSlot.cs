using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 핸드 슬롯 인터페이스입니다. 핸드에 존재하는 카드의 데이터 및 UI를 제어합니다.
    /// </summary>
    public interface IHandCardSlot
    {
        /// <summary>
        /// 슬롯에 카드를 등록합니다.
        /// </summary>
        /// <param name="card">등록할 스킬 카드</param>
        void SetCard(ISkillCard card);

        /// <summary>
        /// 슬롯의 카드와 UI를 모두 제거합니다.
        /// </summary>
        void Clear();

        /// <summary>
        /// 슬롯에 등록된 카드 데이터를 반환합니다.
        /// </summary>
        /// <returns>등록된 스킬 카드</returns>
        ISkillCard GetCard();

        /// <summary>
        /// 슬롯의 고유 위치를 반환합니다.
        /// </summary>
        /// <returns>슬롯 위치</returns>
        SkillCardSlotPosition GetSlotPosition();

        /// <summary>
        /// 슬롯 소유자(플레이어 또는 적)를 반환합니다.
        /// </summary>
        /// <returns>슬롯 소유자</returns>
        SlotOwner GetOwner();

        /// <summary>
        /// 슬롯에 카드가 등록되어 있는지 여부를 반환합니다.
        /// </summary>
        /// <returns>카드 존재 여부</returns>
        bool HasCard();

        /// <summary>
        /// 슬롯에 연결된 카드 UI를 반환합니다.
        /// </summary>
        /// <returns>카드 UI 객체</returns>
        ISkillCardUI GetCardUI();

        /// <summary>
        /// Zenject에 의해 등록된 기본 프리팹으로 카드를 슬롯에 부착합니다.
        /// </summary>
        /// <param name="card">부착할 카드</param>
        /// <returns>생성된 카드 UI</returns>
        SkillCardUI AttachCard(ISkillCard card);

        /// <summary>
        /// 명시적으로 전달된 프리팹으로 카드를 슬롯에 부착합니다.
        /// </summary>
        /// <param name="card">부착할 카드</param>
        /// <param name="prefab">사용할 카드 UI 프리팹</param>
        /// <returns>생성된 카드 UI</returns>
        SkillCardUI AttachCard(ISkillCard card, SkillCardUI prefab);

        /// <summary>
        /// 슬롯에서 카드 및 UI를 제거합니다.
        /// </summary>
        void DetachCard();
    }
}
