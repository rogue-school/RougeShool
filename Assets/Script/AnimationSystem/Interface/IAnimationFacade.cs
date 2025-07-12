using UnityEngine;

namespace AnimationSystem.Interface
{
    /// <summary>
    /// 애니메이션 시스템의 메인 파사드 인터페이스
    /// 클라이언트는 이 인터페이스만 의존합니다.
    /// </summary>
    public interface IAnimationFacade
    {
        ICharacterAnimationFacade Character { get; }
        ISkillCardAnimationFacade SkillCard { get; }
        void LoadAllData();
        void PrintStatus();
    }
} 