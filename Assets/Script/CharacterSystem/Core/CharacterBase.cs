using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Game.VFXSystem.Manager;
using System;
using DG.Tweening;
using UnityEngine.UI;
using Zenject;

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
        public event Action<bool> OnInvincibleStateChanged;
        public event Action<IReadOnlyList<IPerTurnEffect>> OnBuffsChanged;
        #endregion
        #region 캐릭터 상태 필드

        /// <summary>캐릭터의 최대 체력</summary>
        protected int maxHP;

        /// <summary>캐릭터의 현재 체력</summary>
        protected int currentHP;

        /// <summary>현재 가드 상태 여부 (GuardBuff에 의해 관리됨)</summary>
        protected bool isGuarded = false;

        /// <summary>현재 무적 상태 여부 (InvincibilityBuff에 의해 관리됨)</summary>
        protected bool isInvincible = false;

        /// <summary>분신 추가 체력 (CloneBuff에 의해 관리됨)</summary>
        protected int cloneHP = 0;

        /// <summary>과거 체력 히스토리 (시공간 역행용, 최대 10턴 저장)</summary>
        protected List<int> hpHistory = new List<int>();

        /// <summary>턴마다 적용되는 효과 리스트</summary>
        protected List<IPerTurnEffect> perTurnEffects = new();

        public virtual Transform Transform => transform;

        #endregion

        #region 의존성 주입

        [Inject(Optional = true)]
        protected VFXManager _vfxManager;

        [Inject(Optional = true)]
        protected IAudioManager _audioManager;

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

        /// <summary>현재 무적 상태 여부 확인</summary>
        public virtual bool IsInvincible() => isInvincible;

        /// <summary>분신 추가 체력 반환</summary>
        public virtual int GetCloneHP() => cloneHP;

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

        /// <summary>무적 상태 설정</summary>
        /// <param name="value">true면 무적 상태 활성</param>
        public virtual void SetInvincible(bool value)
        {
            isInvincible = value;
            GameLogger.LogInfo($"[{GetCharacterName()}] 무적 상태: {isInvincible}", GameLogger.LogCategory.Character);
            OnInvincibleStateChanged?.Invoke(isInvincible);
        }

        /// <summary>분신 추가 체력 설정</summary>
        /// <param name="value">설정할 추가 체력 값</param>
        public virtual void SetCloneHP(int value)
        {
            cloneHP = Mathf.Max(0, value);
            GameLogger.LogInfo($"[{GetCharacterName()}] 분신 추가 체력: {cloneHP}", GameLogger.LogCategory.Character);
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

        /// <summary>
        /// 특정 타입의 효과를 반환합니다.
        /// </summary>
        /// <typeparam name="T">효과 타입</typeparam>
        /// <returns>효과 인스턴스 (없으면 null)</returns>
        public T GetEffect<T>() where T : class, IPerTurnEffect
        {
            for (int i = 0; i < perTurnEffects.Count; i++)
            {
                var effect = perTurnEffects[i];
                if (effect is T typedEffect)
                    return typedEffect;
            }
            return null;
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

            // 무적 상태 확인 (가드보다 우선)
            if (isInvincible)
            {
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 무적으로 데미지 완전 차단: {amount}", GameLogger.LogCategory.Character);
                return; // 무적 상태면 데미지 완전 무효화
            }

            // 가드 상태 확인
            if (isGuarded)
            {
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 가드로 데미지 차단: {amount}", GameLogger.LogCategory.Character);
                
                // GuardBuff에서 가드 차단 이펙트/사운드 가져와서 재생
                PlayGuardBlockEffects();
                
                return; // 가드 상태면 데미지 무효화
            }

            // 분신 추가 체력이 있으면 먼저 소모
            if (cloneHP > 0)
            {
                int remainingDamage = amount - cloneHP;
                cloneHP = Mathf.Max(0, cloneHP - amount);
                
                // CloneBuff의 CloneHP도 동기화
                foreach (var effect in perTurnEffects)
                {
                    if (effect is CloneBuff cloneBuff)
                    {
                        cloneBuff.SetCloneHP(cloneHP);
                        break;
                    }
                }
                
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 분신 추가 체력 소모: {amount} (남은 분신 체력: {cloneHP})", GameLogger.LogCategory.Character);
                
                // 분신 체력이 0이 되면 분신 버프 제거
                if (cloneHP <= 0)
                {
                    RemoveCloneBuff();
                }
                
                // 분신 체력으로 모든 데미지를 흡수했으면 종료
                if (remainingDamage <= 0)
                {
                    return;
                }
                
                // 남은 데미지를 일반 체력에 적용
                amount = remainingDamage;
            }

            currentHP = Mathf.Max(currentHP - amount, 0);
            OnHPChanged?.Invoke(currentHP, maxHP);

            GameLogger.LogInfo($"[{GetCharacterDataName()}] 피해: {amount}, 남은 체력: {currentHP}", GameLogger.LogCategory.Character);

            // 피해 이벤트 발행은 자식 클래스에서 처리
            OnDamaged(amount);

            if (IsDead())
                Die();
        }

        /// <summary>
        /// 분신 버프를 제거합니다.
        /// </summary>
        private void RemoveCloneBuff()
        {
            for (int i = perTurnEffects.Count - 1; i >= 0; i--)
            {
                if (perTurnEffects[i] is CloneBuff)
                {
                    perTurnEffects.RemoveAt(i);
                    cloneHP = 0;
                    OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
                    GameLogger.LogInfo($"[{GetCharacterDataName()}] 분신 버프 제거됨", GameLogger.LogCategory.Character);
                    break;
                }
            }
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

            int oldMaxHP = maxHP;
            maxHP = value;
            
            // 최대 체력이 증가한 경우, 현재 체력도 비례하여 증가 (단, 최대치 초과 금지)
            if (value > oldMaxHP && oldMaxHP > 0)
            {
                // 비율 유지: (현재 체력 / 이전 최대 체력) * 새 최대 체력
                float ratio = (float)currentHP / oldMaxHP;
                currentHP = Mathf.Min(Mathf.RoundToInt(ratio * maxHP), maxHP);
            }
            // 최대 체력이 감소한 경우, 현재 체력을 새 최대치로 제한
            else if (value < oldMaxHP)
            {
                currentHP = Mathf.Min(currentHP, maxHP);
            }
            // 최대 체력이 처음 설정되는 경우 (oldMaxHP == 0)
            else if (oldMaxHP == 0)
            {
                currentHP = maxHP;
            }
            
            OnHPChanged?.Invoke(currentHP, maxHP);
            GameLogger.LogInfo($"[{GetCharacterName()}] 최대 체력 변경: {oldMaxHP} → {maxHP}, 현재 체력: {currentHP}", GameLogger.LogCategory.Character);
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

        #region 효과 관리

        /// <summary>턴 효과 등록</summary>
        /// <param name="effect">턴 효과 인스턴스</param>
        public virtual void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            if (effect == null)
            {
                GameLogger.LogError($"[{GetCharacterDataName()}] null 효과는 등록할 수 없습니다", GameLogger.LogCategory.Error);
                return;
            }

            // 공격력 증가 버프는 출처별로 동작 방식이 다르므로 별도 처리
            if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackPowerBuff)
            {
                // 1) 아이템 유래 버프: SourceItemName이 있는 경우
                //    → 동시에 하나만 유지, 새로운 아이템 버프로 기존 아이템 버프를 교체
                // 2) 스킬 유래 버프: SourceItemName이 비어 있고 SourceEffectName만 있는 경우
                //    → 여러 스킬에서 온 버프가 동시에 존재할 수 있으므로 제거 없이 누적
                bool isItemBuff = !string.IsNullOrEmpty(attackPowerBuff.SourceItemName);

                if (isItemBuff)
                {
                    // 기존 아이템 공격력 버프만 제거 (스킬 유래 버프는 유지)
                    for (int i = perTurnEffects.Count - 1; i >= 0; i--)
                    {
                        if (perTurnEffects[i] is Game.ItemSystem.Effect.AttackPowerBuffEffect existingBuff &&
                            !string.IsNullOrEmpty(existingBuff.SourceItemName))
                        {
                            perTurnEffects.RemoveAt(i);
                        }
                    }

                    GameLogger.LogInfo(
                        $"[{GetCharacterDataName()}] 아이템 공격력 버프 재적용: {attackPowerBuff.SourceItemName ?? "Unknown"}",
                        GameLogger.LogCategory.Character);
                }
            }
            else
            {
                // 기본 규칙: 동일 타입 효과는 하나만 유지(재적용 시 교체)
                var existing = perTurnEffects.Find(e => e != null && e.GetType() == effect.GetType());
                if (existing != null)
                {
                    perTurnEffects.Remove(existing);
                    GameLogger.LogInfo(
                        $"[{GetCharacterDataName()}] 상태이상 재적용: {effect.GetType().Name} → 기존 효과 제거 후 새 효과로 갱신(지속시간 초기화)",
                        GameLogger.LogCategory.Character);
                }
            }

            perTurnEffects.Add(effect);
            
            // 디버깅: 분신 버프 등록 확인
            if (effect is CloneBuff cloneBuff)
            {
                GameLogger.LogInfo($"[{GetCharacterName()}] 분신 버프 등록 완료 (CloneHP: {cloneBuff.CloneHP}, Icon: {(cloneBuff.Icon != null ? cloneBuff.Icon.name : "null")})", GameLogger.LogCategory.Character);
            }
            
            OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
            GameLogger.LogInfo($"[{GetCharacterName()}] OnBuffsChanged 이벤트 발생 (버프 수: {perTurnEffects.Count})", GameLogger.LogCategory.Character);
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

            // 체력 히스토리 저장 (시공간 역행용)
            SaveHPToHistory();

            // 역순 순회로 GC 없이 안전하게 제거
            for (int i = perTurnEffects.Count - 1; i >= 0; i--)
            {
                var effect = perTurnEffects[i];
                if (effect == null) continue;
                
                effect.OnTurnStart(this);

                if (effect.IsExpired)
                {
                    // CloneBuff는 CloneHP가 0이 되면 만료되므로 별도 처리
                    if (effect is CloneBuff cloneBuff && cloneBuff.CloneHP > 0)
                    {
                        // CloneHP가 남아있으면 만료되지 않은 것으로 처리
                        continue;
                    }
                    
                    perTurnEffects.RemoveAt(i);
                    GameLogger.LogInfo($"[{GetCharacterName()}] 효과 만료로 제거: {effect.GetType().Name}", GameLogger.LogCategory.Character);
                }
            }
            // 매 턴 UI가 남은 턴 수를 갱신할 수 있도록 전체 리스트를 통지
            OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
        }

        /// <summary>
        /// 현재 체력을 히스토리에 저장합니다 (시공간 역행용).
        /// 최대 10턴까지 저장하며, 초과 시 가장 오래된 기록을 제거합니다.
        /// </summary>
        protected virtual void SaveHPToHistory()
        {
            hpHistory.Add(currentHP);
            
            // 최대 10턴까지만 저장 (메모리 관리)
            const int MAX_HISTORY = 10;
            if (hpHistory.Count > MAX_HISTORY)
            {
                hpHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// 지정된 턴 수 전의 체력을 반환합니다.
        /// </summary>
        /// <param name="turnsAgo">몇 턴 전인지 (1 = 1턴 전, 3 = 3턴 전)</param>
        /// <returns>해당 턴의 체력, 히스토리가 부족하면 현재 체력 반환</returns>
        public virtual int GetHPFromHistory(int turnsAgo)
        {
            if (turnsAgo <= 0 || turnsAgo > hpHistory.Count)
            {
                GameLogger.LogWarning($"[{GetCharacterDataName()}] 히스토리 부족: 요청 {turnsAgo}턴 전, 저장된 {hpHistory.Count}턴", GameLogger.LogCategory.Character);
                return currentHP; // 히스토리가 부족하면 현재 체력 반환
            }

            // 가장 최근 기록이 리스트의 마지막이므로 역순으로 접근
            int index = hpHistory.Count - turnsAgo;
            return hpHistory[index];
        }

        /// <summary>
        /// 지정된 턴 수 전의 체력으로 복원합니다.
        /// </summary>
        /// <param name="turnsAgo">몇 턴 전인지 (1 = 1턴 전, 3 = 3턴 전)</param>
        public virtual void RestoreHPFromHistory(int turnsAgo)
        {
            int targetHP = GetHPFromHistory(turnsAgo);
            int previousHP = currentHP;
            
            currentHP = Mathf.Clamp(targetHP, 0, maxHP);
            OnHPChanged?.Invoke(currentHP, maxHP);
            
            GameLogger.LogInfo($"[{GetCharacterDataName()}] 시공간 역행: {previousHP} → {currentHP} ({turnsAgo}턴 전 체력으로 복원)", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 등록된 턴 효과를 처리하고 출혈 이펙트 완료를 기다립니다 (코루틴)
        /// </summary>
        public virtual System.Collections.IEnumerator ProcessTurnEffectsCoroutine()
        {
            // GameObject가 비활성화 상태면 턴 효과 처리 안 함
            if (!gameObject.activeInHierarchy)
            {
                yield break;
            }

            // 출혈 효과 개수 카운트 (처리 전에 카운트)
            int bleedEffectCount = 0;
            foreach (var effect in perTurnEffects)
            {
                if (effect is Game.SkillCardSystem.Effect.BleedEffect)
                {
                    bleedEffectCount++;
                }
            }

            // 출혈 효과가 없으면 즉시 처리
            if (bleedEffectCount == 0)
            {
                ProcessTurnEffects();
                yield break;
            }

            // 출혈 효과가 있으면 완료 이벤트 대기
            int completedBleedEffects = 0;
            System.Action onBleedComplete = () => completedBleedEffects++;

            // 이벤트 구독 (ProcessTurnEffects 호출 전에 구독해야 함)
            Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete += onBleedComplete;

            // 턴 효과 처리 (출혈 이펙트 재생 시작)
            ProcessTurnEffects();

            // 모든 출혈 이펙트 완료 대기 (타임아웃: 최대 1.5초)
            float timeout = 1.5f;
            float elapsed = 0f;
            
            while (completedBleedEffects < bleedEffectCount && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 이벤트 구독 해제
            Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete -= onBleedComplete;

            if (completedBleedEffects >= bleedEffectCount)
            {
                GameLogger.LogInfo($"[{GetCharacterDataName()}] 모든 출혈 이펙트 완료 ({completedBleedEffects}/{bleedEffectCount})", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning($"[{GetCharacterDataName()}] 출혈 이펙트 완료 타임아웃 ({completedBleedEffects}/{bleedEffectCount}, {elapsed:F2}초 경과)", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 가드가 데미지를 차단할 때 이펙트와 사운드를 재생합니다.
        /// GuardBuff에서 EffectConfiguration으로부터 가져온 이펙트/사운드를 사용합니다.
        /// </summary>
        private void PlayGuardBlockEffects()
        {
            // GuardBuff 찾기
            GuardBuff guardBuff = null;
            foreach (var effect in perTurnEffects)
            {
                if (effect is GuardBuff gb)
                {
                    guardBuff = gb;
                    break;
                }
            }

            // 가드 차단 이펙트 재생
            if (guardBuff != null && guardBuff.BlockEffectPrefab != null)
            {
                if (_vfxManager != null)
                {
                    var effectInstance = _vfxManager.PlayEffectAtCharacterCenter(guardBuff.BlockEffectPrefab, transform);
                    if (effectInstance != null)
                    {
                        GameLogger.LogInfo($"[{GetCharacterDataName()}] 가드 차단 이펙트 재생: {guardBuff.BlockEffectPrefab.name}", GameLogger.LogCategory.Character);
                    }
                }
            }

            // 가드 차단 사운드 재생
            if (guardBuff != null && guardBuff.BlockSfxClip != null)
            {
                if (_audioManager != null)
                {
                    _audioManager.PlaySFXWithPool(guardBuff.BlockSfxClip, 0.9f);
                    GameLogger.LogInfo($"[{GetCharacterDataName()}] 가드 차단 사운드 재생: {guardBuff.BlockSfxClip.name}", GameLogger.LogCategory.Character);
                }
                else
                {
                    GameLogger.LogWarning($"[{GetCharacterDataName()}] AudioManager를 찾을 수 없습니다. 가드 차단 사운드 재생을 건너뜁니다.", GameLogger.LogCategory.Character);
                }
            }
        }

        /// <summary>
        /// 버프/이펙트 변경을 UI에 알립니다 (수동 트리거)
        /// GameObject 재활성화 후 버프 상태를 UI에 반영할 때 사용합니다.
        /// </summary>
        public virtual void NotifyBuffsChanged()
        {
            OnBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
        }

        #endregion

        #region Unity 생명주기

        /// <summary>
        /// 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 모든 이벤트 구독 해제
            OnHPChanged = null;
            OnGuardStateChanged = null;
            OnBuffsChanged = null;
            // Idle 루프 정리
            StopIdleVisualLoop();
            // 모든 DOTween 애니메이션 정리 (피격 효과 등)
            transform.DOKill();
            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr != null) sr.DOKill();
            }
            var images = GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (var img in images)
            {
                if (img != null) img.DOKill();
            }
        }

        #endregion

        #region 피격 시각 효과

        /// <summary>
        /// 피격 시 시각 효과를 재생합니다 (색상 플래시, 스케일 변화)
        /// </summary>
        /// <param name="damageAmount">피해량 (시각 효과 강도 조절용)</param>
        protected virtual void PlayHitVisualEffects(int damageAmount)
        {
            if (damageAmount <= 0) return;

            // 색상 플래시 효과
            PlayHitColorFlash();

            // 스케일 변화 효과
            PlayHitScaleEffect();
        }

        /// <summary>
        /// 피격 시각 효과를 적용할 비주얼 루트 Transform을 반환합니다.
        /// 기본은 현재 Transform이며, 자식 클래스에서 Portrait 등으로 한정하도록 override 합니다.
        /// </summary>
        protected virtual Transform GetHitVisualRoot()
        {
            return transform;
        }

        /// <summary>
        /// 피격 시 색상 플래시 효과를 재생합니다
        /// </summary>
        private void PlayHitColorFlash()
        {
            // 비주얼 루트 기준으로 한정
            Transform visualRoot = GetHitVisualRoot() != null ? GetHitVisualRoot() : transform;

            // Image 컴포넌트 찾기 (Portrait 등) - 데미지 텍스트/HP/Buff UI 제외
            var images = visualRoot.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                if (image == null) continue;

                // DamageTextUI 하위는 제외
                if (image.GetComponentInParent<Game.CombatSystem.UI.DamageTextUI>() != null)
                    continue;
                // HPTextAnchor/HPTectAnchor 하위는 제외
                var t = image.transform;
                bool underHpTextAnchor = false;
                while (t != null && t != visualRoot)
                {
                    if (t.name == "HPTextAnchor" || t.name == "HPTectAnchor")
                    {
                        underHpTextAnchor = true;
                        break;
                    }
                    t = t.parent;
                }
                if (underHpTextAnchor) continue;

                // 기존 색상 트윈이 남아 있으면 완료 처리 후 현재 색상을 기준으로 사용
                image.DOKill(true);
                Color originalColor = image.color;

                // 빨간색으로 플래시
                image.DOColor(new Color(1f, 0.3f, 0.3f, originalColor.a), 0.1f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // 원래 색상으로 복원
                        image.DOColor(originalColor, 0.15f)
                            .SetEase(Ease.InQuad);
                    });
            }

            // SpriteRenderer 컴포넌트 찾기 (캐릭터 스프라이트 등)
            var spriteRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer == null) continue;

                // 기존 색상 트윈이 남아 있으면 완료 처리 후 현재 색상을 기준으로 사용
                spriteRenderer.DOKill(true);
                Color originalColor = spriteRenderer.color;

                // 빨간색으로 플래시
                spriteRenderer.DOColor(new Color(1f, 0.3f, 0.3f, originalColor.a), 0.1f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // 원래 색상으로 복원
                        spriteRenderer.DOColor(originalColor, 0.15f)
                            .SetEase(Ease.InQuad);
                    });
            }
        }

        /// <summary>
        /// 피격 시 스케일 변화 효과를 재생합니다
        /// </summary>
        private void PlayHitScaleEffect()
        {
            if (transform == null) return;

            // 원래 스케일 저장
            Vector3 originalScale = transform.localScale;

            // 약간 축소 후 복원 (피격 느낌)
            transform.DOScale(originalScale * 0.95f, 0.08f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    transform.DOScale(originalScale, 0.12f)
                        .SetEase(Ease.OutBack);
                });
        }

        // ----- Idle 시각 효과 (부드러운 호흡 스케일) -----
        private Tween _idleTween;
        private Vector3 _idleBaseScale;

        /// <summary>
        /// 캐릭터가 대기 중일 때 부드러운 호흡 느낌의 스케일 루프를 시작합니다.
        /// </summary>
        /// <param name="amplitude">스케일 변화 폭 (기본 0.02 = 2%)</param>
        /// <param name="periodSeconds">한 왕복 주기 시간 (기본 1.5초)</param>
        public void StartIdleVisualLoop(float amplitude = 0.02f, float periodSeconds = 1.5f)
        {
            Transform visualRoot = GetHitVisualRoot() != null ? GetHitVisualRoot() : transform;
            if (visualRoot == null) return;

            // 기존 트윈 정리
            _idleTween?.Kill();

            _idleBaseScale = visualRoot.localScale;
            float amp = Mathf.Clamp(amplitude, 0.005f, 0.05f);
            // 좌우(X) 스케일은 유지하고, 수직(Y)만 호흡하도록 변경
            Vector3 targetScale = new Vector3(_idleBaseScale.x, _idleBaseScale.y * (1f + amp), _idleBaseScale.z);

            _idleTween = visualRoot.DOScale(targetScale, Mathf.Max(0.2f, periodSeconds * 0.5f))
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false);
        }

        /// <summary>
        /// 대기 시각 효과 루프를 중지하고 원래 스케일로 복원합니다.
        /// </summary>
        public void StopIdleVisualLoop()
        {
            if (_idleTween != null)
            {
                var target = _idleTween.target as Transform;
                _idleTween.Kill();
                _idleTween = null;

                if (target != null)
                {
                    // 원래 스케일로 복원
                    target.localScale = _idleBaseScale == Vector3.zero ? Vector3.one : _idleBaseScale;
                }
            }
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

        protected virtual void OnDisable()
        {
            StopIdleVisualLoop();
            // 모든 DOTween 애니메이션 정리 (피격 효과 등)
            transform.DOKill();
            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr != null) sr.DOKill();
            }
            var images = GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (var img in images)
            {
                if (img != null) img.DOKill();
            }
        }

        #region Portrait 초기화 공통 로직

        /// <summary>
        /// Portrait 프리팹을 초기화하는 공통 로직
        /// </summary>
        /// <param name="portraitPrefab">Portrait 프리팹</param>
        /// <param name="portraitParent">Portrait 부모 Transform (null이면 자동 찾기)</param>
        /// <param name="portraitImage">Portrait Image 컴포넌트 참조 (출력)</param>
        /// <param name="hpTextAnchor">HP Text Anchor Transform 참조 (출력)</param>
        /// <param name="characterTransform">캐릭터 Transform</param>
        /// <param name="characterName">캐릭터 이름 (로깅용)</param>
        protected void InitializePortraitCommon(
            GameObject portraitPrefab,
            Transform portraitParent,
            ref Image portraitImage,
            ref Transform hpTextAnchor,
            Transform characterTransform,
            string characterName)
        {
            if (portraitPrefab == null)
            {
                InitializePortraitFromExisting(ref portraitImage, characterTransform, characterName);
                return;
            }

            Transform parent = GetPortraitParent(portraitParent, characterTransform);
            GameObject portraitInstance = Instantiate(portraitPrefab, parent);
            portraitInstance.name = "Portrait";

            FindPortraitImage(portraitInstance, ref portraitImage, characterName);
            FindHPTextAnchor(portraitInstance, ref hpTextAnchor);
        }

        /// <summary>
        /// Portrait 부모 Transform을 찾습니다
        /// </summary>
        private Transform GetPortraitParent(Transform portraitParent, Transform characterTransform)
        {
            if (portraitParent != null)
                return portraitParent;

            var existingPortrait = characterTransform.Find("Portrait");
            if (existingPortrait != null)
            {
                existingPortrait.gameObject.SetActive(false);
                return existingPortrait.parent;
            }

            return characterTransform;
        }

        /// <summary>
        /// Portrait Image 컴포넌트를 찾습니다
        /// </summary>
        private void FindPortraitImage(GameObject portraitInstance, ref Image portraitImage, string characterName)
        {
            if (portraitImage != null)
                return;

            portraitImage = portraitInstance.GetComponentInChildren<Image>(true);
            if (portraitImage == null)
            {
                GameLogger.LogWarning($"[{characterName}] Portrait 프리팹에서 Image 컴포넌트를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// HP Text Anchor를 찾습니다
        /// </summary>
        private void FindHPTextAnchor(GameObject portraitInstance, ref Transform hpTextAnchor)
        {
            if (hpTextAnchor != null)
                return;

            var hpAnchor = portraitInstance.transform.Find("HPTectAnchor");
            if (hpAnchor == null)
            {
                hpAnchor = portraitInstance.transform.Find("HPTextAnchor");
            }

            if (hpAnchor != null)
            {
                hpTextAnchor = hpAnchor;
            }
        }

        /// <summary>
        /// 기존 Portrait GameObject에서 Image를 찾습니다
        /// </summary>
        private void InitializePortraitFromExisting(ref Image portraitImage, Transform characterTransform, string characterName)
        {
            if (portraitImage != null)
                return;

            var existingPortrait = characterTransform.Find("Portrait");
            if (existingPortrait != null)
            {
                portraitImage = existingPortrait.GetComponent<Image>();
            }

            if (portraitImage == null)
            {
                GameLogger.LogWarning($"[{characterName}] Portrait Image를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
            }
        }

        #endregion
    }
}
