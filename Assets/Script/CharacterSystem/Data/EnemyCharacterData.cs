using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Deck;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 적 캐릭터의 설정 데이터를 보관하는 ScriptableObject입니다.
    /// 전투 시 능력치, 이미지, 덱, 패시브 효과 등을 제공합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character/Enemy Character Data")]
    public class EnemyCharacterData : ScriptableObject
    {
        #region 기본 정보

        [field: Header("기본 정보")]

        /// <summary>
        /// 게임 내에서 표시될 적 캐릭터 이름입니다.
        /// </summary>
        [field: SerializeField]
        public string DisplayName { get; private set; }

        /// <summary>
        /// 적 캐릭터의 최대 체력입니다.
        /// </summary>
        [field: SerializeField]
        public int MaxHP { get; private set; }

        /// <summary>
        /// UI에 표시될 캐릭터 초상화 이미지입니다.
        /// </summary>
        [field: SerializeField]
        public Sprite Portrait { get; private set; }

        #endregion

        #region 프리팹 참조

        [field: Header("프리팹 참조")]

        /// <summary>
        /// 이 캐릭터에 해당하는 프리팹 (씬에 배치될 오브젝트)
        /// </summary>
        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        #endregion

        #region 스킬 덱

        [field: Header("스킬 덱 (확률 기반)")]

        /// <summary>
        /// 적 캐릭터가 사용할 스킬 카드 덱입니다. (확률 기반으로 카드 선택)
        /// </summary>
        [field: SerializeField]
        public EnemySkillDeck EnemyDeck { get; private set; }

        #endregion

        #region 패시브 이펙트

        [field: Header("패시브 이펙트")]

        /// <summary>
        /// 전투 시작 시 적용되는 패시브 효과 리스트입니다.
        /// 각 이펙트는 ScriptableObject 형태이며, ICardEffect 등으로 캐스팅하여 사용됩니다.
        /// </summary>
        [field: SerializeField]
        public List<ScriptableObject> PassiveEffects { get; private set; }

        #endregion
    }
}
