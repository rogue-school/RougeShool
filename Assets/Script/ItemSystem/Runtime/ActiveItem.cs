using System.Collections.Generic;
using UnityEngine;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Data;
using Game.ItemSystem.Effect;
using Game.ItemSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 액티브 아이템의 런타임 인스턴스를 나타내는 클래스입니다.
    /// 아이템 사용, 연출 처리, 효과 관리를 담당합니다.
    /// </summary>
    public class ActiveItem : IActiveItem
    {
        #region 필드
        
        private ActiveItemDefinition definition;
        private List<IItemEffectCommand> effectCommands = new();

        // 의존성 주입 (생성자에서 주입)
        private IAudioManager audioManager;
        private static ItemEffectCommandFactory effectFactory = new();
        
        #endregion
        
        #region 생성자
        
        /// <summary>
        /// 액티브 아이템 생성자
        /// </summary>
        /// <param name="definition">아이템 정의</param>
        /// <param name="audioManager">오디오 매니저</param>
        public ActiveItem(ActiveItemDefinition definition, IAudioManager audioManager)
        {
            this.definition = definition;
            this.audioManager = audioManager;
            
            SetupEffectCommands();
        }
        
        #endregion
        
        #region === 초기화 ===
        
        /// <summary>
        /// 액티브 아이템을 초기화합니다.
        /// </summary>
        /// <param name="definition">아이템 정의</param>
        public void Initialize(ActiveItemDefinition definition)
        {
            this.definition = definition;
            
            SetupEffectCommands();
        }
        
        /// <summary>
        /// 효과 명령들을 설정합니다.
        /// </summary>
        private void SetupEffectCommands()
        {
            effectCommands.Clear();

            // 효과 구성 처리
            if (definition.effectConfiguration.effects.Count > 0)
            {
                foreach (var effectConfig in definition.effectConfiguration.effects)
                {
                    if (effectConfig.effectSO == null) continue;

                    var command = effectFactory.CreateCommand(effectConfig);
                    if (command != null)
                    {
                        effectCommands.Add(command);
                    }
                }
            }

            // 실행 순서로 정렬
            SortCommandsByExecutionOrder();
        }

        /// <summary>
        /// 명령들을 실행 순서로 정렬합니다.
        /// </summary>
        private void SortCommandsByExecutionOrder()
        {
            effectCommands.Sort((a, b) =>
            {
                var orderA = GetExecutionOrder(a);
                var orderB = GetExecutionOrder(b);
                return orderA.CompareTo(orderB);
            });
        }
        
        private int GetExecutionOrder(IItemEffectCommand command)
        {
            // 정의에서 순서 가져오기
            foreach (var effectConfig in definition.effectConfiguration.effects)
            {
                if (effectConfig.effectSO != null)
                {
                    var testCommand = effectConfig.effectSO.CreateEffectCommand(0);
                    if (testCommand != null && testCommand.GetType() == command.GetType())
                    {
                        return effectConfig.executionOrder;
                    }
                }
            }
            
            return 999; // 기본값
        }
        
        #endregion
        
        #region === IActiveItem 구현 ===
        
        public ActiveItemDefinition ItemDefinition => definition;
        
        public string GetItemName() => definition?.DisplayName ?? "[Unnamed Item]";
        public string GetDescription() => definition?.Description ?? "[No Description]";
        public Sprite GetIcon() => definition?.Icon;
        
        public int GetEffectPower(ItemEffectSO effect)
        {
            // 커스텀 설정에서 가져오기
            foreach (var effectConfig in definition.effectConfiguration.effects)
            {
                if (effectConfig.effectSO == effect)
                {
                    if (effectConfig.useCustomSettings)
                    {
                        return GetCustomEffectPower(effectConfig.customSettings, effect);
                    }
                    return 0; // 기본값
                }
            }
            
            return 0;
        }
        
        private int GetCustomEffectPower(ItemEffectCustomSettings settings, ItemEffectSO effect)
        {
            if (effect is HealEffectSO && settings is HealEffectCustomSettings healSettings)
                return healSettings.healAmount;
            
            if (effect is AttackBuffEffectSO && settings is AttackBuffEffectCustomSettings buffSettings)
                return buffSettings.buffAmount;
            
            if (effect is RerollEffectSO && settings is RerollEffectCustomSettings rerollSettings)
                return rerollSettings.rerollCount;
            
            if (effect is ShieldBreakerEffectSO && settings is ShieldBreakerEffectCustomSettings shieldSettings)
                return shieldSettings.duration;
            
            return 0;
        }
        
        public List<ItemEffectSO> CreateEffects()
        {
            var effects = new List<ItemEffectSO>();
            
            foreach (var effectConfig in definition.effectConfiguration.effects)
            {
                if (effectConfig.effectSO != null)
                {
                    effects.Add(effectConfig.effectSO);
                }
            }
            
            return effects;
        }
        
        #endregion
        
        #region === 사용 관련 ===
        
        /// <summary>
        /// 아이템을 사용합니다.
        /// </summary>
        /// <param name="user">사용자</param>
        /// <param name="target">대상</param>
        public bool UseItem(ICharacter user, ICharacter target)
        {
            var context = new DefaultItemUseContext(this, user, target);
            return ExecuteItemAutomatically(context);
        }
        
        /// <summary>
        /// 아이템을 자동 실행합니다.
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        public bool ExecuteItemAutomatically(IItemUseContext context)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogWarning("[ActiveItem] 사용자가 null이거나 사망 상태입니다", GameLogger.LogCategory.Core);
                return false;
            }
            
            // 연출 시작
            StartPresentation(context);
            
            // 효과 실행
            bool success = ExecuteEffects(context);
            
            return success;
        }
        
        /// <summary>
        /// 효과들을 실행합니다.
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        private bool ExecuteEffects(IItemUseContext context)
        {
            bool allSuccess = true;
            
            foreach (var command in effectCommands)
            {
                if (!command.Execute(context))
                {
                    allSuccess = false;
                }
            }
            
            return allSuccess;
        }
        
        #endregion
        
        #region === 연출 처리 ===
        
        /// <summary>
        /// 연출을 시작합니다.
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        private void StartPresentation(IItemUseContext context)
        {
            var presentation = definition.presentation;
            
            // 사운드 재생 (즉시, 풀링 우선)
            if (presentation.sfxClip != null)
            {
                PlaySFXPooled(presentation.sfxClip);
            }
            
            // 비주얼 이펙트 생성 (즉시)
            if (presentation.visualEffectPrefab != null)
            {
                CreateVisualEffect(context, presentation);
            }
        }
        
        private void PlaySFXPooled(AudioClip clip)
        {
            if (audioManager == null || clip == null) return;
            // 전역 오디오 풀을 통한 재생으로 동시 재생/우선순위 대응
            audioManager.PlaySFXWithPool(clip, 0.9f);
        }
        
        private void CreateVisualEffect(IItemUseContext context, ItemPresentation presentation)
        {
            var target = context.Target;
            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null) return;

            // VFX 매니저를 통한 이펙트 생성 (Object Pooling 적용)
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<Game.VFXSystem.Manager.VFXManager>();
            if (vfxManager != null && presentation.visualEffectPrefab != null)
            {
                vfxManager.PlayEffect(presentation.visualEffectPrefab, targetTransform.position);
            }
        }
        
        #endregion
        
        #region === 유틸리티 ===
        
        /// <summary>
        /// 아이템 정의를 반환합니다.
        /// </summary>
        /// <returns>아이템 정의</returns>
        public ActiveItemDefinition GetDefinition() => definition;
        
        /// <summary>
        /// 연출 설정을 반환합니다.
        /// </summary>
        /// <returns>연출 설정</returns>
        public ItemPresentation GetPresentation() => definition.presentation;
        
        /// <summary>
        /// 효과 명령을 추가합니다.
        /// </summary>
        /// <param name="command">효과 명령</param>
        /// <param name="order">실행 순서</param>
        public void AddEffectCommand(IItemEffectCommand command, int order = 0)
        {
            effectCommands.Add(command);
            SetupEffectCommands(); // 재정렬
        }

        #endregion
    }
}
