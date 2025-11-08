using UnityEngine;

namespace Game.VFXSystem
{
    /// <summary>
    /// 이펙트의 지속 시간을 명시적으로 설정하는 컴포넌트입니다.
    /// 이펙트 프리팹에 이 컴포넌트를 추가하여 지속 시간을 설정할 수 있습니다.
    /// </summary>
    public class EffectDuration : MonoBehaviour
    {
        [Header("이펙트 지속 시간 설정")]
        [Tooltip("이펙트의 지속 시간 (초). 0보다 크면 이 값이 우선적으로 사용됩니다.")]
        [SerializeField] private float duration = 0f;

        /// <summary>
        /// 설정된 지속 시간을 반환합니다.
        /// </summary>
        public float Duration => duration;

        /// <summary>
        /// 지속 시간이 설정되어 있는지 확인합니다.
        /// </summary>
        public bool HasCustomDuration => duration > 0f;
    }
}

