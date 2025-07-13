using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.CombatSystem.Interface;
using Zenject;
using Game.CombatSystem.Utility;
using Game.CombatSystem;
using AnimationSystem.Manager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 적 캐릭터 프리팹을 스폰하여 슬롯에 배치하는 매니저입니다.
    /// 실제 전투 설정은 StageManager와 HandManager에서 수행됩니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour, IEnemySpawnerManager
    {
        #region 인스펙터 설정

        [Header("기본 적 프리팹")]
        [SerializeField] private GameObject defaultEnemyPrefab;

        #endregion

        #region 의존성

        [Inject] private ISlotRegistry slotRegistry;
        [Inject] private IEnemyManager enemyManager;

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
            if (data == null || slotRegistry == null)
            {
                onComplete?.Invoke(null);
                yield break;
            }

            var slot = slotRegistry.GetCharacterSlotRegistry()?.GetCharacterSlot(SlotOwner.ENEMY);
            if (slot == null)
            {
                Debug.LogError("[EnemySpawnerManager] 적 캐릭터 슬롯을 찾을 수 없습니다.");
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

            // 1. EnemyCharacter 컴포넌트 및 데이터 세팅 (애니메이션 전에)
            if (!instance.TryGetComponent(out EnemyCharacter enemy))
            {
                Debug.LogError("[EnemySpawnerManager] EnemyCharacter 컴포넌트 누락");
                Object.Destroy(instance);
                onComplete?.Invoke(null);
                yield break;
            }
            enemy.Initialize(data); // ★ 데이터 먼저 주입

            // 2. 등장 애니메이션 실행 및 대기 (파사드 패턴 적용)
            bool animDone = false;
            AnimationFacade.Instance.PlayEnemyCharacterAnimation(
                data.name, // 캐릭터 ID (ScriptableObject의 name)
                "spawn",
                instance,
                () => animDone = true
            );
            yield return new WaitUntil(() => animDone);

            // 3. 슬롯/매니저 등록
            slot.SetCharacter(enemy);
            enemyManager?.RegisterEnemy(enemy);
            spawnedEnemies.Add(enemy);

            // 다음 적 스폰 이벤트 발행
            CombatEvents.RaiseNextEnemySpawned(data);

            onComplete?.Invoke(new EnemySpawnResult(enemy, true));
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
