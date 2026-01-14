using Game.SkillCardSystem.Interface;
using UnityEngine;
using System;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터의 공통 인터페이스입니다.
    /// 플레이어나 적 캐릭터의 핵심 동작을 정의합니다.
    /// </summary>
    public interface ICharacter
    {
        #region 이벤트

        /// <summary>
        /// 체력 변경 이벤트 (현재 체력, 최대 체력)
        /// </summary>
        event Action<int, int> OnHPChanged;

        /// <summary>
        /// 가드 상태 변경 이벤트 (isGuarded)
        /// </summary>
        event Action<bool> OnGuardStateChanged;

        /// <summary>
        /// 버프/디버프 목록 변경 이벤트 (읽기 전용 리스트 스냅샷)
        /// </summary>
        event Action<System.Collections.Generic.IReadOnlyList<IPerTurnEffect>> OnBuffsChanged;

        #endregion
        #region 정보 조회

        /// <summary>
        /// 캐릭터의 이름을 반환합니다
        /// </summary>
        /// <returns>캐릭터 이름</returns>
        string GetCharacterName();

        /// <summary>
        /// 캐릭터의 이름을 반환합니다. (프로퍼티)
        /// </summary>
        string CharacterName { get; }

        /// <summary>
        /// 캐릭터 데이터를 반환합니다.
        /// </summary>
        object CharacterData { get; }

        /// <summary>
        /// 현재 체력을 반환합니다
        /// </summary>
        /// <returns>현재 체력</returns>
        int GetHP();

        /// <summary>
        /// 현재 체력을 반환합니다 (명시적 호출용)
        /// </summary>
        /// <returns>현재 체력</returns>
        int GetCurrentHP();

        /// <summary>
        /// 최대 체력을 반환합니다
        /// </summary>
        /// <returns>최대 체력</returns>
        int GetMaxHP();

        /// <summary>
        /// 현재 적용 중인 버프/디버프 목록을 반환합니다
        /// </summary>
        /// <returns>버프/디버프 목록</returns>
        System.Collections.Generic.IReadOnlyList<IPerTurnEffect> GetBuffs();

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
        /// 가드를 무시하고 피해를 입습니다.
        /// </summary>
        /// <param name="amount">피해량</param>
        void TakeDamageIgnoreGuard(int amount);

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

        #endregion

        #region 턴 효과

        /// <summary>
        /// 턴마다 실행되는 효과를 등록합니다.
        /// </summary>
        /// <param name="effect">등록할 턴 효과</param>
        void RegisterPerTurnEffect(IPerTurnEffect effect);

        /// <summary>
        /// 상태이상 효과를 등록합니다 (가드 상태 확인).
        /// </summary>
        /// <param name="effect">등록할 상태이상 효과</param>
        /// <returns>등록 성공 여부</returns>
        bool RegisterStatusEffect(IPerTurnEffect effect);

        /// <summary>
        /// 등록된 턴 효과들을 처리합니다.
        /// </summary>
        void ProcessTurnEffects();

        /// <summary>
        /// 캐릭터 데이터를 설정합니다.
        /// </summary>
        /// <param name="data">설정할 캐릭터 데이터</param>
        void SetCharacterData(object data);

        /// <summary>
        /// 핸드 매니저를 주입합니다.
        /// </summary>
        /// <param name="handManager">주입할 핸드 매니저</param>
        void InjectHandManager(object handManager);

        #endregion
        Transform Transform { get; }
    }
}
