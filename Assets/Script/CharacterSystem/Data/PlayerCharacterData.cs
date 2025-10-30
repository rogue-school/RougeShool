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
        /// 캐릭터의 타입입니다. (Sword/Bow/Staff)
        /// </summary>
        [field: SerializeField]
        public PlayerCharacterType CharacterType { get; private set; }

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

        /// <summary>
        /// UI 전용(플레이어 HUD/패널용) 초상화 이미지입니다.
        /// 캐릭터 본체 스프라이트와 별개로, 비율/여백이 UI에 맞게 제작된 아트를 연결합니다.
        /// 비어 있으면 Portrait를 사용합니다(폴백).
        /// </summary>
        [field: SerializeField]
        public Sprite PlayerUIPortrait { get; private set; }

        /// <summary>
        /// 캐릭터의 문양(앰블렘) 이미지입니다.
        /// </summary>
        [field: SerializeField]
        public Sprite Emblem { get; private set; }

        /// <summary>
        /// 로비/프리뷰 등에서 표시할 캐릭터 설명입니다.
        /// 실제 전투 로직과 무관한 순수 UI 텍스트입니다.
        /// </summary>
        [field: SerializeField, TextArea(2, 4)]
        public string Description { get; private set; }

        #endregion

        #region 리소스 시스템

        /// <summary>
        /// 캐릭터의 최대 리소스입니다. (Bow: 화살, Staff: 마나, Sword: 0)
        /// </summary>
        [field: SerializeField]
        public int MaxResource { get; private set; }

        /// <summary>
        /// 캐릭터 생성 시 시작 리소스입니다. (0 ~ MaxResource 범위 권장)
        /// </summary>
        [field: SerializeField]
        public int InitialResource { get; private set; }

        /// <summary>
        /// 리소스 이름입니다. (예: "화살", "마나")
        /// </summary>
        [field: SerializeField]
        public string ResourceName { get; private set; }

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
