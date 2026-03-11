using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    public AudioMixer mixer;
    public AudioSource musicSource, sfxSource;
    public AudioClip menuMusic;
    /*public AudioClip[] menuMusics, combatMusics, idleMusics, shopMusics, bossMusics;
    public AudioClip[] bellSfx, receiveCardSfx, buttonClickSfx, shuffleDeckSfx, playCardSfx, ambienceSfx;*/

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void ChangeMasterVol(float vol)
    {
        mixer.SetFloat("MasterVol", vol);
    }
    public void ChangeMusicVol(float vol)
    {
        mixer.SetFloat("MusicVol", vol);
    }
    public void ChangeSFXVol(float vol)
    {
        mixer.SetFloat("SFXVol", vol);
    }

    public void StartMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
