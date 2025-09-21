using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Zenject;
using Game.CombatSystem.Context;
using Game.StageSystem.Interface;

namespace Game.CombatSystem.Utility
{
    public class CharacterDeathHandler : MonoBehaviour
    {
        [Inject] private TurnContext turnContext;
        [Inject] private IStageManager stageManager;
        // TODO: VictoryManager와 GameOverManager 구현 필요

        public void OnCharacterDied(ICharacter character)
        {
            Debug.Log($"[CharacterDeathHandler] 캐릭터 사망 처리: {character.GetCharacterName()}");
            
            if (character is EnemyCharacter)
            {
                turnContext.MarkEnemyDefeated();
            }
            else if (character.IsPlayerControlled())
            {
                // TODO: 게임 오버 처리 구현 필요
                Debug.Log("[CharacterDeathHandler] 플레이어 사망 - 게임 오버 처리 필요");
            }
        }

        public void OnEnemyDeath(ICharacter enemy)
        {
            Debug.Log($"[CharacterDeathHandler] 적 사망 처리: {enemy.GetCharacterName()}");
            turnContext.MarkEnemyDefeated();
        }
    }
}
