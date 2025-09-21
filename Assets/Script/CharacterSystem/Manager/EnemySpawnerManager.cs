using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Zenject;
using Game.CombatSystem.Utility;
using Game.CombatSystem;
using Game.CombatSystem.Manager;
using Game.CharacterSystem.Manager;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 적 캐릭터 프리팹을 스폰하여 슬롯에 배치하는 매니저입니다.
    /// 실제 전투 설정은 StageManager와 HandManager에서 수행됩니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour
    {
        #region 인스펙터 설정

        [Header("기본 적 프리팹")]
        [SerializeField] private GameObject defaultEnemyPrefab;

        #endregion

        #region 의존성

        [Inject] private CharacterSlotRegistry slotRegistry;

        #endregion

        #region 싱글톤 시스템 호환성

        /// <summary>
        /// 새로운 싱글톤 시스템과의 호환성을 위한 의존성 확인 및 대체
        /// </summary>
        private void EnsureDependencies()
        {
            if (slotRegistry == null)
            {
                GameLogger.LogWarning("slotRegistry가 주입되지 않았습니다. 새로운 싱글톤 시스템을 사용합니다.", GameLogger.LogCategory.Combat);
            }
        }

        #endregion

        #region 내부 상태

        private readonly List<EnemyCharacter> spawnedEnemies = new();

        #endregion

        #region 적 스폰 메서드

        /// <summary>
        /// 지정된 데이터 기반으로 적을 스폰하고 슬롯에 배치합니다.
        /// </summary>
        /// <param name="data">적 캐릭터 데이터</param>
        /// <returns>적 스폰 결과</returns>
        public EnemySpawnResult SpawnEnemy(EnemyCharacterData data)
        {
            EnemySpawnResult result = null;
            bool done = false;
            StartCoroutine(SpawnEnemyWithAnimation(data, r => { result = r; done = true; }));
            while (!done) { } // 동기 방식이므로 실제 게임에서는 코루틴 사용 권장
            return result;
        }

        /// <summary>
        /// 적 프리팹 생성/데이터 주입 → 등장 애니메이션(1.5초) → 슬롯/매니저 등록 → 콜백 호출까지 순차적으로 처리하는 코루틴
        /// </summary>
        public System.Collections.IEnumerator SpawnEnemyWithAnimation(EnemyCharacterData data, System.Action<EnemySpawnResult> onComplete)
        {
            if (data == null)
            {
                GameLogger.LogError("적 데이터가 null입니다", GameLogger.LogCategory.Combat);
                onComplete?.Invoke(null);
                yield break;
            }

            // 의존성 확인
            EnsureDependencies();

            // 새로운 싱글톤 시스템과의 호환성을 위한 슬롯 찾기
            var slot = FindEnemyCharacterSlot();
            if (slot == null)
            {
                GameLogger.LogError("적 캐릭터 슬롯을 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                onComplete?.Invoke(null);
                yield break;
            }

            // 기존 적 제거
            var existing = slot.GetCharacter() as EnemyCharacter;
            if (existing != null && !existing.IsDead())
            {
                onComplete?.Invoke(new EnemySpawnResult(existing, false));
                yield break;
            }

            foreach (Transform child in slot.GetTransform())
                Object.Destroy(child.gameObject);

            var prefab = data.Prefab ?? defaultEnemyPrefab;
            if (prefab == null)
            {
                Debug.LogError("[EnemySpawnerManager] 프리팹이 설정되지 않았습니다.");
                onComplete?.Invoke(null);
                yield break;
            }

            var instance = Object.Instantiate(prefab, slot.GetTransform());
            instance.name = data.DisplayName;
            instance.transform.localPosition = Vector3.zero;
            
            // HideFlags 초기화 (Unity Assertion 에러 방지)
            instance.hideFlags = HideFlags.None;

            // 1. EnemyCharacter 컴포넌트 및 데이터 세팅 (애니메이션 전에)
            if (!instance.TryGetComponent(out EnemyCharacter enemy))
            {
                Debug.LogError("[EnemySpawnerManager] EnemyCharacter 컴포넌트 누락");
                Object.Destroy(instance);
                onComplete?.Invoke(null);
                yield break;
            }
            enemy.Initialize(data); // ★ 데이터 먼저 주입

            // 2. 등장 애니메이션 건너뛰기 (AnimationSystem 제거로 인해 임시 비활성화)
            GameLogger.LogInfo("적 캐릭터 애니메이션을 건너뜁니다.", GameLogger.LogCategory.Combat);
            yield return new WaitForSeconds(0.5f); // 짧은 대기시간

            // 3. 슬롯/매니저 등록
            slot.SetCharacter(enemy);
            
            // 새로운 싱글톤 시스템과의 호환성을 위한 매니저 등록
            var manager = FindFirstObjectByType<EnemyManager>();
            manager?.RegisterEnemy(enemy);
            
            spawnedEnemies.Add(enemy);

            // 다음 적 스폰 이벤트 발행
            CombatEvents.RaiseNextEnemySpawned(data);

            GameLogger.LogInfo($"적 캐릭터 스폰 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
            onComplete?.Invoke(new EnemySpawnResult(enemy, true));
        }

        /// <summary>
        /// 적 캐릭터 슬롯을 찾습니다. 새로운 싱글톤 시스템과 호환됩니다.
        /// </summary>
        /// <returns>적 캐릭터 슬롯</returns>
        private ICharacterSlot FindEnemyCharacterSlot()
        {
            // 기존 시스템 사용
            if (slotRegistry != null)
            {
                var slot = slotRegistry?.GetCharacterSlot(SlotOwner.ENEMY);
                if (slot != null)
                {
                    GameLogger.LogInfo("기존 slotRegistry를 통한 적 슬롯 발견", GameLogger.LogCategory.Combat);
                    return slot;
                }
            }

            // 새로운 싱글톤 시스템 사용 - Unity 씬에서 직접 찾기
            var enemySlotGameObject = GameObject.Find("EnemyCharacterSlot");
            if (enemySlotGameObject != null)
            {
                var slot = enemySlotGameObject.GetComponent<ICharacterSlot>();
                if (slot != null)
                {
                    GameLogger.LogInfo("Unity 씬에서 적 슬롯 발견", GameLogger.LogCategory.Combat);
                    return slot;
                }
            }

            GameLogger.LogWarning("적 캐릭터 슬롯을 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            return null;
        }

        /// <summary>
        /// 더 이상 사용되지 않는 초기 적 스폰 메서드입니다.
        /// </summary>
        public void SpawnInitialEnemy()
        {
            Debug.LogWarning("[EnemySpawnerManager] StageManager를 통해 적을 생성하세요. 이 메서드는 더 이상 사용되지 않습니다.");
        }

        #endregion

        #region 기타 유틸리티

        /// <summary>
        /// 현재까지 스폰된 모든 적 캐릭터 리스트를 반환합니다.
        /// </summary>
        public List<EnemyCharacter> GetAllEnemies() => spawnedEnemies;

        #endregion
    }
}
