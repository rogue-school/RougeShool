using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using UnityEngine.UI;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 자신의 체력을 회복시키는 명령입니다.
    /// </summary>
    public class HealEffectCommand : ICardEffectCommand
    {
        private readonly int healAmount;
        private readonly int maxHealAmount;
        private readonly GameObject visualEffectPrefab;
        private readonly AudioClip healSfxClip;
        private readonly VFXManager vfxManager;

        public HealEffectCommand(int healAmount, int maxHealAmount = 0, GameObject visualEffectPrefab = null, AudioClip healSfxClip = null, VFXManager vfxManager = null)
        {
            this.healAmount = healAmount;
            this.maxHealAmount = maxHealAmount;
            this.visualEffectPrefab = visualEffectPrefab;
            this.healSfxClip = healSfxClip;
            this.vfxManager = vfxManager;
            
            // GameLogger.LogInfo($"[HealEffectCommand] 생성됨 - 치유량: {healAmount}, 최대: {maxHealAmount}", 
            //    GameLogger.LogCategory.SkillCard);
        }
        
        /// <summary>
        /// EffectCustomSettings를 통해 생성하는 생성자
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        public HealEffectCommand(Game.SkillCardSystem.Data.EffectCustomSettings customSettings)
        {
            this.healAmount = customSettings.healAmount;
            this.maxHealAmount = 0; // 커스텀 설정에서는 최대치 제한 없음
            this.visualEffectPrefab = customSettings.healEffectPrefab;
            this.healSfxClip = customSettings.healSfxClip;
            this.vfxManager = null; // VFXManager는 Execute에서 찾음
            
            // GameLogger.LogInfo($"[HealEffectCommand] 생성됨 (CustomSettings) - 치유량: {healAmount}", 
            //    GameLogger.LogCategory.SkillCard);
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[HealEffectCommand] 시전자가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // EffectConfiguration에서 사운드 가져오기 (우선순위)
            var customSfxClip = GetHealSfxClip(context);
            if (customSfxClip != null)
            {
                // context에서 가져온 사운드로 재생
                PlayHealSoundFromClip(customSfxClip);
            }
            else if (healSfxClip != null)
            {
                // 생성자에서 받은 사운드로 재생
                PlayHealSoundFromClip(healSfxClip);
            }

            var source = context.Source;
            string sourceName = source.GetCharacterName();
            
            // 현재 체력과 최대 체력 확인
            int currentHP = source.GetCurrentHP();
            int maxHP = source.GetMaxHP();
            
            // 이미 최대 체력이면 치유 불가하지만 이펙트와 사운드는 재생
            bool isFullHP = currentHP >= maxHP;
            
            // 실제 치유량 계산 (최대 체력을 넘지 않도록)
            int actualHealAmount = healAmount;
            if (maxHealAmount > 0)
            {
                actualHealAmount = Mathf.Min(healAmount, maxHealAmount);
            }
            
            // 풀피가 아니면 체력 회복 적용
            if (!isFullHP)
            {
                // 최대 체력을 넘지 않도록 제한
                int maxPossibleHeal = maxHP - currentHP;
                actualHealAmount = Mathf.Min(actualHealAmount, maxPossibleHeal);
                
                if (actualHealAmount > 0)
                {
                    // 체력 회복 적용
                    source.Heal(actualHealAmount);
                    
                    int newHP = source.GetCurrentHP();
                    
                    GameLogger.LogInfo($"[HealEffectCommand] '{sourceName}' 체력 회복 완료 - 치유량: {actualHealAmount}, 체력: {currentHP} → {newHP}/{maxHP}", 
                        GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogInfo($"[HealEffectCommand] '{sourceName}' 이미 최대 체력입니다. (현재: {currentHP}/{maxHP})", 
                    GameLogger.LogCategory.SkillCard);
            }

            // 풀피여도 이펙트와 사운드는 재생 (스킬이 작동 중임을 알리기 위해)
            // 시각적 이펙트: 시전자 위치에 생성
            TrySpawnEffectAtSource(context);
        }

        /// <summary>
        /// 실행 가능 여부 (항상 true)
        /// </summary>
        public bool CanExecute() => true;

        /// <summary>
        /// 효과 비용 (무료)
        /// </summary>
        public int GetCost() => 0;

        /// <summary>
        /// 시전자 위치에 힐 비주얼 이펙트를 생성합니다.
        /// </summary>
        private void TrySpawnEffectAtSource(ICardExecutionContext context)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[HealEffectCommand] 시전자(Source)가 null입니다. 힐 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[HealEffectCommand] visualEffectPrefab이 지정되지 않았습니다. 힐 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 포트레잇 이미지의 정확한 중앙에서 스폰되도록 계산
            var spawnPos = GetPortraitCenterWorldPosition(context.Source.Transform);
            GameLogger.LogInfo($"[HealEffectCommand] 힐 VFX 생성 시작 - 프리팹: {visualEffectPrefab.name}, 위치: {spawnPos}", GameLogger.LogCategory.SkillCard);

            // VFXManager를 통한 이펙트 생성 (정확한 위치 지정)
            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                var instance = finalVfxManager.PlayEffectAtCharacterCenter(visualEffectPrefab, context.Source.Transform);
                if (instance != null)
                {
                    SetEffectLayer(instance);
                    GameLogger.LogInfo($"[HealEffectCommand] VFXManager로 힐 VFX 재생: {instance.name}", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogError("[HealEffectCommand] VFXManager를 찾을 수 없습니다. 힐 VFX를 생성할 수 없습니다.", GameLogger.LogCategory.SkillCard);
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

            GameLogger.LogInfo($"[HealEffectCommand] 힐 VFX 레이어 설정 완료 (Renderer: {rendererCount}, Particle: {particleCount})", GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 캐릭터 트랜스폼 하위의 포트레잇 Image 중심(시각적 중심)을 월드 좌표로 계산합니다.
        /// 이름이 'Portrait'인 Image를 우선 사용하고, 없으면 첫 번째 Image를 사용합니다.
        /// Image가 없으면 RectTransform/SpriteRenderer/Transform 순으로 폴백합니다.
        /// </summary>
        private static Vector3 GetPortraitCenterWorldPosition(Transform root)
        {
            if (root == null) return Vector3.zero;

            // 0) 디자이너 지정 앵커 우선 (정밀 제어용)
            var explicitAnchor = FindExplicitVfxAnchor(root);
            if (explicitAnchor != null)
            {
                return explicitAnchor.position;
            }

            // 1) Portrait Image 우선
            var portraitImage = FindPortraitImage(root);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                return GetRectTransformCenterWorld(portraitImage.rectTransform);
            }

            // 2) RectTransform 폴백
            var anyRect = root.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                return GetRectTransformCenterWorld(anyRect);
            }

            // 3) SpriteRenderer 폴백
            var sprite = root.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null)
            {
                return sprite.bounds.center;
            }

            // 4) 최종 폴백
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

        // 디자이너가 배치하는 선택적 VFX 앵커를 찾습니다.
        // 우선순위: "VFXAnchor" → "PortraitVFXAnchor"
        private static Transform FindExplicitVfxAnchor(Transform root)
        {
            var a = root.Find("VFXAnchor");
            if (a != null) return a;
            return root.Find("PortraitVFXAnchor");
        }

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }

        /// <summary>
        /// EffectConfiguration에서 치유 사운드를 가져옵니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>치유 사운드</returns>
        private AudioClip GetHealSfxClip(ICardExecutionContext context)
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
                if (effectConfig.effectSO is HealEffectSO && effectConfig.useCustomSettings && effectConfig.customSettings != null)
                {
                    return effectConfig.customSettings.healSfxClip;
                }
            }

            return null;
        }

        /// <summary>
        /// 치유 사운드를 재생합니다.
        /// </summary>
        /// <param name="sfxClip">재생할 사운드 클립</param>
        private void PlayHealSoundFromClip(AudioClip sfxClip)
        {
            if (sfxClip == null)
            {
                return;
            }

            var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[HealEffectCommand] 치유 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning("[HealEffectCommand] AudioManager를 찾을 수 없습니다. 치유 사운드 재생을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
            }
        }
    }
}
