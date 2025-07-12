using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 캐릭터 상태 관리자 (State Pattern)
    /// 캐릭터의 다양한 상태를 관리하고 상태 전환을 처리합니다.
    /// </summary>
    public class CharacterStateManager : MonoBehaviour, ICharacterStateManager
    {
        [Header("상태 관리")]
        [SerializeField] private ICharacterState currentState;
        [SerializeField] private ICharacterState previousState;

        private ICharacter character;
        private Dictionary<string, ICharacterState> availableStates = new();

        #region Unity Lifecycle

        private void Awake()
        {
            character = GetComponent<ICharacter>();
            InitializeStates();
        }

        private void Update()
        {
            currentState?.Update(character);
        }

        #endregion

        #region 상태 초기화

        /// <summary>
        /// 사용 가능한 상태들을 초기화합니다.
        /// </summary>
        private void InitializeStates()
        {
            // 기본 상태들 등록
            RegisterState(new CharacterIdleState());
            RegisterState(new CharacterDamagedState());
            RegisterState(new CharacterHealedState());
            RegisterState(new CharacterGuardedState());
            RegisterState(new CharacterDeadState());

            // 초기 상태 설정
            ChangeState(new CharacterIdleState());
        }

        /// <summary>
        /// 새로운 상태를 등록합니다.
        /// </summary>
        /// <param name="state">등록할 상태</param>
        public void RegisterState(ICharacterState state)
        {
            if (state != null && !availableStates.ContainsKey(state.StateName))
            {
                availableStates[state.StateName] = state;
            }
        }

        #endregion

        #region ICharacterStateManager 구현

        public ICharacterState CurrentState => currentState;

        public void ChangeState(ICharacterState newState)
        {
            if (newState == null) return;

            // 이전 상태 저장
            previousState = currentState;

            // 현재 상태 종료
            currentState?.Exit(character);

            // 새 상태 설정
            currentState = newState;

            // 새 상태 진입
            currentState.Enter(character);

            Debug.Log($"[{character?.GetCharacterName()}] 상태 변경: {previousState?.StateName} → {currentState.StateName}");
        }

        public void RevertToPreviousState()
        {
            if (previousState != null)
            {
                ChangeState(previousState);
            }
        }

        public bool IsInState(ICharacterState state)
        {
            return currentState == state;
        }

        #endregion

        #region 편의 메서드

        /// <summary>
        /// 상태 이름으로 상태를 변경합니다.
        /// </summary>
        /// <param name="stateName">상태 이름</param>
        public void ChangeStateByName(string stateName)
        {
            if (availableStates.TryGetValue(stateName, out ICharacterState state))
            {
                ChangeState(state);
            }
            else
            {
                Debug.LogWarning($"[CharacterStateManager] 알 수 없는 상태: {stateName}");
            }
        }

        /// <summary>
        /// 특정 상태인지 확인합니다.
        /// </summary>
        /// <param name="stateName">확인할 상태 이름</param>
        /// <returns>해당 상태이면 true</returns>
        public bool IsInStateByName(string stateName)
        {
            return currentState?.StateName == stateName;
        }

        /// <summary>
        /// 사용 가능한 모든 상태 이름을 반환합니다.
        /// </summary>
        /// <returns>상태 이름 배열</returns>
        public string[] GetAvailableStateNames()
        {
            string[] names = new string[availableStates.Count];
            availableStates.Keys.CopyTo(names, 0);
            return names;
        }

        #endregion
    }

    #region 구체적인 상태 클래스들

    /// <summary>
    /// 캐릭터 대기 상태
    /// </summary>
    public class CharacterIdleState : ICharacterState
    {
        public string StateName => "Idle";

        public void Enter(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 대기 상태 진입");
        }

        public void Update(ICharacter character)
        {
            // 대기 상태에서는 특별한 업데이트 로직이 없음
        }

        public void Exit(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 대기 상태 종료");
        }
    }

    /// <summary>
    /// 캐릭터 피해 상태
    /// </summary>
    public class CharacterDamagedState : ICharacterState
    {
        public string StateName => "Damaged";

        public void Enter(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 피해 상태 진입");
        }

        public void Update(ICharacter character)
        {
            // 피해 상태에서는 특별한 업데이트 로직이 없음
        }

        public void Exit(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 피해 상태 종료");
        }
    }

    /// <summary>
    /// 캐릭터 회복 상태
    /// </summary>
    public class CharacterHealedState : ICharacterState
    {
        public string StateName => "Healed";

        public void Enter(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 회복 상태 진입");
        }

        public void Update(ICharacter character)
        {
            // 회복 상태에서는 특별한 업데이트 로직이 없음
        }

        public void Exit(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 회복 상태 종료");
        }
    }

    /// <summary>
    /// 캐릭터 가드 상태
    /// </summary>
    public class CharacterGuardedState : ICharacterState
    {
        public string StateName => "Guarded";

        public void Enter(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 가드 상태 진입");
        }

        public void Update(ICharacter character)
        {
            // 가드 상태에서는 특별한 업데이트 로직이 없음
        }

        public void Exit(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 가드 상태 종료");
        }
    }

    /// <summary>
    /// 캐릭터 사망 상태
    /// </summary>
    public class CharacterDeadState : ICharacterState
    {
        public string StateName => "Dead";

        public void Enter(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 사망 상태 진입");
        }

        public void Update(ICharacter character)
        {
            // 사망 상태에서는 특별한 업데이트 로직이 없음
        }

        public void Exit(ICharacter character)
        {
            Debug.Log($"[{character.GetCharacterName()}] 사망 상태 종료");
        }
    }

    #endregion
} 