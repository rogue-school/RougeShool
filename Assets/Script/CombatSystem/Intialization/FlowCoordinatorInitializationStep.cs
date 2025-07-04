using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using System.Collections;
using Game.CombatSystem.Core;
using Game.CombatSystem.Manager;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 전투 흐름 초기화를 담당하는 CombatInitializerStep입니다.
    /// TurnManager, 상태 팩토리, UI 핸들러 등의 의존성을 주입하고,
    /// CombatFlowCoordinator를 통해 전투 준비 절차를 수행합니다.
    /// </summary>
    public class FlowCoordinatorInitializationStep : MonoBehaviour, ICombatInitializerStep
    {
        [Inject] private ICombatFlowCoordinator flowCoordinator;
        [Inject] private ICombatTurnManager turnManager;
        [Inject] private ICombatStateFactory stateFactory;
        [Inject] private ITurnStartConditionChecker conditionChecker;
        [Inject] private ITurnCardRegistry cardRegistry;
        [Inject] private TurnStartButtonHandler buttonHandler;
        [Inject] private IEnemyHandManager enemyHandManager;

        /// <summary>
        /// 초기화 순서. 이 단계는 50번째로 실행됩니다.
        /// </summary>
        public int Order => 50;

        /// <summary>
        /// 전투 흐름 초기화 절차를 수행합니다.
        /// Turn 버튼 핸들러, 상태 매니저 등을 세팅하고 전투 준비 코루틴을 실행합니다.
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 흐름 초기화 시작");

            // 턴 시작 버튼 핸들러에 의존성 주입
            buttonHandler.Inject(conditionChecker, turnManager, stateFactory, cardRegistry);

            // 턴 매니저 초기화 및 상태 팩토리 주입
            turnManager.Initialize();
            flowCoordinator.InjectTurnStateDependencies(turnManager, stateFactory);

            // 전투 준비 수행
            yield return flowCoordinator.PerformCombatPreparation();

            Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 성공");

            // 전투 흐름 시작
            flowCoordinator.StartCombatFlow();
        }
    }
}
