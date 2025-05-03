using UnityEngine;
using Game.Interface;
using Game.Units;
using Game.Battle;
using Game.UI;

namespace Game.Battle
{
    /// <summary>
    /// 플레이어 전투 슬롯에 카드가 모두 등록되었는지 확인하고, 실행하는 매니저입니다.
    /// </summary>
    public class BattleSlotManager : MonoBehaviour
    {
        [SerializeField] private BattleCardSlotUI frontSlot;
        [SerializeField] private BattleCardSlotUI backSlot;

        [SerializeField] private PlayerUnit playerUnit;
        [SerializeField] private EnemyUnit enemyUnit;

        public bool IsReady()
        {
            return frontSlot.HasCard() && backSlot.HasCard();
        }

        public void StartBattle()
        {
            frontSlot.ExecuteEffect(playerUnit, enemyUnit);
            backSlot.ExecuteEffect(playerUnit, enemyUnit);

            frontSlot.Clear();
            backSlot.Clear();
        }
    }
}
