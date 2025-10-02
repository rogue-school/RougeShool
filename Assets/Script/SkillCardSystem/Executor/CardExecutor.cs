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
using Zenject;

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

        /// <summary>
        /// CardExecutor 생성자.
        /// </summary>
        /// <param name="commandFactory">카드 이펙트 커맨드 생성 팩토리</param>
        /// <param name="validator">카드 실행 유효성 검사기</param>
        /// <param name="audioManager">오디오 매니저</param>
        public CardExecutor(
            ICardEffectCommandFactory commandFactory,
            ICardValidator validator,
            IAudioManager audioManager)
        {
            this.commandFactory = commandFactory;
            this.validator = validator;
            this.audioManager = audioManager;
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

            // 카드 비주얼 이펙트 재생
            var vfx = card.CardDefinition?.VisualEffectPrefab;
            if (vfx != null)
            {
                GameLogger.LogInfo($"[CardExecutor] 이펙트 프리팹 발견: {vfx.name}", GameLogger.LogCategory.SkillCard);
                
                // 힐/가드 계열은 시전자(Source) 위치에서 출력하도록 우선 처리
                bool spawnAtSource = false;
                try
                {
                    // 이펙트 SO 타입으로 판단 (HealEffectSO, GuardEffectSO 포함 시 시전자 기준)
                    for (int i = 0; i < effects.Count; i++)
                    {
                        var effectSo = effects[i];
                        if (effectSo is HealEffectSO || effectSo is GuardEffectSO)
                        {
                            spawnAtSource = true;
                            break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogWarning($"[CardExecutor] 이펙트 유형 판별 중 오류: {ex.Message}", GameLogger.LogCategory.SkillCard);
                }

                ICharacter spawnCharacter = spawnAtSource ? context?.Source : context?.Target;
                if (spawnCharacter == null)
                {
                    // 폴백: 소스가 비어 있으면 타겟, 타겟도 없으면 소스
                    spawnCharacter = context?.Target ?? context?.Source;
                }

                if (spawnCharacter != null)
                {
                    Vector3 spawnPos = GetCenterWorldPosition(spawnCharacter.Transform);
                    GameLogger.LogInfo($"[CardExecutor] 이펙트 생성 위치: {spawnPos}", GameLogger.LogCategory.SkillCard);
                    
                    var instance = GameObject.Instantiate(vfx, spawnPos, Quaternion.identity);
                    GameLogger.LogInfo($"[CardExecutor] 이펙트 인스턴스 생성 완료: {instance.name}", GameLogger.LogCategory.SkillCard);
                    
                    // 이펙트가 UI 위에 표시되도록 레이어 설정
                    SetEffectLayer(instance);
                    
                    // 이펙트는 프리팹 자체에서 자동 제거되도록 설정하거나 기본 지속 시간 사용
                    GameObject.Destroy(instance, 2.0f); // 기본 2초 지속
                    GameLogger.LogInfo($"[CardExecutor] 이펙트 2초 후 자동 제거 예약", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    GameLogger.LogWarning($"[CardExecutor] 시전자/대상 모두 null입니다. 이펙트를 생성할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogInfo($"[CardExecutor] '{card.GetCardName()}'에 이펙트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.SkillCard);
            }
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
        /// RectTransform이면 WorldCorners 기반 중앙, 아니면 Transform.position을 사용합니다.
        /// </summary>
        private static Vector3 GetCenterWorldPosition(Transform t)
        {
            if (t == null) return Vector3.zero;
            if (t is RectTransform rt)
            {
                var corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                var centerWorld = (corners[0] + corners[2]) * 0.5f;

                // 이펙트 카메라 기준 z를 일치시켜 투영 오류 방지
                var effectsCam = GetEffectsCamera();
                if (effectsCam == null) return centerWorld;

                // 1) 현재 월드 중심을 이펙트 카메라의 스크린 좌표로 투영
                var screenByEffects = effectsCam.WorldToScreenPoint(centerWorld);
                // 2) 동일 x,y, 동일 z로 역투영 → 해당 카메라의 깊이 평면에 정확히 위치
                return effectsCam.ScreenToWorldPoint(screenByEffects);
            }
            return t.position;
        }

        /// <summary>
        /// 이펙트를 투영할 카메라를 반환합니다. 우선순위: 이름 "EffectsCamera" → Camera.main → 첫 번째 활성 카메라
        /// </summary>
        private static Camera GetEffectsCamera()
        {
            var go = GameObject.Find("EffectsCamera");
            if (go != null && go.TryGetComponent<Camera>(out var camByName)) return camByName;
            if (Camera.main != null) return Camera.main;
            var cams = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            return cams != null && cams.Length > 0 ? cams[0] : null;
        }
    }
}
