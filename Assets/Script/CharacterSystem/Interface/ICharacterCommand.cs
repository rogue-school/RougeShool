using UnityEngine;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터 명령 인터페이스 (Command Pattern)
    /// </summary>
    public interface ICharacterCommand
    {
        /// <summary>
        /// 명령을 실행합니다.
        /// </summary>
        /// <param name="character">명령을 실행할 캐릭터</param>
        void Execute(ICharacter character);

        /// <summary>
        /// 명령을 되돌립니다.
        /// </summary>
        /// <param name="character">명령을 되돌릴 캐릭터</param>
        void Undo(ICharacter character);

        /// <summary>
        /// 명령이 실행 가능한지 확인합니다.
        /// </summary>
        /// <param name="character">확인할 캐릭터</param>
        /// <returns>실행 가능하면 true</returns>
        bool CanExecute(ICharacter character);

        /// <summary>
        /// 명령 설명을 반환합니다.
        /// </summary>
        string Description { get; }
    }

    /// <summary>
    /// 캐릭터 명령 관리자 인터페이스
    /// </summary>
    public interface ICharacterCommandManager
    {
        /// <summary>
        /// 명령을 실행합니다.
        /// </summary>
        /// <param name="command">실행할 명령</param>
        /// <param name="character">명령을 실행할 캐릭터</param>
        void ExecuteCommand(ICharacterCommand command, ICharacter character);

        /// <summary>
        /// 마지막 명령을 되돌립니다.
        /// </summary>
        void UndoLastCommand();

        /// <summary>
        /// 모든 명령을 되돌립니다.
        /// </summary>
        void UndoAllCommands();

        /// <summary>
        /// 명령 히스토리를 반환합니다.
        /// </summary>
        System.Collections.Generic.List<ICharacterCommand> CommandHistory { get; }
    }
} 