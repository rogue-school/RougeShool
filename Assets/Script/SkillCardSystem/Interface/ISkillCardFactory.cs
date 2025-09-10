using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드 객체를 생성하는 팩토리 인터페이스입니다.
    /// 카드 데이터와 이펙트를 기반으로 카드 인스턴스를 만듭니다.
    /// </summary>
    public interface ISkillCardFactory
    {
        /// <summary>
        /// 적 전용 스킬 카드를 생성합니다.
        /// </summary>
        /// <param name="data">카드의 기본 데이터 (이름, 설명, 아트워크 등)</param>
        /// <param name="effects">카드에 포함될 효과 목록</param>
        /// <returns>적 소유의 스킬 카드 인스턴스</returns>
        ISkillCard CreateEnemyCard(SkillCardData data, List<SkillCardEffectSO> effects, string ownerCharacterName);

        /// <summary>
        /// 플레이어 전용 스킬 카드를 생성합니다.
        /// </summary>
        /// <param name="data">카드의 기본 데이터 (이름, 설명, 아트워크 등)</param>
        /// <param name="effects">카드에 포함될 효과 목록</param>
        /// <returns>플레이어 소유의 스킬 카드 인스턴스</returns>
        ISkillCard CreatePlayerCard(SkillCardData data, List<SkillCardEffectSO> effects, string ownerCharacterName);

        /// <summary>
        /// 공용 카드 정의(SkillCardDefinition)와 소유자 정보를 기반으로 카드를 생성합니다.
        /// </summary>
        /// <param name="definition">공용 카드 정의</param>
        /// <param name="owner">플레이어/적 소유자 구분</param>
        /// <param name="ownerCharacterName">소유 캐릭터 식별자(선택)</param>
        /// <returns>생성된 카드 인스턴스(정책 미허용 시 null)</returns>
        ISkillCard CreateFromDefinition(SkillCardDefinition definition, Owner owner, string ownerCharacterName = null);
    }
}
