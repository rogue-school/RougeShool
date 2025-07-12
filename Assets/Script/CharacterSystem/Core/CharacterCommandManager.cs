using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 캐릭터 명령 관리자 (Command Pattern)
    /// 캐릭터 액션을 명령으로 캡슐화하고 실행 히스토리를 관리합니다.
    /// </summary>
    public class CharacterCommandManager : MonoBehaviour, ICharacterCommandManager
    {
        [Header("명령 관리")]
        [SerializeField] private int maxCommandHistory = 50;

        private List<ICharacterCommand> commandHistory = new();
        private ICharacter character;

        #region Unity Lifecycle

        private void Awake()
        {
            character = GetComponent<ICharacter>();
        }

        #endregion

        #region ICharacterCommandManager 구현

        public void ExecuteCommand(ICharacterCommand command, ICharacter targetCharacter)
        {
            if (command == null || targetCharacter == null)
            {
                Debug.LogWarning("[CharacterCommandManager] 명령 또는 캐릭터가 null입니다.");
                return;
            }

            if (!command.CanExecute(targetCharacter))
            {
                Debug.LogWarning($"[CharacterCommandManager] 명령 실행 불가: {command.Description}");
                return;
            }

            // 명령 실행
            command.Execute(targetCharacter);

            // 히스토리에 추가
            AddToHistory(command);

            Debug.Log($"[CharacterCommandManager] 명령 실행: {command.Description}");
        }

        public void UndoLastCommand()
        {
            if (commandHistory.Count > 0)
            {
                var lastCommand = commandHistory[commandHistory.Count - 1];
                lastCommand.Undo(character);
                commandHistory.RemoveAt(commandHistory.Count - 1);

                Debug.Log($"[CharacterCommandManager] 마지막 명령 되돌림: {lastCommand.Description}");
            }
        }

        public void UndoAllCommands()
        {
            for (int i = commandHistory.Count - 1; i >= 0; i--)
            {
                commandHistory[i].Undo(character);
            }

            commandHistory.Clear();
            Debug.Log("[CharacterCommandManager] 모든 명령 되돌림");
        }

        public List<ICharacterCommand> CommandHistory => new List<ICharacterCommand>(commandHistory);

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 명령을 히스토리에 추가합니다.
        /// </summary>
        /// <param name="command">추가할 명령</param>
        private void AddToHistory(ICharacterCommand command)
        {
            commandHistory.Add(command);

            // 히스토리 크기 제한
            if (commandHistory.Count > maxCommandHistory)
            {
                commandHistory.RemoveAt(0);
            }
        }

        #endregion

        #region 편의 메서드

        /// <summary>
        /// 특정 타입의 명령들을 되돌립니다.
        /// </summary>
        /// <param name="commandType">되돌릴 명령 타입</param>
        public void UndoCommandsByType(System.Type commandType)
        {
            for (int i = commandHistory.Count - 1; i >= 0; i--)
            {
                if (commandHistory[i].GetType() == commandType)
                {
                    commandHistory[i].Undo(character);
                    commandHistory.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 명령 히스토리를 출력합니다.
        /// </summary>
        public void PrintCommandHistory()
        {
            Debug.Log($"[CharacterCommandManager] 명령 히스토리 ({commandHistory.Count}개):");
            for (int i = 0; i < commandHistory.Count; i++)
            {
                Debug.Log($"  {i + 1}. {commandHistory[i].Description}");
            }
        }

        /// <summary>
        /// 히스토리를 초기화합니다.
        /// </summary>
        public void ClearHistory()
        {
            commandHistory.Clear();
            Debug.Log("[CharacterCommandManager] 히스토리 초기화");
        }

        #endregion
    }

    #region 구체적인 명령 클래스들

    /// <summary>
    /// 캐릭터 피해 명령
    /// </summary>
    public class DamageCharacterCommand : ICharacterCommand
    {
        private readonly int damage;

        public DamageCharacterCommand(int damage)
        {
            this.damage = damage;
        }

        public string Description => $"피해 입히기: {damage}";

        public void Execute(ICharacter character)
        {
            character.TakeDamage(damage);
        }

        public void Undo(ICharacter character)
        {
            character.Heal(damage);
        }

        public bool CanExecute(ICharacter character)
        {
            return character != null && !character.IsDead() && damage > 0;
        }
    }

    /// <summary>
    /// 캐릭터 회복 명령
    /// </summary>
    public class HealCharacterCommand : ICharacterCommand
    {
        private readonly int heal;

        public HealCharacterCommand(int heal)
        {
            this.heal = heal;
        }

        public string Description => $"회복: {heal}";

        public void Execute(ICharacter character)
        {
            character.Heal(heal);
        }

        public void Undo(ICharacter character)
        {
            character.TakeDamage(heal);
        }

        public bool CanExecute(ICharacter character)
        {
            return character != null && !character.IsDead() && heal > 0;
        }
    }

    /// <summary>
    /// 캐릭터 가드 명령
    /// </summary>
    public class GuardCharacterCommand : ICharacterCommand
    {
        private readonly int guard;

        public GuardCharacterCommand(int guard)
        {
            this.guard = guard;
        }

        public string Description => $"가드 부여: {guard}";

        public void Execute(ICharacter character)
        {
            character.GainGuard(guard);
        }

        public void Undo(ICharacter character)
        {
            // 가드 감소는 별도 메서드가 필요할 수 있음
            // 현재는 기본 구현으로 남겨둠
        }

        public bool CanExecute(ICharacter character)
        {
            return character != null && !character.IsDead() && guard > 0;
        }
    }

    /// <summary>
    /// 캐릭터 상태 변경 명령
    /// </summary>
    public class ChangeCharacterStateCommand : ICharacterCommand
    {
        private readonly string newState;
        private string previousState;

        public ChangeCharacterStateCommand(string newState)
        {
            this.newState = newState;
        }

        public string Description => $"상태 변경: {newState}";

        public void Execute(ICharacter character)
        {
            // 상태 변경 로직은 CharacterStateManager에서 처리
            var stateManager = character.Transform.GetComponent<CharacterStateManager>();
            if (stateManager != null)
            {
                previousState = stateManager.CurrentState?.StateName;
                stateManager.ChangeStateByName(newState);
            }
        }

        public void Undo(ICharacter character)
        {
            if (!string.IsNullOrEmpty(previousState))
            {
                var stateManager = character.Transform.GetComponent<CharacterStateManager>();
                stateManager?.ChangeStateByName(previousState);
            }
        }

        public bool CanExecute(ICharacter character)
        {
            return character != null && !character.IsDead();
        }
    }

    #endregion
} 