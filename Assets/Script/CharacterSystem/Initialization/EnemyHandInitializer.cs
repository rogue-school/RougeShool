using System.Collections;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 전투 시작 시 적 핸드 슬롯을 초기화하는 스텝 클래스입니다.
    /// 슬롯 3번부터 역순으로 카드를 생성하고 한 칸씩 이동시켜 채워 넣습니다.
    /// </summary>
    public class EnemyHandInitializer : MonoBehaviour, ICombatInitializerStep
    {
        [SerializeField]
        [Tooltip("초기화 순서 (낮을수록 먼저 실행됨)")]
        private int order = 40;

        /// <inheritdoc />
        public int Order => order;

        private IEnemyManager _enemyManager;
        private IEnemyHandManager _handManager;

        /// <summary>
        /// 초기화에 필요한 매니저들을 주입합니다.
        /// </summary>
        /// <param name="enemyManager">적 캐릭터 매니저</param>
        /// <param name="handManager">적 핸드 매니저</param>
        [Inject]
        public void Construct(IEnemyManager enemyManager, IEnemyHandManager handManager)
        {
            _enemyManager = enemyManager;
            _handManager = handManager;
        }

        /// <summary>
        /// 적 핸드를 초기화하고 슬롯 3 → 2 → 1 순서로 채웁니다.
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log("<color=cyan>[EnemyHandInitializer] 적 핸드 초기화 시작</color>");
            
            var enemy = _enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogError("[EnemyHandInitializer] 적이 존재하지 않습니다.");
                yield break;
            }

            // 적 핸드 슬롯과 상태 초기화
            _handManager.Initialize(enemy);

            // 슬롯 3 → 2 → 1 순서로 생성 및 이동하며 카드 채우기
            yield return _handManager.StepwiseFillSlotsFromBack(0.3f);
            
            Debug.Log("<color=cyan>[EnemyHandInitializer] 적 핸드 초기화 완료</color>");
        }
    }
}
