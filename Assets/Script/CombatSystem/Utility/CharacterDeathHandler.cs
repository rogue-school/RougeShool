using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.IManager;
using Zenject;
using Game.CombatSystem.Context;

namespace Game.CombatSystem.Utility
{
    public class CharacterDeathHandler : MonoBehaviour, ICharacterDeathListener
    {
        [Inject] private TurnContext turnContext;
        [Inject] private IStageManager stageManager;
        [Inject] private IVictoryManager victoryManager;
        [Inject] private IGameOverManager gameOverManager;

        public void OnCharacterDied(ICharacter character)
        {
            if (character is EnemyCharacter)
            {
                Debug.Log("[CharacterDeathHandler] 적 사망 처리");
                turnContext.MarkEnemyDefeated();

                if (stageManager.HasNextEnemy())
                    stageManager.SpawnNextEnemy();
                else
                    victoryManager.ProcessVictory();
            }
            else if (character.IsPlayerControlled())
            {
                Debug.Log("[CharacterDeathHandler] 플레이어 사망 → 게임 오버 처리");
                gameOverManager.TriggerGameOver();
            }
        }
    }
}
