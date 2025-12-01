using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 같은 캐릭터가 이전 턴에 사용했던 비-연계 스킬 카드를
    /// 지정된 횟수만큼 재실행하는 연계 효과입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ReplayPreviousTurnCardEffect", menuName = "SkillEffects/ReplayPreviousTurnCardEffect")]
    public class ReplayPreviousTurnCardEffectSO : SkillCardEffectSO
    {
        [Header("연계 설정")]
        [Tooltip("이전 턴 스킬을 몇 번 재실행할지 설정합니다")]
        [SerializeField] private int _repeatCount = 2;

        /// <summary>
        /// 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치 (현재는 재실행 횟수 보정에만 사용)</param>
        /// <returns>이전 턴 카드 재실행 명령</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            int finalRepeat = Mathf.Max(0, _repeatCount + power);
            return new ReplayPreviousTurnCardCommand(finalRepeat);
        }

        /// <summary>
        /// 직접 적용은 사용하지 않습니다. 실제 로직은 커맨드에서 처리합니다.
        /// </summary>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            // 실제 실행은 ReplayPreviousTurnCardCommand에서 처리합니다.
        }
    }
}


