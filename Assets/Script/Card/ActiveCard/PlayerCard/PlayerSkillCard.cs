using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.Effect;

namespace Game.Player
{
    /// <summary>
    /// 플레이어가 사용하는 공격 스킬 카드의 ScriptableObject 정의입니다.
    /// 효과 및 카드 메타데이터만 포함합니다.
    /// 수치는 외부에서 주입됩니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Assets/Skill Cards/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject
    {
        [Header("카드 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;

        [Header("이 카드가 가지는 효과")]
        [SerializeField] private List<ScriptableObject> effectObjects;

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;

        /// <summary>
        /// 안전한 형변환을 통해 ICardEffect 리스트를 반환합니다.
        /// </summary>
        public List<ICardEffect> GetEffects()
        {
            List<ICardEffect> effects = new();
            foreach (var obj in effectObjects)
            {
                if (obj is ICardEffect effect)
                    effects.Add(effect);
            }
            return effects;
        }
    }
}
