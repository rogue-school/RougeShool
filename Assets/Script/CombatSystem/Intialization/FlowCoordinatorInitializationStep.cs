using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using System.Collections;
using Game.CombatSystem.Core;
using Game.CombatSystem.Manager;

namespace Game.CombatSystem.Initialization
{
    public class FlowCoordinatorInitializationStep : MonoBehaviour, ICombatInitializerStep
    {
        [Inject] private ICombatFlowCoordinator flowCoordinator;
        [Inject] private ICombatTurnManager turnManager;
        [Inject] private ICombatStateFactory stateFactory;
        [Inject] private ITurnStartConditionChecker conditionChecker;
        [Inject] private ITurnCardRegistry cardRegistry;
        [Inject] private TurnStartButtonHandler buttonHandler;
        [Inject] private IEnemyHandManager enemyHandManager;

        public int Order => 50;

        public IEnumerator Initialize()
        {
            Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 흐름 초기화 시작");

            buttonHandler.Inject(conditionChecker, turnManager, stateFactory, cardRegistry);
            turnManager.Initialize();
            flowCoordinator.InjectTurnStateDependencies(turnManager, stateFactory);

            yield return flowCoordinator.PerformCombatPreparation();

            Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 성공");
            flowCoordinator.StartCombatFlow();
        }

    }
}
