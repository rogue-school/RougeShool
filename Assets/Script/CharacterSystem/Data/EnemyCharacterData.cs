using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Deck;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 적 캐릭터의 설정 데이터를 보관하는 ScriptableObject입니다.
    /// 전투 시 능력치, 이미지, 덱, 패시브 효과 등을 제공합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character/Enemy Character Data")]
    public class EnemyCharacterData : ScriptableObject, ICharacterData
    {
        #region 기본 정보

        [field: Header("기본 정보")]

        /// <summary>
        /// 게임 내에서 표시될 적 캐릭터 이름입니다.
        /// </summary>
        [field: SerializeField]
        public string DisplayName { get; private set; }

        /// <summary>
        /// 캐릭터 이름 (별칭)
        /// </summary>
        /// <returns>캐릭터 이름</returns>
        public string CharacterName => DisplayName;

        /// <summary>
        /// 적 캐릭터의 최대 체력입니다.
        /// </summary>
        [field: SerializeField]
        public int MaxHP { get; private set; }

        /// <summary>
        /// 인덱스 UI 등에 사용될 전용 아이콘 이미지입니다.
        /// </summary>
        [field: SerializeField]
        public Sprite IndexIcon { get; private set; }

        /// <summary>
        /// 캐릭터의 Portrait 프리팹입니다.
        /// 각 캐릭터마다 독립적인 Portrait 프리팹을 설정할 수 있습니다.
        /// 비어 있으면 기본 Portrait GameObject를 사용합니다(폴백).
        /// </summary>
        [field: SerializeField]
        public GameObject PortraitPrefab { get; private set; }

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

        #region 캐릭터 이펙트

        [field: Header("캐릭터 이펙트")]

        /// <summary>
        /// 캐릭터 이펙트 목록입니다.
        /// 각 이펙트는 개별 설정을 가질 수 있습니다.
        /// </summary>
        [field: SerializeField]
        public List<CharacterEffectEntry> CharacterEffects { get; private set; } = new List<CharacterEffectEntry>();

        #endregion

        #region 페이즈 시스템

        [field: Header("페이즈 시스템")]

        /// <summary>
        /// 적 캐릭터의 페이즈 목록입니다.
        /// 체력 임계값에 따라 자동으로 페이즈가 전환됩니다.
        /// 빈 리스트이면 페이즈 시스템을 사용하지 않습니다.
        /// </summary>
        [field: SerializeField]
        public List<EnemyPhaseData> Phases { get; private set; } = new List<EnemyPhaseData>();

        /// <summary>
        /// 페이즈 시스템을 사용하는지 확인합니다
        /// </summary>
        /// <returns>페이즈 시스템 사용 여부</returns>
        public bool HasPhases => Phases != null && Phases.Count > 0;

        #endregion
    }
}
