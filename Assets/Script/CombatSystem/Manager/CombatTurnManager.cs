using UnityEngine;
using System;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Zenject;
using Game.CombatSystem;
using Game.AnimationSystem.Manager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 턴 타입을 나타내는 열거형입니다.
    /// </summary>
    public enum TurnType
    {
        /// <summary>
        /// 플레이어 턴
        /// </summary>
        Player,
        
        /// <summary>
        /// 적 턴
        /// </summary>
        Enemy,
        
        /// <summary>
        /// 알 수 없는 턴
        /// </summary>
        Unknown
    }

    /// <summary>
    /// 전투 단계를 나타내는 열거형입니다.
    /// </summary>
    public enum CombatPhase
    {
        /// <summary>
        /// 셋업 단계 (전투 시작 전 카드 배치)
        /// </summary>
        Setup,
        
        /// <summary>
        /// 전투 단계 (카드 실행 및 턴 진행)
        /// </summary>
        Battle,
        
        /// <summary>
        /// 전투 종료
        /// </summary>
        End
    }

    /// <summary>
    /// 전투 턴을 제어하고 상태 전이, 카드 등록, 턴 진행 가능 여부 등을 관리하는 클래스입니다.
    /// </summary>
    public class CombatTurnManager : MonoBehaviour, ICombatTurnManager
    {
        #region 의존성 주입

        [Inject] private ICombatStateFactory stateFactory;
        [Inject] private ITurnCardRegistry cardRegistry;

        #endregion

        #region 내부 상태

        private ICombatTurnState currentState;
        private ICombatTurnState pendingNextState;
        private CombatSlotPosition? reservedEnemySlot;
        private bool isTurnReady;
        private int currentTurn = 1;

        // 새로운 5슬롯 시스템을 위한 상태
        private CombatPhase currentPhase = CombatPhase.Setup;
        private int setupStep = 0; // 셋업 단계 (0~8)
        private bool isSetupComplete = false;

        /// <summary>
        /// 턴 시작 가능 상태가 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<bool> OnTurnReadyChanged;

        /// <summary>
        /// 전투 단계가 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<CombatPhase> OnCombatPhaseChanged;

        /// <summary>
        /// 셋업 단계가 진행될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<int> OnSetupStepChanged;

        #endregion

        #region 초기화

        /// <summary>
        /// 턴 매니저를 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            currentState = null;
            pendingNextState = null;
            reservedEnemySlot = null;
            isTurnReady = false;
        }

        /// <summary>
        /// 상태 실행 및 대기 중인 상태 전이를 처리합니다.
        /// </summary>
        private void Update()
        {
            if (pendingNextState != null)
            {
                ApplyPendingState();
            }

            currentState?.ExecuteState();
        }

        #endregion

        #region 상태 전이

        /// <summary>
        /// 다음 상태로의 전이를 요청합니다.
        /// </summary>
        /// <param name="nextState">전이할 상태</param>
        public void RequestStateChange(ICombatTurnState nextState)
        {
            pendingNextState = nextState;
        }

        /// <summary>
        /// 요청된 상태 전이를 실제로 적용합니다.
        /// </summary>
        public void ApplyPendingState()
        {
            if (pendingNextState == null) return;

            ChangeState(pendingNextState);
            pendingNextState = null;
        }

        /// <summary>
        /// 현재 상태를 종료하고 새로운 상태로 전이합니다.
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        public void ChangeState(ICombatTurnState newState)
        {
            currentState?.ExitState();

            // 턴 종료 이벤트 발행
            if (currentState != null)
                CombatEvents.RaiseTurnEnded();

            currentState = newState;

            currentState?.EnterState();

            // 턴 시작 이벤트 발행
            if (currentState != null)
                CombatEvents.RaiseTurnStarted();
        }

        #endregion

        #region 상태 및 팩토리 접근

        /// <summary>
        /// 현재 상태를 반환합니다.
        /// </summary>
        public ICombatTurnState GetCurrentState() => currentState;

        /// <summary>
        /// 상태 생성 팩토리를 반환합니다.
        /// </summary>
        public ICombatStateFactory GetStateFactory() => stateFactory;

        #endregion

        #region 리셋 및 턴 준비 상태 관리

        /// <summary>
        /// 상태, 슬롯 예약, 준비 상태 등을 모두 초기화합니다.
        /// </summary>
        public void Reset()
        {
            currentState = null;
            pendingNextState = null;
            reservedEnemySlot = null;
            isTurnReady = false;
        }

        /// <summary>
        /// 턴 시작 준비가 되었는지 여부를 확인하고 이벤트를 발생시킵니다. (레거시 시스템용)
        /// 1번 슬롯에 카드가 있으면 턴 시작 가능합니다.
        /// </summary>
        public void UpdateTurnReady()
        {
#pragma warning disable CS0618 // 레거시 호환성을 위해 의도적으로 사용
            bool ready = cardRegistry.GetCardInSlot(CombatSlotPosition.SLOT_1) != null;
#pragma warning restore CS0618

            if (isTurnReady != ready)
            {
                isTurnReady = ready;
                OnTurnReadyChanged?.Invoke(isTurnReady);
            }
        }

        #endregion

        #region 카드 등록 및 턴 흐름 제어

        /// <summary>
        /// 슬롯에 카드를 등록하고 턴 준비 상태를 갱신합니다.
        /// </summary>
        /// <param name="slot">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">소유자</param>
        public void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            cardRegistry.RegisterCard(slot, card, ui, owner);
            UpdateTurnReady();
        }

        /// <summary>
        /// 턴을 시작할 수 있는 상태인지 확인합니다.
        /// </summary>
        public bool CanStartTurn() => isTurnReady;

        /// <summary>
        /// 현재 상태가 플레이어 입력 상태인지 확인합니다.
        /// </summary>
        public bool IsPlayerInputTurn() => currentState is State.CombatPlayerInputState;

        #endregion

        #region 적 슬롯 예약

        /// <summary>
        /// 적 카드를 예약할 슬롯을 설정합니다.
        /// </summary>
        /// <param name="slot">예약할 슬롯 위치</param>
        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
        }

        /// <summary>
        /// 예약된 적 카드 슬롯 정보를 반환합니다.
        /// </summary>
        public CombatSlotPosition? GetReservedEnemySlot()
        {
            return reservedEnemySlot;
        }

        #endregion

        #region 저장 시스템용 메서드

        /// <summary>
        /// 현재 턴을 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="turn">설정할 턴</param>
        public void SetCurrentTurn(int turn)
        {
            currentTurn = turn;
            Debug.Log($"[CombatTurnManager] 현재 턴 설정: {currentTurn}");
        }

        /// <summary>
        /// 현재 턴을 반환합니다. (저장 시스템용)
        /// </summary>
        /// <returns>현재 턴 번호</returns>
        public int GetCurrentTurn()
        {
            return currentTurn;
        }

        #endregion

        #region 새로운 턴 관리 시스템 (최적화됨)

        /// <summary>
        /// 현재 턴 타입을 반환합니다. (최적화된 버전, 레거시 시스템용)
        /// </summary>
        /// <returns>턴 타입 (Player, Enemy, Unknown)</returns>
        public TurnType GetCurrentTurnType()
        {
#pragma warning disable CS0618 // 레거시 호환성을 위해 의도적으로 사용
            var card = cardRegistry.GetCardInSlot(CombatSlotPosition.SLOT_1);
#pragma warning restore CS0618
            if (card == null)
                return TurnType.Player;
            
            return card.GetOwner() == SlotOwner.ENEMY ? TurnType.Enemy : TurnType.Player;
        }

        /// <summary>
        /// 현재 턴이 플레이어 턴인지 확인합니다. (최적화된 버전)
        /// </summary>
        public bool IsPlayerTurn()
        {
            return GetCurrentTurnType() == TurnType.Player;
        }

        /// <summary>
        /// 현재 턴이 적 턴인지 확인합니다. (최적화된 버전)
        /// </summary>
        public bool IsEnemyTurn()
        {
            return GetCurrentTurnType() == TurnType.Enemy;
        }

        /// <summary>
        /// 다음 턴을 진행합니다. (최적화된 버전)
        /// </summary>
        public void ProceedToNextTurn()
        {
            var turnType = GetCurrentTurnType();
            
            switch (turnType)
            {
                case TurnType.Player:
                    Debug.Log("[CombatTurnManager] 플레이어 턴 - 카드 배치 대기");
                    // 플레이어가 카드를 배치할 때까지 대기
                    break;
                case TurnType.Enemy:
                    Debug.Log("[CombatTurnManager] 적 턴 - 카드 실행");
                    // 적 카드 실행 (CombatExecutorService.ExecuteImmediately() 호출)
                    break;
                default:
                    Debug.LogWarning("[CombatTurnManager] 알 수 없는 턴 타입");
                    break;
            }
        }

        /// <summary>
        /// 4번 슬롯에 새로운 적 카드를 등록합니다. (레거시 시스템용)
        /// </summary>
        /// <param name="card">등록할 적 스킬카드</param>
        public void RegisterEnemyCardInSlot4(ISkillCard card)
        {
#pragma warning disable CS0618 // 레거시 호환성을 위해 의도적으로 사용
            cardRegistry.RegisterCard(CombatSlotPosition.SLOT_4, card, null, SlotOwner.ENEMY);
#pragma warning restore CS0618
            Debug.Log($"[CombatTurnManager] 4번 슬롯에 적 카드 등록: {card.GetCardName()}");
        }

        #endregion

        #region 가드 효과

        /// <summary>
        /// 가드 효과를 적용합니다.
        /// 다음 슬롯의 적 스킬카드를 무효화시킵니다.
        /// </summary>
        public void ApplyGuardEffect()
        {
            // 가드 상태 설정 (다음 슬롯의 적 카드 무효화를 위해)
            Debug.Log("[CombatTurnManager] 가드 효과 적용됨 - 다음 슬롯의 적 스킬카드 무효화");
            
            // TODO: 실제 가드 로직 구현
            // 1. 가드 상태 플래그 설정
            // 2. 다음 슬롯의 적 카드 실행 시 무효화 처리
        }

        #endregion

        #region 새로운 5슬롯 시스템

        /// <summary>
        /// 현재 전투 단계를 반환합니다.
        /// </summary>
        /// <returns>현재 전투 단계</returns>
        public CombatPhase GetCurrentPhase()
        {
            return currentPhase;
        }

        /// <summary>
        /// 현재 셋업 단계를 반환합니다.
        /// </summary>
        /// <returns>현재 셋업 단계 (0~8)</returns>
        public int GetCurrentSetupStep()
        {
            return setupStep;
        }

        /// <summary>
        /// 셋업이 완료되었는지 확인합니다.
        /// </summary>
        /// <returns>셋업 완료 여부</returns>
        public bool IsSetupComplete()
        {
            return isSetupComplete;
        }

        /// <summary>
        /// 셋업 단계를 시작합니다.
        /// </summary>
        public void StartSetupPhase()
        {
            currentPhase = CombatPhase.Setup;
            setupStep = 0;
            isSetupComplete = false;
            
            Debug.Log("[CombatTurnManager] 셋업 단계 시작");
            OnCombatPhaseChanged?.Invoke(currentPhase);
            OnSetupStepChanged?.Invoke(setupStep);
        }

        /// <summary>
        /// 셋업 단계에서 다음 단계로 진행합니다.
        /// </summary>
        /// <param name="slotPosition">카드를 배치할 슬롯 위치</param>
        /// <param name="cardOwner">카드 소유자</param>
        public void ProceedSetupStep(CombatSlotPosition slotPosition, SlotOwner cardOwner)
        {
            if (currentPhase != CombatPhase.Setup || isSetupComplete)
            {
                Debug.LogWarning("[CombatTurnManager] 셋업 단계가 아니거나 이미 완료되었습니다.");
                return;
            }

            setupStep++;
            Debug.Log($"[CombatTurnManager] 셋업 단계 {setupStep}: {slotPosition}에 {cardOwner} 카드 배치");
            OnSetupStepChanged?.Invoke(setupStep);

            // 셋업 완료 조건 확인 (전투슬롯에 카드 배치)
            if (slotPosition == CombatSlotPosition.BATTLE_SLOT)
            {
                CompleteSetup();
            }
        }

        /// <summary>
        /// 셋업을 완료하고 전투 단계로 전환합니다.
        /// </summary>
        private void CompleteSetup()
        {
            isSetupComplete = true;
            currentPhase = CombatPhase.Battle;
            
            Debug.Log("[CombatTurnManager] 셋업 완료 - 전투 단계 시작");
            OnCombatPhaseChanged?.Invoke(currentPhase);
        }

        /// <summary>
        /// 새로운 5슬롯 시스템에서 현재 턴 타입을 반환합니다.
        /// </summary>
        /// <returns>턴 타입 (Player, Enemy, Unknown)</returns>
        public TurnType GetCurrentTurnTypeNew()
        {
            if (currentPhase == CombatPhase.Setup)
            {
                // 셋업 단계에서는 플레이어부터 시작
                return TurnType.Player;
            }

            if (currentPhase == CombatPhase.Battle)
            {
                // 전투 단계에서는 전투슬롯에 카드가 있으면 해당 소유자의 턴
                var battleSlotCard = cardRegistry.GetCardInSlot(CombatSlotPosition.BATTLE_SLOT);
                if (battleSlotCard != null)
                {
                    return battleSlotCard.GetOwner() == SlotOwner.ENEMY ? TurnType.Enemy : TurnType.Player;
                }
                
                // 전투슬롯이 비어있으면 플레이어 턴
                return TurnType.Player;
            }

            return TurnType.Unknown;
        }

        /// <summary>
        /// 새로운 5슬롯 시스템에서 턴을 완료합니다.
        /// 전투슬롯에서 카드 사용 시 호출됩니다.
        /// </summary>
        public void CompleteTurn()
        {
            if (currentPhase != CombatPhase.Battle)
            {
                Debug.LogWarning("[CombatTurnManager] 전투 단계가 아닙니다.");
                return;
            }

            currentTurn++;
            Debug.Log($"[CombatTurnManager] 턴 완료 - 현재 턴: {currentTurn}");
            
            // 카드 이동은 CombatExecutorService에서 처리
            // 여기서는 턴 완료 이벤트만 발생
        }

        /// <summary>
        /// 새로운 5슬롯 시스템에서 턴이 진행 가능한지 확인합니다.
        /// </summary>
        /// <returns>턴 진행 가능 여부</returns>
        public bool CanProceedTurn()
        {
            if (currentPhase == CombatPhase.Setup)
            {
                return !isSetupComplete;
            }

            if (currentPhase == CombatPhase.Battle)
            {
                // 전투슬롯에 카드가 있으면 턴 진행 가능
                var battleSlotCard = cardRegistry.GetCardInSlot(CombatSlotPosition.BATTLE_SLOT);
                return battleSlotCard != null;
            }

            return false;
        }

        #endregion

        private void OnTurnStart(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterAnimation(characterId, "turnStart", characterObject); // 제거
        }

        private void OnTurnEnd(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterAnimation(characterId, "turnEnd", characterObject); // 제거
        }

        private void OnSkillCardUsed(string cardId, GameObject cardObject)
        {
            // AnimationFacade.Instance.PlaySkillCardAnimation(cardId, "use", cardObject); // 제거
        }

        private void OnCharacterDeath(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterDeathAnimation(characterId, characterObject); // 제거
        }
    }
}
