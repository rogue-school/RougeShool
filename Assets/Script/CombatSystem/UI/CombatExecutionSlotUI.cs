using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Enemy;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.UI
{
    public class CombatExecutionSlotUI : MonoBehaviour, ICombatCardSlot
    {
        [SerializeField] private CombatSlotPosition position;
        private ISkillCard currentCard;
        private SkillCardUI currentCardUI;

        public CombatSlotPosition GetCombatPosition() => position;
        public void SetCombatPosition(CombatSlotPosition newPosition) => position = newPosition;
        public SlotOwner GetOwner() => position == CombatSlotPosition.FIRST ? SlotOwner.ENEMY : SlotOwner.PLAYER;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard?.SetCombatSlot(position);
        }

        public ISkillCard GetCard() => currentCard;

        public void Clear()
        {
            currentCard = null;

            if (currentCardUI != null)
            {
                Destroy(currentCardUI.gameObject);
                currentCardUI = null;
            }

            Debug.Log($"[CombatExecutionSlotUI] 슬롯 클리어 완료: {gameObject.name}");
        }


        public void SetCardUI(SkillCardUI cardUI) => currentCardUI = cardUI;
        public SkillCardUI GetCardUI() => currentCardUI;

        public void ExecuteCardAutomatically()
        {
            if (currentCard == null)
            {
                Debug.LogWarning("[CombatExecutionSlotUI] currentCard가 null입니다.");
                return;
            }

            CharacterBase caster = null;
            CharacterBase target = null;

            if (currentCard.GetOwner() == SlotOwner.PLAYER)
            {
                caster = PlayerManager.Instance?.GetPlayer();
                target = EnemyManager.Instance?.GetCurrentEnemy();
            }
            else
            {
                caster = EnemyManager.Instance?.GetCurrentEnemy();
                target = PlayerManager.Instance?.GetPlayer();
            }

            if (caster == null || target == null || target.IsDead())
            {
                Debug.LogWarning($"[CombatExecutionSlotUI] 이펙트 실행 생략: 대상이 null 또는 사망 상태입니다.");
                return;
            }

            foreach (var effect in currentCard.CreateEffects())
            {
                if (effect == null) continue;

                int power = currentCard.GetEffectPower(effect);
                effect.ExecuteEffect(caster, target, power);

                string effectName = effect.GetType().Name;
                Debug.Log($"[CombatExecutionSlotUI] 실행됨 → {effectName}, {caster.name} → {target.name}, power: {power}");

                if (effect is IPerTurnEffect)
                {
                    Debug.Log($"[CombatExecutionSlotUI] 지속 이펙트 적용됨: {effectName}");
                }
            }
        }
    }
}
