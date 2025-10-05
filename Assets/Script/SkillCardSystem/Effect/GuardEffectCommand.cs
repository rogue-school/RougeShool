using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using System;
using UnityEngine.UI;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 효과를 적용하는 커맨드 클래스입니다.
    /// 캐릭터에게 지정된 턴 동안 가드 버프를 적용합니다.
    /// </summary>
    public class GuardEffectCommand : ICardEffectCommand
    {
        private readonly int duration;
        private readonly GameObject visualEffectPrefab;
        private readonly VFXManager vfxManager;

        /// <summary>
        /// 가드 효과 커맨드 생성자
        /// </summary>
        /// <param name="duration">가드 지속 턴 수 (기본값: 1)</param>
        /// <param name="visualEffectPrefab">VFX 프리팹 (선택적)</param>
        /// <param name="vfxManager">VFX 매니저 (선택적)</param>
        public GuardEffectCommand(int duration = 1, GameObject visualEffectPrefab = null, VFXManager vfxManager = null)
        {
            this.duration = duration;
            this.visualEffectPrefab = visualEffectPrefab;
            this.vfxManager = vfxManager;
        }
        
        /// <summary>
        /// 가드 효과를 실행합니다.
        /// 캐릭터에게 지정된 턴 동안 가드 버프를 적용합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="turnManager">전투 턴 매니저</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[GuardEffectCommand] 소스가 null입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            // 소스 캐릭터에게 가드 버프 적용
            if (context.Source is ICharacter character)
            {
                // 가드 아이콘 로드 (ScriptableObject에서)
                Sprite guardIcon = null;
                var guardEffectSO = Resources.Load<Game.SkillCardSystem.Effect.GuardEffectSO>("Data/SkillCard/SkillEffect/GuardEffect");
                if (guardEffectSO != null)
                {
                    guardIcon = guardEffectSO.GetIcon();
                    GameLogger.LogInfo($"[GuardEffectCommand] 가드 아이콘 로드 성공: {guardIcon?.name ?? "null"}", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning("[GuardEffectCommand] GuardEffectSO를 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                }
                
                // 아이콘이 없으면 기본 아이콘 시도
                if (guardIcon == null)
                {
                    guardIcon = Resources.Load<Sprite>("Image/UI (1)/UI/shield_icon");
                    if (guardIcon != null)
                    {
                        GameLogger.LogInfo("[GuardEffectCommand] 대체 가드 아이콘 로드 성공", GameLogger.LogCategory.Combat);
                    }
                    else
                    {
                        GameLogger.LogWarning("[GuardEffectCommand] 대체 가드 아이콘도 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                    }
                }
                
                var guardBuff = new GuardBuff(duration, guardIcon); // 커스텀 지속 시간과 아이콘으로 생성
                character.RegisterPerTurnEffect(guardBuff);
                // 즉시 보호 활성화: 다음 자신의 턴 시작 시 카운트가 0이 되면 해제됨
                character.SetGuarded(true);
                
                GameLogger.LogInfo($"[GuardEffectCommand] {character.GetCharacterName()}에게 가드 버프 적용 ({duration}턴 지속, 아이콘: {guardIcon?.name ?? "없음"})", GameLogger.LogCategory.Combat);

                // 가드 비주얼 이펙트: 시전자 위치에 생성
                TrySpawnEffectAtSource(context);
            }
            else
            {
                GameLogger.LogWarning("[GuardEffectCommand] 소스가 캐릭터가 아닙니다.", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 가드 효과 실행 가능 여부를 확인합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>실행 가능 여부</returns>
        public bool CanExecute(ICardExecutionContext context)
        {
            return context?.Source != null;
        }

        /// <summary>
        /// 가드 효과의 비용을 반환합니다.
        /// </summary>
        /// <returns>비용 (가드 효과는 비용 없음)</returns>
        public int GetCost()
        {
            return 0;
        }

        /// <summary>
        /// 시전자 위치에 가드 비주얼 이펙트를 생성합니다.
        /// </summary>
        private void TrySpawnEffectAtSource(ICardExecutionContext context)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[GuardEffectCommand] 시전자(Source)가 null입니다. 가드 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }
            if (visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[GuardEffectCommand] visualEffectPrefab이 지정되지 않았습니다. 가드 VFX 생성을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 포트레잇 이미지의 정확한 중앙에서 스폰되도록 계산
            var spawnPos = GetPortraitCenterWorldPosition(context.Source.Transform);
            GameLogger.LogInfo($"[GuardEffectCommand] 가드 VFX 생성 시작 - 프리팹: {visualEffectPrefab.name}, 위치: {spawnPos}", GameLogger.LogCategory.SkillCard);

            // VFXManager를 통한 이펙트 생성 (Object Pooling)
            if (vfxManager != null)
            {
                var instance = vfxManager.PlayEffect(visualEffectPrefab, spawnPos);
                if (instance != null)
                {
                    SetEffectLayer(instance);
                    GameLogger.LogInfo($"[GuardEffectCommand] VFXManager로 가드 VFX 재생: {instance.name}", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var instance = UnityEngine.Object.Instantiate(visualEffectPrefab, spawnPos, Quaternion.identity);
                GameLogger.LogInfo($"[GuardEffectCommand] 가드 VFX 인스턴스 생성 완료: {instance.name}", GameLogger.LogCategory.SkillCard);

                SetEffectLayer(instance);

                UnityEngine.Object.Destroy(instance, 2.0f);
                GameLogger.LogInfo("[GuardEffectCommand] 가드 VFX 2초 후 자동 제거 예약", GameLogger.LogCategory.SkillCard);
            }
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

        private static Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }

        // 디자이너가 배치하는 선택적 VFX 앵커를 찾습니다.
        // 우선순위: "VFXAnchor" → "PortraitVFXAnchor"
        private static Transform FindExplicitVfxAnchor(Transform root)
        {
            var a = root.Find("VFXAnchor");
            if (a != null) return a;
            return root.Find("PortraitVFXAnchor");
        }

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

            GameLogger.LogInfo($"[GuardEffectCommand] 가드 VFX 레이어 설정 완료 (Renderer: {rendererCount}, Particle: {particleCount})", GameLogger.LogCategory.SkillCard);
        }
    }
}
