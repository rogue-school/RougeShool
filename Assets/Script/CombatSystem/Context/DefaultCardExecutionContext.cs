using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 카드 실행 시 사용되는 기본 실행 컨텍스트입니다.
    /// 카드, 시전자(Source), 대상자(Target)에 대한 정보를 포함합니다.
    /// </summary>
    public class DefaultCardExecutionContext : ICardExecutionContext
    {
        #region Properties

        /// <summary>
        /// 실행될 카드
        /// </summary>
        public ISkillCard Card { get; }

        /// <summary>
        /// 카드의 시전자 (플레이어 또는 적)
        /// </summary>
        public ICharacter Source { get; }

        /// <summary>
        /// 카드의 대상자 (플레이어 또는 적)
        /// </summary>
        public ICharacter Target { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// 기본 카드 실행 컨텍스트 생성자.
        /// </summary>
        /// <param name="card">실행할 카드</param>
        /// <param name="source">시전자</param>
        /// <param name="target">대상자</param>
        public DefaultCardExecutionContext(ISkillCard card, ICharacter source, ICharacter target)
        {
            Card = card;
            Source = source;
            Target = target;
        }

        #endregion

        #region Role Resolution

        /// <summary>
        /// 플레이어 캐릭터를 반환합니다.
        /// Source 또는 Target 중 하나가 플레이어여야 합니다.
        /// </summary>
        /// <returns>플레이어 캐릭터 또는 null</returns>
        public ICharacter GetPlayer()
        {
            return Source?.IsPlayerControlled() == true ? Source :
                   Target?.IsPlayerControlled() == true ? Target : null;
        }

        /// <summary>
        /// 적 캐릭터를 반환합니다.
        /// Source 또는 Target 중 하나가 적이어야 합니다.
        /// </summary>
        /// <returns>적 캐릭터 또는 null</returns>
        public ICharacter GetEnemy()
        {
            return Source?.IsPlayerControlled() == false ? Source :
                   Target?.IsPlayerControlled() == false ? Target : null;
        }

        #endregion
    }
}
