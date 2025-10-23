using UnityEngine;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Pool;
using Zenject;

namespace Game.VFXSystem.Manager
{
    /// <summary>
    /// VFX(Visual Effects) 관리자
    /// 이펙트와 데미지 텍스트를 Object Pooling으로 관리합니다.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        #region 설정

        [Header("VFX 풀 설정")]
        [Tooltip("데미지 텍스트 풀")]
        [SerializeField] private DamageTextPool damageTextPool;

        [Tooltip("버프 아이콘 풀")]
        [SerializeField] private BuffIconPool buffIconPool;

        [Tooltip("스킬 카드 UI 풀")]
        [SerializeField] private SkillCardUIPool skillCardUIPool;

        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true; // 기본값을 true로 변경하여 디버깅 용이하게 함

        #endregion

        #region 이펙트 관리

        private Transform effectContainer;

        [Header("이펙트 설정")]
        [Tooltip("이펙트 기본 지속 시간 (초)")]
        [SerializeField] private float defaultEffectDuration = 2f;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // 이펙트 컨테이너 생성
            effectContainer = new GameObject("VFX_EffectContainer").transform;
            effectContainer.SetParent(transform);

            // 메인 카메라가 Effects 레이어를 렌더링하는지 확인하고 설정
            EnsureMainCameraRendersEffects();

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("VFXManager 초기화 완료 (간단 모드)", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 메인 카메라가 Effects 레이어를 렌더링하도록 설정합니다.
        /// </summary>
        private void EnsureMainCameraRendersEffects()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }

            if (mainCamera == null)
            {
                GameLogger.LogWarning("[VFXManager] 메인 카메라를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                return;
            }

            // Effects 레이어가 포함되도록 Culling Mask 설정
            int effectsLayer = LayerMask.NameToLayer("Effects");
            if (effectsLayer != -1)
            {
                // 현재 Culling Mask에 Effects 레이어 추가
                int currentMask = mainCamera.cullingMask;
                int effectsLayerMask = 1 << effectsLayer;
                
                if ((currentMask & effectsLayerMask) == 0)
                {
                    mainCamera.cullingMask = currentMask | effectsLayerMask;
                    GameLogger.LogInfo($"[VFXManager] {mainCamera.name}에 Effects 레이어 추가: {mainCamera.cullingMask}", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogInfo($"[VFXManager] {mainCamera.name}가 이미 Effects 레이어를 렌더링 중: {mainCamera.cullingMask}", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogWarning("[VFXManager] Effects 레이어를 찾을 수 없습니다. 모든 레이어를 렌더링합니다.", GameLogger.LogCategory.Combat);
                mainCamera.cullingMask = -1; // 모든 레이어 렌더링
            }
        }

        #endregion

        #region 데미지 텍스트

        /// <summary>
        /// 데미지 텍스트를 표시합니다.
        /// </summary>
        /// <param name="damageAmount">데미지 양</param>
        /// <param name="position">표시 위치</param>
        /// <param name="parent">부모 Transform (선택적)</param>
        public void ShowDamageText(int damageAmount, Vector3 position, Transform parent = null)
        {
            if (damageTextPool == null)
            {
                GameLogger.LogWarning("DamageTextPool이 설정되지 않았습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            GameObject damageText = damageTextPool.Get(position, parent);
            if (damageText != null)
            {
                // DamageText 컴포넌트 설정 (존재하는 경우)
                var textComponent = damageText.GetComponent<TMPro.TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = damageAmount.ToString();
                }

                if (enableDebugLogging)
                {
                    GameLogger.LogInfo($"데미지 텍스트 표시: {damageAmount}", GameLogger.LogCategory.Combat);
                }

                // 자동 반환 (애니메이션 완료 후)
                StartCoroutine(ReturnDamageTextAfterDelay(damageText, 1.5f));
            }
        }

        private System.Collections.IEnumerator ReturnDamageTextAfterDelay(GameObject damageText, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (damageTextPool != null)
            {
                damageTextPool.Return(damageText);
            }
        }

        #endregion

        #region 이펙트

        /// <summary>
        /// 이펙트를 재생합니다. (간단 모드 - 풀링 없음)
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="position">재생 위치</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <param name="parent">부모 Transform (선택적)</param>
        public GameObject PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
        {
            GameLogger.LogInfo($"[VFXManager] 이펙트 재생: {effectPrefab?.name ?? "null"}, 위치: {position}", GameLogger.LogCategory.Combat);
            
            if (effectPrefab == null)
            {
                GameLogger.LogWarning("[VFXManager] 이펙트 프리팹이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            // 이펙트 인스턴스 생성
            GameObject effect = Instantiate(effectPrefab, position, rotation ?? Quaternion.identity);
            
            // 이펙트 레이어를 Effects로 설정
            int effectsLayer = LayerMask.NameToLayer("Effects");
            if (effectsLayer != -1)
            {
                effect.layer = effectsLayer;
                SetLayerRecursively(effect, effectsLayer);
                GameLogger.LogInfo($"[VFXManager] 이펙트 레이어를 Effects로 설정: {effectsLayer}", GameLogger.LogCategory.Combat);
            }

            // 부모 설정
            Transform targetParent = parent ?? effectContainer;
            effect.transform.SetParent(targetParent);
            
            GameLogger.LogInfo($"[VFXManager] 이펙트 생성 완료: {effect.name}, 위치: {effect.transform.position}", GameLogger.LogCategory.Combat);

            // 파티클 시스템 재생
            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            // 이펙트 지속 시간 후 자동 제거
            float duration = GetEffectDuration(effect);
            StartCoroutine(DestroyEffectAfterDelay(effect, duration));

            return effect;
        }

        /// <summary>
        /// 캐릭터의 시각적 중심에서 이펙트를 재생합니다.
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="characterTransform">캐릭터 Transform</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <param name="parent">부모 Transform (선택적)</param>
        public GameObject PlayEffectAtCharacterCenter(GameObject effectPrefab, Transform characterTransform, Quaternion? rotation = null, Transform parent = null)
        {
            if (characterTransform == null)
            {
                GameLogger.LogWarning("[VFXManager] 캐릭터 Transform이 null입니다.", GameLogger.LogCategory.Combat);
                return PlayEffect(effectPrefab, Vector3.zero, rotation, parent);
            }

            // 캐릭터의 시각적 중심 위치 계산
            Vector3 centerPosition = GetCharacterVisualCenter(characterTransform);
            
            GameLogger.LogInfo($"[VFXManager] 캐릭터 중심 이펙트 재생: {effectPrefab?.name}, 캐릭터: {characterTransform.name}, 중심 위치: {centerPosition}", GameLogger.LogCategory.Combat);
            
            return PlayEffect(effectPrefab, centerPosition, rotation, parent);
        }

        /// <summary>
        /// 이펙트를 지속 시간 후 제거합니다.
        /// </summary>
        /// <param name="effect">제거할 이펙트</param>
        /// <param name="delay">지연 시간</param>
        private System.Collections.IEnumerator DestroyEffectAfterDelay(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (effect != null)
            {
                GameLogger.LogInfo($"[VFXManager] 이펙트 자동 제거: {effect.name}", GameLogger.LogCategory.Combat);
                Destroy(effect);
            }
        }

        /// <summary>
        /// 오브젝트와 모든 자식 오브젝트의 레이어를 설정합니다.
        /// </summary>
        /// <param name="obj">대상 오브젝트</param>
        /// <param name="layer">설정할 레이어</param>
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// 캐릭터의 시각적 중심 위치를 계산합니다.
        /// </summary>
        /// <param name="characterTransform">캐릭터 Transform</param>
        /// <returns>시각적 중심 월드 좌표</returns>
        private Vector3 GetCharacterVisualCenter(Transform characterTransform)
        {
            if (characterTransform == null) return Vector3.zero;

            // 1) 명시적 VFX 앵커 우선 (디자이너가 설정한 정확한 위치)
            var explicitAnchor = FindExplicitVfxAnchor(characterTransform);
            if (explicitAnchor != null)
            {
                GameLogger.LogInfo($"[VFXManager] 명시적 VFX 앵커 사용: {explicitAnchor.name}", GameLogger.LogCategory.Combat);
                return explicitAnchor.position;
            }

            // 2) Portrait Image 우선 (UI 캐릭터의 경우)
            var portraitImage = FindPortraitImage(characterTransform);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                Vector3 centerPos = GetRectTransformCenterWorld(portraitImage.rectTransform);
                GameLogger.LogInfo($"[VFXManager] Portrait Image 중심 사용: {centerPos}", GameLogger.LogCategory.Combat);
                return centerPos;
            }

            // 3) RectTransform 폴백
            var anyRect = characterTransform.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                Vector3 centerPos = GetRectTransformCenterWorld(anyRect);
                GameLogger.LogInfo($"[VFXManager] RectTransform 중심 사용: {centerPos}", GameLogger.LogCategory.Combat);
                return centerPos;
            }

            // 4) SpriteRenderer 폴백 (2D 스프라이트 캐릭터)
            var sprite = characterTransform.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null)
            {
                Vector3 centerPos = sprite.bounds.center;
                GameLogger.LogInfo($"[VFXManager] SpriteRenderer 중심 사용: {centerPos}", GameLogger.LogCategory.Combat);
                return centerPos;
            }

            // 5) 최종 폴백: Transform.position
            GameLogger.LogInfo($"[VFXManager] Transform.position 폴백 사용: {characterTransform.position}", GameLogger.LogCategory.Combat);
            return characterTransform.position;
        }

        /// <summary>
        /// 명시적 VFX 앵커를 찾습니다.
        /// </summary>
        /// <param name="root">루트 Transform</param>
        /// <returns>VFX 앵커 Transform</returns>
        private Transform FindExplicitVfxAnchor(Transform root)
        {
            var anchor = root.Find("VFXAnchor");
            if (anchor != null) return anchor;
            return root.Find("PortraitVFXAnchor");
        }

        /// <summary>
        /// Portrait Image를 찾습니다.
        /// </summary>
        /// <param name="root">루트 Transform</param>
        /// <returns>Portrait Image</returns>
        private UnityEngine.UI.Image FindPortraitImage(Transform root)
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

        /// <summary>
        /// RectTransform의 중심을 월드 좌표로 계산합니다.
        /// </summary>
        /// <param name="rt">RectTransform</param>
        /// <returns>월드 중심 좌표</returns>
        private Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, rt);
            var localCenter = bounds.center;
            return rt.TransformPoint(localCenter);
        }


        private float GetEffectDuration(GameObject effect)
        {
            float maxDuration = 0f;

            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                float duration = main.duration + main.startLifetime.constantMax;
                
                // 파티클이 루프인 경우 최대 지속 시간 제한
                if (main.loop)
                {
                    duration = Mathf.Min(duration, 3f); // 루프 파티클은 최대 3초
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

            return maxDuration > 0 ? maxDuration : defaultEffectDuration;
        }

        #endregion

        #region 버프 아이콘

        /// <summary>
        /// 버프/디버프 아이콘을 생성합니다.
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <returns>아이콘 GameObject</returns>
        public GameObject GetBuffIcon(Transform parent)
        {
            if (buffIconPool == null)
            {
                GameLogger.LogWarning("BuffIconPool이 설정되지 않았습니다.", GameLogger.LogCategory.UI);
                return null;
            }

            GameObject icon = buffIconPool.Get(parent);
            if (icon != null && enableDebugLogging)
            {
                GameLogger.LogInfo("버프 아이콘 생성", GameLogger.LogCategory.UI);
            }

            return icon;
        }

        /// <summary>
        /// 버프/디버프 아이콘을 풀에 반환합니다.
        /// </summary>
        /// <param name="icon">반환할 아이콘</param>
        public void ReturnBuffIcon(GameObject icon)
        {
            if (buffIconPool != null)
            {
                buffIconPool.Return(icon);
            }
        }

        #endregion

        #region 스킬 카드 UI

        /// <summary>
        /// 스킬 카드 UI를 풀에서 가져옵니다.
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <returns>스킬 카드 UI</returns>
        public Game.SkillCardSystem.UI.SkillCardUI GetSkillCardUI(Transform parent = null)
        {
            if (skillCardUIPool == null)
            {
                GameLogger.LogWarning("SkillCardUIPool이 설정되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                return null;
            }

            var cardUI = skillCardUIPool.Get(parent);
            if (cardUI != null && enableDebugLogging)
            {
                GameLogger.LogInfo("스킬 카드 UI 생성", GameLogger.LogCategory.SkillCard);
            }

            return cardUI;
        }

        /// <summary>
        /// 스킬 카드 UI를 풀에 반환합니다.
        /// </summary>
        /// <param name="cardUI">반환할 카드 UI</param>
        public void ReturnSkillCardUI(Game.SkillCardSystem.UI.SkillCardUI cardUI)
        {
            if (skillCardUIPool != null)
            {
                skillCardUIPool.Return(cardUI);
            }
        }

        #endregion

        #region 정리

        /// <summary>
        /// 모든 활성 이펙트를 즉시 제거합니다. (간단 모드)
        /// </summary>
        public void ClearAllEffects()
        {
            // 이펙트 컨테이너의 모든 자식 이펙트 제거
            if (effectContainer != null)
            {
                int childCount = effectContainer.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    Transform child = effectContainer.GetChild(i);
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            // 다른 풀들도 정리
            if (damageTextPool != null)
            {
                damageTextPool.ReturnAll();
            }

            if (buffIconPool != null)
            {
                buffIconPool.ReturnAll();
            }

            if (skillCardUIPool != null)
            {
                skillCardUIPool.ReturnAll();
            }

            GameLogger.LogInfo("모든 VFX 정리 완료 (간단 모드)", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 디버그

        [ContextMenu("VFX 상태 출력")]
        private void LogVFXStatus()
        {
            // 이펙트 컨테이너의 자식 개수 확인
            int activeEffectCount = effectContainer != null ? effectContainer.childCount : 0;
            GameLogger.LogInfo($"[VFXManager] 활성 이펙트: {activeEffectCount}개 (간단 모드)", GameLogger.LogCategory.Combat);

            if (damageTextPool != null)
            {
                GameLogger.LogInfo($"[DamageTextPool] 사용 가능: {damageTextPool.AvailableCount}, 활성화: {damageTextPool.ActiveCount}", GameLogger.LogCategory.Combat);
            }

            if (buffIconPool != null)
            {
                GameLogger.LogInfo($"[BuffIconPool] 사용 가능: {buffIconPool.AvailableCount}, 활성화: {buffIconPool.ActiveCount}", GameLogger.LogCategory.UI);
            }

            if (skillCardUIPool != null)
            {
                GameLogger.LogInfo($"[SkillCardUIPool] 사용 가능: {skillCardUIPool.AvailableCount}, 활성화: {skillCardUIPool.ActiveCount}", GameLogger.LogCategory.SkillCard);
            }
        }

        #endregion
    }
}
