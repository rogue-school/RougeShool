using UnityEngine;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Pool;
using Game.VFXSystem.Component;
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

        [Header("이펙트 설정")]
        [Tooltip("이펙트 기본 지속 시간 (초)")]
        [SerializeField] private float defaultEffectDuration = 2f;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // 메인 카메라가 Effects 레이어를 렌더링하는지 확인하고 설정
            EnsureMainCameraRendersEffects();

            if (enableDebugLogging)
            {
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
                // Fallback으로 FindFirstObjectByType 사용 (Camera.main이 null인 경우 대비)
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

        // 각 부모 Transform별로 활성화된 데미지 텍스트 리스트 관리 (메이플스토리 스타일 쌓기)
        private Dictionary<Transform, List<GameObject>> activeDamageTextsByParent = new Dictionary<Transform, List<GameObject>>();

        [Header("데미지 텍스트 쌓기 설정")]
        [Tooltip("데미지 텍스트 간 수직 간격 (픽셀)")]
        [SerializeField] private float damageTextSpacing = 30f;

        /// <summary>
        /// 데미지 텍스트를 표시합니다.
        /// </summary>
        /// <param name="damageAmount">데미지 양</param>
        /// <param name="position">표시 위치</param>
        /// <param name="parent">부모 Transform (선택적)</param>
        /// <param name="yOffset">Y 좌표 오프셋 (다단 히트용, 기본값: 0)</param>
        /// <returns>데미지 텍스트 표시 성공 여부</returns>
        public bool ShowDamageText(int damageAmount, Vector3 position, Transform parent = null, float yOffset = 0f)
        {
            if (damageTextPool == null)
            {
                GameLogger.LogWarning("DamageTextPool이 설정되지 않았습니다.", GameLogger.LogCategory.Combat);
                return false;
            }

            // 기존 텍스트 개수 확인 (새 텍스트를 위에 배치하기 위해)
            int existingTextCount = 0;
            if (parent != null && activeDamageTextsByParent.ContainsKey(parent))
            {
                var activeTexts = activeDamageTextsByParent[parent];
                if (activeTexts != null)
                {
                    // 활성화된 텍스트 개수 계산
                    foreach (var textObj in activeTexts)
                    {
                        if (textObj != null && textObj.activeInHierarchy)
                        {
                            existingTextCount++;
                        }
                    }
                }
            }

            // Y 오프셋 적용
            Vector3 finalPosition = position;
            if (yOffset != 0f)
            {
                finalPosition = new Vector3(position.x, position.y + yOffset, position.z);
            }

            GameObject damageText = damageTextPool.Get(finalPosition, parent);
            if (damageText != null)
            {
                // DamageTextUI 컴포넌트 찾기
                var damageTextUI = damageText.GetComponent<Game.CombatSystem.UI.DamageTextUI>();
                
                // 기존 텍스트 개수만큼 위에 배치하기 위한 초기 Y 오프셋 계산
                // 메이플스토리 스타일: 새 텍스트가 기존 텍스트 위에 생성됨
                float initialYOffset = existingTextCount * damageTextSpacing;
                
                if (damageTextUI != null)
                {
                    // Show() 메서드 호출 (초기 Y 오프셋 전달, 양수값으로 위에 배치)
                    damageTextUI.Show(damageAmount, Color.red, "-", initialYOffset);
                }
                else
                {
                    // DamageTextUI가 없는 경우 텍스트 컴포넌트에 직접 설정
                var textComponent = damageText.GetComponent<TMPro.TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = damageAmount.ToString();
                    }
                }

                // 부모가 있는 경우 새 텍스트 등록
                if (parent != null)
                {
                    RegisterDamageTextInternal(damageText, parent);
                }

                // 자동 반환 (애니메이션 완료 후)
                StartCoroutine(ReturnDamageTextAfterDelay(damageText, 1.5f));
                return true;
            }

            return false;
        }


        /// <summary>
        /// 데미지 텍스트를 등록합니다 (내부용)
        /// </summary>
        private void RegisterDamageTextInternal(GameObject damageText, Transform parent)
        {
            if (parent == null) return;

            if (!activeDamageTextsByParent.ContainsKey(parent))
            {
                activeDamageTextsByParent[parent] = new List<GameObject>();
            }

            activeDamageTextsByParent[parent].Add(damageText);
        }

        /// <summary>
        /// 데미지 텍스트를 등록 해제합니다
        /// </summary>
        private void UnregisterDamageText(GameObject damageText, Transform parent)
        {
            if (parent == null) return;
            if (!activeDamageTextsByParent.ContainsKey(parent)) return;

            activeDamageTextsByParent[parent].Remove(damageText);

            // 리스트가 비어있으면 Dictionary에서 제거
            if (activeDamageTextsByParent[parent].Count == 0)
            {
                activeDamageTextsByParent.Remove(parent);
            }
        }

        /// <summary>
        /// 외부에서 생성된 데미지 텍스트를 등록합니다 (fallback 사용 시)
        /// Show()가 이미 호출되어 올바른 위치에 배치되었으므로 그냥 등록만 함
        /// </summary>
        /// <param name="damageText">데미지 텍스트 GameObject</param>
        /// <param name="parent">부모 Transform</param>
        public void RegisterDamageText(GameObject damageText, Transform parent)
        {
            if (damageText == null || parent == null) return;
            
            // 새 데미지 텍스트 등록 (Show()에서 이미 올바른 위치로 설정됨)
            RegisterDamageTextInternal(damageText, parent);
        }

        /// <summary>
        /// 기존 텍스트 개수를 반환합니다 (fallback 경로에서 사용)
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <returns>기존 텍스트 개수</returns>
        public int GetExistingTextCount(Transform parent)
        {
            if (parent == null) return 0;
            if (!activeDamageTextsByParent.ContainsKey(parent)) return 0;

            var activeTexts = activeDamageTextsByParent[parent];
            if (activeTexts == null) return 0;

            int count = 0;
            foreach (var textObj in activeTexts)
            {
                if (textObj != null && textObj.activeInHierarchy)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 데미지 텍스트 간격을 반환합니다 (fallback 경로에서 사용)
        /// </summary>
        /// <returns>데미지 텍스트 간격</returns>
        public float GetDamageTextSpacing()
        {
            return damageTextSpacing;
        }

        private System.Collections.IEnumerator ReturnDamageTextAfterDelay(GameObject damageText, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (damageTextPool != null && damageText != null)
            {
                // 부모 Transform 찾기
                Transform parent = damageText.transform.parent;
                
                // 등록 해제
                UnregisterDamageText(damageText, parent);
                
                // 풀에 반환
                damageTextPool.Return(damageText);
            }
        }

        #endregion

        #region 이펙트

        /// <summary>
        /// 이펙트를 재생합니다 (간단 모드 - 풀링 없음)
        /// 부모 Transform이 필수입니다. 이펙트는 부모의 자식으로 생성됩니다
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="position">재생 위치</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <param name="parent">부모 Transform (필수)</param>
        /// <returns>생성된 이펙트 GameObject, 실패 시 null</returns>
        public GameObject PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
        {
            if (effectPrefab == null)
            {
                GameLogger.LogWarning("[VFXManager] 이펙트 프리팹이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            if (parent == null)
            {
                GameLogger.LogWarning("[VFXManager] 부모 Transform이 null입니다. 이펙트를 생성할 수 없습니다.", GameLogger.LogCategory.Combat);
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
            }

            // 부모 설정 (월드 위치 유지)
            effect.transform.SetParent(parent, worldPositionStays: true);
            
            // 부모 설정 후에도 위치가 올바른지 확인 및 재설정
            if (effect.transform.position != position)
            {
                effect.transform.position = position;
                GameLogger.LogWarning($"[VFXManager] 이펙트 위치 보정: {effect.transform.position} → {position}", GameLogger.LogCategory.Combat);
            }
            
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
        /// 이펙트를 감싸는 부모 GameObject를 생성하고, 그 부모를 캐릭터의 자식으로 설정합니다.
        /// 이펙트 프리팹의 Transform은 원본 그대로 유지됩니다.
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="characterTransform">캐릭터 Transform</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <returns>생성된 이펙트 GameObject</returns>
        public GameObject PlayEffectAtCharacterCenter(GameObject effectPrefab, Transform characterTransform, Quaternion? rotation = null)
        {
            if (effectPrefab == null)
            {
                GameLogger.LogWarning("[VFXManager] 이펙트 프리팹이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            if (characterTransform == null)
            {
                GameLogger.LogWarning("[VFXManager] 캐릭터 Transform이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            // 캐릭터의 시각적 중심 위치 계산
            Vector3 centerPosition = GetCharacterVisualCenter(characterTransform);
            // 이펙트를 감싸는 부모 GameObject 생성
            GameObject effectParent = new GameObject($"{effectPrefab.name}_Parent");
            
            // 부모 GameObject를 캐릭터의 자식으로 설정 (월드 위치 유지)
            effectParent.transform.SetParent(characterTransform, worldPositionStays: true);
            effectParent.transform.position = centerPosition;
            
            // 부모가 활성화되어 있는지 확인
            if (!effectParent.activeSelf)
            {
                effectParent.SetActive(true);
            }
            
            // 이펙트 인스턴스 생성 (프리팹의 원본 Transform 그대로 유지)
            GameObject effect = Instantiate(effectPrefab);
            
            // 이펙트가 활성화되어 있는지 확인
            if (!effect.activeSelf)
            {
                effect.SetActive(true);
            }
            
            // 이펙트를 부모 GameObject의 자식으로 설정 (프리팹의 원본 로컬 Transform 유지)
            effect.transform.SetParent(effectParent.transform, worldPositionStays: false);
            
            // 회전이 지정된 경우에만 적용
            if (rotation.HasValue)
            {
                effect.transform.localRotation = rotation.Value;
            }
            
            // 이펙트 레이어를 Effects로 설정
            int effectsLayer = LayerMask.NameToLayer("Effects");
            if (effectsLayer != -1)
            {
                effect.layer = effectsLayer;
                SetLayerRecursively(effect, effectsLayer);
            }

            // 파티클 시스템 재생
            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            // 이펙트 지속 시간 후 자동 제거 (부모도 함께 제거)
            float duration = GetEffectDuration(effect);
            StartCoroutine(DestroyEffectAfterDelay(effectParent, duration));

            return effect;
        }

        /// <summary>
        /// 지정된 Transform의 위치에서 이펙트를 재생합니다.
        /// 이펙트를 감싸는 부모 GameObject를 생성하고, 그 부모를 대상 Transform의 자식으로 설정합니다.
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="targetTransform">대상 Transform</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <returns>생성된 이펙트 GameObject</returns>
        public GameObject PlayEffectAtTransform(GameObject effectPrefab, Transform targetTransform, Quaternion? rotation = null)
        {
            if (effectPrefab == null)
            {
                GameLogger.LogWarning("[VFXManager] 이펙트 프리팹이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            if (targetTransform == null)
            {
                GameLogger.LogWarning("[VFXManager] 대상 Transform이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            Vector3 position = targetTransform.position;
            
            // 이펙트를 감싸는 부모 GameObject 생성
            GameObject effectParent = new GameObject($"{effectPrefab.name}_Parent");
            
            // 부모 GameObject를 먼저 대상 Transform의 자식으로 설정 (월드 위치 유지)
            effectParent.transform.SetParent(targetTransform, worldPositionStays: true);
            effectParent.transform.position = position;
            
            // 부모가 활성화되어 있는지 확인
            if (!effectParent.activeSelf)
            {
                effectParent.SetActive(true);
            }
            
            // 이펙트 인스턴스 생성 (프리팹의 원본 Transform 그대로 유지)
            GameObject effect = Instantiate(effectPrefab);
            
            // 이펙트가 활성화되어 있는지 확인
            if (!effect.activeSelf)
            {
                effect.SetActive(true);
            }
            
            // 이펙트를 부모 GameObject의 자식으로 설정 (프리팹의 원본 로컬 Transform 유지)
            effect.transform.SetParent(effectParent.transform, worldPositionStays: false);
            
            // 회전이 지정된 경우에만 적용
            if (rotation.HasValue)
            {
                effect.transform.localRotation = rotation.Value;
            }
            
            // 이펙트 레이어를 Effects로 설정
            int effectsLayer = LayerMask.NameToLayer("Effects");
            if (effectsLayer != -1)
            {
                effect.layer = effectsLayer;
                SetLayerRecursively(effect, effectsLayer);
            }

            // 파티클 시스템 재생
            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            // 이펙트 지속 시간 후 자동 제거 (부모도 함께 제거)
            float duration = GetEffectDuration(effect);
            StartCoroutine(DestroyEffectAfterDelay(effectParent, duration));

            return effect;
        }

        /// <summary>
        /// 캐릭터 Transform에서 RectTransform을 찾아 이펙트를 재생합니다.
        /// 이펙트 프리팹의 Transform은 원본 그대로 유지되며, 부모 RectTransform의 중심에 배치됩니다.
        /// 이펙트를 감싸는 부모 GameObject를 생성하고, 그 부모를 대상 RectTransform의 자식으로 설정합니다.
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="characterTransform">캐릭터 Transform</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <returns>생성된 이펙트 GameObject</returns>
        public GameObject PlayEffectAtCharacterRectTransformCenter(GameObject effectPrefab, Transform characterTransform, Quaternion? rotation = null)
        {
            if (effectPrefab == null)
            {
                GameLogger.LogWarning("[VFXManager] 이펙트 프리팹이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            if (characterTransform == null)
            {
                GameLogger.LogWarning("[VFXManager] 캐릭터 Transform이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            // 캐릭터 Transform에서 RectTransform 찾기
            RectTransform targetRectTransform = FindCharacterRectTransform(characterTransform);
            if (targetRectTransform == null)
            {
                GameLogger.LogWarning($"[VFXManager] 캐릭터 Transform에서 RectTransform을 찾을 수 없습니다: {characterTransform.name}. 월드 좌표로 생성합니다.", GameLogger.LogCategory.Combat);
                return PlayEffectAtCharacterCenter(effectPrefab, characterTransform, rotation);
            }

            return PlayEffectAtRectTransformCenter(effectPrefab, targetRectTransform, rotation);
        }

        /// <summary>
        /// RectTransform의 중심에 이펙트를 재생하고, 이펙트를 부모의 자식으로 생성합니다.
        /// 이펙트 프리팹의 Transform은 원본 그대로 유지되며, 부모 RectTransform의 중심에 배치됩니다.
        /// 이펙트를 감싸는 부모 GameObject를 생성하고, 그 부모를 대상 RectTransform의 자식으로 설정합니다.
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="parentRectTransform">부모 RectTransform (이펙트가 자식으로 생성됨)</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <returns>생성된 이펙트 GameObject</returns>
        public GameObject PlayEffectAtRectTransformCenter(GameObject effectPrefab, RectTransform parentRectTransform, Quaternion? rotation = null)
        {
            if (effectPrefab == null)
            {
                GameLogger.LogWarning("[VFXManager] 이펙트 프리팹이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            if (parentRectTransform == null)
            {
                GameLogger.LogWarning("[VFXManager] 부모 RectTransform이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            // 부모 RectTransform의 중심 위치 계산
            Vector3 centerPosition = GetRectTransformCenterWorld(parentRectTransform);

            // 이펙트를 감싸는 부모 GameObject 생성
            GameObject effectParent = new GameObject($"{effectPrefab.name}_Parent");
            RectTransform effectParentRect = effectParent.AddComponent<RectTransform>();
            
            // 부모 RectTransform의 자식으로 설정
            effectParentRect.SetParent(parentRectTransform, false);
            
            // 부모 RectTransform의 중심에 배치 (로컬 좌표 기준)
            effectParentRect.anchorMin = new Vector2(0.5f, 0.5f);
            effectParentRect.anchorMax = new Vector2(0.5f, 0.5f);
            effectParentRect.pivot = new Vector2(0.5f, 0.5f);
            effectParentRect.anchoredPosition = Vector2.zero;
            effectParentRect.sizeDelta = Vector2.zero;

            // 이펙트 인스턴스 생성 (원본 Transform 유지)
            GameObject effect = Instantiate(effectPrefab, centerPosition, rotation ?? Quaternion.identity);
            
            // 이펙트를 부모 GameObject의 자식으로 설정 (월드 위치 유지)
            effect.transform.SetParent(effectParent.transform, worldPositionStays: true);
            
            // 부모 설정 후에도 위치가 올바른지 확인 및 재설정
            Vector3 verifiedCenterPosition = GetRectTransformCenterWorld(parentRectTransform);
            if (Vector3.Distance(effect.transform.position, verifiedCenterPosition) > 0.01f)
            {
                effect.transform.position = verifiedCenterPosition;
                GameLogger.LogWarning($"[VFXManager] 이펙트 위치 보정: {effect.transform.position} → {verifiedCenterPosition}", GameLogger.LogCategory.Combat);
            }
            
            // 이펙트 레이어를 Effects로 설정
            int effectsLayer = LayerMask.NameToLayer("Effects");
            if (effectsLayer != -1)
            {
                effect.layer = effectsLayer;
                SetLayerRecursively(effect, effectsLayer);
            }

            // 파티클 시스템 재생
            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            // 이펙트 지속 시간 후 자동 제거 (부모도 함께 제거)
            float duration = GetEffectDuration(effect);
            StartCoroutine(DestroyEffectAfterDelay(effectParent, duration));

            return effect;
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
                return explicitAnchor.position;
            }

            // 2) Portrait Image 우선 (UI 캐릭터의 경우)
            var portraitImage = FindPortraitImage(characterTransform);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                Vector3 centerPos = GetRectTransformCenterWorld(portraitImage.rectTransform);
                return centerPos;
            }

            // 3) RectTransform 폴백
            var anyRect = characterTransform.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                Vector3 centerPos = GetRectTransformCenterWorld(anyRect);
                return centerPos;
            }

            // 4) SpriteRenderer 폴백 (2D 스프라이트 캐릭터)
            var sprite = characterTransform.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null)
            {
                Vector3 centerPos = sprite.bounds.center;
                return centerPos;
            }

            // 5) 최종 폴백: Transform.position
            return characterTransform.position;
        }

        /// <summary>
        /// 명시적 VFX 앵커를 찾습니다.
        /// 우선순위: VFXAnchorPoint 컴포넌트 > "VFXAnchor" 이름 > "PortraitVFXAnchor" 이름
        /// </summary>
        /// <param name="root">루트 Transform</param>
        /// <returns>VFX 앵커 Transform</returns>
        private Transform FindExplicitVfxAnchor(Transform root)
        {
            // 1) VFXAnchorPoint 컴포넌트를 가진 자식 오브젝트 찾기 (최우선)
            var anchorPoint = root.GetComponentInChildren<VFXAnchorPoint>(true);
            if (anchorPoint != null)
            {
                return anchorPoint.transform;
            }

            // 2) "VFXAnchor" 이름의 자식 오브젝트 찾기
            var anchor = root.Find("VFXAnchor");
            if (anchor != null) return anchor;

            // 3) "PortraitVFXAnchor" 이름의 자식 오브젝트 찾기
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
        /// 캐릭터 Transform에서 RectTransform을 찾습니다.
        /// Portrait Image 우선, 그 다음 첫 번째 RectTransform을 반환합니다.
        /// </summary>
        /// <param name="characterTransform">캐릭터 Transform</param>
        /// <returns>찾은 RectTransform (없으면 null)</returns>
        private RectTransform FindCharacterRectTransform(Transform characterTransform)
        {
            if (characterTransform == null) return null;

            // 1) Portrait Image 우선
            var portraitImage = FindPortraitImage(characterTransform);
            if (portraitImage != null && portraitImage.rectTransform != null)
            {
                return portraitImage.rectTransform;
            }

            // 2) 첫 번째 RectTransform 폴백
            var anyRect = characterTransform.GetComponentInChildren<RectTransform>(true);
            if (anyRect != null)
            {
                return anyRect;
            }

            return null;
        }

        /// <summary>
        /// RectTransform의 중심을 월드 좌표로 계산합니다.
        /// GetWorldCorners를 사용하여 더 안정적으로 계산합니다.
        /// </summary>
        /// <param name="rt">RectTransform</param>
        /// <returns>월드 중심 좌표</returns>
        private Vector3 GetRectTransformCenterWorld(RectTransform rt)
        {
            if (rt == null) return Vector3.zero;
            
            // GetWorldCorners를 사용하여 월드 좌표로 4개 코너 가져오기
            // 0: BL (Bottom Left), 1: TL (Top Left), 2: TR (Top Right), 3: BR (Bottom Right)
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            
            // 4개 코너의 중심 계산
            Vector3 center = (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;
            
            return center;
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
        /// 이펙트는 각 캐릭터의 자식으로 생성되므로, 씬에서 직접 찾아 제거합니다.
        /// </summary>
        public void ClearAllEffects()
        {
            // 씬에서 모든 이펙트 부모 GameObject 찾아 제거
            var effectParents = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int removedCount = 0;
            foreach (var obj in effectParents)
            {
                if (obj != null && obj.name.EndsWith("_Parent"))
                {
                    Destroy(obj);
                    removedCount++;
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
        }

        #endregion

        #region 디버그

        [ContextMenu("VFX 상태 출력")]
        private void LogVFXStatus()
        {
            // 씬에서 모든 이펙트 부모 GameObject 찾아 개수 확인
            var effectParents = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int activeEffectCount = 0;
            foreach (var obj in effectParents)
            {
                if (obj != null && obj.name.EndsWith("_Parent"))
                {
                    activeEffectCount++;
                }
            }
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
