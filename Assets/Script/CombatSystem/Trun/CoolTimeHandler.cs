using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.IManager;
using UnityEngine;

namespace Game.CombatSystem.CoolTime
{
    public class CoolTimeHandler : ICoolTimeHandler
    {
        private readonly IPlayerHandManager playerHandManager;

        public CoolTimeHandler(IPlayerHandManager playerHandManager)
        {
            this.playerHandManager = playerHandManager;
        }

        public void ReduceCoolTimes()
        {
            var handCards = playerHandManager.GetAllHandCards();

            foreach (var (card, ui) in handCards)
            {
                if (card == null) continue;

                int current = card.GetCurrentCoolTime();
                if (current > 0)
                {
                    card.SetCurrentCoolTime(current - 1);

                    if (ui is SkillCardUI cardUI)
                        cardUI.ShowCoolTime(card.GetCurrentCoolTime(), card.GetCurrentCoolTime() > 0);
                }
            }
        }
    }
}
