using System.Collections.Generic;
using UnityEngine;
using Game.CharacterSystem.Data;
using Game.IManager;
using Game.Utility.GameFlow;
using Game.CharacterSystem.Interface;
using Zenject;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 플레이어가 사용할 캐릭터를 선택하는 선택자 클래스입니다.
    /// 선택된 캐릭터는 정적으로 보관되며, 이후 씬에서도 참조할 수 있습니다.
    /// </summary>
    public class PlayerCharacterSelector : MonoBehaviour, IPlayerCharacterSelector
    {
        #region Static

        /// <summary>
        /// 선택된 캐릭터는 정적 필드에 저장되어 씬 간 유지됩니다.
        /// </summary>
        public static PlayerCharacterData SelectedCharacter { get; private set; }

        /// <summary>
        /// 강제로 선택된 캐릭터를 설정합니다. (디버깅/테스트용)
        /// </summary>
        /// <param name="data">선택할 캐릭터 데이터</param>
        public static void ForceSetSelectedCharacter(PlayerCharacterData data)
        {
            SelectedCharacter = data;
            Debug.Log($"[Selector] 강제 캐릭터 설정됨: {data.DisplayName}");
        }

        #endregion

        #region Serialized Fields

        [Header("등록된 캐릭터 목록")]
        [Tooltip("게임에서 선택 가능한 플레이어 캐릭터 데이터 리스트")]
        [SerializeField] private List<PlayerCharacterData> characterCandidates;

        #endregion

        #region Dependencies

        /// <summary>
        /// 씬 전환을 담당하는 로더
        /// </summary>
        private ISceneLoader sceneLoader;

        /// <summary>
        /// Zenject를 통한 의존성 주입
        /// </summary>
        /// <param name="loader">씬 로더</param>
        [Inject]
        public void Construct(ISceneLoader loader)
        {
            sceneLoader = loader;
        }

        #endregion

        #region Public API

        /// <summary>
        /// 인덱스를 기반으로 캐릭터를 선택하고 전투 씬으로 전환합니다.
        /// </summary>
        /// <param name="index">캐릭터 후보 목록의 인덱스</param>
        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= characterCandidates.Count)
            {
                Debug.LogError($"[Selector] 잘못된 인덱스: {index}");
                return;
            }

            SelectedCharacter = characterCandidates[index];
            Debug.Log($"[Selector] 캐릭터 선택됨: {SelectedCharacter.DisplayName}");

            sceneLoader.LoadScene("Combat");
        }

        /// <summary>
        /// 현재 선택된 캐릭터 데이터를 반환합니다.
        /// </summary>
        public PlayerCharacterData GetSelectedCharacter()
        {
            return SelectedCharacter;
        }

        #endregion
    }
}
