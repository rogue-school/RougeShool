using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using System.Collections;
using Game.CombatSystem.Core;

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

        public int Order => 50;

        public IEnumerator Initialize()
        {
            Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 흐름 초기화 시작");

            buttonHandler.Inject(conditionChecker, turnManager, stateFactory, cardRegistry);

            // 1. 턴 매니저 초기화만 실행 (상태 전이 X)
            turnManager.Initialize();

            // 2. flowCoordinator에 턴 상태 연결만 먼저 수행
            flowCoordinator.InjectTurnStateDependencies(turnManager, stateFactory);

            // 3. 전투 흐름 실행은 CombatPreparation이 완료된 후로 지연
            bool isComplete = false;

            flowCoordinator.RequestCombatPreparation(success =>
            {
                if (success)
                {
                    Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 성공");
                    flowCoordinator.StartCombatFlow(); // 여기서 실행
                }
                else
                {
                    Debug.LogError("[FlowCoordinatorInitializationStep] 전투 준비 실패");
                }

                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }

    }
}
