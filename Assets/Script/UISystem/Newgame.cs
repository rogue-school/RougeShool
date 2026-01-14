using UnityEngine;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.UISystem
{
    /// <summary>
    /// 새 게임 시작 버튼 컨트롤러
    /// </summary>
    public class NewGame : MonoBehaviour
    {
        [Header("씬 설정")]
        [Tooltip("로드할 씬 이름")]
        [SerializeField] private string sceneToLoad = "StageScene";
        
        // 의존성 주입
        [Inject] private ISceneTransitionManager sceneTransitionManager;

        /// <summary>
        /// 새 게임 시작 버튼 클릭
        /// </summary>
        public void OnButtonClicked()
        {
            Debug.Log($"[NewGame] 새 게임 시작 - 씬: {sceneToLoad}");
            
            // 중앙 전환 매니저 사용: StageScene으로 이동
            _ = sceneTransitionManager.TransitionToStageScene();
        }
    }
}