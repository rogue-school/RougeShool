using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Utility;

namespace Game.IManager
{
    /// <summary>
    /// 적 캐릭터를 소환하는 매니저의 인터페이스입니다.
    /// </summary>
    public interface IEnemySpawnerManager
    {
        /// <summary>
        /// 초기에 적을 소환합니다. 일반적으로 전투 시작 시 호출됩니다.
        /// </summary>
        void SpawnInitialEnemy();

        /// <summary>
        /// 지정된 적 데이터를 기반으로 적을 인스턴스화하고 슬롯에 배치합니다.
        /// </summary>
        /// <param name="data">소환할 적 캐릭터 데이터</param>
        /// <returns>소환 결과 (새 적 여부 포함)</returns>
        EnemySpawnResult SpawnEnemy(EnemyCharacterData data);

        /// <summary>
        /// 현재까지 소환된 모든 적 캐릭터 리스트를 반환합니다.
        /// </summary>
        /// <returns>적 캐릭터 리스트</returns>
        List<EnemyCharacter> GetAllEnemies();

        /// <summary>
        /// StageManager를 주입합니다.
        /// </summary>
        /// <param name="stageManager">현재 스테이지 매니저</param>
        void InjectStageManager(IStageManager stageManager);
    }
}
