using UnityEngine;
using Game.Characters;
using Game.Data;

namespace Game.Enemy
{
    /// <summary>
    /// 적 캐릭터. EnemyCharacterData를 기반으로 초기화되며, CharacterBase를 상속합니다.
    /// </summary>
    public class EnemyCharacter : CharacterBase
    {
        [SerializeField] private EnemyCharacterData characterData;

        public EnemyCharacterData Data => characterData;

        public void Initialize(int maxHP)
        {
            SetMaxHP(maxHP);
        }
        private void Awake()
        {
            if (characterData != null)
            {
                SetMaxHP(characterData.maxHP);
                Debug.Log($"[EnemyCharacter] {characterData.displayName} 초기화 완료");
            }
        }

        public void SetCharacterData(EnemyCharacterData data)
        {
            characterData = data;
        }

        public override void Die()
        {
            base.Die();
            Debug.Log("[EnemyCharacter] 사망 → 전투 종료 처리 필요");
        }
    }
}
