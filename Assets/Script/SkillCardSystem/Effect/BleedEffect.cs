using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.VFXSystem.Manager;
using Game.VFXSystem;
using Game.CoreSystem.Utility;
using UnityEngine.UI;
using Game.CombatSystem;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 일정 턴 동안 대상에게 매 턴 피해를 입히는 출혈 효과입니다.
    /// </summary>
    public class BleedEffect : IStatusEffectDebuff
    {
        private readonly int amount;
        private int remainingTurns;
        private readonly Sprite icon;
        private readonly GameObject perTurnEffectPrefab;
        private readonly AudioClip perTurnSfxClip;
        private readonly VFXManager vfxManager;
        private readonly string sourceEffectName;

        /// <summary>
        /// 출혈 효과 생성자
        /// </summary>
        /// <param name="amount">매 턴 입힐 피해량</param>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">출혈 효과 아이콘</param>
        /// <param name="perTurnEffectPrefab">출혈 피해 발생 시 매 턴 재생할 이펙트 프리팹</param>
        /// <param name="vfxManager">VFX 매니저 (선택적)</param>
        /// <param name="sourceEffectName">원본 효과 SO 이름 (툴팁 표시용)</param>
        public BleedEffect(int amount, int duration, Sprite icon = null, GameObject perTurnEffectPrefab = null, VFXManager vfxManager = null, string sourceEffectName = null)
        {
            this.amount = amount;
            this.remainingTurns = duration;
            this.icon = icon;
            this.perTurnEffectPrefab = perTurnEffectPrefab;
            this.perTurnSfxClip = null;
            this.vfxManager = vfxManager ?? Game.VFXSystem.Manager.VFXManager.Instance;
            this.sourceEffectName = sourceEffectName;
        }

        /// <summary>
        /// 출혈 효과 생성자 (턴당 사운드 포함)
        /// </summary>
        /// <param name="amount">매 턴 입힐 피해량</param>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">출혈 효과 아이콘</param>
        /// <param name="perTurnEffectPrefab">출혈 피해 발생 시 매 턴 재생할 이펙트 프리팹</param>
        /// <param name="perTurnSfxClip">출혈 피해 발생 시 매 턴 재생할 사운드</param>
        /// <param name="vfxManager">VFX 매니저 (선택적)</param>
        /// <param name="sourceEffectName">원본 효과 SO 이름 (툴팁 표시용)</param>
        public BleedEffect(int amount, int duration, Sprite icon, GameObject perTurnEffectPrefab, AudioClip perTurnSfxClip, VFXManager vfxManager, string sourceEffectName = null)
        {
            this.amount = amount;
            this.remainingTurns = duration;
            this.icon = icon;
            this.perTurnEffectPrefab = perTurnEffectPrefab;
            this.perTurnSfxClip = perTurnSfxClip;
            this.vfxManager = vfxManager ?? Game.VFXSystem.Manager.VFXManager.Instance;
            this.sourceEffectName = sourceEffectName;
        }

        /// <summary>
        /// 출혈 효과가 만료되었는지 여부를 반환합니다.
        /// </summary>
        public bool IsExpired => remainingTurns <= 0;
        public int RemainingTurns => remainingTurns;
        public Sprite Icon => icon;

        /// <summary>
        /// 턴 시작 시 대상에게 피해를 입히고 남은 턴을 감소시킵니다.
        /// </summary>
        /// <param name="target">출혈 피해를 받을 대상 캐릭터</param>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null)
            {
                GameLogger.LogWarning("[BleedEffect] 대상이 null입니다. 출혈 효과 무시됨.", GameLogger.LogCategory.SkillCard);
                return;
            }
            
            // 가드에 영향을 받지 않는 지속 피해로 처리
            target.TakeDamageIgnoreGuard(amount);
            remainingTurns--;

            GameLogger.LogInfo($"[BleedEffect] {target.GetCharacterName()} 출혈 피해: {amount} (남은 턴: {remainingTurns})", GameLogger.LogCategory.SkillCard);
            
            // 출혈 피해 발생 시 이펙트 및 사운드 재생
            TrySpawnPerTurnEffect(target);
            PlayPerTurnSound();
        }

        /// <summary>
        /// 매 턴 출혈 피해 발생 시 이펙트를 재생합니다.
        /// </summary>
        private void TrySpawnPerTurnEffect(ICharacter target)
        {
            if (perTurnEffectPrefab == null)
            {
                // 이펙트가 없어도 완료 이벤트 발행 (피해는 이미 발생했으므로)
                CombatEvents.RaiseBleedTurnStartEffectComplete();
                return;
            }

            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                GameLogger.LogWarning("[BleedEffect] 대상 Transform이 null입니다. 출혈 턴별 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                CombatEvents.RaiseBleedTurnStartEffectComplete();
                return;
            }

            // VFXManager를 통한 이펙트 생성 (RectTransform 중심, UI 기반, 정확한 위치 지정)
            var finalVfxManager = vfxManager ?? Game.VFXSystem.Manager.VFXManager.Instance;
            if (finalVfxManager != null)
            {
                var instance = finalVfxManager.PlayEffectAtCharacterRectTransformCenter(perTurnEffectPrefab, targetTransform);
                if (instance != null)
                {
                    SetEffectLayer(instance);
                    GameLogger.LogInfo($"[BleedEffect] 턴별 출혈 VFX 재생 (RectTransform 중심): {instance.name}", GameLogger.LogCategory.SkillCard);
                    
                    // 이펙트 지속 시간 계산 후 완료 이벤트 발행
                    float duration = GetEffectDuration(instance);
                    finalVfxManager.StartCoroutine(DelayedBleedEffectComplete(duration));
                }
                else
                {
                    CombatEvents.RaiseBleedTurnStartEffectComplete();
                }
            }
            else
            {
                GameLogger.LogError("[BleedEffect] VFXManager를 찾을 수 없습니다. 턴별 출혈 VFX를 생성할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                CombatEvents.RaiseBleedTurnStartEffectComplete();
            }
        }

        /// <summary>
        /// 이펙트 지속 시간 후 출혈 이펙트 완료 이벤트를 발행합니다.
        /// </summary>
        private System.Collections.IEnumerator DelayedBleedEffectComplete(float duration)
        {
            // 최대 0.8초로 제한 (너무 오래 기다리지 않도록)
            float actualDuration = Mathf.Min(duration, 0.8f);
            yield return new WaitForSeconds(actualDuration);
            CombatEvents.RaiseBleedTurnStartEffectComplete();
        }

        /// <summary>
        /// 이펙트의 지속 시간을 계산합니다.
        /// EffectDuration 컴포넌트가 있으면 우선적으로 사용하고,
        /// 없으면 ParticleSystem과 Animator를 기반으로 자동 계산합니다.
        /// </summary>
        private float GetEffectDuration(GameObject effect)
        {
            // EffectDuration 컴포넌트가 있으면 우선 사용
            var effectDuration = effect.GetComponent<EffectDuration>();
            if (effectDuration != null && effectDuration.HasCustomDuration)
            {
                float customDuration = effectDuration.Duration;
                GameLogger.LogInfo($"[BleedEffect] 커스텀 지속 시간 사용: {customDuration}초 (이펙트: {effect.name})", GameLogger.LogCategory.SkillCard);
                return Mathf.Min(customDuration, 0.8f); // 최대 0.8초로 제한
            }

            // 자동 계산 (기존 로직)
            float maxDuration = 0f;

            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                float duration = main.duration + main.startLifetime.constantMax;
                
                // 파티클이 루프인 경우 최대 지속 시간 제한
                if (main.loop)
                {
                    duration = Mathf.Min(duration, 1.0f); // 출혈 이펙트는 최대 1초
                }
                
                if (duration > maxDuration)
                {
                    maxDuration = duration;
                }
            }

            // 애니메이션 클립이 있으면 확인
            var animator = effect.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                foreach (var clip in clips)
                {
                    if (clip.length > maxDuration)
                    {
                        maxDuration = clip.length;
                    }
                }
            }

            // 최대 지속 시간 제한 (출혈 이펙트는 빠르게 처리)
            return Mathf.Min(maxDuration > 0 ? maxDuration : 0.6f, 0.8f);
        }

        /// <summary>
        /// 이펙트 렌더 순서를 UI 위로 설정합니다.
        /// </summary>
        private static void SetEffectLayer(GameObject effectInstance)
        {
            if (effectInstance == null) return;
            
            var renderers = effectInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Effects";
                r.sortingOrder = 10;
            }
            
            var pss = effectInstance.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var pr in pss)
            {
                pr.sortingLayerName = "Effects";
                pr.sortingOrder = 10;
            }
        }

        /// <summary>
        /// 캐릭터 트랜스폼 하위의 포트레잇 Image 중심(시각적 중심)을 월드 좌표로 계산합니다.
        /// </summary>
        private static Vector3 GetPortraitCenterWorldPosition(Transform root)
        {
            if (root == null) return Vector3.zero;

            // Portrait Image 우선
            var portraitImage = FindPortraitImage(root);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                return GetRectTransformCenterWorld(portraitImage.rectTransform);
            }

            // RectTransform 폴백
            var anyRect = root.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                return GetRectTransformCenterWorld(anyRect);
            }

            // SpriteRenderer 폴백
            var sprite = root.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null)
            {
                return sprite.bounds.center;
            }

            // 최종 폴백
            return root.position;
        }

        private static Image FindPortraitImage(Transform root)
        {
            var images = root.GetComponentsInChildren<Image>(true);
            if (images == null || images.Length == 0) return null;

            // 정확한 이름 우선
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject != null && images[i].gameObject.name == "Portrait")
                {
                    return images[i];
                }
            }
            
            // 폴백: 첫 번째 Image
            return images[0];
        }

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }

        /// <summary>
        /// 매 턴 출혈 피해 발생 시 사운드를 재생합니다.
        /// </summary>
        private void PlayPerTurnSound()
        {
            if (perTurnSfxClip == null)
            {
                return;
            }

            var audioManager = Game.CoreSystem.Audio.AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlaySFXWithPool(perTurnSfxClip, 0.9f);
                GameLogger.LogInfo($"[BleedEffect] 출혈 턴당 사운드 재생: {perTurnSfxClip.name}", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning("[BleedEffect] AudioManager를 찾을 수 없습니다. 출혈 턴당 사운드 재생을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
            }
        }
    }
}
