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
        [SerializeField] private bool enableDebugLogging = false;

        #endregion

        #region 이펙트 풀링

        private Dictionary<GameObject, Queue<GameObject>> effectPools = new Dictionary<GameObject, Queue<GameObject>>();
        private Dictionary<GameObject, GameObject> activeEffects = new Dictionary<GameObject, GameObject>();
        private Transform effectContainer;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // 이펙트 컨테이너 생성
            effectContainer = new GameObject("VFX_EffectContainer").transform;
            effectContainer.SetParent(transform);

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("VFXManager 초기화 완료", GameLogger.LogCategory.Combat);
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
        /// 이펙트를 재생합니다.
        /// </summary>
        /// <param name="effectPrefab">이펙트 프리팹</param>
        /// <param name="position">재생 위치</param>
        /// <param name="rotation">회전 (선택적)</param>
        /// <param name="parent">부모 Transform (선택적)</param>
        public GameObject PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
        {
            if (effectPrefab == null)
            {
                GameLogger.LogWarning("이펙트 프리팹이 null입니다.", GameLogger.LogCategory.Combat);
                return null;
            }

            GameObject effect = GetFromPool(effectPrefab);

            effect.transform.position = position;
            effect.transform.rotation = rotation ?? Quaternion.identity;
            effect.transform.SetParent(parent);
            effect.SetActive(true);

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"이펙트 재생: {effectPrefab.name}", GameLogger.LogCategory.Combat);
            }

            // 파티클 시스템 재생
            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            // 자동 반환 (파티클 재생 완료 후)
            float duration = GetEffectDuration(effect);
            StartCoroutine(ReturnEffectAfterDelay(effect, effectPrefab, duration));

            return effect;
        }

        private GameObject GetFromPool(GameObject prefab)
        {
            // 풀이 없으면 생성
            if (!effectPools.ContainsKey(prefab))
            {
                effectPools[prefab] = new Queue<GameObject>();
            }

            Queue<GameObject> pool = effectPools[prefab];

            // 사용 가능한 오브젝트가 있으면 재사용
            GameObject obj;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                // 새로 생성
                obj = Instantiate(prefab, effectContainer);
                obj.SetActive(false);
            }

            activeEffects[obj] = prefab;
            return obj;
        }

        private void ReturnToPool(GameObject effect, GameObject prefab)
        {
            if (effect == null) return;

            activeEffects.Remove(effect);
            effect.SetActive(false);
            effect.transform.SetParent(effectContainer);

            if (!effectPools.ContainsKey(prefab))
            {
                effectPools[prefab] = new Queue<GameObject>();
            }

            effectPools[prefab].Enqueue(effect);
        }

        private System.Collections.IEnumerator ReturnEffectAfterDelay(GameObject effect, GameObject prefab, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(effect, prefab);
        }

        private float GetEffectDuration(GameObject effect)
        {
            float maxDuration = 0f;

            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                if (duration > maxDuration)
                {
                    maxDuration = duration;
                }
            }

            return maxDuration > 0 ? maxDuration : 2f; // 기본값 2초
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
        /// 모든 활성 이펙트를 즉시 풀에 반환합니다.
        /// </summary>
        public void ClearAllEffects()
        {
            var effectsToReturn = new List<GameObject>(activeEffects.Keys);
            foreach (var effect in effectsToReturn)
            {
                if (activeEffects.TryGetValue(effect, out GameObject prefab))
                {
                    ReturnToPool(effect, prefab);
                }
            }

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

            GameLogger.LogInfo("모든 VFX 정리 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 디버그

        [ContextMenu("VFX 상태 출력")]
        private void LogVFXStatus()
        {
            GameLogger.LogInfo($"[VFXManager] 활성 이펙트: {activeEffects.Count}, 풀 종류: {effectPools.Count}", GameLogger.LogCategory.Combat);

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
