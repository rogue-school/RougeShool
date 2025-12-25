using System.Collections;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 슬롯 이동 전담 인터페이스
    /// 카드 슬롯 간 이동 및 초기 셋업을 담당합니다.
    /// </summary>
    public interface ISlotMovementController
    {
        /// <summary>
        /// 모든 슬롯의 카드를 앞으로 한 칸씩 이동시킵니다.
        /// 대기4 → 대기3 → 대기2 → 대기1 → 배틀슬롯
        /// </summary>
        IEnumerator MoveAllSlotsForwardRoutine();

        /// <summary>
        /// 초기 적 큐를 설정합니다.
        /// </summary>
        /// <param name="enemyData">적 데이터</param>
        /// <param name="enemyName">적 이름</param>
        IEnumerator SetupInitialEnemyQueueRoutine(EnemyCharacterData enemyData, string enemyName);

        /// <summary>
        /// 턴 시작 시 슬롯 전진을 수행합니다.
        /// 배틀 슬롯이 비어있으면 대기 슬롯을 앞으로 이동시킵니다.
        /// </summary>
        IEnumerator AdvanceQueueAtTurnStartRoutine();

        /// <summary>
        /// 4번 슬롯에 새로운 적 카드를 등록합니다
        /// </summary>
        /// <param name="card">등록할 적 카드</param>
        void RegisterEnemyCardInSlot4(ISkillCard card);

        /// <summary>
        /// 적 캐시를 초기화합니다. 적이 교체될 때 호출되어야 합니다.
        /// </summary>
        void ClearEnemyCache();

        /// <summary>
        /// 적 덱 캐시를 업데이트합니다 (페이즈 전환 시 사용)
        /// </summary>
        /// <param name="enemyData">새로운 적 데이터</param>
        /// <param name="enemyName">새로운 적 이름</param>
        void UpdateEnemyCache(EnemyCharacterData enemyData, string enemyName);

        /// <summary>
        /// 슬롯 상태를 완전히 리셋합니다 (소환 전환 시 사용)
        /// </summary>
        void ResetSlotStates();

        /// <summary>
        /// 소환/복귀 모드를 설정합니다
        /// </summary>
        /// <param name="isSummonMode">소환 모드 여부</param>
        void SetSummonMode(bool isSummonMode);

        /// <summary>
        /// 소환 모드를 해제합니다 (PlayerTurnState 진입 시 호출)
        /// </summary>
        void ClearSummonMode();

        /// <summary>
        /// 현재 소환/복귀 모드 상태를 반환합니다.
        /// </summary>
        bool IsSummonMode { get; }

        /// <summary>
        /// 슬롯 이동 중인지 확인합니다.
        /// </summary>
        bool IsAdvancingQueue { get; }

        /// <summary>
        /// 초기 슬롯 셋업이 완료되었는지 확인합니다.
        /// </summary>
        bool IsInitialSlotSetupCompleted { get; }
    }
}
