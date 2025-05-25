using UnityEngine;
using Game.CombatSystem.Interface;

public class BattleSceneInitializer : MonoBehaviour
{
    private IBattleResetService _resetService;

    public void Construct(IBattleResetService resetService)
    {
        _resetService = resetService;
    }

    private void Awake()
    {
        _resetService?.ResetAll();
    }
}
