using UnityEngine;
using UnityEngine.EventSystems;
using Game.Interface;

namespace Game.Battle
{
    /// <summary>
    /// 드래그 가능한 카드 UI에 부착되어 카드 정보를 제공하는 핸들러입니다.
    /// </summary>
    public class CardDragHandler : MonoBehaviour
    {
        [SerializeField] private ISkillCard card;

        public bool HasCard() => card != null;

        public ISkillCard GetCard() => card;
    }
}
