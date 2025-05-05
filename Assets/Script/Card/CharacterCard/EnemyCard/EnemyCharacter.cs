using UnityEngine;
using Game.Characters;
using Game.Cards;
using Game.Battle;

namespace Game.Enemy
{
    /// <summary>
    /// 적 캐릭터 클래스입니다. 인스펙터에서 SO를 자동으로 참조하여 초기화합니다.
    /// </summary>
    public class EnemyCharacter : CharacterBase
    {
        [Header("적 캐릭터 데이터")]
        [SerializeField] private EnemyCharacterData characterData;

        public SlotPosition SlotPosition { get; private set; }

        public void Initialize(EnemyCharacterData data, SlotPosition position)
        {
            characterData = data;
            SlotPosition = position;

            if (characterData != null)
            {
                base.Initialize(characterData.maxHP);
            }
            else
            {
                Debug.LogError($"[{name}] EnemyCharacterData가 연결되지 않았습니다.");
            }
        }

        public override string GetName() => characterData.characterName;
        public override Sprite GetPortrait() => characterData.portrait;
    }
}
