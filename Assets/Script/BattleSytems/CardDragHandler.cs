using UnityEngine;
using Game.Interface;

namespace Game.Battle
{
    /// <summary>
    /// 드래그 중인 카드 정보를 저장하고 접근할 수 있는 클래스입니다.
    /// 싱글턴 대신 정적 필드 방식으로 간단히 처리합니다.
    /// </summary>
    public class CardDragHandler : MonoBehaviour
    {
        public static ISkillCard CurrentCard { get; private set; }

        /// <summary>
        /// 드래그 시작 시 카드 등록
        /// </summary>
        public static void SetDraggedCard(ISkillCard card)
        {
            CurrentCard = card;
        }

        /// <summary>
        /// 드래그 종료 시 카드 초기화
        /// </summary>
        public static void Clear()
        {
            CurrentCard = null;
        }
    }
}
