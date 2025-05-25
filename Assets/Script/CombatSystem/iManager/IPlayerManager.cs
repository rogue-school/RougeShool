using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.IManager
{
    /// <summary>
    /// 플레이어 정보를 관리하는 매니저 인터페이스입니다.
    /// </summary>
    public interface IPlayerManager
    {

        /// <summary>플레이어 캐릭터를 등록합니다.</summary>
        void SetPlayer(IPlayerCharacter player);

        /// <summary>현재 플레이어를 반환합니다.</summary>
        IPlayerCharacter GetPlayer();

        /// <summary>핸드 매니저를 등록합니다.</summary>
        void SetPlayerHandManager(IPlayerHandManager manager);

        /// <summary>핸드 매니저를 반환합니다.</summary>
        IPlayerHandManager GetPlayerHandManager();

        /// <summary>
        /// 선택된 캐릭터를 기반으로 플레이어 생성 및 핸드 초기화를 수행합니다.
        /// </summary>
        void CreateAndRegisterPlayer();
        ISkillCard GetCardInSlot(SkillCardSlotPosition pos);
        ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos);
        void Reset();
    }
}
