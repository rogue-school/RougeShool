using Game.Domain.Card.ValueObjects;
using Game.Domain.Character.ValueObjects;

namespace Game.Domain.Card.Interfaces
{
    /// <summary>
    /// 도메인 레벨에서의 스킬 카드 인터페이스입니다.
    /// </summary>
    public interface ISkillCard
    {
        /// <summary>
        /// 카드의 정적 정의입니다.
        /// </summary>
        CardDefinition Definition { get; }

        /// <summary>
        /// 카드 ID입니다.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 카드 이름입니다.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 카드 설명입니다.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 카드의 전투 수치입니다.
        /// </summary>
        CardStats Stats { get; }

        /// <summary>
        /// 현재 리소스로 카드 사용 비용을 지불할 수 있는지 여부를 반환합니다.
        /// </summary>
        /// <param name="resource">플레이어 리소스</param>
        /// <returns>비용 지불 가능 여부</returns>
        bool CanPayCost(Resource resource);
    }
}


