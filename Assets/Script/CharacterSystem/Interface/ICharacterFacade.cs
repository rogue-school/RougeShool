using UnityEngine;
using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터 시스템의 파사드 인터페이스
    /// 복잡한 캐릭터 시스템을 단순한 인터페이스로 제공
    /// </summary>
    public interface ICharacterFacade
    {
        /// <summary>
        /// 플레이어 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="data">캐릭터 데이터</param>
        /// <param name="position">생성 위치</param>
        /// <returns>생성된 플레이어 캐릭터</returns>
        IPlayerCharacter CreatePlayerCharacter(PlayerCharacterData data, Vector3 position);

        /// <summary>
        /// 적 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="data">캐릭터 데이터</param>
        /// <param name="position">생성 위치</param>
        /// <returns>생성된 적 캐릭터</returns>
        IEnemyCharacter CreateEnemyCharacter(EnemyCharacterData data, Vector3 position);

        /// <summary>
        /// 캐릭터에게 피해를 입힙니다.
        /// </summary>
        /// <param name="character">대상 캐릭터</param>
        /// <param name="damage">피해량</param>
        void DamageCharacter(ICharacter character, int damage);

        /// <summary>
        /// 캐릭터를 회복시킵니다.
        /// </summary>
        /// <param name="character">대상 캐릭터</param>
        /// <param name="heal">회복량</param>
        void HealCharacter(ICharacter character, int heal);

        /// <summary>
        /// 캐릭터에게 가드를 부여합니다.
        /// </summary>
        /// <param name="character">대상 캐릭터</param>
        /// <param name="guard">가드량</param>
        void GiveGuardToCharacter(ICharacter character, int guard);

        /// <summary>
        /// 캐릭터 상태를 변경합니다.
        /// </summary>
        /// <param name="character">대상 캐릭터</param>
        /// <param name="stateName">새로운 상태 이름</param>
        void ChangeCharacterState(ICharacter character, string stateName);

        /// <summary>
        /// 캐릭터를 제거합니다.
        /// </summary>
        /// <param name="character">제거할 캐릭터</param>
        void RemoveCharacter(ICharacter character);

        /// <summary>
        /// 모든 캐릭터를 초기화합니다.
        /// </summary>
        void ResetAllCharacters();

        /// <summary>
        /// 시스템 상태를 확인합니다.
        /// </summary>
        void CheckSystemStatus();
    }
} 