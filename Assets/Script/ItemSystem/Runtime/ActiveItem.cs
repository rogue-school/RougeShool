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
		private IVFXManager _vfxManager; // 주입된 VFX 매니저
		
		#endregion
		
		#region 생성자
		
		/// <summary>
		/// 액티브 아이템 생성자
		/// </summary>
		/// <param name="definition">아이템 정의</param>
		/// <param name="audioManager">오디오 매니저</param>
		/// <param name="vfxManager">VFX 매니저(선택)</param>
		public ActiveItem(ActiveItemDefinition definition, IAudioManager audioManager, IVFXManager vfxManager = null)
		{
			this.definition = definition;
			this.audioManager = audioManager;
			this._vfxManager = vfxManager;
			
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

			GameLogger.LogInfo($"[ActiveItem] 효과 명령 설정 시작: {definition.DisplayName}", GameLogger.LogCategory.Core);

			// 효과 구성 처리
			if (definition.effectConfiguration.effects.Count > 0)
			{
				GameLogger.LogInfo($"[ActiveItem] 효과 개수: {definition.effectConfiguration.effects.Count}", GameLogger.LogCategory.Core);
				
				foreach (var effectConfig in definition.effectConfiguration.effects)
				{
					if (effectConfig.effectSO == null) 
					{
						GameLogger.LogWarning("[ActiveItem] effectSO가 null입니다", GameLogger.LogCategory.Core);
						continue;
					}

					GameLogger.LogInfo($"[ActiveItem] 효과 처리 중: {effectConfig.effectSO.name}", GameLogger.LogCategory.Core);

					var command = effectFactory.CreateCommand(effectConfig);
					if (command != null)
					{
						effectCommands.Add(command);
						GameLogger.LogInfo($"[ActiveItem] 효과 명령 생성 성공: {effectConfig.effectSO.name}", GameLogger.LogCategory.Core);
					}
					else
					{
						GameLogger.LogError($"[ActiveItem] 효과 명령 생성 실패: {effectConfig.effectSO.name}", GameLogger.LogCategory.Core);
					}
				}
			}
			else
			{
				GameLogger.LogWarning("[ActiveItem] 효과가 설정되지 않았습니다", GameLogger.LogCategory.Core);
			}

			// 실행 순서로 정렬
			SortCommandsByExecutionOrder();
			
			GameLogger.LogInfo($"[ActiveItem] 효과 명령 설정 완료: {effectCommands.Count}개", GameLogger.LogCategory.Core);
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
			if (context?.User == null)
			{
				GameLogger.LogWarning("[ActiveItem] 사용자가 null입니다", GameLogger.LogCategory.Core);
				return false;
			}

			// 부활 아이템이 아닌 경우 사망 상태 체크
			bool isReviveItem = definition.DisplayName.Contains("부활") || 
			                   definition.DisplayName.Contains("Revive") ||
			                   definition.DisplayName.Contains("징표");
			
			if (!isReviveItem && context.User.IsDead())
			{
				GameLogger.LogWarning("[ActiveItem] 사용자가 사망 상태입니다", GameLogger.LogCategory.Core);
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
			GameLogger.LogInfo($"[ActiveItem] 효과 실행 시작: {effectCommands.Count}개 명령", GameLogger.LogCategory.Core);
			
			bool allSuccess = true;
			
			foreach (var command in effectCommands)
			{
				GameLogger.LogInfo($"[ActiveItem] 효과 명령 실행: {command.GetType().Name}", GameLogger.LogCategory.Core);
				
				if (!command.Execute(context))
				{
					GameLogger.LogError($"[ActiveItem] 효과 명령 실행 실패: {command.GetType().Name}", GameLogger.LogCategory.Core);
					allSuccess = false;
				}
				else
				{
					GameLogger.LogInfo($"[ActiveItem] 효과 명령 실행 성공: {command.GetType().Name}", GameLogger.LogCategory.Core);
				}
			}
			
			GameLogger.LogInfo($"[ActiveItem] 효과 실행 완료: {(allSuccess ? "성공" : "실패")}", GameLogger.LogCategory.Core);
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

			// 주입된 VFX 매니저 사용(없으면 스킵)
			if (_vfxManager != null && presentation.visualEffectPrefab != null)
			{
				_vfxManager.PlayEffect(presentation.visualEffectPrefab, targetTransform.position);
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
