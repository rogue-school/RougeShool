using System.Collections;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.StageSystem.Interface;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 전투 시작 시 적 캐릭터를 초기화하는 스텝 클래스입니다.
    /// 스테이지 매니저를 통해 적을 생성하며, 초기화 순서를 설정할 수 있습니다.
    /// </summary>
    public class EnemyCharacterInitializer : MonoBehaviour, ICombatInitializerStep
    {
        [SerializeField]
        [Tooltip("초기화 순서 (낮을수록 먼저 실행됨)")]
        private int order = 30;

        /// <inheritdoc />
        public int Order => order;

        private IStageManager _stageManager;

        /// <summary>
        /// 스테이지 매니저를 주입받아 적 생성 기능을 사용합니다.
        /// </summary>
        /// <param name="stageManager">스테이지 관리 매니저</param>
        [Inject]
        public void Construct(IStageManager stageManager)
        {
            _stageManager = stageManager;
        }

        /// <summary>
        /// 적 캐릭터를 스폰하여 전투에 배치합니다.
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log("<color=cyan>[EnemyCharacterInitializer] 적 캐릭터 초기화 시작</color>");

            // 적 스폰 (동기 방식)
            _stageManager.SpawnNextEnemy();

            Debug.Log("<color=cyan>[EnemyCharacterInitializer] 적 캐릭터 생성 완료</color>");
            yield return null;
        }
    }
}
