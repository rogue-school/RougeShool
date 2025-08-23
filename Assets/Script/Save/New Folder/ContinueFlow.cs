using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Save;

/// <summary>
/// 이어하기 흐름: 메인 메뉴 → 배틀 씬
/// </summary>
public class ContinueFlow : MonoBehaviour
{
    // 로드 후 씬에서 쓸 임시 버퍼
    public static BattleSaveData PendingLoadedData;

    [Header("Scene Names")]
    public string battleSceneName = "BattleScene"; // 배틀 씬 이름

    /// <summary>
    /// 메인 메뉴의 '이어하기' 버튼에 연결
    /// </summary>
    public void OnClickContinue()
    {
        if (!SaveSystem.Exists())
        {
            Debug.LogWarning("[ContinueFlow] 저장 파일이 없어 이어하기가 불가합니다.");
            return;
        }

        PendingLoadedData = SaveSystem.Load();
        if (PendingLoadedData == null)
        {
            Debug.LogWarning("[ContinueFlow] 로드 실패.");
            return;
        }

        // 배틀 씬으로 이동
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(battleSceneName);
    }

    /// <summary>
    /// 배틀 씬이 로드되면, BattleSaveController를 찾아서 데이터 적용
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != battleSceneName)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        var applier = FindObjectOfType<BattleSaveController>();
        if (applier == null)
        {
            Debug.LogError("[ContinueFlow] BattleSaveController를 씬에서 찾지 못했습니다.");
            return;
        }

        applier.Apply(PendingLoadedData);
        PendingLoadedData = null;
    }
}
