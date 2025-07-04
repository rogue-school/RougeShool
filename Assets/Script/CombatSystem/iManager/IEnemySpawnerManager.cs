using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Utility;

namespace Game.IManager
{
    /// <summary>
    /// 적 캐릭터의 생성 및 관리 기능을 제공하는 스폰 매니저 인터페이스입니다.
    /// 초기 적 생성, 특정 데이터 기반 생성, 전체 적 리스트 조회 기능을 포함합니다.
    /// </summary>
    public interface IEnemySpawnerManager
    {
        /// <summary>
        /// 전투 시작 시 초기 적을 스폰합니다.
        /// 보통 현재 스테이지의 기본 적 데이터를 사용합니다.
        /// </summary>
        void SpawnInitialEnemy();

        /// <summary>
        /// 특정 적 캐릭터 데이터를 기반으로 적을 스폰합니다.
        /// </summary>
        /// <param name="data">적 캐릭터 데이터</param>
        /// <returns>스폰 결과 (성공 여부 및 참조 정보 포함)</returns>
        EnemySpawnResult SpawnEnemy(EnemyCharacterData data);

        /// <summary>
        /// 적 프리팹 생성/데이터 주입 → 등장 애니메이션 → 슬롯/매니저 등록 → 콜백 호출까지 순차적으로 처리하는 코루틴
        /// </summary>
        System.Collections.IEnumerator SpawnEnemyWithAnimation(EnemyCharacterData data, System.Action<EnemySpawnResult> onComplete);

        /// <summary>
        /// 현재 스폰된 모든 적 캐릭터 목록을 반환합니다.
        /// </summary>
        /// <returns>적 캐릭터 리스트</returns>
        List<EnemyCharacter> GetAllEnemies();
    }
}
