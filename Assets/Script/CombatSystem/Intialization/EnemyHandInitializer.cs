using System.Collections;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Initialization
{
    public class EnemyHandInitializer : MonoBehaviour, ICombatInitializerStep
    {
        [SerializeField] private int order = 40;
        public int Order => order;

        private IEnemyManager _enemyManager;
        private IEnemyHandManager _handManager;

        [Inject]
        public void Construct(IEnemyManager enemyManager, IEnemyHandManager handManager)
        {
            _enemyManager = enemyManager;
            _handManager = handManager;
        }

        public IEnumerator Initialize()
        {
            Debug.Log("[EnemyHandInitializer] 적 핸드 초기화 시작");

            var enemy = _enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogError("[EnemyHandInitializer] 적이 존재하지 않습니다.");
                yield break;
            }

            _handManager.Initialize(enemy);
            yield return StartCoroutine(_handManager.StepwiseFillSlotsFromBack(0.5f));
            _handManager.FillEmptySlots(); // <-- 이 줄 추가

            Debug.Log("[EnemyHandInitializer] 적 핸드 초기화 완료");
        }

    }
}
