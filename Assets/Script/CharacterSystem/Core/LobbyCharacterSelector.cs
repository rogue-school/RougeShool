using UnityEngine;
using UnityEngine.UI;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.Core
{
	/// <summary>
	/// 로비의 3개 고정 버튼을 특정 캐릭터 데이터에 1:1로 바인딩합니다.
	/// 버튼 클릭 시 MainMenuController에 선택을 위임합니다.
	/// </summary>
	public class LobbyCharacterSelector : MonoBehaviour
	{
		[Header("버튼 연결")]
		[Tooltip("검 캐릭터 선택 버튼")]
		[SerializeField] private Button swordButton;
		[Tooltip("활 캐릭터 선택 버튼")]
		[SerializeField] private Button bowButton;
		[Tooltip("지팡이 캐릭터 선택 버튼")]
		[SerializeField] private Button staffButton;

		[Header("캐릭터 데이터")]
		[Tooltip("검 캐릭터 데이터")]
		[SerializeField] private PlayerCharacterData swordCharacter;
		[Tooltip("활 캐릭터 데이터")]
		[SerializeField] private PlayerCharacterData bowCharacter;
		[Tooltip("지팡이 캐릭터 데이터")]
		[SerializeField] private PlayerCharacterData staffCharacter;

		[Header("연결 대상")]
		[Tooltip("선택 처리를 수행할 MainMenuController")]
		[SerializeField] private Game.UISystem.MainMenuController mainMenuController;

		private void Awake()
		{
			if (mainMenuController == null)
			{
				mainMenuController = FindFirstObjectByType<Game.UISystem.MainMenuController>();
			}
		}

		private void Start()
		{
			Bind(swordButton, swordCharacter, "검");
			Bind(bowButton, bowCharacter, "활");
			Bind(staffButton, staffCharacter, "지팡이");
		}

		private void Bind(Button button, PlayerCharacterData data, string label)
		{
			if (button == null)
			{
				GameLogger.LogWarning($"[LobbyCharacterSelector] {label} 버튼이 설정되지 않았습니다", GameLogger.LogCategory.UI);
				return;
			}
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => OnClick(data, label));
		}

		private void OnClick(PlayerCharacterData data, string label)
		{
			if (data == null)
			{
				GameLogger.LogWarning($"[LobbyCharacterSelector] {label} 캐릭터 데이터가 비어 있습니다", GameLogger.LogCategory.UI);
				return;
			}
			if (mainMenuController == null)
			{
				GameLogger.LogError("[LobbyCharacterSelector] MainMenuController를 찾을 수 없습니다", GameLogger.LogCategory.Error);
				return;
			}
			mainMenuController.SelectCharacterFromExternal(data);
		}
	}
}
