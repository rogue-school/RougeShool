using UnityEngine;
using DG.Tweening;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;

namespace Game.AnimationSystem.Controllers
{
    /// <summary>
    /// 스킬카드 애니메이션을 스크립트로 제어하는 컨트롤러
    /// 애니메이션 클립 대신 DOTween을 사용하여 프로그래밍 방식으로 애니메이션을 제어합니다.
    /// </summary>
    public class SkillCardAnimationController
    {
        #region Data
        private readonly ScriptableObject cardData;
        private readonly string cardName;
        #endregion
        
        #region Animation Settings
        [System.Serializable]
        public class AnimationSettings
        {
            [Header("스폰 애니메이션")]
            public float spawnDuration = 0.6f;
            public Vector3 spawnStartScale = Vector3.zero;
            public Vector3 spawnEndScale = Vector3.one;
            public Ease spawnEase = Ease.OutBack;
            public bool useSpawnRotation = true;
            public float spawnRotationAngle = 360f;
            public bool useSpawnGlow = true;
            public Color spawnGlowColor = Color.yellow;
            public float spawnGlowIntensity = 1.5f;
            
            [Header("이동 애니메이션")]
            public float moveDuration = 0.4f;
            public Ease moveEase = Ease.OutCubic;
            public bool useArcMovement = true;
            public float arcHeight = 2f;
            public bool useMoveScale = true;
            public float moveScaleMultiplier = 1.1f;
            public bool useMoveRotation = true;
            public float moveRotationAngle = 15f;
            
            [Header("사용 이펙트")]
            public float useEffectDuration = 0.8f;
            public Ease useEffectEase = Ease.InOutQuad;
            public bool useUseEffectGlow = true;
            public Color useEffectGlowColor = Color.red;
            public float useEffectGlowIntensity = 2.0f;
            public bool useUseEffectRotation = true;
            public float useEffectRotationAngle = 180f;
            public bool useUseEffectScale = true;
            public float useEffectScaleMultiplier = 1.2f;
            public bool useUseEffectFade = false;
            
            [Header("사운드")]
            public bool useSpawnSound = true;
            public AudioClip spawnSound;
            public float spawnSoundVolume = 1.0f;
            public bool useMoveSound = false;
            public AudioClip moveSound;
            public float moveSoundVolume = 0.8f;
            public bool useUseEffectSound = true;
            public AudioClip useEffectSound;
            public float useEffectSoundVolume = 1.0f;
        }
        
        private AnimationSettings settings;
        #endregion
        
        #region Constructor
        public SkillCardAnimationController(ScriptableObject card)
        {
            cardData = card;
            cardName = card.name;
            InitializeDefaultSettings();
        }
        #endregion
        
        #region Initialization
        private void InitializeDefaultSettings()
        {
            settings = new AnimationSettings();
            
            // 카드 타입에 따른 기본 설정 적용
            if (cardData is PlayerSkillCard playerCard)
            {
                ApplyPlayerCardDefaults();
            }
            else if (cardData is EnemySkillCard enemyCard)
            {
                ApplyEnemyCardDefaults();
            }
        }
        
        private void ApplyPlayerCardDefaults()
        {
            // 플레이어 카드 기본 설정
            settings.spawnGlowColor = Color.blue;
            settings.useEffectGlowColor = Color.cyan;
        }
        
        private void ApplyEnemyCardDefaults()
        {
            // 적 카드 기본 설정
            settings.spawnGlowColor = Color.red;
            settings.useEffectGlowColor = Color.magenta;
        }
        #endregion
        
        #region Public API
        /// <summary>
        /// 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="animationType">애니메이션 타입 (Spawn, Move, UseEffect)</param>
        /// <param name="target">애니메이션 대상 오브젝트</param>
        public void PlayAnimation(string animationType, GameObject target)
        {
            if (target == null)
            {
                Debug.LogError($"[SkillCardAnimationController] 대상 오브젝트가 null입니다: {cardName}");
                return;
            }
            
            switch (animationType.ToLower())
            {
                case "spawn":
                    PlaySpawnAnimation(target);
                    break;
                case "move":
                    PlayMoveAnimation(target);
                    break;
                case "useeffect":
                    PlayUseEffectAnimation(target);
                    break;
                default:
                    Debug.LogWarning($"[SkillCardAnimationController] 알 수 없는 애니메이션 타입: {animationType}");
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
        public void UpdateSpawnSettings(float duration, Ease ease, Vector3 startScale, Vector3 endScale)
        {
            settings.spawnDuration = duration;
            settings.spawnEase = ease;
            settings.spawnStartScale = startScale;
            settings.spawnEndScale = endScale;
        }
        
        public void UpdateMoveSettings(float duration, Ease ease, bool useArc, float arcHeight)
        {
            settings.moveDuration = duration;
            settings.moveEase = ease;
            settings.useArcMovement = useArc;
            settings.arcHeight = arcHeight;
        }
        
        public void UpdateUseEffectSettings(float duration, Ease ease, Color glowColor, float glowIntensity)
        {
            settings.useEffectDuration = duration;
            settings.useEffectEase = ease;
            settings.useEffectGlowColor = glowColor;
            settings.useEffectGlowIntensity = glowIntensity;
        }
        #endregion
        
        #region Animation Implementations
        private void PlaySpawnAnimation(GameObject target)
        {
            // 초기 상태 설정
            target.transform.localScale = settings.spawnStartScale;
            
            // 스케일 애니메이션
            var scaleTween = target.transform.DOScale(settings.spawnEndScale, settings.spawnDuration)
                .SetEase(settings.spawnEase);
            
            // 회전 애니메이션
            if (settings.useSpawnRotation)
            {
                target.transform.DORotate(new Vector3(0, 0, settings.spawnRotationAngle), settings.spawnDuration, RotateMode.FastBeyond360)
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
            
            Debug.Log($"[SkillCardAnimationController] 스폰 애니메이션 실행: {cardName}");
        }
        
        private void PlayMoveAnimation(GameObject target)
        {
            // 아크 이동
            if (settings.useArcMovement)
            {
                var startPos = target.transform.position;
                var endPos = target.transform.position + Vector3.up * settings.arcHeight;
                
                target.transform.DOPath(new Vector3[] { startPos, endPos, startPos }, settings.moveDuration)
                    .SetEase(settings.moveEase);
            }
            
            // 스케일 애니메이션
            if (settings.useMoveScale)
            {
                target.transform.DOScale(target.transform.localScale * settings.moveScaleMultiplier, settings.moveDuration)
                    .SetEase(settings.moveEase);
            }
            
            // 회전 애니메이션
            if (settings.useMoveRotation)
            {
                target.transform.DORotate(new Vector3(0, 0, settings.moveRotationAngle), settings.moveDuration)
                    .SetEase(settings.moveEase);
            }
            
            // 사운드 재생
            if (settings.useMoveSound && settings.moveSound != null)
            {
                PlaySound(settings.moveSound, settings.moveSoundVolume);
            }
            
            Debug.Log($"[SkillCardAnimationController] 이동 애니메이션 실행: {cardName}");
        }
        
        private void PlayUseEffectAnimation(GameObject target)
        {
            // 글로우 이펙트
            if (settings.useUseEffectGlow)
            {
                ApplyGlowEffect(target, settings.useEffectGlowColor, settings.useEffectGlowIntensity, settings.useEffectDuration);
            }
            
            // 회전 애니메이션
            if (settings.useUseEffectRotation)
            {
                target.transform.DORotate(new Vector3(0, 0, settings.useEffectRotationAngle), settings.useEffectDuration, RotateMode.FastBeyond360)
                    .SetEase(settings.useEffectEase);
            }
            
            // 스케일 애니메이션
            if (settings.useUseEffectScale)
            {
                target.transform.DOScale(target.transform.localScale * settings.useEffectScaleMultiplier, settings.useEffectDuration)
                    .SetEase(settings.useEffectEase);
            }
            
            // 페이드 아웃
            if (settings.useUseEffectFade)
            {
                var canvasGroup = target.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = target.AddComponent<CanvasGroup>();
                }
                
                canvasGroup.DOFade(0, settings.useEffectDuration)
                    .SetEase(settings.useEffectEase);
            }
            
            // 사운드 재생
            if (settings.useUseEffectSound && settings.useEffectSound != null)
            {
                PlaySound(settings.useEffectSound, settings.useEffectSoundVolume);
            }
            
            Debug.Log($"[SkillCardAnimationController] 사용 이펙트 애니메이션 실행: {cardName}");
        }
        #endregion
        
        #region Utility Methods
        private void ApplyGlowEffect(GameObject target, Color color, float intensity, float duration)
        {
            // 간단한 글로우 이펙트 구현
            // 실제로는 더 복잡한 셰이더나 파티클 시스템을 사용할 수 있습니다
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                var originalColor = renderer.material.color;
                renderer.material.DOColor(color * intensity, duration)
                    .OnComplete(() => renderer.material.DOColor(originalColor, duration * 0.5f));
            }
        }
        
        private void PlaySound(AudioClip clip, float volume)
        {
            // AudioSource를 찾거나 생성하여 사운드 재생
            var audioSource = Object.FindFirstObjectByType<AudioSource>();
            if (audioSource == null)
            {
                var go = new GameObject("AnimationAudioSource");
                audioSource = go.AddComponent<AudioSource>();
            }
            
            audioSource.PlayOneShot(clip, volume);
        }
        #endregion
    }
} 