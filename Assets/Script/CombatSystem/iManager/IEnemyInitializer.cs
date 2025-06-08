using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;

namespace Game.IManager
{
    /// <summary>
    /// 적 캐릭터 초기화를 담당하는 인터페이스입니다.
    /// 외부 데이터로부터 적을 생성하고 접근할 수 있도록 합니다.
    /// </summary>
    public interface IEnemyInitializer
    {
        /// <summary>
        /// 적 캐릭터 데이터를 기반으로 초기화 작업을 수행합니다.
        /// </summary>
        /// <param name="data">초기화에 사용할 적 캐릭터 데이터</param>
        void SetupWithData(EnemyCharacterData data);

        /// <summary>
        /// 초기화 후 생성된 적 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>생성된 적 캐릭터 인스턴스</returns>
        IEnemyCharacter GetSpawnedEnemy();
    }
}
