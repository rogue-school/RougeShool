using UnityEngine;
using DG.Tweening;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.AnimationSystem.Controllers
{
	/// <summary>
	/// 캐릭터 애니메이션을 스크립트로 제어하는 컨트롤러
	/// 애니메이션 클립 대신 DOTween을 사용하여 프로그래밍 방식으로 애니메이션을 제어합니다.
	/// </summary>
	public class CharacterAnimationController
	{
		#region Data
		private readonly ScriptableObject characterData;
		private readonly string characterName;
		private readonly IAudioManager audioManager;
		#endregion
		
		#region Constructor
		public CharacterAnimationController(IAudioManager audioManager)
		{
			this.audioManager = audioManager;
		}
		#endregion
		
		#region Animation Settings
		[System.Serializable]
		public class AnimationSettings
		{
			[Header("등장 애니메이션")]
			public float spawnDuration = 1.0f;
			public Vector3 spawnStartScale = Vector3.zero;
			public Vector3 spawnEndScale = Vector3.one;
			public Ease spawnEase = Ease.OutBack;
			public bool useSpawnGlow = true;
			public Color spawnGlowColor = Color.blue;
			public float spawnGlowIntensity = 1.5f;
			public bool useSpawnRotation = false;
			public float spawnRotationAngle = 0f;
			public bool useSpawnFade = true;
			public float spawnFadeInTime = 0.5f;
			
			[Header("사망 애니메이션")]
			public float deathDuration = 1.5f;
			public Ease deathEase = Ease.InBack;
			public bool useDeathFade = true;
			public float deathFadeOutTime = 0.3f;
			public bool useDeathScale = true;
			public Vector3 deathEndScale = Vector3.zero;
			public bool useDeathRotation = true;
			public float deathRotationAngle = 360f;
			public Color deathGlowColor = Color.red;
			public float deathGlowIntensity = 2.0f;
			
			[Header("피해 애니메이션")]
			public float damageDuration = 0.3f;
			public Ease damageEase = Ease.OutBounce;
			public bool useDamageShake = true;
			public float damageShakeStrength = 0.1f;
			public bool useDamageFlash = true;
			public Color damageFlashColor = Color.red;
			public float damageFlashIntensity = 1.0f;
			
			[Header("치유 애니메이션")]
			public float healDuration = 0.8f;
			public Ease healEase = Ease.OutQuad;
			public bool useHealGlow = true;
			public Color healGlowColor = Color.green;
			public float healGlowIntensity = 1.8f;
			public bool useHealScale = true;
			public float healScaleMultiplier = 1.1f;
			
			[Header("사운드")]
			public bool useSpawnSound = true;
			public AudioClip spawnSound;
			public float spawnSoundVolume = 1.0f;
			public bool useDeathSound = true;
			public AudioClip deathSound;
			public float deathSoundVolume = 1.0f;
			public bool useDamageSound = true;
			public AudioClip damageSound;
			public float damageSoundVolume = 0.8f;
			public bool useHealSound = true;
			public AudioClip healSound;
			public float healSoundVolume = 0.9f;
		}
		
		private AnimationSettings settings;
		#endregion
		
		#region Constructor
		public CharacterAnimationController(ScriptableObject character)
		{
			characterData = character;
			characterName = character.name;
			InitializeDefaultSettings();
		}
		#endregion
		
		#region Initialization
		private void InitializeDefaultSettings()
		{
			settings = new AnimationSettings();
			
			// 캐릭터 타입에 따른 기본 설정 적용
			if (characterData is PlayerCharacterData playerChar)
			{
				ApplyPlayerCharacterDefaults();
			}
			else if (characterData is EnemyCharacterData enemyChar)
			{
				ApplyEnemyCharacterDefaults();
			}
		}
		
		private void ApplyPlayerCharacterDefaults()
		{
			// 플레이어 캐릭터 기본 설정
			settings.spawnGlowColor = Color.blue;
			settings.healGlowColor = Color.green;
			settings.damageFlashColor = Color.red;
		}
		
		private void ApplyEnemyCharacterDefaults()
		{
			// 적 캐릭터 기본 설정
			settings.spawnGlowColor = Color.red;
			settings.deathGlowColor = Color.magenta;
			settings.damageFlashColor = new Color(1f, 0.5f, 0f, 1f); // Orange
		}
		#endregion
		
		#region Public API
		/// <summary>
		/// 애니메이션을 실행합니다.
		/// </summary>
		/// <param name="animationType">애니메이션 타입 (Spawn, Death, Damage, Heal)</param>
		/// <param name="target">애니메이션 대상 오브젝트</param>
		public void PlayAnimation(string animationType, GameObject target)
		{
			if (target == null)
			{
				Debug.LogError($"[CharacterAnimationController] 대상 오브젝트가 null입니다: {characterName}");
				return;
			}
			
			switch (animationType.ToLower())
			{
				case "spawn":
					PlaySpawnAnimation(target);
					break;
				case "death":
					PlayDeathAnimation(target);
					break;
				case "damage":
					PlayDamageAnimation(target);
					break;
				case "heal":
					PlayHealAnimation(target);
					break;
				default:
					Debug.LogWarning($"[CharacterAnimationController] 알 수 없는 애니메이션 타입: {animationType}");
					break;
			}
		}
		
		/// <summary>
		/// 애니메이션 설정을 가져옵니다.
		/// </summary>
		public AnimationSettings GetSettings() => settings;
		
		/// <summary>
		/// 애니메이션 설정을 업데이트합니다.
		/// </summary>
		public void UpdateSettings(AnimationSettings newSettings)
		{
			settings = newSettings;
		}
		
		/// <summary>
		/// 특정 설정을 업데이트합니다.
		/// </summary>
		public void UpdateSpawnSettings(float duration, Ease ease, Vector3 startScale, Vector3 endScale, Color glowColor)
		{
			settings.spawnDuration = duration;
			settings.spawnEase = ease;
			settings.spawnStartScale = startScale;
			settings.spawnEndScale = endScale;
			settings.spawnGlowColor = glowColor;
		}
		
		public void UpdateDeathSettings(float duration, Ease ease, float fadeOutTime, Color glowColor)
		{
			settings.deathDuration = duration;
			settings.deathEase = ease;
			settings.deathFadeOutTime = fadeOutTime;
			settings.deathGlowColor = glowColor;
		}
		
		public void UpdateDamageSettings(float duration, Ease ease, float shakeStrength, Color flashColor)
		{
			settings.damageDuration = duration;
			settings.damageEase = ease;
			settings.damageShakeStrength = shakeStrength;
			settings.damageFlashColor = flashColor;
		}
		
		public void UpdateHealSettings(float duration, Ease ease, Color glowColor, float scaleMultiplier)
		{
			settings.healDuration = duration;
			settings.healEase = ease;
			settings.healGlowColor = glowColor;
			settings.healScaleMultiplier = scaleMultiplier;
		}
		#endregion
		
		#region Animation Implementations
		private void PlaySpawnAnimation(GameObject target)
		{
			// 초기 상태 설정
			target.transform.localScale = settings.spawnStartScale;
			
			// 스케일 애니메이션
			target.transform.DOScale(settings.spawnEndScale, settings.spawnDuration)
				.SetEase(settings.spawnEase);
			
			// 회전 애니메이션
			if (settings.useSpawnRotation)
			{
				target.transform.DORotate(new Vector3(0, 0, settings.spawnRotationAngle), settings.spawnDuration, RotateMode.FastBeyond360)
					.SetEase(settings.spawnEase);
			}
			
			// 페이드 인
			if (settings.useSpawnFade)
			{
				var canvasGroup = target.GetComponent<CanvasGroup>();
				if (canvasGroup == null)
				{
					canvasGroup = target.AddComponent<CanvasGroup>();
				}
				
				canvasGroup.alpha = 0f;
				canvasGroup.DOFade(1f, settings.spawnFadeInTime)
					.SetEase(settings.spawnEase);
			}
			
			// 글로우 이펙트
			if (settings.useSpawnGlow)
			{
				ApplyGlowEffect(target, settings.spawnGlowColor, settings.spawnGlowIntensity, settings.spawnDuration);
			}
			
			// 사운드 재생
			if (settings.useSpawnSound && settings.spawnSound != null)
			{
				PlaySound(settings.spawnSound, settings.spawnSoundVolume);
			}
			
			Debug.Log($"[CharacterAnimationController] 등장 애니메이션 실행: {characterName}");
		}
		
		private void PlayDeathAnimation(GameObject target)
		{
			// 페이드 아웃
			if (settings.useDeathFade)
			{
				var canvasGroup = target.GetComponent<CanvasGroup>();
				if (canvasGroup == null)
				{
					canvasGroup = target.AddComponent<CanvasGroup>();
				}
				
				canvasGroup.DOFade(0f, settings.deathFadeOutTime)
					.SetEase(settings.deathEase);
			}
			
			// 스케일 애니메이션
			if (settings.useDeathScale)
			{
				target.transform.DOScale(settings.deathEndScale, settings.deathDuration)
					.SetEase(settings.deathEase);
			}
			
			// 회전 애니메이션
			if (settings.useDeathRotation)
			{
				target.transform.DORotate(new Vector3(0, 0, settings.deathRotationAngle), settings.deathDuration, RotateMode.FastBeyond360)
					.SetEase(settings.deathEase);
			}
			
			// 글로우 이펙트
			ApplyGlowEffect(target, settings.deathGlowColor, settings.deathGlowIntensity, settings.deathDuration);
			
			// 사운드 재생
			if (settings.useDeathSound && settings.deathSound != null)
			{
				PlaySound(settings.deathSound, settings.deathSoundVolume);
			}
			
			Debug.Log($"[CharacterAnimationController] 사망 애니메이션 실행: {characterName}");
		}
		
		private void PlayDamageAnimation(GameObject target)
		{
			// 흔들림 효과
			if (settings.useDamageShake)
			{
				var originalPos = target.transform.position;
				target.transform.DOShakePosition(settings.damageDuration, settings.damageShakeStrength)
					.SetEase(settings.damageEase);
			}
			
			// 플래시 효과
			if (settings.useDamageFlash)
			{
				ApplyFlashEffect(target, settings.damageFlashColor, settings.damageFlashIntensity, settings.damageDuration);
			}
			
			// 사운드 재생
			if (settings.useDamageSound && settings.damageSound != null)
			{
				PlaySound(settings.damageSound, settings.damageSoundVolume);
			}
			
			Debug.Log($"[CharacterAnimationController] 피해 애니메이션 실행: {characterName}");
		}
		
		private void PlayHealAnimation(GameObject target)
		{
			// 글로우 이펙트
			if (settings.useHealGlow)
			{
				ApplyGlowEffect(target, settings.healGlowColor, settings.healGlowIntensity, settings.healDuration);
			}
			
			// 스케일 애니메이션
			if (settings.useHealScale)
			{
				var originalScale = target.transform.localScale;
				target.transform.DOScale(originalScale * settings.healScaleMultiplier, settings.healDuration)
					.SetEase(settings.healEase)
					.OnComplete(() => target.transform.DOScale(originalScale, settings.healDuration * 0.5f));
			}
			
			// 사운드 재생
			if (settings.useHealSound && settings.healSound != null)
			{
				PlaySound(settings.healSound, settings.healSoundVolume);
			}
			
			Debug.Log($"[CharacterAnimationController] 치유 애니메이션 실행: {characterName}");
		}
		#endregion
		
		#region Utility Methods
		private void ApplyGlowEffect(GameObject target, Color color, float intensity, float duration)
		{
			// 간단한 글로우 이펙트 구현
			var renderer = target.GetComponent<Renderer>();
			if (renderer != null)
			{
				var originalColor = renderer.material.color;
				renderer.material.DOColor(color * intensity, duration)
					.OnComplete(() => renderer.material.DOColor(originalColor, duration * 0.5f));
			}
		}
		
		private void ApplyFlashEffect(GameObject target, Color color, float intensity, float duration)
		{
			// 간단한 플래시 이펙트 구현
			var renderer = target.GetComponent<Renderer>();
			if (renderer != null)
			{
				var originalColor = renderer.material.color;
				renderer.material.DOColor(color * intensity, duration * 0.1f)
					.OnComplete(() => renderer.material.DOColor(originalColor, duration * 0.9f));
			}
		}
		
		private void PlaySound(AudioClip clip, float volume)
		{
			// 중앙 오디오 시스템 경유 재생
			if (clip == null)
			{
				Debug.LogWarning("[CharacterAnimationController] 재생할 클립이 없습니다.");
				return;
			}
			audioManager?.PlaySFXWithPool(clip, volume);
		}
		#endregion
	}
} 