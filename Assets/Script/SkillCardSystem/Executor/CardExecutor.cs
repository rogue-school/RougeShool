using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Validator;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Effect; // Heal/Guard 식별을 위해 추가
using Game.VFXSystem.Manager;
using Zenject;
using UnityEngine.UI;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 스킬 카드의 이펙트를 실제로 실행하는 클래스입니다.
    /// 유효성 검사 후 커맨드 패턴 기반으로 이펙트를 처리합니다.
    /// </summary>
    public class CardExecutor
    {
        private readonly ICardEffectCommandFactory commandFactory;
        private readonly ICardValidator validator;
        private readonly IAudioManager audioManager;
        private readonly VFXManager vfxManager;

        /// <summary>
        /// CardExecutor 생성자.
        /// </summary>
        /// <param name="commandFactory">카드 이펙트 커맨드 생성 팩토리</param>
        /// <param name="validator">카드 실행 유효성 검사기</param>
        /// <param name="audioManager">오디오 매니저</param>
        /// <param name="vfxManager">VFX 매니저 (선택적)</param>
        public CardExecutor(
            ICardEffectCommandFactory commandFactory,
            ICardValidator validator,
            IAudioManager audioManager,
            VFXManager vfxManager = null)
        {
            this.commandFactory = commandFactory;
            this.validator = validator;
            this.audioManager = audioManager;
            this.vfxManager = vfxManager;
        }

        /// <summary>
        /// 주어진 카드와 컨텍스트를 기반으로 이펙트를 실행합니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <param name="context">실행 컨텍스트 (시전자/대상 등)</param>
        /// <param name="turnManager">전투 턴 매니저 (이펙트가 턴에 영향을 미칠 경우 사용)</param>
        public void Execute(ISkillCard card, ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (!validator.CanExecute(card, context))
            {
                GameLogger.LogWarning($"[CardExecutor] 실행 조건 불충족: {card?.GetCardName() ?? "알 수 없음"}", GameLogger.LogCategory.SkillCard);
                return;
            }

            var effects = card.CreateEffects();
            if (effects == null || effects.Count == 0)
            {
                GameLogger.LogWarning($"[CardExecutor] '{card.GetCardName()}'에 등록된 이펙트가 없습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 카드가 순서 제공을 지원하면 우선 사용 (삭제된 인터페이스)
            // if (card is ICardEffectOrderProvider orderProvider)
            // {
            //     var ordered = orderProvider.GetOrderedEffects();
            //     if (ordered != null && ordered.Count > 0) effects = ordered;
            // }

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                int power = card.GetEffectPower(effect);
                var command = commandFactory.Create(effect, power);
                command?.Execute(context, turnManager);

                GameLogger.LogInfo($"[CardExecutor] {card.GetCardName()} → {effect.GetEffectName()}, power: {power}", GameLogger.LogCategory.SkillCard);
            }

            // 효과 후 카드 사운드 재생
            var clip = card.CardDefinition?.SfxClip;
            if (clip != null)
            {
                audioManager?.PlaySFX(clip);
            }

            // VFX 스폰은 각 EffectCommand가 책임집니다 (CardExecutor는 관여하지 않음)
        }
        
        /// <summary>
        /// 이펙트가 UI 위에 표시되도록 레이어를 설정합니다.
        /// </summary>
        /// <param name="effectInstance">설정할 이펙트 인스턴스</param>
        private void SetEffectLayer(GameObject effectInstance)
        {
            if (effectInstance == null) return;
            
            GameLogger.LogInfo($"[CardExecutor] 이펙트 레이어 설정 시작: {effectInstance.name}", GameLogger.LogCategory.SkillCard);
            
            // 모든 렌더러 컴포넌트에 Effects 레이어 적용
            var renderers = effectInstance.GetComponentsInChildren<Renderer>(true);
            int rendererCount = 0;
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    // Effects 레이어로 설정 (UI보다 위에 표시)
                    renderer.sortingLayerName = "Effects";
                    renderer.sortingOrder = 10; // UI보다 높은 우선순위
                    rendererCount++;
                    GameLogger.LogInfo($"[CardExecutor] 렌더러 레이어 설정: {renderer.name} → Effects/10", GameLogger.LogCategory.SkillCard);
                }
            }
            
            // 파티클 시스템이 있는 경우에도 레이어 설정
            var particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
            int particleCount = 0;
            foreach (var ps in particleSystems)
            {
                if (ps != null)
                {
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        renderer.sortingLayerName = "Effects";
                        renderer.sortingOrder = 10;
                        particleCount++;
                        GameLogger.LogInfo($"[CardExecutor] 파티클 시스템 레이어 설정: {ps.name} → Effects/10", GameLogger.LogCategory.SkillCard);
                    }
                }
            }
            
            GameLogger.LogInfo($"[CardExecutor] 이펙트 레이어 설정 완료: {effectInstance.name} (렌더러: {rendererCount}, 파티클: {particleCount})", GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 주어진 트랜스폼의 시각적 중앙 월드 좌표를 반환합니다.
        /// RectTransform/SpriteRenderer가 있으면 그 시각적 중심을 사용하고,
        /// 없으면 Transform.position을 사용합니다.
        /// </summary>
        private static Vector3 GetCenterWorldPosition(Transform t)
        {
            if (t == null) return Vector3.zero;

            // 0) 우선 정확하게 'Portrait' 라는 이름의 Image를 찾는다
            var portraitImage = FindPortraitImage(t);
            if (portraitImage != null)
            {
                return GetRectTransformCenterInEffectsCamera(portraitImage.rectTransform);
            }

            // 1) 우선 자식들 중 포트레잇 후보를 탐색하여 중앙을 계산
            //    - 이름 힌트("Portrait", "PortraitRoot", "CharacterPortrait")
            //    - 전용 컴포넌트 이름(있다면) "CharacterPortrait"
            //    - SpriteRenderer가 있으면 bounds.center 사용
            //    - RectTransform이 있으면 월드 코너 중앙 사용
            Transform portraitTransform = null;

            // 이름 기반 탐색
            portraitTransform = t.Find("Portrait") ?? t.Find("PortraitRoot") ?? t.Find("CharacterPortrait");

            // 전용 컴포넌트 이름 기반 탐색 (리플렉션 비용을 피하기 위해 GetComponent(string) 대신 TryGetComponent를 순회로 대체)
            if (portraitTransform == null)
            {
                // SpriteRenderer 우선 탐색
                var sprite = t.GetComponentInChildren<SpriteRenderer>(true);
                if (sprite != null)
                {
                    return sprite.bounds.center;
                }
            }

            // RectTransform 우선 탐색
            if (portraitTransform == null)
            {
                var firstRect = t.GetComponentInChildren<RectTransform>(true);
                if (firstRect != null)
                {
                    return GetRectTransformCenterInEffectsCamera(firstRect);
                }
            }

            // 명시적 포트레잇 트랜스폼이 있으면 해당 중앙 반환
            if (portraitTransform != null)
            {
                var portraitRect = portraitTransform as RectTransform;
                if (portraitRect != null)
                {
                    return GetRectTransformCenterInEffectsCamera(portraitRect);
                }

                var childSprite = portraitTransform.GetComponentInChildren<SpriteRenderer>(true);
                if (childSprite != null)
                {
                    return childSprite.bounds.center;
                }

                // 일반 트랜스폼이면 그 위치 사용
                return portraitTransform.position;
            }

            // 2) 현재 트랜스폼이 RectTransform이면 중앙 계산
            if (t is RectTransform rt)
            {
                return GetRectTransformCenterInEffectsCamera(rt);
            }

            // 3) 마지막 폴백: Transform.position
            return t.position;
        }

        /// <summary>
        /// 포트레잇(Image/RectTransform/SpriteRenderer)을 기준으로 시각적 중앙 좌표를 계산합니다.
        /// Guard/Heal 같은 자기 대상 버프 이펙트 정렬에 사용됩니다.
        /// </summary>
        private static Vector3 GetPortraitCenterWorldPosition(Transform root)
        {
            if (root == null) return Vector3.zero;

            // 1) Portrait Image 우선
            var portraitImage = FindPortraitImage(root);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                return GetRectTransformCenterInEffectsCamera(portraitImage.rectTransform);
            }

            // 2) RectTransform 폴백
            var anyRect = root.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                return GetRectTransformCenterInEffectsCamera(anyRect);
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

        /// <summary>
        /// 캐릭터 하위에서 이름이 'Portrait' 인 Image를 우선적으로 찾고,
        /// 없으면 첫 번째 Image를 반환합니다.
        /// </summary>
        private static Image FindPortraitImage(Transform root)
        {
            if (root == null) return null;
            Image[] images = root.GetComponentsInChildren<Image>(true);
            if (images == null || images.Length == 0) return null;

            // 정확한 이름 우선
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject != null && images[i].gameObject.name == "Portrait")
                {
                    return images[i];
                }
            }

            // 폴백: 첫 번째 Image 사용 (대부분 포트레잇이 유일)
            return images[0];
        }

        /// <summary>
        /// RectTransform의 월드 코너 중앙을 이펙트 카메라 깊이에 맞춰 보정하여 반환합니다.
        /// </summary>
        private static Vector3 GetRectTransformCenterInEffectsCamera(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;

            // 시각적 표시 영역 기준의 정확한 중앙 계산 (피벗/앵커/PreserveAspect 무관)
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            Vector3 localCenter = bounds.center; // rt 로컬 좌표의 중앙
            Vector3 centerWorld = rt.TransformPoint(localCenter);

            var effectsCam = GetEffectsCamera();
            if (effectsCam == null) return centerWorld;

            var screenByEffects = effectsCam.WorldToScreenPoint(centerWorld);
            return effectsCam.ScreenToWorldPoint(screenByEffects);
        }

        /// <summary>
        /// 이펙트를 투영할 카메라를 반환합니다. 우선순위: 이름 "EffectsCamera" → Camera.main → 첫 번째 활성 카메라
        /// 이펙트 카메라가 없어도 메인 카메라에서 이펙트가 보이도록 보장합니다.
        /// </summary>
        private static Camera GetEffectsCamera()
        {
            var go = GameObject.Find("EffectsCamera");
            if (go != null && go.TryGetComponent<Camera>(out var camByName)) return camByName;
            
            // 이펙트 카메라가 없으면 메인 카메라 사용 (폴백)
            if (Camera.main != null) 
            {
                // 메인 카메라가 Effects 레이어를 렌더링하도록 보장
                EnsureMainCameraRendersEffects(Camera.main);
                return Camera.main;
            }
            
            var cams = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            if (cams != null && cams.Length > 0)
            {
                // 첫 번째 카메라도 Effects 레이어를 렌더링하도록 보장
                EnsureMainCameraRendersEffects(cams[0]);
                return cams[0];
            }
            
            return null;
        }
        
        /// <summary>
        /// 메인 카메라가 Effects 레이어를 렌더링하도록 보장합니다.
        /// </summary>
        private static void EnsureMainCameraRendersEffects(Camera camera)
        {
            if (camera == null) return;
            
            // Effects 레이어가 포함되도록 Culling Mask 설정
            int effectsLayer = LayerMask.NameToLayer("Effects");
            if (effectsLayer != -1)
            {
                // 현재 Culling Mask에 Effects 레이어 추가
                int currentMask = camera.cullingMask;
                int effectsLayerMask = 1 << effectsLayer;
                
                if ((currentMask & effectsLayerMask) == 0)
                {
                    camera.cullingMask = currentMask | effectsLayerMask;
                    GameLogger.LogInfo($"[CardExecutor] {camera.name}에 Effects 레이어 추가: {camera.cullingMask}", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogWarning("[CardExecutor] Effects 레이어를 찾을 수 없습니다. 모든 레이어를 렌더링합니다.", GameLogger.LogCategory.SkillCard);
                camera.cullingMask = -1; // 모든 레이어 렌더링
            }
        }
    }
}
