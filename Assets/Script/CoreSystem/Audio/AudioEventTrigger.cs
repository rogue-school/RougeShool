using UnityEngine;
using Game.CoreSystem.Audio;
using Zenject;

namespace Game.CoreSystem.Audio
{
    /// <summary>
    /// 오디오 이벤트 트리거
    /// 게임 이벤트와 사운드 연동을 위한 트리거
    /// </summary>
    public class AudioEventTrigger : MonoBehaviour
    {
        #region 의존성 주입

        [Inject] private AudioManager audioManager;

        #endregion

        #region 전투 사운드 이벤트

        /// <summary>
        /// 카드 사용 사운드 트리거
        /// </summary>
        public void OnCardUsed()
        {
            Debug.Log("[AudioEventTrigger] 카드 사용 사운드 트리거");
            audioManager?.PlayCardUseSound();
        }

        /// <summary>
        /// 적 처치 사운드 트리거
        /// </summary>
        public void OnEnemyDefeated()
        {
            Debug.Log("[AudioEventTrigger] 적 처치 사운드 트리거");
            audioManager?.PlayEnemyDefeatSound();
        }

        /// <summary>
        /// 스킬 발동 사운드 트리거
        /// </summary>
        public void OnSkillActivated()
        {
            Debug.Log("[AudioEventTrigger] 스킬 발동 사운드 트리거");
            audioManager?.PlaySkillActivationSound();
        }

        /// <summary>
        /// 턴 시작 사운드 트리거
        /// </summary>
        public void OnTurnStarted()
        {
            Debug.Log("[AudioEventTrigger] 턴 시작 사운드 트리거");
            audioManager?.PlayTurnStartSound();
        }

        /// <summary>
        /// 턴 완료 사운드 트리거
        /// </summary>
        public void OnTurnCompleted()
        {
            Debug.Log("[AudioEventTrigger] 턴 완료 사운드 트리거");
            audioManager?.PlayTurnCompleteSound();
        }

        /// <summary>
        /// 준보스 처치 사운드 트리거
        /// </summary>
        public void OnSubBossDefeated()
        {
            Debug.Log("[AudioEventTrigger] 준보스 처치 사운드 트리거");
            audioManager?.PlaySubBossDefeatSound();
        }

        /// <summary>
        /// 보스 처치 사운드 트리거
        /// </summary>
        public void OnBossDefeated()
        {
            Debug.Log("[AudioEventTrigger] 보스 처치 사운드 트리거");
            audioManager?.PlayBossDefeatSound();
        }

        /// <summary>
        /// 힐 사운드 트리거
        /// </summary>
        public void OnHeal()
        {
            Debug.Log("[AudioEventTrigger] 힐 사운드 트리거");
            audioManager?.PlayHealSound();
        }

        /// <summary>
        /// 방패 사운드 트리거
        /// </summary>
        public void OnShield()
        {
            Debug.Log("[AudioEventTrigger] 방패 사운드 트리거");
            audioManager?.PlayShieldSound();
        }

        /// <summary>
        /// 셔플 사운드 트리거
        /// </summary>
        public void OnShuffle()
        {
            Debug.Log("[AudioEventTrigger] 셔플 사운드 트리거");
            audioManager?.PlayShuffleSound();
        }

        #endregion

        #region UI 사운드 이벤트

        /// <summary>
        /// 버튼 클릭 사운드 트리거
        /// </summary>
        public void OnButtonClicked()
        {
            Debug.Log("[AudioEventTrigger] 버튼 클릭 사운드 트리거");
            audioManager?.PlayButtonClickSound();
        }

        /// <summary>
        /// 카드 드래그 사운드 트리거
        /// </summary>
        public void OnCardDragged()
        {
            Debug.Log("[AudioEventTrigger] 카드 드래그 사운드 트리거");
            audioManager?.PlayCardDragSound();
        }

        /// <summary>
        /// 카드 드롭 사운드 트리거
        /// </summary>
        public void OnCardDropped()
        {
            Debug.Log("[AudioEventTrigger] 카드 드롭 사운드 트리거");
            audioManager?.PlayCardDropSound();
        }

        /// <summary>
        /// 메뉴 열기 사운드 트리거
        /// </summary>
        public void OnMenuOpened()
        {
            Debug.Log("[AudioEventTrigger] 메뉴 열기 사운드 트리거");
            audioManager?.PlayMenuOpenSound();
        }

        /// <summary>
        /// 메뉴 닫기 사운드 트리거
        /// </summary>
        public void OnMenuClosed()
        {
            Debug.Log("[AudioEventTrigger] 메뉴 닫기 사운드 트리거");
            audioManager?.PlayMenuCloseSound();
        }

        #endregion

        #region BGM 이벤트

        /// <summary>
        /// 메인 메뉴 BGM 트리거
        /// </summary>
        public void OnMainMenuBGM()
        {
            Debug.Log("[AudioEventTrigger] 메인 메뉴 BGM 트리거");
            audioManager?.PlayMainMenuBGM();
        }

        /// <summary>
        /// 전투 BGM 트리거
        /// </summary>
        public void OnBattleBGM()
        {
            Debug.Log("[AudioEventTrigger] 전투 BGM 트리거");
            audioManager?.PlayBattleBGM();
        }

        /// <summary>
        /// 상점 BGM 트리거
        /// </summary>
        public void OnShopBGM()
        {
            Debug.Log("[AudioEventTrigger] 상점 BGM 트리거");
            audioManager?.PlayShopBGM();
        }

        /// <summary>
        /// 인벤토리 BGM 트리거
        /// </summary>
        public void OnInventoryBGM()
        {
            Debug.Log("[AudioEventTrigger] 인벤토리 BGM 트리거");
            audioManager?.PlayInventoryBGM();
        }

        /// <summary>
        /// 씬별 BGM 트리거
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        public void OnSceneBGM(string sceneName)
        {
            Debug.Log($"[AudioEventTrigger] 씬별 BGM 트리거: {sceneName}");
            audioManager?.PlaySceneBGM(sceneName);
        }

        #endregion

        #region 설정 관리

        /// <summary>
        /// BGM 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
        public void SetBGMVolume(float volume)
        {
            audioManager?.SetBGMVolume(volume);
        }

        /// <summary>
        /// SFX 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
        public void SetSFXVolume(float volume)
        {
            audioManager?.SetSFXVolume(volume);
        }

        /// <summary>
        /// 오디오 풀 쿨다운 시간 설정
        /// </summary>
        /// <param name="cooldown">쿨다운 시간 (초)</param>
        public void SetAudioPoolCooldown(float cooldown)
        {
            var poolManager = audioManager?.GetAudioPoolManager();
            poolManager?.SetSoundCooldown(cooldown);
        }

        /// <summary>
        /// 오디오 풀 쿨다운 활성화/비활성화
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        public void SetAudioPoolCooldownEnabled(bool enabled)
        {
            var poolManager = audioManager?.GetAudioPoolManager();
            poolManager?.SetCooldownEnabled(enabled);
        }

        /// <summary>
        /// 오디오 풀 우선순위 활성화/비활성화
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        public void SetAudioPoolPriorityEnabled(bool enabled)
        {
            var poolManager = audioManager?.GetAudioPoolManager();
            poolManager?.SetPriorityEnabled(enabled);
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 오디오 풀 상태 출력
        /// </summary>
        [ContextMenu("오디오 풀 상태 출력")]
        public void PrintAudioPoolStatus()
        {
            audioManager?.PrintAudioPoolStatus();
        }

        /// <summary>
        /// 오디오 풀 쿨다운 상태 출력
        /// </summary>
        [ContextMenu("오디오 풀 쿨다운 상태 출력")]
        public void PrintAudioPoolCooldownStatus()
        {
            audioManager?.PrintAudioPoolCooldownStatus();
        }

        /// <summary>
        /// 현재 BGM 정보 출력
        /// </summary>
        [ContextMenu("현재 BGM 정보 출력")]
        public void PrintCurrentBGMInfo()
        {
            if (audioManager != null)
            {
                Debug.Log($"[AudioEventTrigger] 현재 BGM: {audioManager.GetCurrentBGMName()}");
                Debug.Log($"[AudioEventTrigger] BGM 볼륨: {audioManager.BGMVolume}");
                Debug.Log($"[AudioEventTrigger] SFX 볼륨: {audioManager.SFXVolume}");
            }
            else
            {
                Debug.LogWarning("[AudioEventTrigger] AudioManager가 없습니다.");
            }
        }

        #endregion

        #region 테스트 메서드

        /// <summary>
        /// 모든 전투 사운드 테스트
        /// </summary>
        [ContextMenu("전투 사운드 테스트")]
        public void TestCombatSounds()
        {
            Debug.Log("[AudioEventTrigger] 전투 사운드 테스트 시작");
            
            OnCardUsed();
            OnEnemyDefeated();
            OnSkillActivated();
            OnTurnStarted();
            OnTurnCompleted();
            OnSubBossDefeated();
            OnBossDefeated();
            OnHeal();
            OnShield();
            OnShuffle();
            
            Debug.Log("[AudioEventTrigger] 전투 사운드 테스트 완료");
        }

        /// <summary>
        /// 모든 UI 사운드 테스트
        /// </summary>
        [ContextMenu("UI 사운드 테스트")]
        public void TestUISounds()
        {
            Debug.Log("[AudioEventTrigger] UI 사운드 테스트 시작");
            
            OnButtonClicked();
            OnCardDragged();
            OnCardDropped();
            OnMenuOpened();
            OnMenuClosed();
            
            Debug.Log("[AudioEventTrigger] UI 사운드 테스트 완료");
        }

        /// <summary>
        /// 모든 BGM 테스트
        /// </summary>
        [ContextMenu("BGM 테스트")]
        public void TestBGMs()
        {
            Debug.Log("[AudioEventTrigger] BGM 테스트 시작");
            
            OnMainMenuBGM();
            OnBattleBGM();
            OnShopBGM();
            OnInventoryBGM();
            
            Debug.Log("[AudioEventTrigger] BGM 테스트 완료");
        }

        #endregion
    }
}
