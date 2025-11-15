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
            AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();

            float clipLength = audioClip.length;

            Destroy(audioSource.gameObject, clipLength); 
        }

        public void PlayRandomSoundFXClip(AudioClip[] audioClips, Transform spawnTransform, float volume = 1.0f)
        {
            AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
            PlaySoundFXClip(randomClip, spawnTransform, volume);
        }
    }
}