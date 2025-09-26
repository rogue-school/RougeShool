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
        void SaveAudioSettings(float bgmVolume, float sfxVolume);
        
        /// <summary>
        /// 오디오 설정 로드
        /// </summary>
        (float bgmVolume, float sfxVolume) LoadAudioSettings(float defaultBgmVolume = 0.7f, float defaultSfxVolume = 1.0f);
        
        /// <summary>
        /// 현재 씬 저장
        /// </summary>
        Task SaveCurrentScene();
        
        /// <summary>
        /// 게임 상태 저장
        /// </summary>
        Task SaveGameState(string saveName);
        
        /// <summary>
        /// 게임 상태 로드
        /// </summary>
        Task LoadGameState(string saveName);
        
        /// <summary>
        /// 자동 저장 실행
        /// </summary>
        Task TriggerAutoSave(string condition);
        
        /// <summary>
        /// 저장된 씬 로드
        /// </summary>
        Task<bool> LoadSavedScene();
        
        /// <summary>
        /// 저장 파일 존재 여부 확인
        /// </summary>
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
        Task<bool> LoadStageProgress();
    }
}
