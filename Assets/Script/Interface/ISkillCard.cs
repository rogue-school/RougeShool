// Game.Interface.ISkillCard.cs
using System.Collections.Generic;
using UnityEngine;

namespace Game.Interface
{
    public interface ISkillCard
    {
        string GetName();
        string GetDescription();
        Sprite GetArtwork();

        /// <summary>
        /// 카드가 생성하는 모든 효과 리스트 반환
        /// </summary>
        List<ICardEffect> CreateEffects();
    }
}
