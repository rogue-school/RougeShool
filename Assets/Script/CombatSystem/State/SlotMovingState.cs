using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 슬롯 이동 상태
    /// - 카드 실행 후 슬롯을 앞으로 이동시킵니다
    /// - 대기 슬롯의 카드들이 한 칸씩 전진합니다
    /// - 이동 완료 후 다음 턴 상태로 전환합니다
    /// </summary>
    public class SlotMovingState : BaseCombatState
    {
        public override string StateName => "SlotMoving";

        // 슬롯 이동 중에는 슬롯 이동만 허용
        // 적 카드가 배틀 슬롯에 도착할 수 있으므로 적 실행 허용
        public override bool AllowPlayerCardDrag => false;
        public override bool AllowEnemyAutoExecution => true;
        public override bool AllowSlotMovement => true;
        public override bool AllowTurnSwitch => false;

        private bool _isMoving = false;
        private MonoBehaviour _coroutineRunner;

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            LogStateTransition("슬롯 이동 시작");

            // 코루틴 실행을 위한 MonoBehaviour 찾기
            _coroutineRunner = context.StateMachine;

            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(MoveSlots(context));
            }
            else
            {
                LogError("코루틴 실행을 위한 MonoBehaviour를 찾을 수 없습니다");
                ProceedToNextTurn(context);
            }
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            _isMoving = false;
            LogStateTransition("슬롯 이동 완료");
        }

        /// <summary>
        /// 슬롯을 이동시키는 코루틴
        /// </summary>
        private IEnumerator MoveSlots(CombatStateContext context)
        {
            if (_isMoving)
            {
                LogWarning("이미 슬롯 이동 중입니다");
                yield break;
            }

            _isMoving = true;

            // SlotMovementController의 슬롯 전진 처리 호출
            if (context?.SlotMovement != null)
            {
                LogStateTransition("SlotMovementController 슬롯 전진 시작");

                // SlotMovementController의 MoveAllSlotsForward 메서드 호출 (슬롯 이동)
                yield return context.StateMachine.StartCoroutine(
                    context.SlotMovement.MoveAllSlotsForwardRoutine()
                );

                LogStateTransition("슬롯 이동 완료");
                
                // 슬롯 이동 애니메이션이 완전히 끝날 때까지 대기
                LogStateTransition("슬롯 이동 애니메이션 완료 대기 중...");
                yield return new WaitForSeconds(0.1f); // 애니메이션 완료를 위한 추가 대기
            }
            else
            {
                LogWarning("SlotMovement가 null - 슬롯 이동 건너뜀");
                yield return new WaitForSeconds(0.5f);
            }

            // 다음 턴으로 전환
            LogStateTransition("다음 턴으로 전환 시작");
            ProceedToNextTurn(context);
        }

        /// <summary>
        /// 상태 전환 전 완료 검증
        /// 슬롯 이동과 모든 애니메이션이 완료되었는지 확인
        /// </summary>
        public override bool CanTransitionToNextState(CombatStateContext context)
        {
            LogStateTransition($"[검증] {StateName} 전환 가능 여부 확인");

            // 1. 슬롯 이동 완료 검증
            if (_isMoving)
            {
                LogWarning("[검증] 슬롯 이동이 아직 진행 중 - 전환 불가능");
                return false;
            }

            // 2. 컨텍스트 검증
            if (context?.SlotRegistry == null)
            {
                LogError("[검증] SlotRegistry가 null - 전환 불가능");
                return false;
            }

            LogStateTransition("[검증] 슬롯 이동 완료 확인 - 전환 가능");
            return true;
        }

        /// <summary>
        /// 상태 완료 대기
        /// 슬롯 이동과 모든 애니메이션이 완료될 때까지 대기
        /// </summary>
        public override System.Collections.IEnumerator WaitForCompletion(CombatStateContext context)
        {
            LogStateTransition($"[대기] {StateName} 완료 대기 시작");

            // 1. 슬롯 이동 완료 대기
            LogStateTransition("[대기] 슬롯 이동 완료 대기 중...");
            while (_isMoving)
            {
                yield return null;
            }

            // 2. 애니메이션 완료 대기
            LogStateTransition("[대기] 슬롯 이동 애니메이션 완료 대기 중...");
            yield return new WaitForSeconds(0.1f);

            LogStateTransition($"[완료] {StateName} 모든 작업 완료 확인");
        }

        /// <summary>
        /// 다음 턴으로 진행합니다
        /// 새로운 로직: 배틀 슬롯의 카드 타입에 따라 턴 결정
        /// - 플레이어 턴 마커 → PlayerTurnState
        /// - 적 스킬카드 → EnemyTurnState
        /// - 빈 슬롯 → PlayerTurnState (기본값)
        /// </summary>
        private void ProceedToNextTurn(CombatStateContext context)
        {
            if (context?.SlotRegistry == null)
            {
                LogError("SlotRegistry가 null입니다");
                return;
            }

            // 배틀 슬롯의 카드 확인 (SlotRegistry 사용)
            var battleCard = context.SlotRegistry.GetCardInSlot(Slot.CombatSlotPosition.BATTLE_SLOT);
            
            if (battleCard == null)
            {
                // 배틀 슬롯이 비어있음 → 플레이어 턴으로 시작
                LogStateTransition("배틀 슬롯 비어있음 → 플레이어 턴 시작");
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
                return;
            }

            // 카드 타입에 따른 턴 결정
            bool isPlayerMarker = IsPlayerTurnMarker(battleCard);
            bool isEnemySkillCard = !battleCard.IsFromPlayer();

            LogStateTransition($"슬롯 이동 완료 - 배틀 슬롯: {battleCard.GetCardName()}, 플레이어 마커: {isPlayerMarker}, 적 카드: {isEnemySkillCard}");

            if (isPlayerMarker)
            {
                // 플레이어 턴 마커 → 플레이어 턴
                LogStateTransition("플레이어 턴 마커 감지 → 플레이어 턴 시작");
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
            }
            else if (isEnemySkillCard)
            {
                // 적 스킬카드 → 적 턴
                LogStateTransition($"적 스킬카드 감지 → 적 턴 시작: {battleCard.GetCardName()}");
                var enemyTurnState = new EnemyTurnState();
                RequestTransition(context, enemyTurnState);
            }
            else
            {
                // 예상치 못한 상황 → 플레이어 턴으로 기본 처리
                LogWarning($"예상치 못한 카드 타입: {battleCard.GetCardName()} → 플레이어 턴으로 기본 처리");
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
            }
        }

        /// <summary>
        /// 카드가 플레이어 턴 마커인지 확인합니다
        /// </summary>
        private bool IsPlayerTurnMarker(Game.SkillCardSystem.Interface.ISkillCard card)
        {
            if (card?.CardDefinition == null) return false;
            
            // PLAYER_MARKER ID이면서 플레이어 소유인 경우
            return card.CardDefinition.cardId == "PLAYER_MARKER" && card.IsFromPlayer();
        }
    }
}
