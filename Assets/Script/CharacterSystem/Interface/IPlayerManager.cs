using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 플레이어 캐릭터 및 핸드 관리를 담당하는 매니저 인터페이스입니다.
    /// 플레이어 초기화, 카드 접근, 핸드 접근 등의 기능을 제공합니다.
    /// </summary>
    public interface IPlayerManager
    {
        /// <summary>
        /// 플레이어 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="player">플레이어 캐릭터 인스턴스</param>
        void SetPlayer(IPlayerCharacter player);

        /// <summary>
        /// 현재 플레이어 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>플레이어 캐릭터</returns>
        IPlayerCharacter GetPlayer();

        /// <summary>
        /// 플레이어의 핸드 매니저를 반환합니다.
        /// </summary>
        /// <returns>핸드 매니저 인스턴스</returns>
        IPlayerHandManager GetPlayerHandManager();

        /// <summary>
        /// 플레이어 캐릭터를 생성하고 시스템에 등록합니다.
        /// </summary>
        void CreateAndRegisterPlayer();

        /// <summary>
        /// 지정된 슬롯 위치에 있는 카드를 반환합니다.
        /// </summary>
        /// <param name="pos">카드 슬롯 위치</param>
        /// <returns>해당 위치의 카드</returns>
        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);

        /// <summary>
        /// 지정된 슬롯 위치에 있는 카드 UI를 반환합니다.
        /// </summary>
        /// <param name="pos">카드 슬롯 위치</param>
        /// <returns>해당 위치의 카드 UI</returns>
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);

        /// <summary>
        /// 플레이어 정보 및 상태를 초기화합니다.
        /// </summary>
        void Reset();
    }
}
