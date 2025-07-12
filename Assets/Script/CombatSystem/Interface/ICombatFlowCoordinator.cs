using System;
using System.Collections;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 흐름의 전체 과정을 조율하는 인터페이스입니다.
    /// 카드 등록, 공격 처리, 상태 전이, UI 제어 등 전투의 메인 컨트롤 역할을 수행합니다.
    /// </summary>
    public interface ICombatFlowCoordinator
    {
        #region 초기화 및 준비
        void RegisterCardToTurnRegistry(CombatSlotPosition pos, ISkillCard card, ISkillCardUI ui);
        IEnumerator RegisterCardToCombatSlotCoroutine(CombatSlotPosition pos, ISkillCard card, ISkillCardUI ui);
        IEnumerator RegisterCardToCombatSlotAsync(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui);

        /// <summary>
        /// 슬롯 초기화, 캐릭터 배치 등 전투 준비 절차를 수행합니다.
        /// </summary>
        IEnumerator PerformCombatPreparation();

        /// <summary>
        /// 전투 준비 완료 후 콜백을 실행하는 비동기 초기화 메서드입니다.
        /// </summary>
        /// <param name="onComplete">전투 준비 완료 시 호출될 콜백</param>
        IEnumerator PerformCombatPreparation(Action<bool> onComplete);

        /// <summary>
        /// 전투 흐름을 시작합니다. (상태 초기화 포함)
        /// </summary>
        void StartCombatFlow();

        /// <summary>
        /// 비동기 방식으로 전투 준비를 요청합니다.
        /// </summary>
        /// <param name="onComplete">준비 완료 후 호출될 콜백</param>
        void RequestCombatPreparation(Action<bool> onComplete);

        /// <summary>
        /// 턴 상태 매니저와 상태 팩토리를 주입합니다.
        /// </summary>
        void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory);

        #endregion

        #region 전투 흐름

        /// <summary>
        /// 선공 캐릭터의 카드 실행 절차를 수행합니다.
        /// </summary>
        IEnumerator PerformFirstAttack();

        /// <summary>
        /// 후공 캐릭터의 카드 실행 절차를 수행합니다.
        /// </summary>
        IEnumerator PerformSecondAttack();

        /// <summary>
        /// 공격 후 결과를 처리합니다. (체력 감소, 사망 처리 등)
        /// </summary>
        IEnumerator PerformResultPhase();

        /// <summary>
        /// 전투 승리 시 후속 처리 절차를 수행합니다.
        /// </summary>
        IEnumerator PerformVictoryPhase();

        /// <summary>
        /// 게임 오버 시 처리 절차를 수행합니다.
        /// </summary>
        IEnumerator PerformGameOverPhase();

        /// <summary>
        /// 선공 공격을 비동기 요청 방식으로 실행합니다.
        /// </summary>
        /// <param name="onComplete">완료 후 호출될 콜백</param>
        void RequestFirstAttack(Action onComplete = null);

        #endregion

        #region 입력 제어

        /// <summary>
        /// 플레이어의 카드 입력을 활성화합니다.
        /// </summary>
        void EnablePlayerInput();

        /// <summary>
        /// 플레이어의 카드 입력을 비활성화합니다.
        /// </summary>
        void DisablePlayerInput();

        /// <summary>
        /// 현재 플레이어가 입력 가능한 상태인지 반환합니다.
        /// </summary>
        /// <returns>입력 가능 여부</returns>
        bool IsPlayerInputEnabled();

        #endregion

        #region 적 관리

        /// <summary>
        /// 다음 적을 생성 및 배치합니다.
        /// </summary>
        void SpawnNextEnemy();

        /// <summary>
        /// 현재 적 캐릭터를 전장에서 제거합니다.
        /// </summary>
        void RemoveEnemyCharacter();

        /// <summary>
        /// 적의 핸드 슬롯을 초기화합니다.
        /// </summary>
        IEnumerator ClearEnemyHandSafely();

        /// <summary>
        /// 적 전투 슬롯에 등록된 카드만 제거합니다.
        /// </summary>
        void ClearEnemyCombatSlots();

        /// <summary>
        /// 현재 전투 중인 적 캐릭터를 반환합니다.
        /// </summary>
        IEnemyCharacter GetEnemy();

        /// <summary>
        /// 현재 적 캐릭터가 존재하는지 여부를 반환합니다.
        /// </summary>
        bool HasEnemy();

        /// <summary>
        /// 현재 적 캐릭터가 사망했는지 여부를 반환합니다.
        /// </summary>
        bool IsEnemyDead();

        /// <summary>
        /// 다음 전투에 참여할 적이 존재하는지 확인합니다.
        /// </summary>
        bool CheckHasNextEnemy();

        /// <summary>
        /// 현재 턴이 적 선공인지 여부를 반환합니다.
        /// </summary>
        bool IsEnemyFirst { get; set; }


        #endregion

        #region 플레이어 상태

        /// <summary>
        /// 플레이어 캐릭터가 사망했는지 여부를 반환합니다.
        /// </summary>
        bool IsPlayerDead();

        #endregion

        #region 카드 관리

        /// <summary>
        /// 카드와 UI를 특정 전투 슬롯에 등록합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <param name="card">카드 데이터</param>
        /// <param name="ui">카드 UI</param>
        void RegisterCardToCombatSlot(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui);

        /// <summary>
        /// 슬롯에 등록된 카드 데이터를 반환합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        ISkillCard GetCardInSlot(CombatSlotPosition pos);

        /// <summary>
        /// 턴별 카드 등록 정보를 관리하는 레지스트리 객체를 반환합니다.
        /// </summary>
        ITurnCardRegistry GetTurnCardRegistry();

        #endregion

        #region UI 제어

        /// <summary>
        /// 플레이어 카드 선택 UI를 표시합니다.
        /// </summary>
        void ShowPlayerCardSelectionUI();

        /// <summary>
        /// 플레이어 카드 선택 UI를 숨깁니다.
        /// </summary>
        void HidePlayerCardSelectionUI();

        /// <summary>
        /// 턴 시작 버튼을 활성화합니다.
        /// </summary>
        void EnableStartButton();

        /// <summary>
        /// 턴 시작 버튼을 비활성화합니다.
        /// </summary>
        void DisableStartButton();

        /// <summary>
        /// 턴 시작 버튼 클릭 시 실행될 콜백을 등록합니다.
        /// </summary>
        void RegisterStartButton(Action onClick);

        /// <summary>
        /// 등록된 턴 시작 버튼 콜백을 해제합니다.
        /// </summary>
        void UnregisterStartButton();

        #endregion

        #region 클린업

        /// <summary>
        /// 전투 승리 후 상태 정리 작업을 수행합니다.
        /// </summary>
        IEnumerator CleanupAfterVictory();

        #endregion
    }
}
