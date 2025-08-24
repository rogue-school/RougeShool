// SaveModels.cs
using System;

namespace Game.Save
{
    [Serializable]
    public class EnemyState
    {
        public string enemyId;
        public int currentHp;
        public bool isDead;
    }

    [Serializable]
    public class BattleSaveData
    {
        public int playerHp;
        public EnemyState[] enemies;
    }
}
