using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 플레이어의 핸드(손패) 관리 인터페이스입니다.
    /// 카드의 등록, 제거, UI 연동, 입력 제어 등의 기능을 제공합니다.
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
        /// 지정한 카드를 핸드에 복원합니다. 슬롯 자동 지정.
        /// </summary>
        /// <param name="card">복원할 카드</param>
        void RestoreCardToHand(ISkillCard card);

        /// <summary>
        /// 지정한 슬롯 위치에 카드를 핸드에 복원합니다.
        /// </summary>
        /// <param name="card">복원할 카드</param>
        /// <param name="slot">복원할 슬롯 위치</param>
        void RestoreCardToHand(ISkillCard card, SkillCardSlotPosition slot);

        /// <summary>
        /// 현재 핸드 슬롯 상태를 디버깅 로그로 출력합니다.
        /// </summary>
        void LogPlayerHandSlotStates();

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
        /// 핸드에서 특정 카드를 제거합니다.
        /// </summary>
        /// <param name="card">제거할 카드</param>
        void RemoveCard(ISkillCard card);

        /// <summary>
        /// 현재 핸드에 존재하는 모든 카드와 UI를 튜플로 반환합니다.
        /// </summary>
        /// <returns>(카드, 카드 UI) 쌍 목록</returns>
        IEnumerable<(ISkillCard card, ISkillCardUI ui)> GetAllHandCards();

        // IPlayerCharacter 반환하는 GetPlayer() 메서드 추가
        IPlayerCharacter GetPlayer();
    }
}
