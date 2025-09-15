using UnityEngine;
using Game.CoreSystem.Manager;

public class Playbutton : MonoBehaviour
{
    public string sceneToLoad; // Inspector���� ���� ����

    public void OnButtonClicked()
    {
        // 중앙 전환 매니저 사용: StageScene으로 이동 (설정 시 다른 씬 메서드로 교체 가능)
        _ = SceneTransitionManager.Instance.TransitionToStageScene();
    }
}