using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Zenject;
using Game.CombatSystem.Context;
using Game.StageSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// 캐릭터 사망 처리를 담당하는 핸들러입니다.
    /// 플레이어 사망 시 게임 오버, 적 사망 시 스테이지 진행을 처리합니다.
    /// </summary>
    public class CharacterDeathHandler : MonoBehaviour
    {
        [Inject] private TurnContext turnContext;
        [Inject] private IStageManager stageManager;

        public void OnCharacterDied(ICharacter character)
        {
            GameLogger.LogInfo($"캐릭터 사망 처리: {character.GetCharacterName()}", GameLogger.LogCategory.Combat);
            
            if (character is EnemyCharacter)
            {
                OnEnemyDeath(character);
            }
            else if (character.IsPlayerControlled())
            {
                OnPlayerDeath(character);
            }
        }

        /// <summary>
        /// 적 사망 처리
        /// </summary>
        public void OnEnemyDeath(ICharacter enemy)
        {
            GameLogger.LogInfo($"적 사망 처리: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
            
            // 턴 컨텍스트에 적 처치 기록
            turnContext.MarkEnemyDefeated();
            
            // 스테이지 매니저에 적 사망 알림
            stageManager.OnEnemyDeath(enemy);
        }

        /// <summary>
        /// 플레이어 사망 처리 (게임 오버)
        /// </summary>
        private void OnPlayerDeath(ICharacter player)
        {
            GameLogger.LogError($"플레이어 사망 - 게임 오버!", GameLogger.LogCategory.Combat);
            
            // 스테이지 실패 처리
            stageManager.FailStage();
            
            // 게임 오버 이벤트 발생 (향후 게임 오버 UI 표시용)
            OnGameOver?.Invoke();
        }

        /// <summary>
        /// 게임 오버 이벤트
        /// </summary>
        public event System.Action OnGameOver;
    }
}
