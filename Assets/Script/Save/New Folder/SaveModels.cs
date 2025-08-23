using System;

namespace Game.Save
{
    /// <summary>
    /// 적 1명의 저장 상태
    /// </summary>
    [Serializable]
    public class EnemyState
    {
        public string enemyId;     // ScriptableObject의 name (고유 ID 용도)
        public int currentHp;
        public bool isDead;
    }

    /// <summary>
    /// 전투(배틀) 씬에서 저장할 전체 데이터: 플레이어 HP + 적들 상태
    /// </summary>
    [Serializable]
    public class BattleSaveData
    {
        public int playerHp;
        public EnemyState[] enemies;
    }
}
