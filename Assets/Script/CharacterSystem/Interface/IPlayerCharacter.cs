using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 플레이어 캐릭터 전용 인터페이스입니다.
    /// ICharacter를 상속하며, 카드 핸들링 및 플레이어 전용 데이터에 대한 기능을 포함합니다.
    /// </summary>
    public interface IPlayerCharacter : ICharacter
    {
        #region 데이터 접근 및 설정

        /// <summary>
        /// 플레이어 캐릭터의 데이터 스크립터블 객체를 반환합니다.
        /// </summary>
        PlayerCharacterData CharacterData { get; }

        /// <summary>
        /// 플레이어 캐릭터 데이터를 설정합니다.
        /// </summary>
        /// <param name="data">설정할 데이터</param>
        void SetCharacterData(PlayerCharacterData data);

        #endregion

        #region 카드 핸들링

        /// <summary>
        /// 마지막으로 사용한 스킬 카드를 설정합니다.
        /// </summary>
        /// <param name="card">사용한 스킬 카드</param>
        void SetLastUsedCard(ISkillCard card);

        /// <summary>
        /// 마지막으로 사용한 스킬 카드를 반환합니다.
        /// </summary>
        /// <returns>최근 사용한 스킬 카드</returns>
        ISkillCard GetLastUsedCard();

        /// <summary>
        /// 지정된 카드를 핸드로 복원합니다.
        /// </summary>
        /// <param name="card">복원할 카드</param>
        void RestoreCardToHand(ISkillCard card);

        /// <summary>
        /// 지정한 핸드 슬롯 위치의 카드를 반환합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <returns>슬롯에 있는 스킬 카드</returns>
        ISkillCard GetCardInHandSlot(SkillCardSlotPosition pos);

        /// <summary>
        /// 지정한 핸드 슬롯 위치의 카드 UI를 반환합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <returns>슬롯에 있는 스킬 카드 UI</returns>
        ISkillCardUI GetCardUIInHandSlot(SkillCardSlotPosition pos);

        #endregion

        #region 핸드 매니저 주입

        /// <summary>
        /// 핸드 매니저를 의존성 주입합니다.
        /// </summary>
        /// <param name="manager">핸드 매니저</param>
        void InjectHandManager(IPlayerHandManager manager);

        #endregion

        #region 상태 확인

        /// <summary>
        /// 캐릭터가 생존 상태인지 확인합니다.
        /// </summary>
        /// <returns>생존 중이면 true</returns>
        bool IsAlive();

        #endregion
    }
}
