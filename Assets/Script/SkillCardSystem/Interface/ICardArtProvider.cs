using UnityEngine;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드 아트워크(Sprite)를 제공하는 데이터가 구현해야 하는 인터페이스입니다.
    /// </summary>
    public interface ICardArtProvider
    {
        /// <summary>
        /// 카드에 표시할 이미지(Sprite)를 반환합니다.
        /// </summary>
        Sprite GetArt();
    }
}
