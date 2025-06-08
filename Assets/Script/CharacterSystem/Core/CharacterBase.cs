using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 캐릭터의 기본 베이스 클래스입니다.
    /// 체력, 가드, 턴 효과, UI 연동 등의 기능을 제공합니다.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour, ICharacter, IHitEffectPlayable
    {
        [Header("피격 이펙트")]
        [SerializeField] protected GameObject hitEffectPrefab;

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

        /// <summary>UI와 연동되는 캐릭터 카드 컨트롤러</summary>
        protected CharacterUIController characterCardUI;

        #endregion

        #region 상태 정보 접근자

        /// <summary>캐릭터 이름 반환</summary>
        public virtual string GetCharacterName() => gameObject.name;

        /// <summary>현재 체력 반환</summary>
        public virtual int GetHP() => currentHP;

        /// <summary>현재 체력 반환 (명시적 호출용)</summary>
        public int GetCurrentHP() => currentHP;

        /// <summary>최대 체력 반환</summary>
        public virtual int GetMaxHP() => maxHP;

        /// <summary>캐릭터 초상화 이미지 반환 (기본은 null)</summary>
        public virtual Sprite GetPortrait() => null;

        /// <summary>현재 가드 상태 여부 확인</summary>
        public virtual bool IsGuarded() => isGuarded;

        /// <summary>캐릭터가 사망했는지 확인</summary>
        public virtual bool IsDead() => currentHP <= 0;

        /// <summary>캐릭터가 살아있는지 확인</summary>
        public virtual bool IsAlive() => currentHP > 0;

        #endregion

        #region 상태 설정 메서드

        /// <summary>가드 상태 설정</summary>
        /// <param name="value">true면 가드 상태 활성</param>
        public virtual void SetGuarded(bool value)
        {
            isGuarded = value;
            Debug.Log($"[{GetCharacterName()}] 가드 상태: {isGuarded}");
        }

        /// <summary>가드 수치 증가</summary>
        /// <param name="amount">증가량</param>
        public virtual void GainGuard(int amount)
        {
            currentGuard += amount;
            Debug.Log($"[{GetCharacterName()}] 가드 +{amount} → 현재 가드: {currentGuard}");
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
            characterCardUI?.SetHP(currentHP, maxHP);

            Debug.Log($"[{GetCharacterName()}] 회복: {amount}, 현재 체력: {currentHP}");
        }

        /// <summary>피해를 받아 체력을 감소시킵니다</summary>
        /// <param name="amount">피해량</param>
        public virtual void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[{GetCharacterName()}] 잘못된 피해량 무시됨: {amount}");
                return;
            }

            currentHP = Mathf.Max(currentHP - amount, 0);
            characterCardUI?.SetHP(currentHP, maxHP);

            Debug.Log($"[{GetCharacterName()}] 피해: {amount}, 남은 체력: {currentHP}");

            if (IsDead())
                Die();
        }

        /// <summary>최대 체력을 설정하고 현재 체력을 동일하게 초기화합니다</summary>
        /// <param name="value">최대 체력 값</param>
        public virtual void SetMaxHP(int value)
        {
            maxHP = value;
            currentHP = maxHP;
            characterCardUI?.SetHP(currentHP, maxHP);
        }

        #endregion

        #region UI 및 효과 관리

        /// <summary>캐릭터 UI 연결 및 초기화</summary>
        /// <param name="ui">UI 컨트롤러</param>
        public virtual void SetCardUI(CharacterUIController ui)
        {
            characterCardUI = ui;
            characterCardUI?.Initialize(this);
        }

        /// <summary>턴 효과 등록</summary>
        /// <param name="effect">턴 효과 인스턴스</param>
        public virtual void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            if (!perTurnEffects.Contains(effect))
                perTurnEffects.Add(effect);
        }

        /// <summary>등록된 턴 효과를 처리합니다 (만료된 효과 제거 포함)</summary>
        public virtual void ProcessTurnEffects()
        {
            foreach (var effect in perTurnEffects.ToArray())
            {
                effect.OnTurnStart(this);

                if (effect.IsExpired)
                    perTurnEffects.Remove(effect);
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

        #endregion
        public virtual void PlayHitEffect()
        {
            Debug.Log($"[PlayHitEffect] {GetCharacterName()} - 이펙트 실행 시도");

            if (hitEffectPrefab == null)
            {
                Debug.LogWarning("[PlayHitEffect] hitEffectPrefab이 연결되지 않았습니다.");
                return;
            }

            // 머리 위 위치 기준으로 스폰 (기본: 1.0f 위, 0.5f 앞으로)
            Vector3 offset = new Vector3(0, 1f, -0.5f);
            Vector3 spawnPosition = transform.position + offset;

            GameObject effectInstance = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
            if (effectInstance == null)
            {
                Debug.LogError("[PlayHitEffect] 이펙트 인스턴스 생성 실패");
                return;
            }

            Debug.Log($"[PlayHitEffect] 이펙트 생성 위치: {spawnPosition}");

            // Particle System 존재 여부 확인 및 재생
            ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                if (!ps.isPlaying)
                {
                    ps.Play();
                    Debug.Log("[PlayHitEffect] ParticleSystem 수동 재생");
                }
                else
                {
                    Debug.Log("[PlayHitEffect] ParticleSystem 자동 재생됨");
                }
            }
            else
            {
                Debug.LogWarning("[PlayHitEffect] ParticleSystem 없음");
            }

            // Particle Renderer 설정 (없을 수도 있으니 조건부 처리)
            ParticleSystemRenderer psRenderer = effectInstance.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sortingLayerName = "Effects"; // 사전에 프로젝트에 정의된 레이어일 것
                psRenderer.sortingOrder = 100;

                Debug.Log($"[PlayHitEffect] Sorting Layer: {psRenderer.sortingLayerName}, Order: {psRenderer.sortingOrder}");
            }
            else
            {
                Debug.LogWarning("[PlayHitEffect] ParticleSystemRenderer 없음");
            }

            // 이펙트 일정 시간 후 제거
            Destroy(effectInstance, 2f);
        }

    }
}
