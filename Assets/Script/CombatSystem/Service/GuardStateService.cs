using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 캐릭터의 가드 상태를 관리하는 서비스 구현체
    /// </summary>
    public class GuardStateService : IGuardStateService
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemyManager enemyManager;
        
        private const string COMPONENT_NAME = "GuardStateService";

        public GuardStateService(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        public void ClearAllGuardStates()
        {
            Debug.Log($"[{COMPONENT_NAME}] 모든 가드 상태 해제 시작");
            
            ClearPlayerGuardState();
            ClearEnemyGuardState();
            
            Debug.Log($"[{COMPONENT_NAME}] 모든 가드 상태 해제 완료");
        }

        public void SetGuardState(ICharacter character, bool isGuarded)
        {
            if (character == null)
            {
                Debug.LogWarning($"[{COMPONENT_NAME}] 캐릭터가 null입니다.");
                return;
            }

            character.SetGuarded(isGuarded);
            Debug.Log($"[{COMPONENT_NAME}] {character.GetCharacterName()} 가드 상태 설정: {isGuarded}");
        }

        public bool IsGuarded(ICharacter character)
        {
            if (character == null)
            {
                Debug.LogWarning($"[{COMPONENT_NAME}] 캐릭터가 null입니다.");
                return false;
            }

            return character.IsGuarded();
        }

        public void ClearPlayerGuardState()
        {
            var player = playerManager?.GetPlayer();
            if (player != null && player.IsGuarded())
            {
                player.SetGuarded(false);
                Debug.Log($"[{COMPONENT_NAME}] 플레이어 가드 상태 해제: {player.GetCharacterName()}");
            }
        }

        public void ClearEnemyGuardState()
        {
            var enemy = enemyManager?.GetEnemy();
            if (enemy != null && enemy.IsGuarded())
            {
                enemy.SetGuarded(false);
                Debug.Log($"[{COMPONENT_NAME}] 적 가드 상태 해제: {enemy.GetCharacterName()}");
            }
        }
    }
} 