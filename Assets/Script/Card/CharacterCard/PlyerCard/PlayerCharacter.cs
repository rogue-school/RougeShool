using UnityEngine;
using Game.Characters;
using Game.Cards;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 캐릭터 클래스입니다. 인스펙터에서 SO를 자동으로 참조하여 초기화합니다.
    /// </summary>
    public class PlayerCharacter : CharacterBase
    {
        [Header("플레이어 캐릭터 데이터")]
        [SerializeField] private PlayerCharacterData characterData;

        private void Awake()
        {
            if (characterData != null)
            {
                base.Initialize(characterData.maxHP);
            }
            else
            {
                Debug.LogError($"[{name}] PlayerCharacterData가 연결되지 않았습니다.");
            }
        }

        public override string GetName() => characterData.characterName;
        public override Sprite GetPortrait() => characterData.portrait;
    }
}
