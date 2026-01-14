using UnityEngine;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 실행 명령을 나타내는 클래스
    /// </summary>
    [System.Serializable]
    public class ExecutionCommand
    {
        [Header("실행 명령 설정")]
        [Tooltip("명령 타입")]
        public ExecutionCommandType commandType;
        
        [Tooltip("대상 캐릭터")]
        public GameObject targetCharacter;
        
        [Tooltip("실행할 카드")]
        public GameObject card;
        
        [Tooltip("명령 우선순위")]
        public int priority = 0;
        
        [Tooltip("명령 설명")]
        public string description = "";
        
        public ExecutionCommand(ExecutionCommandType type, GameObject target = null, GameObject cardObj = null)
        {
            commandType = type;
            targetCharacter = target;
            card = cardObj;
        }
    }
    
    /// <summary>
    /// 실행 명령 타입 열거형
    /// </summary>
    public enum ExecutionCommandType
    {
        Attack,     // 공격
        Defend,     // 방어
        Skill,      // 스킬
        Move,       // 이동
        Wait        // 대기
    }
}
