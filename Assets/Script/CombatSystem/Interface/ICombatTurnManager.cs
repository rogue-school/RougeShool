using System;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Manager;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 턴의 상태 관리 및 카드 등록, 턴 흐름 제어를 담당하는 통합 인터페이스입니다.
    /// ITurnManager와 ICombatTurnManager를 통합하여 단순화했습니다.
    /// </summary>
    public interface ICombatTurnManager
    {
        /// <summary>
        /// 전투 턴 시스템을 초기화합니다.
        /// 초기 상태 설정 및 슬롯/카드 관련 초기 처리를 포함할 수 있습니다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// 전투 턴 시스템을 재설정합니다.
        /// 상태 및 내부 데이터 초기화.
        /// </summary>
        void Reset();

        /// <summary>
        /// 다음 턴 상태 전이를 예약합니다.
        /// </summary>
        /// <param name="nextState">전이할 다음 상태</param>
        void RequestStateChange(object nextState);

        /// <summary>
        /// 즉시 새로운 상태로 전이합니다.
        /// </summary>
        /// <param name="newState">전이할 상태</param>
        void ChangeState(object newState);

        /// <summary>
        /// 현재 턴 상태를 반환합니다.
        /// </summary>
        /// <returns>현재 턴 상태</returns>
        object GetCurrentState();

        /// <summary>
        /// 상태 생성 팩토리를 반환합니다.
        /// </summary>
        /// <returns>전투 상태 팩토리</returns>
        object GetStateFactory();

        /// <summary>
        /// 적이 사용할 슬롯을 예약합니다 (예: 후공 실행용).
        /// </summary>
        /// <param name="slot">예약할 슬롯</param>
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 예약된 적 슬롯을 가져옵니다.
        /// </summary>
        /// <returns>예약된 슬롯 위치, 없으면 null</returns>
        CombatSlotPosition? GetReservedEnemySlot();

        /// <summary>
        /// 현재 턴이 플레이어 입력 턴인지 확인합니다.
        /// </summary>
        /// <returns>플레이어 입력 턴 여부</returns>
        bool IsPlayerInputTurn();

        /// <summary>
        /// 전투 슬롯에 카드 및 UI를 등록합니다.
        /// </summary>
        /// <param name="slot">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">카드 소유자 (플레이어 또는 적)</param>
        void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardUI ui, SlotOwner owner);

        /// <summary>
        /// 현재 턴을 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="turn">설정할 턴</param>
        void SetCurrentTurn(int turn);

        /// <summary>
        /// 현재 턴을 반환합니다. (저장 시스템용)
        /// </summary>
        /// <returns>현재 턴 번호</returns>
        int GetCurrentTurn();

        /// <summary>
        /// 가드 효과를 적용합니다.
        /// 다음 슬롯의 적 스킬카드를 무효화시킵니다.
        /// </summary>
        void ApplyGuardEffect();

        /// <summary>
        /// 현재 턴이 플레이어 턴인지 확인합니다.
        /// 1번 슬롯이 비어있으면 플레이어 턴입니다.
        /// </summary>
        /// <returns>플레이어 턴 여부</returns>
        bool IsPlayerTurn();

        /// <summary>
        /// 현재 턴이 적 턴인지 확인합니다.
        /// 1번 슬롯에 적 스킬카드가 있으면 적 턴입니다.
        /// </summary>
        /// <returns>적 턴 여부</returns>
        bool IsEnemyTurn();

        /// <summary>
        /// 다음 턴을 진행합니다.
        /// 1번 슬롯의 카드를 실행하고 슬롯을 이동시킵니다.
        /// </summary>
        void ProceedToNextTurn();

        /// <summary>
        /// 4번 슬롯에 새로운 적 카드를 등록합니다.
        /// </summary>
        /// <param name="card">등록할 적 스킬카드</param>
        void RegisterEnemyCardInSlot4(ISkillCard card);
        
        #region ITurnManager 통합 기능
        
        /// <summary>현재 턴 타입</summary>
        TurnManager.TurnType CurrentTurn { get; }
        
        /// <summary>현재 턴 수</summary>
        int TurnCount { get; }
        
        /// <summary>게임 활성화 상태</summary>
        bool IsGameActive { get; }
        
        /// <summary>턴 시간 제한 (초)</summary>
        float TurnTimeLimit { get; }
        
        /// <summary>남은 턴 시간</summary>
        float RemainingTurnTime { get; }
        
        /// <summary>
        /// 특정 턴 타입으로 설정합니다.
        /// </summary>
        /// <param name="turnType">설정할 턴 타입</param>
        void SetTurn(TurnManager.TurnType turnType);
        
        /// <summary>
        /// 게임을 시작합니다.
        /// </summary>
        void StartGame();
        
        /// <summary>
        /// 게임을 종료합니다.
        /// </summary>
        void EndGame();
        
        /// <summary>
        /// 턴을 일시정지합니다.
        /// </summary>
        void PauseTurn();
        
        /// <summary>
        /// 턴을 재개합니다.
        /// </summary>
        void ResumeTurn();
        
        /// <summary>
        /// 턴 시간을 리셋합니다.
        /// </summary>
        void ResetTurnTimer();
        
        // 이벤트들
        /// <summary>턴이 변경될 때 발생하는 이벤트</summary>
        event Action<TurnManager.TurnType> OnTurnChanged;
        
        /// <summary>턴 카운트가 변경될 때 발생하는 이벤트</summary>
        event Action<int> OnTurnCountChanged;
        
        /// <summary>게임이 시작될 때 발생하는 이벤트</summary>
        event Action OnGameStarted;
        
        /// <summary>게임이 종료될 때 발생하는 이벤트</summary>
        event Action OnGameEnded;
        
        #endregion
    }
}
