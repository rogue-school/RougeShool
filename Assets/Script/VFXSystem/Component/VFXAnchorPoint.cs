using UnityEngine;

namespace Game.VFXSystem.Component
{
    /// <summary>
    /// VFX 이펙트 포인트 컴포넌트
    /// 캐릭터 프리팹에 이 컴포넌트를 가진 자식 오브젝트를 추가하여 이펙트가 나타날 위치를 지정합니다
    /// </summary>
    public class VFXAnchorPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            // 에디터에서 포인트 위치를 시각적으로 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }

        private void OnDrawGizmosSelected()
        {
            // 선택된 경우 더 크게 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.15f);
        }
    }
}

