using UnityEngine;
using System.Collections.Generic;
using Game.Interface;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 전용 스킬 카드 데이터입니다. 다중 효과를 지원합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Assets/Skill Cards/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject, ISkillCard
    {
        [Header("기본 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;
        [SerializeField] private int coolTime = 0;

        [Header("카드 효과 (다중 효과 허용)")]
        [SerializeField] private List<ScriptableObject> effectObjects;

        public string GetName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;

        /// <summary>
        /// 단일 효과 대신, 모든 효과를 리스트로 반환합니다.
        /// </summary>
        public List<ICardEffect> CreateEffects()
        {
            List<ICardEffect> validEffects = new();

            foreach (var obj in effectObjects)
            {
                if (obj is ICardEffect effect)
                    validEffects.Add(effect);
            }

            return validEffects;
        }
    }
}