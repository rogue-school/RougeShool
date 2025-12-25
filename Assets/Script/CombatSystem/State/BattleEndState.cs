using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 종료 상태
    /// - 승리 또는 패배 시 진입하는 상태입니다
    /// - 전투 결과를 처리하고 보상 화면으로 전환합니다
    /// </summary>
    public class BattleEndState : BaseCombatState
    {
        public override string StateName => "BattleEnd";

        // 전투 종료 후에는 모든 액션 차단
        public override bool AllowPlayerCardDrag => false;
        public override bool AllowEnemyAutoExecution => false;
        public override bool AllowSlotMovement => false;
        public override bool AllowTurnSwitch => false;

        private bool _isVictory;

        public BattleEndState(bool isVictory)
        {
            _isVictory = isVictory;
        }

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            if (_isVictory)
            {
                LogStateTransition("전투 승리!");
                HandleVictory(context);
            }
            else
            {
                LogStateTransition("전투 패배...");
                HandleDefeat(context);
            }
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            LogStateTransition("전투 종료 상태 종료");
        }

        /// <summary>
        /// 승리 처리
        /// </summary>
        private void HandleVictory(CombatStateContext context)
        {
            // 게임 종료 (TurnController 사용)
            if (context.TurnController != null)
            {
                context.TurnController.EndGame();
            }

            // 승리 이벤트 발생 (전투 통계 수집을 위해 호출)
            Game.CombatSystem.CombatEvents.RaiseVictory();
            LogStateTransition("승리 처리 완료");

            // 여기서 보상 화면 표시 등의 로직 추가 가능
        }

        /// <summary>
        /// 패배 처리
        /// </summary>
        private void HandleDefeat(CombatStateContext context)
        {
            // 게임 종료 (TurnController 사용)
            if (context.TurnController != null)
            {
                context.TurnController.EndGame();
            }

            // 패배 이벤트 발생 (전투 통계 수집을 위해 먼저 호출)
            Game.CombatSystem.CombatEvents.RaiseDefeat();
            LogStateTransition("패배 처리 완료");

            // 게임 오버 이벤트 발생 (세션 종료를 위해 호출)
            Game.CombatSystem.CombatEvents.RaiseGameOver();

            // 게임 오버 UI 표시
            if (context.GameOverUI != null)
            {
                context.GameOverUI.ShowGameOver();
                LogStateTransition("게임 오버 UI 표시 완료");
            }
            else
            {
                LogError("GameOverUI를 찾을 수 없습니다");
            }
        }
    }
}
