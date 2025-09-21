using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Initializer
{
    /// <summary>
    /// 플레이어 핸드를 초기화하는 클래스입니다.
    /// 적 카드는 핸드 없이 대기 슬롯에서 직접 관리됩니다.
    /// </summary>
    public class HandInitializer
    {
        private readonly IPlayerHandManager playerHand;

        /// <summary>
        /// 핸드 초기화기 생성자입니다.
        /// </summary>
        /// <param name="playerHand">플레이어 핸드 매니저</param>
        public HandInitializer(IPlayerHandManager playerHand)
        {
            this.playerHand = playerHand;
        }

        /// <summary>
        /// 플레이어 핸드를 클리어합니다.
        /// 적 카드는 StageManager에서 WAIT_SLOT_4에 직접 생성됩니다.
        /// </summary>
        public void SetupHands()
        {
            playerHand.ClearAll();
            Debug.Log("<color=cyan>[HandInitializer] 플레이어 핸드 초기화 완료 (적 카드는 대기 슬롯에서 직접 관리)</color>");
        }

        /// <summary>
        /// 모든 슬롯을 자동으로 바인딩합니다.
        /// </summary>
        public void AutoBindAllSlots()
        {
            // 현재는 자동 바인딩 기능이 구현되지 않음
            Debug.Log("<color=cyan>[HandInitializer] 자동 슬롯 바인딩 기능은 현재 구현되지 않음</color>");
        }
    }
}
