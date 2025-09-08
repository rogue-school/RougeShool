using UnityEngine;

namespace Game.SkillCardSystem.Data
{
    /// <summary>
    /// 스킬 카드의 정적 정보를 담는 데이터 클래스입니다.
    /// 이름, 설명, 이미지, 쿨타임, 데미지 등 기본 정보를 포함합니다.
    /// </summary>
    [System.Serializable]
    public class SkillCardData
    {
        [Header("사운드")]
        public AudioClip SfxClip;

        [Header("비주얼 이펙트")]
        [Tooltip("카드 실행 시 재생할 비주얼 이펙트 프리팹")]
        public GameObject VisualEffectPrefab;

        [Tooltip("이펙트 지속 시간 (초)")]
        public float EffectDuration = 2f;

        /// <summary>
        /// 카드 고유 ID
        /// </summary>
        public string CardId;

        /// <summary>
        /// 카드 이름
        /// </summary>
        public string Name;

        /// <summary>
        /// 카드 이름 (별칭)
        /// </summary>
        public string CardName => Name;

        /// <summary>
        /// 카드 설명 (툴팁 등에서 사용)
        /// </summary>
        public string Description;

        /// <summary>
        /// 카드 아트워크 이미지
        /// </summary>
        public Sprite Artwork;

        /// <summary>
        /// 카드의 쿨타임 (턴 단위)
        /// </summary>
        public int CoolTime;

        /// <summary>
        /// 카드 사용 비용
        /// </summary>
        public int Cost;

        /// <summary>
        /// 카드가 주는 데미지 수치
        /// </summary>
        public int Damage;

        /// <summary>
        /// 카드 타입
        /// </summary>
        public string CardType;

        /// <summary>
        /// 카드 소유 캐릭터 이름(식별자)
        /// </summary>
        public string OwnerCharacterName;

        /// <summary>
        /// 카드 정보를 초기화하는 생성자입니다.
        /// 코드상에서 직접 SkillCardData를 생성할 경우 사용됩니다.
        /// </summary>
        /// <param name="name">카드 이름</param>
        /// <param name="description">설명</param>
        /// <param name="artwork">이미지</param>
        /// <param name="coolTime">쿨타임</param>
        /// <param name="damage">데미지</param>
        public SkillCardData(string name, string description, Sprite artwork, int coolTime, int damage)
        {
            Name = name;
            Description = description;
            Artwork = artwork;
            CoolTime = coolTime;
            Damage = damage;
        }
    }
}
