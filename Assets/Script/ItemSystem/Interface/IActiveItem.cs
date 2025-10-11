using Game.ItemSystem.Data;
using Game.ItemSystem.Effect;
using Game.ItemSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.ItemSystem.Interface
{
    /// <summary>
    /// 액티브 아이템 인터페이스입니다.
    /// </summary>
    public interface IActiveItem
    {
        /// <summary>
        /// 아이템 정의를 반환합니다.
        /// </summary>
        ActiveItemDefinition ItemDefinition { get; }
        
        /// <summary>
        /// 아이템 이름을 반환합니다.
        /// </summary>
        string GetItemName();
        
        /// <summary>
        /// 아이템 설명을 반환합니다.
        /// </summary>
        string GetDescription();
        
        /// <summary>
        /// 아이템 아이콘을 반환합니다.
        /// </summary>
        UnityEngine.Sprite GetIcon();
        
        /// <summary>
        /// 효과 파워를 반환합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>파워 값</returns>
        int GetEffectPower(ItemEffectSO effect);
        
        /// <summary>
        /// 효과 목록을 생성합니다.
        /// </summary>
        /// <returns>효과 목록</returns>
        System.Collections.Generic.List<ItemEffectSO> CreateEffects();
        
        /// <summary>
        /// 아이템을 사용합니다.
        /// </summary>
        /// <param name="user">사용자</param>
        /// <param name="target">대상</param>
        /// <returns>사용 성공 여부</returns>
        bool UseItem(ICharacter user, ICharacter target);
        
        /// <summary>
        /// 아이템을 자동 실행합니다.
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        /// <returns>실행 성공 여부</returns>
        bool ExecuteItemAutomatically(IItemUseContext context);
    }
}
