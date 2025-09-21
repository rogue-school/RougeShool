using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.StageSystem.Data;

namespace Game.StageSystem.Interface
{
    public interface IStageManager
    {
        /// <summary>
        /// 다음 적이 존재하는지 확인합니다.
        /// </summary>
        /// <returns>다음 적이 존재하면 true, 없으면 false</returns>
        bool HasNextEnemy();

        /// <summary>
        /// 다음 적을 스폰합니다.
        /// 스폰이 불가능할 경우 내부적으로 처리하거나 무시할 수 있습니다.
        /// </summary>
        void SpawnNextEnemy();

        /// <summary>
        /// 현재 진행 중인 스테이지 데이터를 반환합니다.
        /// </summary>
        /// <returns>현재 스테이지 데이터</returns>
        StageData GetCurrentStage();

        /// <summary>
        /// 다음에 등장할 적의 데이터를 반환합니다. (미리보기 용도)
        /// </summary>
        /// <returns>다음 적 캐릭터 데이터</returns>
        EnemyCharacterData PeekNextEnemyData();

        /// <summary>
        /// 스테이지를 시작합니다.
        /// </summary>
        void StartStage();

        /// <summary>
        /// 스테이지를 실패 처리합니다.
        /// </summary>
        void FailStage();

        /// <summary>
        /// 적 사망 시 호출되는 메서드입니다.
        /// </summary>
        /// <param name="enemy">사망한 적 캐릭터</param>
        void OnEnemyDeath(ICharacter enemy);

        /// <summary>
        /// 현재 스테이지 진행 상태를 반환합니다.
        /// </summary>
        /// <returns>스테이지 진행 상태</returns>
        StageProgressState ProgressState { get; }

        /// <summary>
        /// 스테이지 완료 여부를 반환합니다.
        /// </summary>
        /// <returns>스테이지가 완료되었으면 true</returns>
        bool IsStageCompleted { get; }
    }
}
