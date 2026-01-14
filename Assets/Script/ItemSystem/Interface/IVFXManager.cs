using UnityEngine;

namespace Game.CoreSystem.Interface
{
	/// <summary>
	/// VFX 재생을 위한 최소 인터페이스입니다.
	/// 구현체는 오브젝트 풀 등을 내부에서 처리해야 합니다.
	/// </summary>
	public interface IVFXManager
	{
		/// <summary>
		/// 지정 위치에 VFX 프리팹을 재생합니다.
		/// </summary>
		void PlayEffect(GameObject vfxPrefab, Vector3 position);
	}
}
