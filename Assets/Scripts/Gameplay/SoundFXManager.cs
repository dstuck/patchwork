using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Patchwork.Gameplay
{
    public class SoundFXManager : MonoBehaviour
    {
        public static SoundFXManager instance;

        [SerializeField] public AudioSource soundFXObject;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume = 1.0f)
        {
            if (audioClip == null || soundFXObject == null || spawnTransform == null)
            {
                Debug.LogWarning("[SoundFXManager] Missing clip, prefab, or spawn transform.");
                return;
            }

            AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();

            float clipLength = audioClip.length;
            Destroy(audioSource.gameObject, clipLength);
        }

        public void PlayRandomSoundFXClip(AudioClip[] audioClips, Transform spawnTransform, float volume = 1.0f)
        {
            if (audioClips == null || audioClips.Length == 0)
            {
                Debug.LogWarning("[SoundFXManager] No clips provided for random selection.");
                return;
            }

            AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
            PlaySoundFXClip(randomClip, spawnTransform, volume);
        }
    }
}