using UnityEngine;
using Game.Interface;
using Game.Slots;
using Game.Managers;
using Game.Characters;

namespace Game.UI.Combat
{
    /// <summary>
    /// 실제 전투가 실행되는 슬롯의 UI를 제어하는 컴포넌트입니다.
    /// 카드가 배치되고 실행되는 물리 슬롯을 나타냅니다.
    /// </summary>
    public class CombatExecutionSlotUI : MonoBehaviour, ICombatCardSlot
    {
        [SerializeField] private CombatSlotPosition position;
        private ISkillCard currentCard;

        public CombatSlotPosition GetCombatPosition() => position;

        public void SetCombatPosition(CombatSlotPosition newPosition)
        {
            this.position = newPosition;
        }

        public SlotOwner GetOwner()
        {
            return position == CombatSlotPosition.FIRST ? SlotOwner.ENEMY : SlotOwner.PLAYER;
        }

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard.SetCombatSlot(position); // 전투 슬롯 위치 주입
        }

        public ISkillCard GetCard()
        {
            return currentCard;
        }

        public void Clear()
        {
            currentCard = null;
        }

        public void ExecuteCardAutomatically()
        {
            if (currentCard == null) return;

            var enemy = EnemyManager.Instance.GetCurrentEnemy();
            var player = PlayerManager.Instance.GetPlayer();

            CharacterBase owner = (currentCard.GetCombatSlot() == CombatSlotPosition.FIRST)
                ? (CharacterBase)enemy
                : (CharacterBase)player;

            CharacterBase target = (owner == enemy)
                ? (CharacterBase)player
                : (CharacterBase)enemy;

            foreach (var effect in currentCard.CreateEffects())
            {
                int power = currentCard.GetEffectPower(effect);
                effect.ExecuteEffect(owner, target, power);
            }
        }
    }
}
