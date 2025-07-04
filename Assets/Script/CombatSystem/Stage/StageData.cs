using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Data;

namespace Game.CombatSystem.Stage
{
    [CreateAssetMenu(menuName = "Game/Stage/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("이 스테이지에 등장할 적 목록")]
        public List<EnemyCharacterData> enemies;
    }
}
