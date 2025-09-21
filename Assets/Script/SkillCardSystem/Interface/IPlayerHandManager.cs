using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 플레이어의 핸드(손패) 관리 인터페이스입니다.
    /// 카드 추가/제거와 슬롯 관리를 담당합니다.
    /// </summary>
    public interface IPlayerHandManager
    {
        /// <summary>
        /// 현재 플레이어 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="player">플레이어 캐릭터</param>
        void SetPlayer(IPlayerCharacter player);

        /// <summary>
        /// 게임 시작 시 초기 손패를 생성합니다.
        /// </summary>
        void GenerateInitialHand();

        /// <summary>
        /// 지정한 슬롯 위치에 있는 카드를 반환합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <returns>해당 슬롯의 카드</returns>
        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);

        /// <summary>
        /// 지정한 슬롯 위치에 있는 카드 UI를 반환합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <returns>해당 슬롯의 카드 UI</returns>
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);

        /// <summary>
        /// 빈 슬롯에 카드를 추가합니다.
        /// </summary>
        /// <param name="card">추가할 카드</param>
        void AddCardToHand(ISkillCard card);

        /// <summary>
        /// 특정 슬롯에 카드를 추가합니다.
        /// </summary>
        /// <param name="slot">슬롯 위치</param>
        /// <param name="card">추가할 카드</param>
        void AddCardToSlot(SkillCardSlotPosition slot, ISkillCard card);

        /// <summary>
        /// 카드를 제거합니다.
        /// </summary>
        /// <param name="card">제거할 카드</param>
        void RemoveCard(ISkillCard card);

        /// <summary>
        /// 카드 드래그 등 입력을 활성/비활성화합니다.
        /// </summary>
        /// <param name="enable">활성화 여부</param>
        void EnableInput(bool enable);

        /// <summary>
        /// 모든 핸드 슬롯과 카드 UI를 제거합니다.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 플레이어 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>플레이어 캐릭터</returns>
        IPlayerCharacter GetPlayer();
    }
}
