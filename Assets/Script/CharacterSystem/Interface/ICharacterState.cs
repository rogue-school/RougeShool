using UnityEngine;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터 상태 인터페이스 (State Pattern)
    /// </summary>
    public interface ICharacterState
    {
        /// <summary>
        /// 상태 진입 시 호출됩니다.
        /// </summary>
        /// <param name="character">상태를 적용할 캐릭터</param>
        void Enter(ICharacter character);

        /// <summary>
        /// 상태 업데이트 시 호출됩니다.
        /// </summary>
        /// <param name="character">상태를 적용할 캐릭터</param>
        void Update(ICharacter character);

        /// <summary>
        /// 상태 종료 시 호출됩니다.
        /// </summary>
        /// <param name="character">상태를 적용할 캐릭터</param>
        void Exit(ICharacter character);

        /// <summary>
        /// 상태 이름을 반환합니다.
        /// </summary>
        string StateName { get; }
    }

    /// <summary>
    /// 캐릭터 상태 관리자 인터페이스
    /// </summary>
    public interface ICharacterStateManager
    {
        /// <summary>
        /// 현재 상태를 반환합니다.
        /// </summary>
        ICharacterState CurrentState { get; }

        /// <summary>
        /// 상태를 변경합니다.
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        void ChangeState(ICharacterState newState);

        /// <summary>
        /// 이전 상태로 되돌립니다.
        /// </summary>
        void RevertToPreviousState();

        /// <summary>
        /// 특정 상태인지 확인합니다.
        /// </summary>
        /// <param name="state">확인할 상태</param>
        /// <returns>해당 상태이면 true</returns>
        bool IsInState(ICharacterState state);
    }
} 