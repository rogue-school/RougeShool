using Game.CharacterSystem.Data;
using Game.CombatSystem.Stage;

public interface IStageManager
{
    bool HasNextEnemy();
    void SpawnNextEnemy();
    StageData GetCurrentStage();
    EnemyCharacterData PeekNextEnemyData();
}
