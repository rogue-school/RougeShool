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
        private readonly VFXManager vfxManager;

        public HealEffectCommand(int healAmount, int maxHealAmount = 0, GameObject visualEffectPrefab = null, VFXManager vfxManager = null)
        {
            this.healAmount = healAmount;
            this.maxHealAmount = maxHealAmount;
            this.visualEffectPrefab = visualEffectPrefab;
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
            this.visualEffectPrefab = null;
            
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

            var source = context.Source;
            string sourceName = source.GetCharacterName();
            
            // 현재 체력과 최대 체력 확인
            int currentHP = source.GetCurrentHP();
            int maxHP = source.GetMaxHP();
            
            // 이미 최대 체력이면 치유 불가
            if (currentHP >= maxHP)
            {
                GameLogger.LogInfo($"[HealEffectCommand] '{sourceName}' 이미 최대 체력입니다. (현재: {currentHP}/{maxHP})", 
                    GameLogger.LogCategory.SkillCard);
                return;
            }
            
            // 실제 치유량 계산 (최대 체력을 넘지 않도록)
            int actualHealAmount = healAmount;
            if (maxHealAmount > 0)
            {
                actualHealAmount = Mathf.Min(healAmount, maxHealAmount);
            }
            
            // 최대 체력을 넘지 않도록 제한
            int maxPossibleHeal = maxHP - currentHP;
            actualHealAmount = Mathf.Min(actualHealAmount, maxPossibleHeal);
            
            if (actualHealAmount <= 0)
            {
                GameLogger.LogWarning($"[HealEffectCommand] '{sourceName}' 치유량이 0 이하입니다.", 
                    GameLogger.LogCategory.SkillCard);
                return;
            }
            
            // 체력 회복 적용
            source.Heal(actualHealAmount);
            
            int newHP = source.GetCurrentHP();
            
            GameLogger.LogInfo($"[HealEffectCommand] '{sourceName}' 체력 회복 완료 - 치유량: {actualHealAmount}, 체력: {currentHP} → {newHP}/{maxHP}", 
                GameLogger.LogCategory.SkillCard);

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

            // VFXManager를 통한 이펙트 생성 (Object Pooling)
            if (vfxManager != null)
            {
                var instance = vfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (instance != null)
                {
                    SetEffectLayer(instance);
                    GameLogger.LogInfo($"[HealEffectCommand] VFXManager로 힐 VFX 재생: {instance.name}", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                GameLogger.LogInfo($"[HealEffectCommand] 힐 VFX 인스턴스 생성 완료: {instance.name}", GameLogger.LogCategory.SkillCard);

                SetEffectLayer(instance);

                UnityEngine.Object.Destroy(instance, 2.0f);
            }
            GameLogger.LogInfo("[HealEffectCommand] 힐 VFX 2초 후 자동 제거 예약", GameLogger.LogCategory.SkillCard);
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

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }
    }
}
