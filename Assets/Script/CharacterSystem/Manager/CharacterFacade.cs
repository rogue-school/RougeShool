using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Core;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 캐릭터 시스템 파사드 (Facade Pattern)
    /// 복잡한 캐릭터 시스템을 단순한 인터페이스로 제공합니다.
    /// </summary>
    public class CharacterFacade : MonoBehaviour, ICharacterFacade
    {
        public static CharacterFacade Instance { get; private set; }

        [Header("시스템 컴포넌트")]
        [SerializeField] private CharacterFactory characterFactory;
        [SerializeField] private CharacterStateManager stateManager;
        [SerializeField] private CharacterCommandManager commandManager;

        [Header("캐릭터 관리")]
        [SerializeField] private List<ICharacter> activeCharacters = new();

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeComponents();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 시스템 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // 팩토리 초기화
            if (characterFactory == null)
                characterFactory = GetComponent<CharacterFactory>();

            if (characterFactory == null)
                characterFactory = gameObject.AddComponent<CharacterFactory>();

            Debug.Log("[CharacterFacade] 캐릭터 시스템 초기화 완료");
        }

        #endregion

        #region ICharacterFacade 구현

        public IPlayerCharacter CreatePlayerCharacter(PlayerCharacterData data, Vector3 position)
        {
            if (characterFactory == null)
            {
                Debug.LogError("[CharacterFacade] CharacterFactory가 초기화되지 않았습니다.");
                return null;
            }

            var playerCharacter = characterFactory.CreatePlayerCharacter(data, position);
            if (playerCharacter != null)
            {
                activeCharacters.Add(playerCharacter);
                RegisterCharacterObservers(playerCharacter);
                Debug.Log($"[CharacterFacade] 플레이어 캐릭터 생성: {data.DisplayName}");
            }

            return playerCharacter;
        }

        public IEnemyCharacter CreateEnemyCharacter(EnemyCharacterData data, Vector3 position)
        {
            if (characterFactory == null)
            {
                Debug.LogError("[CharacterFacade] CharacterFactory가 초기화되지 않았습니다.");
                return null;
            }

            var enemyCharacter = characterFactory.CreateEnemyCharacter(data, position);
            if (enemyCharacter != null)
            {
                activeCharacters.Add(enemyCharacter);
                RegisterCharacterObservers(enemyCharacter);
                Debug.Log($"[CharacterFacade] 적 캐릭터 생성: {data.DisplayName}");
            }

            return enemyCharacter;
        }

        public void DamageCharacter(ICharacter character, int damage)
        {
            if (character == null)
            {
                Debug.LogWarning("[CharacterFacade] 캐릭터가 null입니다.");
                return;
            }

            // 명령 패턴을 사용하여 피해 처리
            var damageCommand = new DamageCharacterCommand(damage);
            ExecuteCommand(damageCommand, character);

            Debug.Log($"[CharacterFacade] 캐릭터 피해: {character.GetCharacterName()} - {damage}");
        }

        public void HealCharacter(ICharacter character, int heal)
        {
            if (character == null)
            {
                Debug.LogWarning("[CharacterFacade] 캐릭터가 null입니다.");
                return;
            }

            // 명령 패턴을 사용하여 회복 처리
            var healCommand = new HealCharacterCommand(heal);
            ExecuteCommand(healCommand, character);

            Debug.Log($"[CharacterFacade] 캐릭터 회복: {character.GetCharacterName()} - {heal}");
        }

        public void GiveGuardToCharacter(ICharacter character, int guard)
        {
            if (character == null)
            {
                Debug.LogWarning("[CharacterFacade] 캐릭터가 null입니다.");
                return;
            }

            // 명령 패턴을 사용하여 가드 처리
            var guardCommand = new GuardCharacterCommand(guard);
            ExecuteCommand(guardCommand, character);

            Debug.Log($"[CharacterFacade] 캐릭터 가드: {character.GetCharacterName()} - {guard}");
        }

        public void ChangeCharacterState(ICharacter character, string stateName)
        {
            if (character == null)
            {
                Debug.LogWarning("[CharacterFacade] 캐릭터가 null입니다.");
                return;
            }

            // 명령 패턴을 사용하여 상태 변경 처리
            var stateCommand = new ChangeCharacterStateCommand(stateName);
            ExecuteCommand(stateCommand, character);

            Debug.Log($"[CharacterFacade] 캐릭터 상태 변경: {character.GetCharacterName()} - {stateName}");
        }

        public void RemoveCharacter(ICharacter character)
        {
            if (character == null) return;

            if (activeCharacters.Remove(character))
            {
                // 캐릭터 제거 로직
                if (character.Transform != null)
                {
                    Destroy(character.Transform.gameObject);
                }

                Debug.Log($"[CharacterFacade] 캐릭터 제거: {character.GetCharacterName()}");
            }
        }

        public void ResetAllCharacters()
        {
            foreach (var character in activeCharacters.ToArray())
            {
                RemoveCharacter(character);
            }

            activeCharacters.Clear();
            Debug.Log("[CharacterFacade] 모든 캐릭터 초기화");
        }

        public void CheckSystemStatus()
        {
            Debug.Log($"[CharacterFacade] 시스템 상태:");
            Debug.Log($"  - 활성 캐릭터 수: {activeCharacters.Count}");
            Debug.Log($"  - 팩토리 상태: {(characterFactory != null ? "정상" : "오류")}");
            Debug.Log($"  - 상태 관리자: {(stateManager != null ? "정상" : "오류")}");
            Debug.Log($"  - 명령 관리자: {(commandManager != null ? "정상" : "오류")}");
        }

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 명령을 실행합니다.
        /// </summary>
        /// <param name="command">실행할 명령</param>
        /// <param name="character">대상 캐릭터</param>
        private void ExecuteCommand(ICharacterCommand command, ICharacter character)
        {
            if (commandManager != null)
            {
                commandManager.ExecuteCommand(command, character);
            }
            else
            {
                // 명령 관리자가 없으면 직접 실행
                command.Execute(character);
            }
        }

        /// <summary>
        /// 캐릭터에 관찰자들을 등록합니다.
        /// </summary>
        /// <param name="character">등록할 캐릭터</param>
        private void RegisterCharacterObservers(ICharacter character)
        {
            var subject = character.Transform.GetComponent<CharacterSubject>();
            if (subject != null)
            {
                // UI 관찰자 등록
                var uiObserver = character.Transform.GetComponent<CharacterUIObserver>();
                if (uiObserver != null)
                {
                    subject.RegisterObserver(uiObserver);
                }

                // 애니메이션 관찰자 등록
                var animationObserver = character.Transform.GetComponent<CharacterAnimationObserver>();
                if (animationObserver != null)
                {
                    subject.RegisterObserver(animationObserver);
                }

                // 로그 관찰자 등록
                var logObserver = character.Transform.GetComponent<CharacterLogObserver>();
                if (logObserver != null)
                {
                    subject.RegisterObserver(logObserver);
                }
            }
        }

        #endregion

        #region 편의 메서드

        /// <summary>
        /// 활성 캐릭터 목록을 반환합니다.
        /// </summary>
        /// <returns>활성 캐릭터 배열</returns>
        public ICharacter[] GetActiveCharacters()
        {
            return activeCharacters.ToArray();
        }

        /// <summary>
        /// 특정 타입의 캐릭터들을 반환합니다.
        /// </summary>
        /// <typeparam name="T">캐릭터 타입</typeparam>
        /// <returns>해당 타입의 캐릭터 배열</returns>
        public T[] GetCharactersOfType<T>() where T : class, ICharacter
        {
            var result = new List<T>();
            foreach (var character in activeCharacters)
            {
                if (character is T typedCharacter)
                {
                    result.Add(typedCharacter);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 캐릭터 이름으로 캐릭터를 찾습니다.
        /// </summary>
        /// <param name="characterName">찾을 캐릭터 이름</param>
        /// <returns>찾은 캐릭터 또는 null</returns>
        public ICharacter FindCharacterByName(string characterName)
        {
            foreach (var character in activeCharacters)
            {
                if (character.GetCharacterName() == characterName)
                {
                    return character;
                }
            }
            return null;
        }

        /// <summary>
        /// 모든 캐릭터에게 동일한 액션을 적용합니다.
        /// </summary>
        /// <param name="action">적용할 액션</param>
        public void ApplyToAllCharacters(System.Action<ICharacter> action)
        {
            foreach (var character in activeCharacters)
            {
                action?.Invoke(character);
            }
        }

        #endregion
    }
} 