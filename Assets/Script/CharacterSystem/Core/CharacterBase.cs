using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using Game.CoreSystem.Utility;
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
            GameLogger.LogInfo($"[{GetCharacterName()}] 가드 상태: {isGuarded}", GameLogger.LogCategory.Character);
            OnGuardStateChanged?.Invoke(isGuarded);
        }

        /// <summary>가드 수치 증가</summary>
        /// <param name="amount">증가량</param>
        public virtual void GainGuard(int amount)
        {
            if (amount < 0)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 잘못된 가드 증가량: {amount}", GameLogger.LogCategory.Character);
                throw new ArgumentException($"가드 증가량은 음수일 수 없습니다. 입력값: {amount}", nameof(amount));
            }
            
            currentGuard += amount;
            GameLogger.LogInfo($"[{GetCharacterName()}] 가드 +{amount} → 현재 가드: {currentGuard}", GameLogger.LogCategory.Character);

            // 가드 이벤트 발행은 자식 클래스에서 처리
            OnGuarded(amount);
        }

        /// <summary>
        /// 지정된 타입의 턴 효과가 적용되어 있는지 확인합니다.
        /// </summary>
        public bool HasEffect<T>() where T : IPerTurnEffect
        {
            for (int i = 0; i < perTurnEffects.Count; i++)
            {
                var effect = perTurnEffects[i];
                if (effect != null && effect is T) return true;
            }
            return false;
        }

        /// <summary>체력을 회복시킵니다</summary>
        /// <param name="amount">회복량</param>
        public virtual void Heal(int amount)
        {
            if (amount <= 0)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 잘못된 회복량: {amount}", GameLogger.LogCategory.Character);
                throw new ArgumentException($"회복량은 양수여야 합니다. 입력값: {amount}", nameof(amount));
            }

            currentHP = Mathf.Min(currentHP + amount, maxHP);
            OnHPChanged?.Invoke(currentHP, maxHP);

            GameLogger.LogInfo($"[{GetCharacterName()}] 회복: {amount}, 현재 체력: {currentHP}", GameLogger.LogCategory.Character);

            // 회복 이벤트 발행은 자식 클래스에서 처리
            OnHealed(amount);
        }

        /// <summary>피해를 받아 체력을 감소시킵니다</summary>
        /// <param name="amount">피해량</param>
        public virtual void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                GameLogger.LogError($"[{GetCharacterDataName()}] 잘못된 피해량: {amount}", GameLogger.LogCategory.Character);
                throw new ArgumentException($"피해량은 양수여야 합니다. 입력값: {amount}", nameof(amount));
            }

            // 가드 상태 확인
            if (isGuarded)
            {
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 가드로 데미지 차단: {amount}", GameLogger.LogCategory.Character);
                return; // 가드 상태면 데미지 무효화
            }

            currentHP = Mathf.Max(currentHP - amount, 0);
            OnHPChanged?.Invoke(currentHP, maxHP);

            GameLogger.LogInfo($"[{GetCharacterDataName()}] 피해: {amount}, 남은 체력: {currentHP}", GameLogger.LogCategory.Character);

            // 피해 이벤트 발행은 자식 클래스에서 처리
            OnDamaged(amount);

            if (IsDead())
                Die();
        }

        /// <summary>가드를 무시하고 피해를 받아 체력을 감소시킵니다</summary>
        /// <param name="amount">피해량</param>
        public virtual void TakeDamageIgnoreGuard(int amount)
        {
            if (amount <= 0)
            {
                GameLogger.LogError($"[{GetCharacterDataName()}] 잘못된 피해량: {amount}", GameLogger.LogCategory.Character);
                throw new ArgumentException($"피해량은 양수여야 합니다. 입력값: {amount}", nameof(amount));
            }

            // 가드 체크 없이 직접 데미지 적용
            currentHP = Mathf.Max(currentHP - amount, 0);
            OnHPChanged?.Invoke(currentHP, maxHP);

            GameLogger.LogInfo($"[{GetCharacterDataName()}] 가드 무시 피해: {amount}, 남은 체력: {currentHP}", GameLogger.LogCategory.Character);

            // 피해 이벤트 발행은 자식 클래스에서 처리
            OnDamaged(amount);

            if (IsDead())
                Die();
        }

        /// <summary>최대 체력을 설정하고 현재 체력을 동일하게 초기화합니다</summary>
        /// <param name="value">최대 체력 값</param>
        public virtual void SetMaxHP(int value)
        {
            if (value <= 0)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 잘못된 최대 체력: {value}", GameLogger.LogCategory.Character);
                throw new ArgumentException($"최대 체력은 양수여야 합니다. 입력값: {value}", nameof(value));
            }

            maxHP = value;
            currentHP = maxHP;
            OnHPChanged?.Invoke(currentHP, maxHP);
        }

        /// <summary>현재 체력을 직접 설정합니다 (최대 체력을 초과할 수 없음)</summary>
        /// <param name="value">설정할 체력 값</param>
        public virtual void SetCurrentHP(int value)
        {
            int newHP = Mathf.Clamp(value, 0, maxHP);

            if (currentHP != newHP)
            {
                currentHP = newHP;
                OnHPChanged?.Invoke(currentHP, maxHP);
                GameLogger.LogInfo($"[{GetCharacterName()}] 체력 설정: {currentHP}/{maxHP}", GameLogger.LogCategory.Character);
            }
        }

        #endregion

        #region UI 및 효과 관리

        // 레거시 CharacterUIController 제거됨

        /// <summary>턴 효과 등록</summary>
        /// <param name="effect">턴 효과 인스턴스</param>
        public virtual void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            if (effect == null)
            {
                GameLogger.LogError($"[{GetCharacterDataName()}] null 효과는 등록할 수 없습니다", GameLogger.LogCategory.Error);
                return;
            }

            // 동일 타입 효과가 이미 존재하면 중첩하지 않고 교체(지속시간/수치 초기화 목적)
            var existing = perTurnEffects.Find(e => e != null && e.GetType() == effect.GetType());
            if (existing != null)
            {
                perTurnEffects.Remove(existing);
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 상태이상 재적용: {effect.GetType().Name} → 기존 효과 제거 후 새 효과로 갱신(지속시간 초기화)", GameLogger.LogCategory.Character);
            }

            perTurnEffects.Add(effect);
            OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
        }

        /// <summary>상태이상 효과 등록 (가드 상태 확인)</summary>
        /// <param name="effect">상태이상 효과 인스턴스</param>
        /// <returns>등록 성공 여부</returns>
        public virtual bool RegisterStatusEffect(IPerTurnEffect effect)
        {
            // 가드 상태면 상태이상 효과 차단
            if (isGuarded)
            {
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 가드로 상태이상 효과 차단: {effect.GetType().Name}", GameLogger.LogCategory.Character);
                return false;
            }

            // 반격/가드 활성 중에는 디버프만 차단 (버프는 허용)
            bool isDebuff = effect is Game.SkillCardSystem.Interface.IStatusEffectDebuff;
            if (isDebuff && (isGuarded || HasEffect<Game.SkillCardSystem.Effect.CounterBuff>()))
            {
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 보호 상태로 디버프 차단: {effect.GetType().Name}", GameLogger.LogCategory.Character);
                return false;
            }

            // 가드 상태가 아니면 정상 등록
            RegisterPerTurnEffect(effect);
            return true;
        }

        /// <summary>등록된 턴 효과를 처리합니다 (만료된 효과 제거 포함)</summary>
        public virtual void ProcessTurnEffects()
        {
            // GameObject가 비활성화 상태면 턴 효과 처리 안 함
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            foreach (var effect in perTurnEffects.ToArray())
            {
                effect.OnTurnStart(this);

                if (effect.IsExpired)
                {
                    perTurnEffects.Remove(effect);
                }
            }
            // 매 턴 UI가 남은 턴 수를 갱신할 수 있도록 전체 리스트를 통지
            OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
        }

        #endregion

        #region 사망 및 제어

        /// <summary>캐릭터 사망 처리</summary>
        public virtual void Die()
        {
            GameLogger.LogInfo($"[{GetCharacterName()}] 사망 처리됨.", GameLogger.LogCategory.Character);
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
            // 자식 클래스에서 override하여 구현
        }

        /// <summary>
        /// 핸드 매니저를 주입합니다.
        /// </summary>
        /// <param name="handManager">주입할 핸드 매니저</param>
        public virtual void InjectHandManager(object handManager)
        {
            // 자식 클래스에서 override하여 구현
        }

        #endregion
    }
}
