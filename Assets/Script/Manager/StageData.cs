using UnityEngine;
using Game.Data;

namespace Game.Data
{
    /// <summary>
    /// 하나의 스테이지에 등장할 적 데이터를 정의
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Stage/Stage Data")]
    public class StageData : ScriptableObject
    {
        public string stageName;
        public EnemyCharacterData[] enemies;
    }
}
