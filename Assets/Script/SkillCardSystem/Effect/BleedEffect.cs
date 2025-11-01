using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.VFXSystem.Manager;
using Game.CoreSystem.Utility;
using UnityEngine.UI;

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
        private readonly VFXManager vfxManager;

        /// <summary>
        /// 출혈 효과 생성자
        /// </summary>
        /// <param name="amount">매 턴 입힐 피해량</param>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">출혈 효과 아이콘</param>
        /// <param name="perTurnEffectPrefab">출혈 피해 발생 시 매 턴 재생할 이펙트 프리팹</param>
        /// <param name="vfxManager">VFX 매니저 (선택적)</param>
        public BleedEffect(int amount, int duration, Sprite icon = null, GameObject perTurnEffectPrefab = null, VFXManager vfxManager = null)
        {
            this.amount = amount;
            this.remainingTurns = duration;
            this.icon = icon;
            this.perTurnEffectPrefab = perTurnEffectPrefab;
            this.vfxManager = vfxManager ?? UnityEngine.Object.FindFirstObjectByType<Game.VFXSystem.Manager.VFXManager>();
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
            
            // 출혈 피해 발생 시 이펙트 재생
            TrySpawnPerTurnEffect(target);
        }

        /// <summary>
        /// 매 턴 출혈 피해 발생 시 이펙트를 재생합니다.
        /// </summary>
        private void TrySpawnPerTurnEffect(ICharacter target)
        {
            if (perTurnEffectPrefab == null)
            {
                return;
            }

            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                GameLogger.LogWarning("[BleedEffect] 대상 Transform이 null입니다. 출혈 턴별 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 캐릭터의 시각적 중심 위치 계산
            var spawnPos = GetPortraitCenterWorldPosition(targetTransform);

            // VFXManager를 통한 이펙트 생성
            if (vfxManager != null)
            {
                var instance = vfxManager.PlayEffect(perTurnEffectPrefab, spawnPos);
                if (instance != null)
                {
                    SetEffectLayer(instance);
                    GameLogger.LogInfo($"[BleedEffect] 턴별 출혈 VFX 재생: {instance.name}", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var instance = UnityEngine.Object.Instantiate(perTurnEffectPrefab, spawnPos, Quaternion.identity);
                SetEffectLayer(instance);
                UnityEngine.Object.Destroy(instance, 2.0f);
            }
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
    }
}
