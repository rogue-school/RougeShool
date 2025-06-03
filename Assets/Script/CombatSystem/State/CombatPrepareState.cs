using UnityEngine;
using System.Collections;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.Utility;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ITurnCardRegistry cardRegistry;
        private readonly ICoroutineRunner coroutineRunner;

        private bool isStateEntered;
        private bool isButtonClicked;

        [Inject]
        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ITurnCardRegistry cardRegistry,
            ICoroutineRunner coroutineRunner)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.cardRegistry = cardRegistry;
            this.coroutineRunner = coroutineRunner;
        }

        public void EnterState()
        {
            if (isStateEntered)
            {
                Debug.LogWarning("[CombatPrepareState] 이미 상태에 진입했습니다. 중복 호출 방지됨");
                return;
            }

            isStateEntered = true;
            isButtonClicked = false;

            Debug.Log("<color=yellow>[CombatPrepareState] 상태 진입</color>");

            flowCoordinator.DisablePlayerInput();
            cardRegistry.Reset();

            coroutineRunner.RunCoroutine(PrepareRoutine());
        }

        private IEnumerator PrepareRoutine()
        {
            // 새로운 구조: 적 카드 등록은 CombatFlowCoordinator 내부에서 처리
            yield return flowCoordinator.PerformCombatPreparation();

            flowCoordinator.ShowPlayerCardSelectionUI();
            flowCoordinator.DisableStartButton();
            flowCoordinator.RegisterStartButton(OnStartButtonClicked);

            // 플레이어가 카드 선택 완료할 때까지 대기 (슬롯이 하나라도 등록됐는지 확인)
            yield return new WaitUntil(() =>
                cardRegistry.GetCardInSlot(CombatSlotPosition.FIRST)?.IsFromPlayer() == true ||
                cardRegistry.GetCardInSlot(CombatSlotPosition.SECOND)?.IsFromPlayer() == true
            );

            flowCoordinator.EnableStartButton();
            Debug.Log("<color=lime>[CombatPrepareState] 플레이어 카드 선택 완료 → 시작 버튼 활성화</color>");
        }

        private void OnStartButtonClicked()
        {
            if (isButtonClicked)
            {
                Debug.LogWarning("[CombatPrepareState] 시작 버튼이 이미 클릭됨. 중복 클릭 방지됨");
                return;
            }

            isButtonClicked = true;

            Debug.Log("<color=cyan>[CombatPrepareState] 전투 시작 버튼 클릭됨</color>");

            flowCoordinator.HidePlayerCardSelectionUI();
            flowCoordinator.UnregisterStartButton();

            var nextState = turnManager.GetStateFactory().CreateFirstAttackState();
            turnManager.RequestStateChange(nextState);
        }

        public void ExecuteState()
        {
            // 실시간 처리 없음
        }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatPrepareState] 상태 종료</color>");

            flowCoordinator.UnregisterStartButton();
            flowCoordinator.HidePlayerCardSelectionUI();

            isStateEntered = false;
            isButtonClicked = false;
        }
    }
}
