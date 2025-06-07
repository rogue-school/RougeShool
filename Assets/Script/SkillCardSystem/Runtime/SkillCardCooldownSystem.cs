using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 턴 시작 시 스킬 카드 쿨타임을 감소시키는 시스템 (디버깅 로그 포함).
    /// </summary>
    public class SkillCardCooldownSystem
    {
        private readonly IPlayerHandManager handManager;

        public SkillCardCooldownSystem(IPlayerHandManager handManager)
        {
            this.handManager = handManager;
        }

        public void ReduceAllCooldowns()
        {
            Debug.Log("<color=lime>[CooldownSystem] ReduceAllCooldowns 호출됨</color>");

            int totalCards = 0;
            int reducedCards = 0;

            foreach (var (card, ui) in handManager.GetAllHandCards())
            {
                if (card == null)
                {
                    Debug.Log("[CooldownSystem] null 카드 스킵");
                    continue;
                }

                totalCards++;

                int cur = card.GetCurrentCoolTime();
                int max = card.GetMaxCoolTime();

                Debug.Log($"[CooldownSystem] 카드: {card.GetCardName()}, 현재 쿨타임: {cur}, 최대: {max}");

                if (cur > 0)
                {
                    int newVal = Mathf.Max(0, cur - 1);
                    card.SetCurrentCoolTime(newVal);
                    reducedCards++;

                    Debug.Log($"<color=yellow>[CooldownSystem] {card.GetCardName()} → 쿨타임 감소: {cur} → {newVal}</color>");
                }
                else
                {
                    Debug.Log($"[CooldownSystem] {card.GetCardName()}는 이미 쿨타임 없음");
                }

                // UI 갱신
                if (ui != null)
                {
                    ui.UpdateCoolTimeDisplay();
                }
            }

            Debug.Log($"<color=cyan>[CooldownSystem] 전체 카드: {totalCards}, 쿨타임 감소된 카드: {reducedCards}</color>");
        }
    }
}
