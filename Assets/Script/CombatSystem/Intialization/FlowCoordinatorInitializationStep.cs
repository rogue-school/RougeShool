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

            // 버튼 핸들러 의존성 주입
            buttonHandler.Inject(conditionChecker, (ITurnStateController)turnManager, stateFactory, cardRegistry);

            bool isComplete = false;

            flowCoordinator.RequestCombatPreparation(success =>
            {
                if (success)
                {
                    Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 성공");
                    turnManager.ChangeState(stateFactory.CreatePlayerInputState());
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
