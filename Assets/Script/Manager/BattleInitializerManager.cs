using UnityEngine;

namespace Game.Managers
{
    /// <summary>
    /// 전투 씬에서 초기화 파사드를 호출하는 진입점 MonoBehaviour
    /// </summary>
    public class BattleInitializerManager : MonoBehaviour
    {
        private void Start()
        {
            Game.Battle.BattleStartupFacade.InitializeBattle();
        }
    }
}
