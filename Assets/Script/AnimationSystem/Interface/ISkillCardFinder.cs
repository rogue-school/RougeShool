using System.Collections.Generic;
using UnityEngine;

namespace AnimationSystem.Interface
{
    /// <summary>
    /// 스킬카드 찾기 서비스 인터페이스
    /// 비즈니스 로직을 파사드에서 분리하기 위한 인터페이스
    /// </summary>
    public interface ISkillCardFinder
    {
        /// <summary>
        /// 특정 캐릭터의 스킬카드들을 찾습니다.
        /// </summary>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="isPlayerCharacter">플레이어 캐릭터 여부</param>
        /// <returns>스킬카드 GameObject 리스트</returns>
        List<GameObject> FindCharacterSkillCards(string characterName, bool isPlayerCharacter);
        
        /// <summary>
        /// 스킬카드가 특정 캐릭터에 속하는지 확인합니다.
        /// </summary>
        /// <param name="skillCard">스킬카드</param>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="isPlayerCharacter">플레이어 캐릭터 여부</param>
        /// <returns>캐릭터에 속하는지 여부</returns>
        bool IsSkillCardBelongsToCharacter(Game.SkillCardSystem.Interface.ISkillCard skillCard, string characterName, bool isPlayerCharacter);
    }
} 