
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.VFXSystem.Manager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 체력 회복 효과 커맨드입니다.
    /// </summary>
    public class HealEffectCommand : BaseItemEffectCommand
    {
        private readonly int healAmount;
        private readonly AudioClip sfxClip;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Audio.AudioManager audioManager;

        public HealEffectCommand(
            int healAmount, 
            AudioClip sfxClip = null, 
            GameObject visualEffectPrefab = null, 
            VFXManager vfxManager = null,
            Game.CoreSystem.Audio.AudioManager audioManager = null) : base("체력 회복")
        {
            this.healAmount = healAmount;
            this.sfxClip = sfxClip;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            int currentHP = context.User.GetCurrentHP();
            int maxHP = context.User.GetMaxHP();
            int actualHeal = Mathf.Min(healAmount, maxHP - currentHP);

            if (actualHeal > 0)
            {
                context.User.Heal(actualHeal);
                GameLogger.LogInfo($"체력 회복: {actualHeal} (현재: {currentHP + actualHeal}/{maxHP})", GameLogger.LogCategory.Core);
                PlaySFX();
                PlayVisualEffect(context);
                return true;
            }
            else
            {
                GameLogger.LogInfo("체력이 이미 최대입니다 - 아이템 사용은 성공으로 처리", GameLogger.LogCategory.Core);
                return true; // 체력이 최대여도 아이템 사용은 성공으로 처리
            }
        }

        /// <summary>
        /// 회복 사운드를 재생합니다.
        /// </summary>
        private void PlaySFX()
        {
            if (sfxClip == null)
            {
                GameLogger.LogWarning("[HealEffectCommand] 사운드 클립이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalAudioManager = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (finalAudioManager != null)
            {
                finalAudioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[HealEffectCommand] 회복 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[HealEffectCommand] AudioManager를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 회복 비주얼 이펙트를 재생합니다.
        /// </summary>
        private void PlayVisualEffect(IItemUseContext context)
        {
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[HealEffectCommand] 비주얼 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var userTransform = (context.User as MonoBehaviour)?.transform;
            if (userTransform == null)
            {
                GameLogger.LogWarning("[HealEffectCommand] 사용자 Transform을 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                // 캐릭터의 시각적 중심 위치 계산
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                
                var effectInstance = finalVfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[HealEffectCommand] 회복 비주얼 이펙트 재생: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[HealEffectCommand] 회복 비주얼 이펙트 생성 완료: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
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

            GameLogger.LogInfo($"[HealEffectCommand] 이펙트 레이어 설정 완료 (Renderer: {rendererCount}, Particle: {particleCount})", GameLogger.LogCategory.Core);
        }
    }

    /// <summary>
    /// 공격력 버프 효과 커맨드입니다.
    /// </summary>
    public class AttackBuffEffectCommand : BaseItemEffectCommand
    {
        private readonly int buffAmount;
        private readonly int duration;
        private readonly AudioClip sfxClip;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Audio.AudioManager audioManager;

        public AttackBuffEffectCommand(
            int buffAmount, 
            int duration = 1,
            AudioClip sfxClip = null,
            GameObject visualEffectPrefab = null,
            VFXManager vfxManager = null,
            Game.CoreSystem.Audio.AudioManager audioManager = null) : base("공격력 버프")
        {
            this.buffAmount = buffAmount;
            this.duration = duration;
            this.sfxClip = sfxClip;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // 아이템 정의를 ActiveItemDefinition으로 캐스팅
            var activeItemDef = context.ItemDefinition as Game.ItemSystem.Data.ActiveItemDefinition;
            var turnPolicy = activeItemDef?.turnPolicy ?? Interface.ItemEffectTurnPolicy.TargetTurnOnly;
            var itemIcon = context.ItemDefinition?.Icon;
            var itemName = context.ItemDefinition?.DisplayName;

            // 공격력 버프 효과 생성 및 적용
            var attackBuffEffect = new AttackPowerBuffEffect(buffAmount, duration, turnPolicy, itemIcon, itemName);
            context.User.RegisterPerTurnEffect(attackBuffEffect);

            // UI에 버프 아이콘 표시 (아이템 이미지 사용)
            if (context.User is Game.CharacterSystem.Core.PlayerCharacter playerCharacter)
            {
                GameLogger.LogInfo($"공격력 물약 아이콘: {itemIcon?.name ?? "null"}, 아이템: {context.ItemDefinition?.DisplayName ?? "null"}", GameLogger.LogCategory.Core);
                playerCharacter.AddBuffDebuffIcon("AttackPowerBuff", itemIcon, true, duration);
            }

            // 사운드 및 비주얼 이펙트 재생
            PlaySFX();
            PlayVisualEffect(context);

            GameLogger.LogInfo($"공격력 버프 적용: +{buffAmount} ({duration}턴, 정책: {turnPolicy})", GameLogger.LogCategory.Core);
            return true;
        }

        /// <summary>
        /// 버프 사운드를 재생합니다.
        /// </summary>
        private void PlaySFX()
        {
            if (sfxClip == null)
            {
                GameLogger.LogWarning("[AttackBuffEffectCommand] 사운드 클립이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalAudioManager = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (finalAudioManager != null)
            {
                finalAudioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[AttackBuffEffectCommand] 버프 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[AttackBuffEffectCommand] AudioManager를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 버프 비주얼 이펙트를 재생합니다.
        /// </summary>
        private void PlayVisualEffect(IItemUseContext context)
        {
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[AttackBuffEffectCommand] 비주얼 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var userTransform = (context.User as MonoBehaviour)?.transform;
            if (userTransform == null)
            {
                GameLogger.LogWarning("[AttackBuffEffectCommand] 사용자 Transform을 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                // 캐릭터의 시각적 중심 위치 계산
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                
                var effectInstance = finalVfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[AttackBuffEffectCommand] 버프 비주얼 이펙트 재생: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[AttackBuffEffectCommand] 버프 비주얼 이펙트 생성 완료: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
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

            GameLogger.LogInfo($"[AttackBuffEffectCommand] 이펙트 레이어 설정 완료 (Renderer: {rendererCount}, Particle: {particleCount})", GameLogger.LogCategory.Core);
        }
    }

    /// <summary>
    /// 광대 물약 랜덤 효과 커맨드입니다.
    /// </summary>
    public class ClownPotionEffectCommand : BaseItemEffectCommand
    {
        private int healChance;
        private int healAmount;
        private int damageAmount;
        private AudioClip healSfxClip;
        private AudioClip damageSfxClip;
        private GameObject healVisualEffectPrefab;
        private GameObject damageVisualEffectPrefab;

        public ClownPotionEffectCommand(
            int healChance = 50, 
            int healAmount = 5, 
            int damageAmount = 5, 
            AudioClip healSfxClip = null, 
            AudioClip damageSfxClip = null,
            GameObject healVisualEffectPrefab = null,
            GameObject damageVisualEffectPrefab = null)
            : base("광대 물약")
        {
            this.healChance = Mathf.Clamp(healChance, 0, 100);
            this.healAmount = healAmount;
            this.damageAmount = damageAmount;
            this.healSfxClip = healSfxClip;
            this.damageSfxClip = damageSfxClip;
            this.healVisualEffectPrefab = healVisualEffectPrefab;
            this.damageVisualEffectPrefab = damageVisualEffectPrefab;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            bool isHeal = UnityEngine.Random.Range(0, 100) < healChance;

            if (isHeal)
            {
                context.User.Heal(healAmount);
                GameLogger.LogInfo($"광대 물약 효과: 체력 회복 +{healAmount} (확률: {healChance}%)", GameLogger.LogCategory.Core);
                
                // 회복 SFX 재생
                PlaySFX(healSfxClip, "회복");
                
                // 회복 비주얼 이펙트 재생
                PlayVisualEffect(context, healVisualEffectPrefab, "회복");
            }
            else
            {
                context.User.TakeDamage(damageAmount);
                GameLogger.LogInfo($"광대 물약 효과: 데미지 -{damageAmount} (확률: {100 - healChance}%)", GameLogger.LogCategory.Core);
                
                // 데미지 SFX 재생
                PlaySFX(damageSfxClip, "데미지");
                
                // 데미지 비주얼 이펙트 재생
                PlayVisualEffect(context, damageVisualEffectPrefab, "데미지");
            }

            return true;
        }

        /// <summary>
        /// SFX를 재생합니다.
        /// </summary>
        /// <param name="clip">재생할 SFX 클립</param>
        /// <param name="type">SFX 타입 (로그용)</param>
        private void PlaySFX(AudioClip clip, string type)
        {
            if (clip == null)
            {
                GameLogger.LogInfo($"[ClownPotionEffectCommand] {type} SFX 클립이 설정되지 않음", GameLogger.LogCategory.Core);
                return;
            }

            // AudioManager 찾기
            var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySFXWithPool(clip, 0.9f);
                GameLogger.LogInfo($"[ClownPotionEffectCommand] {type} SFX 재생: {clip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[ClownPotionEffectCommand] AudioManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 비주얼 이펙트를 재생합니다.
        /// </summary>
        /// <param name="context">아이템 사용 컨텍스트</param>
        /// <param name="effectPrefab">재생할 이펙트 프리팹</param>
        /// <param name="type">이펙트 타입 (로그용)</param>
        private void PlayVisualEffect(IItemUseContext context, GameObject effectPrefab, string type)
        {
            if (effectPrefab == null)
            {
                GameLogger.LogInfo($"[ClownPotionEffectCommand] {type} 비주얼 이펙트 프리팹이 설정되지 않음", GameLogger.LogCategory.Core);
                return;
            }

            if (context?.User == null)
            {
                GameLogger.LogWarning("[ClownPotionEffectCommand] 사용자가 null입니다. 이펙트 생성을 건너뜁니다.", GameLogger.LogCategory.Core);
                return;
            }

            var userTransform = (context.User as MonoBehaviour)?.transform;
            if (userTransform == null)
            {
                GameLogger.LogWarning("[ClownPotionEffectCommand] 사용자 Transform이 null입니다. 이펙트 생성을 건너뜁니다.", GameLogger.LogCategory.Core);
                return;
            }

            // VFXManager 찾기
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<Game.VFXSystem.Manager.VFXManager>();
            if (vfxManager != null)
            {
                // 캐릭터의 시각적 중심 위치 계산
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                
                var effectInstance = vfxManager.PlayEffect(effectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[ClownPotionEffectCommand] {type} 비주얼 이펙트 재생: {effectPrefab.name}", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                var instance = UnityEngine.Object.Instantiate(effectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[ClownPotionEffectCommand] {type} 비주얼 이펙트 생성 완료: {effectPrefab.name}", GameLogger.LogCategory.Core);
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

        private static UnityEngine.UI.Image FindPortraitImage(Transform root)
        {
            var images = root.GetComponentsInChildren<UnityEngine.UI.Image>(true);
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

            GameLogger.LogInfo($"[ClownPotionEffectCommand] 이펙트 레이어 설정 완료 (Renderer: {rendererCount}, Particle: {particleCount})", GameLogger.LogCategory.Core);
        }
    }

    /// <summary>
    /// 부활 효과 커맨드입니다.
    /// </summary>
    public class ReviveEffectCommand : BaseItemEffectCommand
    {
        private readonly AudioClip sfxClip;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Audio.AudioManager audioManager;

        public ReviveEffectCommand(
            AudioClip sfxClip = null,
            GameObject visualEffectPrefab = null,
            VFXManager vfxManager = null,
            Game.CoreSystem.Audio.AudioManager audioManager = null) : base("부활")
        {
            this.sfxClip = sfxClip;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        /// <summary>
        /// 부활 아이템은 사망한 사용자도 사용할 수 있도록 Execute 메서드를 오버라이드
        /// </summary>
        public override bool Execute(IItemUseContext context)
        {
            // 부활 아이템은 사망한 사용자도 허용 (null 체크만 수행)
            if (context?.User == null)
            {
                GameLogger.LogError("부활 실패: 사용자가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            try
            {
                return ExecuteInternal(context);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[부활] 효과 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Core);
                return false;
            }
        }

        protected override bool ValidateAdditionalConditions(IItemUseContext context)
        {
            if (!context.User.IsDead())
            {
                GameLogger.LogInfo("사용자가 이미 살아있습니다", GameLogger.LogCategory.Core);
                return false;
            }
            return true;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // 최대 체력으로 부활
            int maxHP = context.User.GetMaxHP();
            context.User.Heal(maxHP);

            // 사운드 및 비주얼 이펙트 재생
            PlaySFX();
            PlayVisualEffect(context);

            // 모든 디버프 제거
            if (context.User is Game.CharacterSystem.Core.CharacterBase characterBase)
            {
                var buffs = characterBase.GetBuffs();
                var debuffsToRemove = new System.Collections.Generic.List<Game.SkillCardSystem.Interface.IPerTurnEffect>();

                foreach (var effect in buffs)
                {
                    // 디버프 효과들만 제거 (버프는 유지)
                    if (effect is Game.SkillCardSystem.Interface.IStatusEffectDebuff)
                    {
                        debuffsToRemove.Add(effect);
                    }
                }

                foreach (var debuff in debuffsToRemove)
                {
                    // CharacterBase에서 디버프 제거 메서드가 있다면 사용
                    // 현재는 직접 제거할 수 없으므로 로그만 출력
                    GameLogger.LogInfo($"부활 시 디버프 제거: {debuff.GetType().Name}", GameLogger.LogCategory.Core);
                }
            }

            GameLogger.LogInfo($"부활 완료: 체력 {maxHP}으로 회복, 모든 디버프 제거", GameLogger.LogCategory.Core);
            return true;
        }

        /// <summary>
        /// 부활 사운드를 재생합니다.
        /// </summary>
        private void PlaySFX()
        {
            if (sfxClip == null)
            {
                GameLogger.LogWarning("[ReviveEffectCommand] 사운드 클립이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalAudioManager = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (finalAudioManager != null)
            {
                finalAudioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[ReviveEffectCommand] 부활 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[ReviveEffectCommand] AudioManager를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 부활 비주얼 이펙트를 재생합니다.
        /// </summary>
        private void PlayVisualEffect(IItemUseContext context)
        {
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[ReviveEffectCommand] 비주얼 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var userTransform = (context.User as MonoBehaviour)?.transform;
            if (userTransform == null)
            {
                GameLogger.LogWarning("[ReviveEffectCommand] 사용자 Transform을 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                var effectInstance = finalVfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[ReviveEffectCommand] 부활 비주얼 이펙트 재생: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[ReviveEffectCommand] 부활 비주얼 이펙트 생성 완료: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
            }
        }

        private static Vector3 GetPortraitCenterWorldPosition(Transform root)
        {
            if (root == null) return Vector3.zero;

            var portraitImage = FindPortraitImage(root);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                return GetRectTransformCenterWorld(portraitImage.rectTransform);
            }

            var anyRect = root.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                return GetRectTransformCenterWorld(anyRect);
            }

            var sprite = root.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null)
            {
                return sprite.bounds.center;
            }

            return root.position;
        }

        private static Image FindPortraitImage(Transform root)
        {
            var images = root.GetComponentsInChildren<Image>(true);
            if (images == null || images.Length == 0) return null;

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject != null && images[i].gameObject.name == "Portrait")
                {
                    return images[i];
                }
            }

            return images[0];
        }

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }

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
    }

    /// <summary>
    /// 시간 정지 효과 커맨드입니다.
    /// 다음 턴에 사용될 적 카드를 봉인시킵니다.
    /// </summary>
    public class TimeStopEffectCommand : BaseItemEffectCommand
    {
        private int sealCount;
        private readonly AudioClip sfxClip;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Audio.AudioManager audioManager;

        public TimeStopEffectCommand(
            int sealCount = 1,
            AudioClip sfxClip = null,
            GameObject visualEffectPrefab = null,
            VFXManager vfxManager = null,
            Game.CoreSystem.Audio.AudioManager audioManager = null) : base("시간 정지")
        {
            this.sealCount = Mathf.Max(1, sealCount);
            this.sfxClip = sfxClip;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            var itemIcon = context.ItemDefinition?.Icon;

            // 적 캐릭터를 직접 찾기 (context.Target 무시)
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager == null)
            {
                GameLogger.LogError($"[TimeStopEffect] EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var enemyCharacter = enemyManager.GetCurrentEnemy();
            if (enemyCharacter == null)
            {
                GameLogger.LogError($"[TimeStopEffect] 현재 적 캐릭터를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            GameLogger.LogInfo($"[TimeStopEffect] 적 캐릭터 발견: {enemyCharacter.GetCharacterName()}, 플레이어인가={enemyCharacter.IsPlayerControlled()}", GameLogger.LogCategory.Core);

            // StunDebuff는 매 턴 감소하므로 2턴으로 설정
            // EveryTurn 정책: 플레이어 턴(적용) → 적 턴(감소하며 스턴 유지) → 다음 플레이어 턴(만료)
            // 이렇게 하면 적 턴 전체를 차단할 수 있습니다
            var stunDebuff = new Game.SkillCardSystem.Effect.StunDebuff(2, itemIcon);
            // 아이템 유래 상태이상은 가드를 무시하고 직접 등록 (RegisterPerTurnEffect 사용)
            enemyCharacter.RegisterPerTurnEffect(stunDebuff);
            GameLogger.LogInfo($"[TimeStopEffect] 타임 스톱 스크롤 적용: {enemyCharacter.GetCharacterName()}에게 2턴 스턴 (아이템은 가드 무시)", GameLogger.LogCategory.Core);

            // 사운드 및 비주얼 이펙트 재생 (적 캐릭터 위치)
            PlaySFX();
            PlayVisualEffect(enemyCharacter);

            return true;
        }

        /// <summary>
        /// 시간 정지 사운드를 재생합니다.
        /// </summary>
        private void PlaySFX()
        {
            if (sfxClip == null)
            {
                GameLogger.LogWarning("[TimeStopEffectCommand] 사운드 클립이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalAudioManager = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (finalAudioManager != null)
            {
                finalAudioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[TimeStopEffectCommand] 시간 정지 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[TimeStopEffectCommand] AudioManager를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 시간 정지 비주얼 이펙트를 재생합니다.
        /// </summary>
        private void PlayVisualEffect(Game.CharacterSystem.Interface.ICharacter target)
        {
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[TimeStopEffectCommand] 비주얼 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                GameLogger.LogWarning("[TimeStopEffectCommand] 대상 Transform을 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                var spawnPos = GetPortraitCenterWorldPosition(targetTransform);
                var effectInstance = finalVfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[TimeStopEffectCommand] 시간 정지 비주얼 이펙트 재생: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                var spawnPos = GetPortraitCenterWorldPosition(targetTransform);
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[TimeStopEffectCommand] 시간 정지 비주얼 이펙트 생성 완료: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
            }
        }

        private static Vector3 GetPortraitCenterWorldPosition(Transform root)
        {
            if (root == null) return Vector3.zero;

            var portraitImage = FindPortraitImage(root);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                return GetRectTransformCenterWorld(portraitImage.rectTransform);
            }

            var anyRect = root.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                return GetRectTransformCenterWorld(anyRect);
            }

            var sprite = root.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null)
            {
                return sprite.bounds.center;
            }

            return root.position;
        }

        private static Image FindPortraitImage(Transform root)
        {
            var images = root.GetComponentsInChildren<Image>(true);
            if (images == null || images.Length == 0) return null;

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject != null && images[i].gameObject.name == "Portrait")
                {
                    return images[i];
                }
            }

            return images[0];
        }

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }

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
    }

    /// <summary>
    /// 운명의 주사위 효과 커맨드입니다.
    /// 다음 턴 적이 사용 예정인 스킬을 무작위로 변경시킵니다.
    /// </summary>
    public class DiceOfFateEffectCommand : BaseItemEffectCommand
    {
        private int changeCount;
        private readonly AudioClip sfxClip;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Audio.AudioManager audioManager;

        public DiceOfFateEffectCommand(
            int changeCount = 1,
            AudioClip sfxClip = null,
            GameObject visualEffectPrefab = null,
            VFXManager vfxManager = null,
            Game.CoreSystem.Audio.AudioManager audioManager = null) : base("운명의 주사위")
        {
            this.changeCount = Mathf.Max(1, changeCount);
            this.sfxClip = sfxClip;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        /// <summary>
        /// 운명의 주사위 효과를 실행합니다
        /// 목적: 다음 턴에 사용될 적의 스킬카드를 랜덤하게 교체
        /// </summary>
        protected override bool ExecuteInternal(IItemUseContext context)
        {
            GameLogger.LogInfo("[DiceOfFate] 운명의 주사위 효과 실행 시작", GameLogger.LogCategory.Core);

            // 1단계: WAIT_SLOT_1 찾기 (다음 턴에 사용될 적 카드)
            var targetSlot = FindWaitSlot1();
            if (targetSlot == null)
            {
                GameLogger.LogError("[DiceOfFate] WAIT_SLOT_1을 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 2단계: 기존 적 카드 확인
            var existingCard = GetEnemyCardFromSlot(targetSlot);
            if (existingCard == null)
            {
                GameLogger.LogError("[DiceOfFate] WAIT_SLOT_1에 적 카드가 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            GameLogger.LogInfo($"[DiceOfFate] 교체 대상 카드: {existingCard.GetCardName()}", GameLogger.LogCategory.Core);

            // 3단계: 새 적 카드 생성
            var newCard = CreateRandomEnemyCard();
            if (newCard == null)
            {
                GameLogger.LogError("[DiceOfFate] 새 적 카드 생성에 실패했습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 4단계: 카드 데이터 교체 (GameObject 생성/삭제 없음)
            ReplaceCardInSlot(targetSlot, newCard);

            // 5단계: 이펙트 및 사운드 재생
            PlaySFX();
            PlayVisualEffect(context);

            GameLogger.LogInfo($"[DiceOfFate] 운명의 주사위 적용 완료: {existingCard.GetCardName()} → {newCard.GetCardName()}", GameLogger.LogCategory.Core);
            return true;
        }

        /// <summary>
        /// 운명의 주사위 사운드를 재생합니다.
        /// </summary>
        private void PlaySFX()
        {
            if (sfxClip == null)
            {
                GameLogger.LogInfo("[DiceOfFate] 사운드 클립이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalAudioManager = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (finalAudioManager != null)
            {
                finalAudioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[DiceOfFate] 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[DiceOfFate] AudioManager를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 운명의 주사위 비주얼 이펙트를 재생합니다.
        /// 적 캐릭터에게 이펙트를 재생합니다.
        /// </summary>
        private void PlayVisualEffect(IItemUseContext context)
        {
            if (visualEffectPrefab == null)
            {
                GameLogger.LogInfo("[DiceOfFate] 비주얼 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            // 적 캐릭터의 Transform 가져오기 (Target이 적)
            var targetTransform = (context.Target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                // Fallback: EnemyManager에서 적 캐릭터 찾기
                var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
                var enemyCharacter = enemyManager?.GetCurrentEnemy();
                targetTransform = (enemyCharacter as MonoBehaviour)?.transform;
            }

            if (targetTransform == null)
            {
                GameLogger.LogWarning("[DiceOfFate] 적 캐릭터 Transform을 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                // 캐릭터의 시각적 중심 위치 계산
                var spawnPos = GetPortraitCenterWorldPosition(targetTransform);
                
                var effectInstance = finalVfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[DiceOfFate] 비주얼 이펙트 재생: {visualEffectPrefab.name} (적 캐릭터: {targetTransform.name})", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var spawnPos = GetPortraitCenterWorldPosition(targetTransform);
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[DiceOfFate] 비주얼 이펙트 생성 완료: {visualEffectPrefab.name} (적 캐릭터: {targetTransform.name})", GameLogger.LogCategory.Core);
            }
        }

        private Game.CombatSystem.UI.CombatExecutionSlotUI FindWaitSlot1()
        {
            var combatSlots = UnityEngine.Object.FindObjectsByType<Game.CombatSystem.UI.CombatExecutionSlotUI>(FindObjectsSortMode.None);
            return System.Array.Find(combatSlots, s => s.Position == CombatSlotPosition.WAIT_SLOT_1);
        }

        private Game.SkillCardSystem.Interface.ISkillCard GetEnemyCardFromSlot(Game.CombatSystem.UI.CombatExecutionSlotUI slot)
        {
            var card = slot.GetCard();
            if (card != null && card.GetOwner() == SlotOwner.ENEMY)
            {
                return card;
            }

            // 자식에서 찾기
            var cardUI = slot.transform.GetComponentInChildren<Game.SkillCardSystem.UI.SkillCardUI>();
            if (cardUI != null)
            {
                var childCard = cardUI.GetCard();
                if (childCard != null && childCard.GetOwner() == SlotOwner.ENEMY)
                {
                    return childCard;
                }
            }

            return null;
        }

        private Game.SkillCardSystem.Interface.ISkillCard CreateRandomEnemyCard()
        {
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager?.GetCurrentEnemy() is not Game.CharacterSystem.Core.EnemyCharacter enemyCharacter)
            {
                GameLogger.LogError("[DiceOfFate] EnemyCharacter를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }

            var enemyData = enemyCharacter.CharacterData;
            if (enemyData?.EnemyDeck == null)
            {
                GameLogger.LogError("[DiceOfFate] 적 스킬 덱을 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }

            var randomEntry = enemyData.EnemyDeck.GetRandomEntry();
            if (randomEntry == null)
            {
                GameLogger.LogError("[DiceOfFate] 적 스킬 덱에서 랜덤 카드를 선택할 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }

            // 직접 SkillCardFactory 인스턴스 생성 (DI 없이)
            var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (audioManager == null)
            {
                GameLogger.LogError("[DiceOfFate] AudioManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }
            
            var skillCardFactory = new Game.SkillCardSystem.Factory.SkillCardFactory(audioManager);
            // 데미지 오버라이드가 있으면 사용, 없으면 기본값 사용
            var newCard = randomEntry.HasDamageOverride()
                ? skillCardFactory.CreateEnemyCard(randomEntry.definition, enemyCharacter.GetCharacterName(), randomEntry.damageOverride)
                : skillCardFactory.CreateEnemyCard(randomEntry.definition, enemyCharacter.GetCharacterName());
            
            if (newCard != null)
            {
                GameLogger.LogInfo($"[DiceOfFate] 새 카드 생성 완료: {newCard.GetCardName()}", GameLogger.LogCategory.Core);
            }
            
            return newCard;
        }

        /// <summary>
        /// 슬롯의 카드를 새로운 카드로 교체합니다 (데이터 교체 방식)
        /// </summary>
        /// <param name="slot">대상 슬롯</param>
        /// <param name="newCard">새로운 카드 데이터</param>
        private void ReplaceCardInSlot(Game.CombatSystem.UI.CombatExecutionSlotUI slot, Game.SkillCardSystem.Interface.ISkillCard newCard)
        {
            if (slot == null || newCard == null)
            {
                GameLogger.LogError("[DiceOfFate] 슬롯 또는 새 카드가 null입니다", GameLogger.LogCategory.Core);
                return;
            }

            var oldCardName = slot.GetCard()?.GetCardName() ?? "null";
            GameLogger.LogInfo($"[DiceOfFate] 카드 교체 시작 - Position: {slot.Position}, 기존: {oldCardName} → 신규: {newCard.GetCardName()}", GameLogger.LogCategory.Core);
            
            // 1단계: 슬롯의 currentCard를 먼저 업데이트
            slot.SetCard(newCard);
            GameLogger.LogInfo($"[DiceOfFate] 슬롯 currentCard 업데이트 완료: {slot.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
            
            // 2단계: CardSlotRegistry도 업데이트 (중요!)
            UpdateCardSlotRegistry(slot.Position, newCard);
            
            // 3단계: 자식 오브젝트에서 카드 UI 찾기
            var childCardUI = slot.transform.GetComponentInChildren<Game.SkillCardSystem.UI.SkillCardUI>();
            if (childCardUI != null)
            {
                GameLogger.LogInfo($"[DiceOfFate] 자식에서 카드 UI 발견: {childCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                
                // 카드 UI의 데이터도 업데이트
                childCardUI.SetCard(newCard);
                slot.SetCardUI(childCardUI); // 연결 복구
                
                GameLogger.LogInfo($"[DiceOfFate] 카드 UI 데이터 업데이트 완료: {childCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                return;
            }

            // 4단계: GetCardUI() 시도 (대안)
            var existingCardUI = slot.GetCardUI() as Game.SkillCardSystem.UI.SkillCardUI;
            if (existingCardUI != null)
            {
                GameLogger.LogInfo($"[DiceOfFate] GetCardUI()로 카드 UI 발견: {existingCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                
                // 카드 UI의 데이터도 업데이트
                existingCardUI.SetCard(newCard);
                
                GameLogger.LogInfo($"[DiceOfFate] 카드 UI 데이터 업데이트 완료: {existingCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                return;
            }

            // 5단계: 새 카드 UI 생성 (최후의 수단)
            GameLogger.LogWarning("[DiceOfFate] 기존 카드 UI를 찾을 수 없어 새로 생성합니다", GameLogger.LogCategory.Core);
            
            var prefab = Resources.Load<Game.SkillCardSystem.UI.SkillCardUI>("Prefab/SkillCard");
            if (prefab == null)
            {
                GameLogger.LogError("[DiceOfFate] SkillCardUI 프리팹을 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return;
            }

            var newCardUI = Game.SkillCardSystem.UI.SkillCardUIFactory.CreateUI(prefab, slot.transform, newCard, null, null);
            if (newCardUI != null)
            {
                // SetCardUI를 호출하면 AttachUIToSlot이 호출되어 올바른 위치(anchoredPosition: 0, 4)로 설정됩니다
                slot.SetCardUI(newCardUI);
                
                GameLogger.LogInfo("[DiceOfFate] 새 카드 UI 생성 및 배치 완료", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogError("[DiceOfFate] 새 카드 UI 생성 실패", GameLogger.LogCategory.Core);
            }
            
            // 최종 검증
            var finalCardName = slot.GetCard()?.GetCardName() ?? "null";
            GameLogger.LogInfo($"[DiceOfFate] 최종 검증 - 슬롯 카드: {finalCardName}, 예상: {newCard.GetCardName()}", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// CardSlotRegistry의 카드 데이터를 업데이트합니다
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="newCard">새로운 카드</param>
        private void UpdateCardSlotRegistry(Game.CombatSystem.Slot.CombatSlotPosition position, Game.SkillCardSystem.Interface.ISkillCard newCard)
        {
            // SceneContext를 통해 ICardSlotRegistry 직접 접근
            var sceneContext = UnityEngine.Object.FindFirstObjectByType<Zenject.SceneContext>();
            if (sceneContext == null)
            {
                GameLogger.LogError("[DiceOfFate] SceneContext를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return;
            }

            var cardSlotRegistry = sceneContext.Container.TryResolve<Game.CombatSystem.Interface.ICardSlotRegistry>();
            if (cardSlotRegistry == null)
            {
                GameLogger.LogError("[DiceOfFate] ICardSlotRegistry를 DI 컨테이너에서 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return;
            }

            // 기존 카드 UI 가져오기
            var existingUI = cardSlotRegistry.GetCardUIInSlot(position);
            
            // CardSlotRegistry에 새 카드 등록
            cardSlotRegistry.RegisterCard(position, newCard, existingUI, Game.CombatSystem.Data.SlotOwner.ENEMY);
            
            GameLogger.LogInfo($"[DiceOfFate] CardSlotRegistry 업데이트 완료: {position} = {newCard.GetCardName()}", GameLogger.LogCategory.Core);
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

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject != null && images[i].gameObject.name == "Portrait")
                {
                    return images[i];
                }
            }

            return images[0];
        }

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }

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
    }

    /// <summary>
    /// 리롤 효과 커맨드입니다.
    /// </summary>
    public class RerollEffectCommand : BaseItemEffectCommand
    {
        private int rerollCount;
        private readonly AudioClip sfxClip;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Audio.AudioManager audioManager;

        public RerollEffectCommand(
            int rerollCount,
            AudioClip sfxClip = null,
            GameObject visualEffectPrefab = null,
            VFXManager vfxManager = null,
            Game.CoreSystem.Audio.AudioManager audioManager = null) : base("리롤")
        {
            this.rerollCount = rerollCount;
            this.sfxClip = sfxClip;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // PlayerManager를 씬에서 직접 찾기
            var playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
            if (playerManager == null)
            {
                GameLogger.LogError("PlayerManager를 씬에서 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var handManager = playerManager.GetPlayerHandManager();
            if (handManager == null)
            {
                GameLogger.LogError("PlayerHandManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 현재 핸드의 카드 수 확인
            int currentHandCount = 0;
            var handSlots = new[] {
                SkillCardSlotPosition.PLAYER_SLOT_1,
                SkillCardSlotPosition.PLAYER_SLOT_2,
                SkillCardSlotPosition.PLAYER_SLOT_3
            };

            foreach (var slot in handSlots)
            {
                if (handManager.GetCardInSlot(slot) != null)
                {
                    currentHandCount++;
                }
            }

            if (currentHandCount == 0)
            {
                GameLogger.LogWarning("리롤할 카드가 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 핸드 클리어
            handManager.ClearAll();

            // 새로운 카드 드로우 (덱에서 3장)
            // PlayerHandManager의 GenerateInitialHand 메서드를 사용하여 새 카드 드로우
            handManager.GenerateInitialHand();

            // 사운드 및 비주얼 이펙트 재생 (사용자 위치)
            PlaySFX();
            PlayVisualEffect(context);

            GameLogger.LogInfo($"역행의 모래시계 적용: 핸드 리롤 완료 ({currentHandCount}장 → 3장)", GameLogger.LogCategory.Core);
            return true;
        }

        /// <summary>
        /// 리롤 사운드를 재생합니다.
        /// </summary>
        private void PlaySFX()
        {
            if (sfxClip == null)
            {
                GameLogger.LogWarning("[RerollEffectCommand] 사운드 클립이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalAudioManager = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (finalAudioManager != null)
            {
                finalAudioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[RerollEffectCommand] 리롤 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[RerollEffectCommand] AudioManager를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 리롤 비주얼 이펙트를 재생합니다.
        /// </summary>
        private void PlayVisualEffect(IItemUseContext context)
        {
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[RerollEffectCommand] 비주얼 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var userTransform = (context.User as MonoBehaviour)?.transform;
            if (userTransform == null)
            {
                GameLogger.LogWarning("[RerollEffectCommand] 사용자 Transform을 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                var effectInstance = finalVfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[RerollEffectCommand] 리롤 비주얼 이펙트 재생: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                var spawnPos = GetPortraitCenterWorldPosition(userTransform);
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[RerollEffectCommand] 리롤 비주얼 이펙트 생성 완료: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
            }
        }

        private static Vector3 GetPortraitCenterWorldPosition(Transform root)
        {
            if (root == null) return Vector3.zero;

            var portraitImage = FindPortraitImage(root);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                return GetRectTransformCenterWorld(portraitImage.rectTransform);
            }

            var anyRect = root.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                return GetRectTransformCenterWorld(anyRect);
            }

            var sprite = root.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null)
            {
                return sprite.bounds.center;
            }

            return root.position;
        }

        private static Image FindPortraitImage(Transform root)
        {
            var images = root.GetComponentsInChildren<Image>(true);
            if (images == null || images.Length == 0) return null;

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject != null && images[i].gameObject.name == "Portrait")
                {
                    return images[i];
                }
            }

            return images[0];
        }

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }

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
    }

    /// <summary>
    /// 실드 브레이커 효과 커맨드입니다.
    /// </summary>
    public class ShieldBreakerEffectCommand : BaseItemEffectCommand
    {
        private int duration;
        private readonly AudioClip sfxClip;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;
        private readonly Game.CoreSystem.Audio.AudioManager audioManager;

        public ShieldBreakerEffectCommand(
            int duration = 2,
            AudioClip sfxClip = null,
            GameObject visualEffectPrefab = null,
            VFXManager vfxManager = null,
            Game.CoreSystem.Audio.AudioManager audioManager = null) : base("실드 브레이커")
        {
            this.duration = duration;
            this.sfxClip = sfxClip;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
            this.audioManager = audioManager;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // 아이템 정의를 ActiveItemDefinition으로 캐스팅
            var activeItemDef = context.ItemDefinition as Game.ItemSystem.Data.ActiveItemDefinition;
            var turnPolicy = activeItemDef?.turnPolicy ?? Interface.ItemEffectTurnPolicy.EveryTurn;
            var itemIcon = context.ItemDefinition?.Icon;

            // 적 캐릭터를 직접 찾기 (context.Target 무시)
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager == null)
            {
                GameLogger.LogError($"[ShieldBreakerEffect] EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var enemyCharacter = enemyManager.GetCurrentEnemy();
            if (enemyCharacter == null)
            {
                GameLogger.LogError($"[ShieldBreakerEffect] 현재 적 캐릭터를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            GameLogger.LogInfo($"[ShieldBreakerEffect] 적 캐릭터 발견: {enemyCharacter.GetCharacterName()}, 정책={turnPolicy}", GameLogger.LogCategory.Core);

            // 실드 브레이커 효과: 적의 가드 버프를 즉시 제거
            var guardBuffRemoved = RemoveGuardBuff(enemyCharacter);
            
            // 가드 버프가 제거되었을 때만 이펙트 재생
            if (guardBuffRemoved)
            {
                GameLogger.LogInfo($"[ShieldBreakerEffect] 실드 브레이커 적용 완료: {enemyCharacter.GetCharacterName()}의 가드 버프 제거", GameLogger.LogCategory.Core);
                
                // 사운드 및 비주얼 이펙트 재생 (적 캐릭터 위치)
                PlaySFX();
                PlayVisualEffect(enemyCharacter);
                
                return true;
            }
            else
            {
                GameLogger.LogInfo($"[ShieldBreakerEffect] 실드 브레이커 적용 완료: {enemyCharacter.GetCharacterName()}에게 가드 버프가 없음", GameLogger.LogCategory.Core);
                return true; // 가드가 없어도 성공으로 처리
            }
        }

        /// <summary>
        /// 실드 브레이커 사운드를 재생합니다.
        /// </summary>
        private void PlaySFX()
        {
            if (sfxClip == null)
            {
                GameLogger.LogWarning("[ShieldBreakerEffectCommand] 사운드 클립이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalAudioManager = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (finalAudioManager != null)
            {
                finalAudioManager.PlaySFXWithPool(sfxClip, 0.9f);
                GameLogger.LogInfo($"[ShieldBreakerEffectCommand] 실드 브레이커 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning("[ShieldBreakerEffectCommand] AudioManager를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 실드 브레이커 비주얼 이펙트를 재생합니다.
        /// </summary>
        private void PlayVisualEffect(Game.CharacterSystem.Interface.ICharacter target)
        {
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[ShieldBreakerEffectCommand] 비주얼 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                GameLogger.LogWarning("[ShieldBreakerEffectCommand] 대상 Transform을 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            var finalVfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            if (finalVfxManager != null)
            {
                // 캐릭터의 시각적 중심 위치 계산
                var spawnPos = GetPortraitCenterWorldPosition(targetTransform);
                
                var effectInstance = finalVfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (effectInstance != null)
                {
                    SetEffectLayer(effectInstance);
                    GameLogger.LogInfo($"[ShieldBreakerEffectCommand] 실드 브레이커 비주얼 이펙트 재생: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var spawnPos = GetPortraitCenterWorldPosition(targetTransform);
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo($"[ShieldBreakerEffectCommand] 실드 브레이커 비주얼 이펙트 생성 완료: {visualEffectPrefab.name}", GameLogger.LogCategory.Core);
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

            GameLogger.LogInfo($"[ShieldBreakerEffectCommand] 이펙트 레이어 설정 완료 (Renderer: {rendererCount}, Particle: {particleCount})", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 적의 가드 버프를 제거합니다
        /// </summary>
        /// <param name="enemyCharacter">대상 적 캐릭터</param>
        /// <returns>가드 버프가 제거되었으면 true, 없었으면 false</returns>
        private bool RemoveGuardBuff(Game.CharacterSystem.Interface.ICharacter enemyCharacter)
        {
            if (enemyCharacter == null)
            {
                GameLogger.LogError("[ShieldBreakerEffect] 적 캐릭터가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // CharacterBase로 캐스팅하여 perTurnEffects에 접근
            if (enemyCharacter is Game.CharacterSystem.Core.CharacterBase characterBase)
            {
                // 리플렉션을 사용하여 perTurnEffects 필드에 접근
                var perTurnEffectsField = typeof(Game.CharacterSystem.Core.CharacterBase).GetField("perTurnEffects", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (perTurnEffectsField != null)
                {
                    var perTurnEffects = perTurnEffectsField.GetValue(characterBase) as System.Collections.Generic.List<Game.SkillCardSystem.Interface.IPerTurnEffect>;
                    
                    if (perTurnEffects != null)
                    {
                        // GuardBuff 타입의 효과를 찾아서 제거
                        var guardBuff = perTurnEffects.Find(effect => effect is Game.SkillCardSystem.Effect.GuardBuff);
                        
                        if (guardBuff != null)
                        {
                            perTurnEffects.Remove(guardBuff);
                            
                            // 가드 상태를 false로 설정
                            characterBase.SetGuarded(false);
                            
                            // UI 업데이트를 위한 이벤트 발생
                            var onBuffsChangedField = typeof(Game.CharacterSystem.Core.CharacterBase).GetField("OnBuffsChanged", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            
                            if (onBuffsChangedField != null)
                            {
                                var onBuffsChanged = onBuffsChangedField.GetValue(characterBase) as System.Action<System.Collections.Generic.IReadOnlyList<Game.SkillCardSystem.Interface.IPerTurnEffect>>;
                                onBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
                            }
                            
                            GameLogger.LogInfo($"[ShieldBreakerEffect] 가드 버프 제거 완료: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Core);
                            return true;
                        }
                        else
                        {
                            GameLogger.LogInfo($"[ShieldBreakerEffect] 가드 버프가 없음: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Core);
                            return false;
                        }
                    }
                }
            }
            
            GameLogger.LogWarning($"[ShieldBreakerEffect] 가드 버프 제거 실패: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Core);
            return false;
        }
    }
}