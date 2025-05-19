using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatGameOverState : ICombatTurnState
    {
        private readonly ICombatTurnManager controller;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;

        public CombatGameOverState(
            ICombatTurnManager controller,
            ICombatFlowCoordinator flowCoordinator,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.flowCoordinator = flowCoordinator;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[CombatGameOverState] 상태 진입 - 게임 오버 처리");

            if (controller is MonoBehaviour mono)
                mono.StartCoroutine(HandleGameOver());
            else
                Debug.LogError("[CombatGameOverState] controller가 MonoBehaviour를 구현하지 않아 코루틴 실행 불가");
        }

        private IEnumerator HandleGameOver()
        {
            yield return flowCoordinator.PerformGameOverPhase();

            Debug.Log("[CombatGameOverState] 게임 오버 처리 완료 - 상태 전이 없음 또는 씬 전환 등");

            // 상태 전이 없음: 게임 종료 화면에서 직접 메인 메뉴로 이동하거나 재시작 버튼 클릭 등 외부 제어로 처리.
            // 예를 들어 외부 UI 시스템에서 컨트롤하거나 씬 로더 호출
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatGameOverState] 상태 종료");
        }
    }
}
