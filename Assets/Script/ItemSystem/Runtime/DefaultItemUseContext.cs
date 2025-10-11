using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CharacterSystem.Interface;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 기본 아이템 사용 컨텍스트 구현체입니다.
    /// </summary>
    public class DefaultItemUseContext : IItemUseContext
    {
        public ICharacter User { get; }
        public ICharacter Target { get; }
        public ItemDefinition ItemDefinition { get; }

        public DefaultItemUseContext(IActiveItem item, ICharacter user, ICharacter target)
        {
            User = user;
            Target = target;
            ItemDefinition = item.ItemDefinition;
        }
    }
}
