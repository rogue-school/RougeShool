using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Core;
using Game.CombatSystem.Manager;

namespace Game.Utility
{
    /// <summary>
    /// 전투 상태 팩토리를 초기화하고 인터페이스로 주입 가능한 구조로 제공합니다.
    /// </summary>
    public class CombatStateFactoryInstaller : MonoBehaviour, ICombatStateFactory
    {
        private ICombatStateFactory factory;

        [Header("필요한 매니저 (인터페이스 구현 필수)")]
        [SerializeField] private MonoBehaviour playerHandManagerSource;
        [SerializeField] private MonoBehaviour enemyHandManagerSource;
        [SerializeField] private MonoBehaviour enemySpawnerManagerSource;
        [SerializeField] private MonoBehaviour combatSlotManagerSource;
        [SerializeField] private MonoBehaviour stageManagerSource;
        [SerializeField] private MonoBehaviour victoryManagerSource;
        [SerializeField] private MonoBehaviour gameOverManagerSource;
        [SerializeField] private CombatTurnManager combatTurnManager;

        private void Awake()
        {
            //Debug.Log("[CombatStateFactoryInstaller] 초기화 시작");

            if (!TryCast(out IPlayerHandManager playerHandManager, playerHandManagerSource) ||
                !TryCast(out IEnemyHandManager enemyHandManager, enemyHandManagerSource) ||
                !TryCast(out IEnemySpawnerManager enemySpawnerManager, enemySpawnerManagerSource) ||
                !TryCast(out ICombatSlotManager combatSlotManager, combatSlotManagerSource) ||
                !TryCast(out IStageManager stageManager, stageManagerSource) ||
                !TryCast(out IVictoryManager victoryManager, victoryManagerSource) ||
                !TryCast(out IGameOverManager gameOverManager, gameOverManagerSource))
            {
                Debug.LogError("[CombatStateFactoryInstaller] 필수 매니저 캐스팅 실패. 초기화 중단.");
                enabled = false;
                return;
            }

            factory = new CombatStateFactory(
                combatTurnManager,
                playerHandManager,
                enemyHandManager,
                enemySpawnerManager,
                combatSlotManager,
                stageManager,
                victoryManager,
                gameOverManager
            );

            combatTurnManager.InjectFactory(this);

            Debug.Log("[CombatStateFactoryInstaller] 전투 상태 팩토리 생성 및 CombatTurnManager에 주입 완료");
        }

        private bool TryCast<T>(out T result, MonoBehaviour source) where T : class
        {
            result = source as T;
            if (result == null)
            {
                Debug.LogError($"[CombatStateFactoryInstaller] {typeof(T).Name} 캐스팅 실패: {source?.name ?? "null"}");
                return false;
            }
            return true;
        }
        public void Initialize()
        {
            Awake(); // 기존 Awake 로직을 내부 호출 (또는 로직을 분리해서 중복 호출 방지)
        }

        // 상태 생성 위임
        public ICombatTurnState CreatePrepareState() => factory.CreatePrepareState();
        public ICombatTurnState CreatePlayerInputState() => factory.CreatePlayerInputState();
        public ICombatTurnState CreateFirstAttackState() => factory.CreateFirstAttackState();
        public ICombatTurnState CreateSecondAttackState() => factory.CreateSecondAttackState();
        public ICombatTurnState CreateResultState() => factory.CreateResultState();
        public ICombatTurnState CreateVictoryState() => factory.CreateVictoryState();
        public ICombatTurnState CreateGameOverState() => factory.CreateGameOverState();
    }

}
