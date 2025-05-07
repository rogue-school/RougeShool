using UnityEngine;
using Game.Characters;
using Game.Data;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 캐릭터. 캐릭터 데이터 기반으로 초기화되며, CharacterBase를 상속받습니다.
    /// </summary>
    public class PlayerCharacter : CharacterBase
    {
        [SerializeField] private PlayerCharacterData characterData;

        public PlayerCharacterData Data => characterData;

        public void Initialize(int maxHP)
        {
            SetMaxHP(maxHP);
        }

        private void Awake()
        {
            if (characterData != null)
            {
                SetMaxHP(characterData.maxHP);
                Debug.Log($"[PlayerCharacter] {characterData.displayName} 초기화 완료");
            }
        }

        public void SetCharacterData(PlayerCharacterData data)
        {
            characterData = data;
        }

        public override void Die()
        {
            base.Die();
            Debug.Log("[PlayerCharacter] 사망 → 게임 오버 처리 필요");
        }
    }
}
