using UnityEngine;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터 데이터의 공통 인터페이스입니다.
    /// 플레이어와 적 캐릭터 데이터가 모두 구현해야 하는 기본 속성들을 정의합니다.
    /// </summary>
    public interface ICharacterData
    {
        /// <summary>
        /// UI 등에서 표시할 캐릭터 이름입니다.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 캐릭터의 최대 체력입니다.
        /// </summary>
        int MaxHP { get; }
    }
} 