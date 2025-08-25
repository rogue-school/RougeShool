using System;

namespace Game.Save
{
    [Serializable]
    public class EnemyState
    {
        public string enemyId;   // 예: EnemyCharacterData ScriptableObject의 name
        public int currentHp;
        public bool isDead;
    }

    [Serializable]
    public class BattleSaveData
    {
        public int playerHp;
        public EnemyState[] enemies;
        // 카드게임이어도, 당장 이어하기만 원하셨으니 HP/적 상태만 최소 저장
    }
}
