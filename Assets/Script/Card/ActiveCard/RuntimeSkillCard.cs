using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.Effect;

namespace Game.Cards
{
    /// <summary>
    /// 런타임에서 실제 전투에 사용되는 카드 인스턴스입니다.
    /// ScriptableObject 기반 카드 데이터와 수치 데이터를 결합하여 생성됩니다.
    /// </summary>
    public class RuntimeSkillCard : ISkillCard
    {
        private readonly string cardName;
        private readonly string description;
        private readonly Sprite artwork;
        private readonly List<ICardEffect> effects;
        private readonly int power;
        private readonly int coolTime;

        public RuntimeSkillCard(string name, string desc, Sprite art, List<ICardEffect> fx, int dmg, int cooldown)
        {
            cardName = name;
            description = desc;
            artwork = art;
            effects = fx;
            power = dmg;
            coolTime = cooldown;
        }

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;

        public List<ICardEffect> CreateEffects() => effects;

        public int GetEffectPower(ICardEffect effect) => power;
    }
}
