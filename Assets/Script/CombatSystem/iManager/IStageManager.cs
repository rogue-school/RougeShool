using Game.CombatSystem.Stage;

namespace Game.IManager
{
    /// <summary>
    /// 스테이지 진행과 적 등장 관리 기능을 제공하는 인터페이스입니다.
    /// </summary>
    public interface IStageManager
    {
        /// <summary>
        /// 다음 적이 존재하는지 여부를 반환합니다.
        /// </summary>
        bool HasNextEnemy();

        /// <summary>
        /// 다음 적을 스테이지에 스폰합니다.
        /// </summary>
        void SpawnNextEnemy();

        StageData GetCurrentStage();
    }
}