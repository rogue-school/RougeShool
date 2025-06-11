using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터의 공통 인터페이스입니다.
    /// 플레이어나 적 캐릭터의 핵심 동작을 정의합니다.
    /// </summary>
    public interface ICharacter
    {
        #region 정보 조회

        /// <summary>
        /// 캐릭터의 이름을 반환합니다.
        /// </summary>
        string GetCharacterName();

        /// <summary>
        /// 현재 체력을 반환합니다.
        /// </summary>
        int GetHP();

        /// <summary>
        /// 현재 체력을 반환합니다. (명시적 호출용)
        /// </summary>
        int GetCurrentHP();

        /// <summary>
        /// 최대 체력을 반환합니다.
        /// </summary>
        int GetMaxHP();

        /// <summary>
        /// 플레이어 조작 캐릭터인지 여부를 반환합니다.
        /// </summary>
        /// <returns>플레이어 캐릭터이면 true</returns>
        bool IsPlayerControlled();

        #endregion

        #region 생명/체력 관리

        /// <summary>
        /// 피해를 입습니다.
        /// </summary>
        /// <param name="amount">피해량</param>
        void TakeDamage(int amount);

        /// <summary>
        /// 체력을 회복합니다.
        /// </summary>
        /// <param name="amount">회복량</param>
        void Heal(int amount);

        /// <summary>
        /// 캐릭터가 사망했는지 확인합니다.
        /// </summary>
        /// <returns>사망 상태면 true</returns>
        bool IsDead();

        #endregion

        #region 가드 관련

        /// <summary>
        /// 가드 상태를 설정합니다.
        /// </summary>
        /// <param name="isGuarded">가드 상태 여부</param>
        void SetGuarded(bool isGuarded);

        /// <summary>
        /// 현재 가드 상태를 반환합니다.
        /// </summary>
        /// <returns>가드 상태면 true</returns>
        bool IsGuarded();

        /// <summary>
        /// 가드 수치를 증가시킵니다.
        /// </summary>
        /// <param name="amount">가드 증가량</param>
        void GainGuard(int amount);

        #endregion

        #region 턴 효과

        /// <summary>
        /// 턴마다 실행되는 효과를 등록합니다.
        /// </summary>
        /// <param name="effect">등록할 턴 효과</param>
        void RegisterPerTurnEffect(IPerTurnEffect effect);

        /// <summary>
        /// 등록된 턴 효과들을 처리합니다.
        /// </summary>
        void ProcessTurnEffects();

        #endregion
        Transform Transform { get; }
    }
}
