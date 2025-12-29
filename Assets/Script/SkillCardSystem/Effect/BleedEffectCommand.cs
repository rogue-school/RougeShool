using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.VFXSystem.Manager;
using UnityEngine.UI;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 출혈 효과를 캐릭터에 적용하는 커맨드 클래스입니다.
    /// 지정된 수치와 지속 시간으로 <see cref="BleedEffect"/>를 생성하여 타겟에 등록합니다.
    /// </summary>
    public class BleedEffectCommand : ICardEffectCommand
    {
        private readonly int amount;
        private readonly int duration;
        private readonly Sprite icon;
        private readonly GameObject visualEffectPrefab;
        private readonly GameObject perTurnEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Interface.IAudioManager audioManager;

        /// <summary>
        /// 출혈 커맨드를 초기화합니다.
        /// </summary>
        /// <param name="amount">매 턴 입힐 피해량</param>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">출혈 효과 아이콘</param>
        /// <param name="visualEffectPrefab">출혈 적용 시 재생할 이펙트 프리팹</param>
        /// <param name="perTurnEffectPrefab">출혈 피해 발생 시 매 턴 재생할 이펙트 프리팹</param>
        /// <param name="vfxManager">VFX 매니저 (선택적)</param>
        public BleedEffectCommand(
            int amount, 
            int duration, 
            Sprite icon = null, 
            GameObject visualEffectPrefab = null,
            GameObject perTurnEffectPrefab = null,
            VFXManager vfxManager = null)
        {
            this.amount = amount;
            this.duration = duration;
            this.icon = icon;
            this.visualEffectPrefab = visualEffectPrefab;
            this.perTurnEffectPrefab = perTurnEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = null;
        }

        /// <summary>
        /// 출혈 커맨드 생성자 (의존성 포함)
        /// </summary>
        /// <param name="amount">매 턴 입힐 피해량</param>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">출혈 효과 아이콘</param>
        /// <param name="visualEffectPrefab">출혈 적용 시 재생할 이펙트 프리팹</param>
        /// <param name="perTurnEffectPrefab">출혈 피해 발생 시 매 턴 재생할 이펙트 프리팹</param>
        /// <param name="vfxManager">VFX 매니저 (선택적)</param>
        /// <param name="audioManager">오디오 매니저 (선택적)</param>
        public BleedEffectCommand(
            int amount, 
            int duration, 
            Sprite icon, 
            GameObject visualEffectPrefab,
            GameObject perTurnEffectPrefab,
            VFXManager vfxManager,
            Game.CoreSystem.Interface.IAudioManager audioManager)
        {
            this.amount = amount;
            this.duration = duration;
            this.icon = icon;
            this.visualEffectPrefab = visualEffectPrefab;
            this.perTurnEffectPrefab = perTurnEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        /// <summary>
        /// 출혈 효과를 대상 캐릭터에 적용합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (시전자, 대상 등 포함)</param>
        /// <param name="turnManager">전투 턴 관리자 (옵션)</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target == null)
            {
                GameLogger.LogWarning("[BleedEffectCommand] 대상이 null이므로 출혈 효과를 적용할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (icon == null)
            {
                GameLogger.LogWarning("[BleedEffectCommand] 아이콘이 null입니다. BleedEffectSO의 Icon이 비어있지 않은지 확인하세요.", GameLogger.LogCategory.SkillCard);
            }

            // EffectConfiguration에서 턴당 사운드 가져오기
            var perTurnSfxClip = GetBleedPerTurnSfxClip(context);
            
            // 원본 효과 SO 이름 가져오기 (툴팁 표시용)
            string sourceEffectName = GetSourceEffectName(context);
            
            // 출혈 효과 생성 (perTurnEffectPrefab 및 perTurnSfxClip 전달, sourceCharacter 전달)
            // 출혈을 적용한 캐릭터 정보를 저장하여 시공의 폭풍 데미지 누적에 사용
            var bleedEffect = new BleedEffect(amount, duration, icon, perTurnEffectPrefab, perTurnSfxClip, sourceEffectName, vfxManager, audioManager, context.Source);
            
            // 가드 상태 확인하여 상태이상 효과 등록
            if (context.Target.RegisterStatusEffect(bleedEffect))
            {
                GameLogger.LogInfo($"[BleedEffectCommand] {context.Target.GetCharacterName()}에게 출혈 {amount} 적용 (지속 {duration}턴)", GameLogger.LogCategory.SkillCard);
                
                // 출혈 적용 시 이펙트 재생
                TrySpawnEffectAtTarget(context);
            }
            else
            {
                GameLogger.LogInfo($"[BleedEffectCommand] {context.Target.GetCharacterName()}의 가드로 출혈 효과 차단됨", GameLogger.LogCategory.SkillCard);
                
                // 가드로 차단될 때 가드 차단 이펙트 재생
                PlayGuardBlockEffect(context);
            }
        }

        /// <summary>
        /// 대상 위치에 출혈 이펙트를 생성합니다.
        /// </summary>
        private void TrySpawnEffectAtTarget(ICardExecutionContext context)
        {
            if (context?.Target == null)
            {
                GameLogger.LogWarning("[BleedEffectCommand] 대상(Target)이 null입니다. 출혈 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }
            
            if (visualEffectPrefab == null)
            {
                GameLogger.LogInfo("[BleedEffectCommand] visualEffectPrefab이 지정되지 않았습니다. 출혈 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            var targetTransform = (context.Target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                GameLogger.LogWarning("[BleedEffectCommand] 대상 Transform이 null입니다. 출혈 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // VFXManager를 통한 이펙트 생성 (RectTransform 중심, UI 기반, 정확한 위치 지정)
            // Fallback으로 FindFirstObjectByType 사용 (의존성 주입 실패 시 대비)
            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                var instance = finalVfxManager.PlayEffectAtCharacterRectTransformCenter(visualEffectPrefab, targetTransform);
                if (instance != null)
                {
                    SetEffectLayer(instance);
                    GameLogger.LogInfo($"[BleedEffectCommand] VFXManager로 출혈 VFX 재생 (RectTransform 중심): {instance.name}", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogError("[BleedEffectCommand] VFXManager를 찾을 수 없습니다. 출혈 VFX를 생성할 수 없습니다.", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 이펙트 렌더 순서를 UI 위로 설정합니다.
        /// </summary>
        private static void SetEffectLayer(GameObject effectInstance)
        {
            if (effectInstance == null) return;
            
            int rendererCount = 0;
            int particleCount = 0;

            var renderers = effectInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Effects";
                r.sortingOrder = 10;
                rendererCount++;
            }
            
            var pss = effectInstance.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var pr in pss)
            {
                pr.sortingLayerName = "Effects";
                pr.sortingOrder = 10;
                particleCount++;
            }

            GameLogger.LogInfo($"[BleedEffectCommand] 출혈 VFX 레이어 설정 완료 (Renderer: {rendererCount}, Particle: {particleCount})", GameLogger.LogCategory.SkillCard);
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
        /// EffectConfiguration에서 출혈 턴당 사운드를 가져옵니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>출혈 턴당 사운드</returns>
        private AudioClip GetBleedPerTurnSfxClip(ICardExecutionContext context)
        {
            if (context?.Card?.CardDefinition == null)
            {
                return null;
            }

            var cardDefinition = context.Card.CardDefinition;
            if (!cardDefinition.configuration.hasEffects)
            {
                return null;
            }

            foreach (var effectConfig in cardDefinition.configuration.effects)
            {
                if (effectConfig.effectSO is BleedEffectSO && effectConfig.useCustomSettings && effectConfig.customSettings != null)
                {
                    return effectConfig.customSettings.bleedPerTurnSfxClip;
                }
            }

            return null;
        }

        /// <summary>
        /// EffectConfiguration에서 원본 효과 SO 이름을 가져옵니다 (툴팁 표시용).
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>원본 효과 SO 이름 (없으면 null)</returns>
        private string GetSourceEffectName(ICardExecutionContext context)
        {
            if (context?.Card?.CardDefinition == null)
            {
                return null;
            }

            var cardDefinition = context.Card.CardDefinition;
            if (!cardDefinition.configuration.hasEffects)
            {
                return null;
            }

            foreach (var effectConfig in cardDefinition.configuration.effects)
            {
                if (effectConfig.effectSO is BleedEffectSO bleedEffectSO)
                {
                    string effectName = bleedEffectSO.GetEffectName();
                    if (!string.IsNullOrWhiteSpace(effectName))
                    {
                        return effectName;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 가드로 출혈 효과가 차단될 때 가드 차단 이펙트를 재생합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        private void PlayGuardBlockEffect(ICardExecutionContext context)
        {
            if (context?.Target == null)
            {
                return;
            }

            // 타겟이 CharacterBase인지 확인
            if (!(context.Target is Game.CharacterSystem.Core.CharacterBase targetCharacter))
            {
                return;
            }

            // 가드 버프 찾기
            var buffs = targetCharacter.GetBuffs();
            Game.SkillCardSystem.Effect.GuardBuff guardBuff = null;
            foreach (var buff in buffs)
            {
                if (buff is Game.SkillCardSystem.Effect.GuardBuff gb)
                {
                    guardBuff = gb;
                    break;
                }
            }

            // 가드 버프가 없거나 만료되었으면 재생하지 않음
            if (guardBuff == null || guardBuff.IsExpired || guardBuff.RemainingTurns <= 0)
            {
                return;
            }

            // GuardBuff에서 가드 차단 이펙트 가져오기
            if (guardBuff.BlockEffectPrefab == null)
            {
                return;
            }

            var targetTransform = (context.Target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                return;
            }

            // VFXManager를 통한 가드 차단 이펙트 재생 (정확한 위치 지정)
            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                var guardBlockEffectInstance = finalVfxManager.PlayEffectAtCharacterCenter(guardBuff.BlockEffectPrefab, targetTransform);
                if (guardBlockEffectInstance != null)
                {
                    GameLogger.LogInfo($"[BleedEffectCommand] 가드 차단 이펙트 재생 성공: {guardBuff.BlockEffectPrefab.name} → {context.Target.GetCharacterName()}", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogError("[BleedEffectCommand] VFXManager를 찾을 수 없습니다. 가드 차단 이펙트를 생성할 수 없습니다.", GameLogger.LogCategory.SkillCard);
            }

            // 가드 차단 사운드 재생
            if (guardBuff.BlockSfxClip != null)
            {
                // Fallback으로 FindFirstObjectByType 사용 (의존성 주입 실패 시 대비)
                var am = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>() as Game.CoreSystem.Interface.IAudioManager;
                if (am != null)
                {
                    am.PlaySFXWithPool(guardBuff.BlockSfxClip, 0.9f);
                    GameLogger.LogInfo($"[BleedEffectCommand] 가드 차단 사운드 재생: {guardBuff.BlockSfxClip.name}", GameLogger.LogCategory.SkillCard);
                }
            }
        }
    }
}
