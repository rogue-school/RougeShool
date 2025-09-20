using UnityEngine;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 실행 결과를 나타내는 클래스
    /// </summary>
    [System.Serializable]
    public class ExecutionResult
    {
        [Header("실행 결과")]
        [Tooltip("실행 성공 여부")]
        public bool isSuccess;
        
        [Tooltip("실행된 명령")]
        public ExecutionCommand executedCommand;
        
        [Tooltip("데미지량")]
        public int damageDealt;
        
        [Tooltip("치유량")]
        public int healingAmount;
        
        [Tooltip("결과 메시지")]
        public string resultMessage = "";
        
        [Tooltip("실행 시간")]
        public float executionTime;
        
        public ExecutionResult(bool success, ExecutionCommand command, string message = "")
        {
            isSuccess = success;
            executedCommand = command;
            resultMessage = message;
            executionTime = Time.time;
        }
        
        public ExecutionResult(bool success, ExecutionCommand command, int damage, int healing, string message = "")
        {
            isSuccess = success;
            executedCommand = command;
            damageDealt = damage;
            healingAmount = healing;
            resultMessage = message;
            executionTime = Time.time;
        }
    }
}
