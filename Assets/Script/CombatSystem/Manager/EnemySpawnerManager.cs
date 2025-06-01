using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.IManager;
using Game.CombatSystem.Interface;
using Zenject;

namespace Game.CombatSystem.Manager
{
    public class EnemySpawnerManager : MonoBehaviour, IEnemySpawnerManager
    {
        [Header("기본 적 프리팹")]
        [SerializeField] private GameObject defaultEnemyPrefab;

        [Inject] private IStageManager stageManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private ISlotRegistry slotRegistry;

        private readonly List<EnemyCharacter> spawnedEnemies = new();

        public void SpawnInitialEnemy()
        {
            if (stageManager == null)
            {
                Debug.LogError("[EnemySpawnerManager] StageManager가 주입되지 않았습니다.");
                return;
            }

            stageManager.SpawnNextEnemy();
        }

        public EnemySpawnResult SpawnEnemy(EnemyCharacterData data)
        {
            if (data == null || slotRegistry == null)
                return null;

            var slot = slotRegistry.GetCharacterSlotRegistry()?.GetCharacterSlot(SlotOwner.ENEMY);
            if (slot == null) return null;

            var existing = slot.GetCharacter() as EnemyCharacter;
            if (existing != null && !existing.IsDead())
                return new EnemySpawnResult(existing, false);

            foreach (Transform child in slot.GetTransform())
                Destroy(child.gameObject);

            var prefab = data.Prefab ?? defaultEnemyPrefab;
            if (prefab == null) return null;

            var instance = Instantiate(prefab, slot.GetTransform());
            instance.name = data.DisplayName;
            instance.transform.localPosition = Vector3.zero;

            if (!instance.TryGetComponent(out EnemyCharacter enemy))
            {
                Destroy(instance);
                return null;
            }

            enemy.Initialize(data);
            slot.SetCharacter(enemy);
            spawnedEnemies.Add(enemy);
            enemyManager?.RegisterEnemy(enemy);

            return new EnemySpawnResult(enemy, true);
        }

        public List<EnemyCharacter> GetAllEnemies() => spawnedEnemies;
    }
}
