using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CombatSystem.Context;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드의 인터페이스입니다.
    /// 카드 데이터, 슬롯 정보, 실행, 쿨타임 등을 포함합니다.
    /// </summary>
    public interface ISkillCard
    {
        #region 카드 데이터

        /// <summary>
        /// 카드에 연결된 데이터 객체입니다.
        /// </summary>
        SkillCardData CardData { get; }

        /// <summary>
        /// 카드 이름을 반환합니다.
        /// </summary>
        string GetCardName();

        /// <summary>
        /// 카드 설명을 반환합니다.
        /// </summary>
        string GetDescription();

        /// <summary>
        /// 카드의 아트워크 이미지를 반환합니다.
        /// </summary>
        Sprite GetArtwork();

        /// <summary>
        /// 특정 이펙트의 파워(적용 수치)를 반환합니다.
        /// </summary>
        /// <param name="effect">대상 이펙트</param>
        int GetEffectPower(SkillCardEffectSO effect);

        /// <summary>
        /// 카드에 포함된 이펙트를 생성합니다.
        /// </summary>
        List<SkillCardEffectSO> CreateEffects();

        #endregion

        #region 슬롯 및 소유자 정보

        /// <summary>
        /// 핸드 슬롯 위치를 설정합니다.
        /// </summary>
        void SetHandSlot(SkillCardSlotPosition slot);

        /// <summary>
        /// 핸드 슬롯 위치를 반환합니다.
        /// </summary>
        SkillCardSlotPosition? GetHandSlot();

        /// <summary>
        /// 전투 슬롯 위치를 설정합니다.
        /// </summary>
        void SetCombatSlot(CombatSlotPosition slot);

        /// <summary>
        /// 전투 슬롯 위치를 반환합니다.
        /// </summary>
        CombatSlotPosition? GetCombatSlot();

        /// <summary>
        /// 이 카드를 소유한 쪽(플레이어/적)을 반환합니다.
        /// </summary>
        SlotOwner GetOwner();

        /// <summary>
        /// 이 카드가 플레이어 소유인지 여부를 반환합니다.
        /// </summary>
        bool IsFromPlayer();

        #endregion

        #region 카드 실행

        /// <summary>
        /// 자동 실행 시 카드 효과를 처리합니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        void ExecuteCardAutomatically(ICardExecutionContext context);

        /// <summary>
        /// 소스와 타겟이 사전에 정해진 상태에서 카드 실행.
        /// </summary>
        void ExecuteSkill();

        /// <summary>
        /// 직접 소스와 타겟을 지정하여 카드 실행.
        /// </summary>
        void ExecuteSkill(ICharacter source, ICharacter target);

        /// <summary>
        /// 컨텍스트를 기반으로 카드의 시전자를 반환합니다.
        /// </summary>
        ICharacter GetOwner(ICardExecutionContext context);

        /// <summary>
        /// 컨텍스트를 기반으로 카드의 대상을 반환합니다.
        /// </summary>
        ICharacter GetTarget(ICardExecutionContext context);

        #endregion

        #region 쿨타임

        /// <summary>
        /// 카드의 최대 쿨타임을 반환합니다.
        /// </summary>
        int GetMaxCoolTime();

        /// <summary>
        /// 카드의 현재 쿨타임을 반환합니다.
        /// </summary>
        int GetCurrentCoolTime();

        /// <summary>
        /// 현재 쿨타임 값을 설정합니다.
        /// </summary>
        void SetCurrentCoolTime(int value);

        #endregion
    }
}
