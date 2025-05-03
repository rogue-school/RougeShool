using UnityEngine;
using Game.Interface;
using Game.Cards;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 전용 스킬 카드 데이터입니다.
    /// 카드의 기본 정보와 이펙트를 ScriptableObject로 연결합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Assets/Skill Cards/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject, ISkillCard
    {
        [Header("기본 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;
        [SerializeField] private int coolTime = 0;

        [Header("카드 효과 연결")]
        [SerializeField] private ScriptableObject effectObject;

        public string GetName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;

        /// <summary>
        /// 카드의 실제 효과 인스턴스를 생성합니다.
        /// </summary>
        public ICardEffect CreateEffect()
        {
            if (effectObject is ICardEffect effect)
                return effect;

            Debug.LogWarning($"[PlayerSkillCard] '{cardName}'에 효과가 설정되지 않았습니다.");
            return null;
        }
    }
}
