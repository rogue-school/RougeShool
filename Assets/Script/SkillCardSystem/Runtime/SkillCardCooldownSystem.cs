using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 플레이어 핸드에 존재하는 모든 스킬 카드의 쿨타임을 턴 시작 시 감소시키는 시스템입니다.
    /// UI 연동 및 디버그 로그도 함께 수행합니다.
    /// </summary>
    public class SkillCardCooldownSystem
    {
        private readonly IPlayerHandManager handManager;

        /// <summary>
        /// 쿨타임 시스템 생성자
        /// </summary>
        /// <param name="handManager">플레이어 핸드 매니저</param>
        public SkillCardCooldownSystem(IPlayerHandManager handManager)
        {
            this.handManager = handManager;
        }

        /// <summary>
        /// 모든 핸드 카드의 쿨타임을 1 감소시키고, UI를 갱신합니다.
        /// 쿨타임이 이미 0인 카드는 영향을 받지 않습니다.
        /// </summary>
        public void ReduceAllCooldowns()
        {
            Debug.Log("<color=lime>[CooldownSystem] ReduceAllCooldowns 호출됨</color>");

            int totalCards = 0;
            int reducedCards = 0;

            foreach (var (card, ui) in handManager.GetAllHandCards())
            {
                if (card == null)
                {
                    Debug.LogWarning("[CooldownSystem] null 카드가 발견되어 건너뜁니다.");
                    continue;
                }

                totalCards++;

                int cur = card.GetCurrentCoolTime();
                int max = card.GetMaxCoolTime();

                Debug.Log($"[CooldownSystem] 카드: {card.GetCardName()}, 현재 쿨타임: {cur}, 최대 쿨타임: {max}");

                if (cur > 0)
                {
                    card.SetCurrentCoolTime(cur - 1);
                    reducedCards++;

                    Debug.Log($"<color=yellow>[CooldownSystem] {card.GetCardName()} → 쿨타임 감소: {cur} → {cur - 1}</color>");
                }
                else
                {
                    Debug.Log($"[CooldownSystem] {card.GetCardName()}는 이미 쿨타임이 0입니다.");
                }

                // UI 갱신
                if (ui != null)
                {
                    ui.UpdateCoolTimeDisplay();
                }
                else
                {
                    Debug.LogWarning($"[CooldownSystem] {card.GetCardName()}의 UI 참조가 null입니다.");
                }
            }

            Debug.Log($"<color=cyan>[CooldownSystem] 총 카드: {totalCards}, 쿨타임 감소된 카드 수: {reducedCards}</color>");
        }
    }
}
