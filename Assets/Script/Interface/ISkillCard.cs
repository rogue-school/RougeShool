using System.Collections.Generic;
using UnityEngine;
using Game.Effect;

namespace Game.Interface
{
    /// <summary>
    /// 모든 스킬 카드(플레이어 및 적)가 구현해야 하는 공통 인터페이스입니다.
    /// </summary>
    public interface ISkillCard
    {
        /// <summary>
        /// 카드 이름 반환
        /// </summary>
        string GetCardName();

        /// <summary>
        /// 카드 설명 반환
        /// </summary>
        string GetDescription();

        /// <summary>
        /// 카드 아트워크 반환
        /// </summary>
        Sprite GetArtwork();

        /// <summary>
        /// 카드 쿨타임 (적은 0으로 고정 가능)
        /// </summary>
        int GetCoolTime();

        /// <summary>
        /// 연결된 효과 리스트를 반환
        /// </summary>
        List<ICardEffect> CreateEffects();

        /// <summary>
        /// 해당 효과에 대한 수치 (공격력 등)를 반환
        /// </summary>
        int GetEffectPower(ICardEffect effect);
    }
}
