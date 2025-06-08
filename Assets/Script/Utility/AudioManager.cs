using UnityEngine;

namespace Game.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;

            sfxSource.PlayOneShot(clip);
            Debug.Log($"[AudioManager] 사운드 재생됨: {clip.name}");
        }
    }
}
