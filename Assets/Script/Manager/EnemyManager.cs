using UnityEngine;
using System.Collections.Generic;
using Game.Battle;
using Game.Characters;

namespace Game.Managers
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        [SerializeField]
        private List<EnemyCharacter> activeEnemies = new(); // 여러 적 관리

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterEnemy(EnemyCharacter enemy)
        {
            if (!activeEnemies.Contains(enemy))
                activeEnemies.Add(enemy);
        }

        public EnemyCharacter GetEnemyBySlot(BattleSlotPosition position)
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy.BattleSlotPosition == position)
                    return enemy;
            }

            return null;
        }

        public EnemyCharacter GetRandomEnemy()
        {
            if (activeEnemies.Count == 0)
                return null;

            return activeEnemies[Random.Range(0, activeEnemies.Count)];
        }
    }
}
