using System.Threading.Tasks;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// 저장 관리 인터페이스
    /// </summary>
    public interface ISaveManager
    {
        /// <summary>
        /// 오디오 설정 저장
        /// </summary>
        /// <param name="bgmVolume">BGM 볼륨 (0.0 ~ 1.0)</param>
        /// <param name="sfxVolume">SFX 볼륨 (0.0 ~ 1.0)</param>
        void SaveAudioSettings(float bgmVolume, float sfxVolume);
        
        /// <summary>
        /// 오디오 설정 로드
        /// </summary>
        /// <param name="defaultBgmVolume">기본 BGM 볼륨 (기본값: 0.7)</param>
        /// <param name="defaultSfxVolume">기본 SFX 볼륨 (기본값: 1.0)</param>
        /// <returns>BGM 볼륨과 SFX 볼륨 튜플</returns>
        (float bgmVolume, float sfxVolume) LoadAudioSettings(float defaultBgmVolume = 0.7f, float defaultSfxVolume = 1.0f);
        
        /// <summary>
        /// 현재 씬 저장
        /// </summary>
        /// <returns>저장 작업</returns>
        Task SaveCurrentScene();
        
        /// <summary>
        /// 게임 상태 저장
        /// </summary>
        /// <param name="saveName">저장 이름</param>
        /// <returns>저장 작업</returns>
        Task SaveGameState(string saveName);
        
        /// <summary>
        /// 게임 상태 로드
        /// </summary>
        /// <param name="saveName">로드할 저장 이름</param>
        /// <returns>로드 작업</returns>
        Task LoadGameState(string saveName);
        
        /// <summary>
        /// 자동 저장 실행
        /// </summary>
        /// <param name="condition">저장 조건 설명</param>
        /// <returns>저장 작업</returns>
        Task TriggerAutoSave(string condition);
        
        /// <summary>
        /// 저장된 씬 로드
        /// </summary>
        /// <returns>로드 성공 여부</returns>
        Task<bool> LoadSavedScene();
        
        /// <summary>
        /// 저장 파일 존재 여부 확인
        /// </summary>
        /// <returns>파일 존재 여부</returns>
        bool HasSaveFile();
        
        /// <summary>
        /// 저장 데이터 초기화
        /// </summary>
        void ClearSave();
        
        /// <summary>
        /// 새 게임 시작 시 기존 저장 데이터 완전 초기화
        /// </summary>
        void InitializeNewGame();
        
        /// <summary>
        /// 스테이지 진행 저장 파일 존재 여부 확인
        /// </summary>
        bool HasStageProgressSave();
        
        /// <summary>
        /// 스테이지 진행 상황 로드
        /// </summary>
        /// <returns>로드 성공 여부</returns>
        Task<bool> LoadStageProgress();
        
        /// <summary>
        /// 현재 진행 상황 저장
        /// </summary>
        /// <param name="trigger">저장 트리거 설명 (기본값: "Manual")</param>
        /// <returns>저장 작업</returns>
        Task SaveCurrentProgress(string trigger = "Manual");
    }
}
