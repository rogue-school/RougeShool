using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 적 캐릭터를 전투 슬롯에 소환하고 관리하는 매니저입니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour, IEnemySpawnerManager
    {
        [Header("기본 적 프리팹")]
        [SerializeField] private GameObject defaultEnemyPrefab;

        private IStageManager stageManager;
        private readonly List<EnemyCharacter> spawnedEnemies = new();

        public void InjectStageManager(IStageManager stageManager)
        {
            this.stageManager = stageManager;
            Debug.Log("[EnemySpawnerManager] StageManager가 주입되었습니다.");
        }

        public void SpawnInitialEnemy()
        {
            if (stageManager == null)
            {
                Debug.LogError("[EnemySpawnerManager] StageManager가 주입되지 않았습니다. 초기 적을 소환할 수 없습니다.");
                return;
            }

            Debug.Log("[EnemySpawnerManager] 초기 적 소환 시도 → StageManager.SpawnNextEnemy 호출");
            stageManager.SpawnNextEnemy();
        }

        public EnemyCharacter SpawnEnemy(EnemyCharacterData data)
        {
            if (data == null)
            {
                Debug.LogError("[EnemySpawnerManager] EnemyCharacterData가 null입니다. 적 소환 중단.");
                return null;
            }

            var slotRegistry = SlotRegistry.Instance;
            if (slotRegistry == null)
            {
                Debug.LogError("[EnemySpawnerManager] SlotRegistry.Instance가 null입니다. 적 소환 실패.");
                return null;
            }

            var slot = slotRegistry.GetCharacterSlot(SlotOwner.ENEMY);
            if (slot == null)
            {
                Debug.LogError("[EnemySpawnerManager] ENEMY용 캐릭터 슬롯을 찾을 수 없습니다.");
                return null;
            }

            var existingEnemy = slot.GetCharacter() as EnemyCharacter;

            // 적이 이미 존재하고 살아있으면 재사용
            if (existingEnemy != null && !existingEnemy.IsDead())
            {
                Debug.LogWarning("[EnemySpawnerManager] 살아있는 적이 이미 존재합니다. 재생성하지 않습니다.");
                return existingEnemy;
            }

            // 기존 자식 오브젝트 제거
            Debug.Log("[EnemySpawnerManager] 기존 슬롯 자식 오브젝트 제거 시작");
            foreach (Transform child in slot.GetTransform())
                Destroy(child.gameObject);

            var prefabToUse = data.prefab != null ? data.prefab : defaultEnemyPrefab;
            if (prefabToUse == null)
            {
                Debug.LogError("[EnemySpawnerManager] 사용할 적 프리팹이 없습니다.");
                return null;
            }

            Debug.Log($"[EnemySpawnerManager] 프리팹 인스턴스 생성: {data.displayName}");
            var instance = Instantiate(prefabToUse, slot.GetTransform());
            instance.name = $"Enemy_{data.displayName}";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            if (!instance.TryGetComponent(out EnemyCharacter enemy))
            {
                Debug.LogError("[EnemySpawnerManager] EnemyCharacter 컴포넌트를 찾지 못했습니다. 인스턴스 파괴.");
                Destroy(instance);
                return null;
            }

            enemy.SetCharacterData(data);
            slot.SetCharacter(enemy);
            spawnedEnemies.Add(enemy);

            Debug.Log($"[EnemySpawnerManager] 적 소환 완료: {data.displayName}");
            return enemy;
        }

        public List<EnemyCharacter> GetAllEnemies()
        {
            Debug.Log($"[EnemySpawnerManager] 현재 소환된 적 수: {spawnedEnemies.Count}");
            return spawnedEnemies;
        }
    }
}
