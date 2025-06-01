using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.IManager;
using Zenject;

namespace Game.CombatSystem.Utility
{
    public class CharacterDeathHandler : MonoBehaviour, ICharacterDeathListener
    {
        [Inject] private IStageManager stageManager;
        [Inject] private IGameOverManager gameOverManager;
        [Inject] private IVictoryManager victoryManager;

        public void OnCharacterDied(ICharacter character)
        {
            Debug.Log($"[CharacterDeathHandler] 캐릭터 사망 감지: {character.GetCharacterName()}");

            if (character is EnemyCharacter)
            {
                Debug.Log("[CharacterDeathHandler] 적 사망 처리");

                // 다음 적이 있다면 소환
                if (stageManager.HasNextEnemy())
                {
                    Debug.Log("[CharacterDeathHandler] 다음 적 소환 시도");
                    stageManager.SpawnNextEnemy();
                }
                else
                {
                    Debug.Log("[CharacterDeathHandler] 모든 적 처치됨 → 승리 처리");
                    victoryManager.ProcessVictory();
                }
            }
            else if (character.IsPlayerControlled())
            {
                Debug.Log("[CharacterDeathHandler] 플레이어 사망 → 게임 오버 처리");
                gameOverManager.TriggerGameOver();
            }
            else
            {
                Debug.LogWarning("[CharacterDeathHandler] 알 수 없는 캐릭터 유형 사망");
            }
        }
    }
}
