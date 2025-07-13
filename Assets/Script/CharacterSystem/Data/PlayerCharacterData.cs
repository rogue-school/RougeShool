using UnityEngine;
using Game.SkillCardSystem.Deck;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 플레이어 캐릭터의 설정 정보를 담는 ScriptableObject입니다.
    /// 이름, 초상화, 최대 체력, 스킬 덱 등을 포함합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character/PlayerCharacterData")]
    public class PlayerCharacterData : ScriptableObject, ICharacterData
    {
        #region 기본 정보

        /// <summary>
        /// UI 등에서 표시할 캐릭터 이름입니다.
        /// </summary>
        [field: SerializeField]
        public string DisplayName { get; private set; }

        /// <summary>
        /// 캐릭터의 최대 체력입니다.
        /// </summary>
        [field: SerializeField]
        public int MaxHP { get; private set; }

        /// <summary>
        /// 캐릭터의 초상화 이미지입니다.
        /// </summary>
        [field: SerializeField]
        public Sprite Portrait { get; private set; }

        #endregion

        #region 스킬 덱

        /// <summary>
        /// 캐릭터가 사용하는 스킬 카드 덱입니다.
        /// </summary>
        [field: SerializeField]
        public PlayerSkillDeck SkillDeck { get; private set; }

        #endregion
    }
}
