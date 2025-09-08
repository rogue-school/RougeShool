using Game.CharacterSystem.Data;
using Game.CombatSystem.Stage;

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
    }
}
