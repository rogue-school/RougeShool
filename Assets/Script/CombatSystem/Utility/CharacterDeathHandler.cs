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
                turnContext.MarkEnemyDefeated();
            }
            else if (character.IsPlayerControlled())
            {
                gameOverManager.TriggerGameOver();
            }
        }
    }
}
