using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using System;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 캐릭터의 기본 베이스 클래스입니다.
    /// 체력, 가드, 턴 효과, UI 연동 등의 기능을 제공합니다.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        #region 이벤트
        public event Action<int, int> OnHPChanged;
        public event Action<bool> OnGuardStateChanged;
        public event Action<IReadOnlyList<IPerTurnEffect>> OnBuffsChanged;
        #endregion
        #region 캐릭터 상태 필드

        /// <summary>캐릭터의 최대 체력</summary>
        protected int maxHP;

        /// <summary>캐릭터의 현재 체력</summary>
        protected int currentHP;

        /// <summary>캐릭터의 현재 가드 수치</summary>
        protected int currentGuard;

        /// <summary>현재 가드 상태 여부</summary>
        protected bool isGuarded = false;

        /// <summary>턴마다 적용되는 효과 리스트</summary>
        protected List<IPerTurnEffect> perTurnEffects = new();

        public virtual Transform Transform => transform;

        #endregion

        #region 상태 정보 접근자

        /// <summary>캐릭터 이름 반환</summary>
        public virtual string GetCharacterName() => gameObject.name;

        /// <summary>캐릭터 이름 프로퍼티</summary>
        public virtual string CharacterName => GetCharacterName();

        /// <summary>캐릭터 데이터 프로퍼티</summary>
        public virtual object CharacterData => null;

        /// <summary>데이터 기반 캐릭터 이름 반환 (자식에서 override)</summary>
        protected virtual string GetCharacterDataName() => "Unknown";

        /// <summary>현재 체력 반환</summary>
        public virtual int GetHP() => currentHP;

        /// <summary>현재 체력 반환 (명시적 호출용)</summary>
        public int GetCurrentHP() => currentHP;

        /// <summary>최대 체력 반환</summary>
        public virtual int GetMaxHP() => maxHP;

        public virtual IReadOnlyList<IPerTurnEffect> GetBuffs() => perTurnEffects.AsReadOnly();

        /// <summary>캐릭터 초상화 이미지 반환 (기본은 null)</summary>
        public virtual Sprite GetPortrait() => null;

        /// <summary>현재 가드 상태 여부 확인</summary>
        public virtual bool IsGuarded() => isGuarded;

        /// <summary>캐릭터가 사망했는지 확인</summary>
        public virtual bool IsDead() => currentHP <= 0;

        /// <summary>캐릭터가 살아있는지 확인</summary>
        public virtual bool IsAlive() => currentHP > 0;

        #endregion

        #region 이벤트 처리 메서드 (자식 클래스에서 오버라이드)

        /// <summary>가드 획득 시 호출되는 메서드</summary>
        /// <param name="amount">가드 획득량</param>
        protected virtual void OnGuarded(int amount) { }

        /// <summary>회복 시 호출되는 메서드</summary>
        /// <param name="amount">회복량</param>
        protected virtual void OnHealed(int amount) { }

        /// <summary>피해 시 호출되는 메서드</summary>
        /// <param name="amount">피해량</param>
        protected virtual void OnDamaged(int amount) { }

        #endregion

        #region 상태 설정 메서드

        /// <summary>가드 상태 설정</summary>
        /// <param name="value">true면 가드 상태 활성</param>
        public virtual void SetGuarded(bool value)
        {
            isGuarded = value;
            Debug.Log($"[{GetCharacterName()}] 가드 상태: {isGuarded}");
            OnGuardStateChanged?.Invoke(isGuarded);
        }

        /// <summary>가드 수치 증가</summary>
        /// <param name="amount">증가량</param>
        public virtual void GainGuard(int amount)
        {
            currentGuard += amount;
            Debug.Log($"[{GetCharacterName()}] 가드 +{amount} → 현재 가드: {currentGuard}");

            // 가드 이벤트 발행은 자식 클래스에서 처리
            OnGuarded(amount);
        }

        /// <summary>체력을 회복시킵니다</summary>
        /// <param name="amount">회복량</param>
        public virtual void Heal(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[{GetCharacterName()}] 잘못된 회복량 무시됨: {amount}");
                return;
            }

            currentHP = Mathf.Min(currentHP + amount, maxHP);
            OnHPChanged?.Invoke(currentHP, maxHP);

            Debug.Log($"[{GetCharacterName()}] 회복: {amount}, 현재 체력: {currentHP}");

            // 회복 이벤트 발행은 자식 클래스에서 처리
            OnHealed(amount);
        }

        /// <summary>피해를 받아 체력을 감소시킵니다</summary>
        /// <param name="amount">피해량</param>
        public virtual void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[{GetCharacterDataName()}] 잘못된 피해량 무시됨: {amount}");
                return;
            }

            currentHP = Mathf.Max(currentHP - amount, 0);
            OnHPChanged?.Invoke(currentHP, maxHP);

            Debug.Log($"[{GetCharacterDataName()}] 피해: {amount}, 남은 체력: {currentHP}");

            // 피해 이벤트 발행은 자식 클래스에서 처리
            OnDamaged(amount);

            if (IsDead())
                Die();
        }

        /// <summary>최대 체력을 설정하고 현재 체력을 동일하게 초기화합니다</summary>
        /// <param name="value">최대 체력 값</param>
        public virtual void SetMaxHP(int value)
        {
            maxHP = value;
            currentHP = maxHP;
            OnHPChanged?.Invoke(currentHP, maxHP);
        }

        #endregion

        #region UI 및 효과 관리

        // 레거시 CharacterUIController 제거됨

        /// <summary>턴 효과 등록</summary>
        /// <param name="effect">턴 효과 인스턴스</param>
        public virtual void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            if (!perTurnEffects.Contains(effect))
            {
                perTurnEffects.Add(effect);
                OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
            }
        }

        /// <summary>등록된 턴 효과를 처리합니다 (만료된 효과 제거 포함)</summary>
        public virtual void ProcessTurnEffects()
        {
            foreach (var effect in perTurnEffects.ToArray())
            {
                effect.OnTurnStart(this);

                if (effect.IsExpired)
                {
                    perTurnEffects.Remove(effect);
                    OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
                }
            }
        }

        #endregion

        #region 사망 및 제어

        /// <summary>캐릭터 사망 처리</summary>
        public virtual void Die()
        {
            Debug.Log($"[{GetCharacterName()}] 사망 처리됨.");
        }

        /// <summary>
        /// 플레이어 조종 캐릭터인지 여부 반환  
        /// 반드시 자식 클래스에서 구현되어야 합니다.
        /// </summary>
        public abstract bool IsPlayerControlled();

        /// <summary>
        /// 캐릭터 데이터를 설정합니다.
        /// </summary>
        /// <param name="data">설정할 캐릭터 데이터</param>
        public virtual void SetCharacterData(object data)
        {
            // TODO: 자식 클래스에서 구현
        }

        /// <summary>
        /// 핸드 매니저를 주입합니다.
        /// </summary>
        /// <param name="handManager">주입할 핸드 매니저</param>
        public virtual void InjectHandManager(object handManager)
        {
            // TODO: 자식 클래스에서 구현
        }

        #endregion
    }
}
