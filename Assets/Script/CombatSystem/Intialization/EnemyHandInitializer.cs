using System.Collections;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 전투 시작 시 적 핸드 슬롯(3→2→1)을 정확한 순서로 초기화합니다.
    /// 슬롯 3번에서만 카드가 생성되며, 앞 슬롯으로 한 칸씩 이동하며 채워집니다.
    /// </summary>
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

            // 적 핸드 초기화 (슬롯과 내부 상태 세팅)
            _handManager.Initialize(enemy);

            // 슬롯 3 → 2 → 1 순서로 생성 + 이동
            yield return _handManager.StepwiseFillSlotsFromBack(0.5f);

            Debug.Log("[EnemyHandInitializer] 적 핸드 초기화 완료");
        }
    }
}
