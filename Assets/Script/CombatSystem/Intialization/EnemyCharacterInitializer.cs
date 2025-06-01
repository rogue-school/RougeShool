using System.Collections;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Initialization
{
    public class EnemyCharacterInitializer : MonoBehaviour, ICombatInitializerStep
    {
        [SerializeField] private int order = 30;
        public int Order => order;

        private IStageManager _stageManager;

        [Inject]
        public void Construct(IStageManager stageManager)
        {
            _stageManager = stageManager;
        }

        public IEnumerator Initialize()
        {
            Debug.Log("[EnemyCharacterInitializer] 적 캐릭터 초기화 시작");

            _stageManager.SpawnNextEnemy();

            Debug.Log("[EnemyCharacterInitializer] 적 캐릭터 생성 완료");
            yield return null;
        }
    }
}
