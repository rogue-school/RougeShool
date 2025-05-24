using UnityEngine;
using Game.SkillCardSystem.Deck;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 플레이어 캐릭터의 스탯, 이미지, 덱 정보를 담는 데이터입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character/PlayerCharacterData")]
    public class PlayerCharacterData : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public int MaxHP { get; private set; }

        [field: SerializeField] public PlayerSkillDeck SkillDeck { get; private set; }
    }
}
