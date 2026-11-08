using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SpellSFXElementEntry
{
    public Elements Element;

    [Header("Projectile")]
    public AudioClip[] Projectile = new AudioClip[5];

    [Header("Area")]
    public AudioClip[] Area = new AudioClip[5];

    [Header("Explosion")]
    public AudioClip[] Explosion = new AudioClip[5];

    public AudioClip[] GetClips(SpellTypes type)
    {
        switch (type)
        {
            case SpellTypes.Projectile: return Projectile;
            case SpellTypes.Area: return Area;
            case SpellTypes.Explosion: return Explosion;
            default: return null;
        }
    }
}

public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    public AudioMixer mixer;
    public AudioSource musicSource, sfxSource;
    public AudioClip menuMusic;

    [Header("Spell SFX")]
    public List<SpellSFXElementEntry> spellSFX = new List<SpellSFXElementEntry>();

    [Range(3f, 12f)]
    [SerializeField] private float sfxVolumeBoostDb = 6f;
    [Range(3f, 15f)]
    [SerializeField] private float sfxFullVolumeDistance3D = 3f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            PreloadSpellSFX();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PreloadSpellSFX()
    {
        foreach (SpellSFXElementEntry entry in spellSFX)
        {
            if (entry == null) continue;

            PreloadClips(entry.Projectile);
            PreloadClips(entry.Area);
            PreloadClips(entry.Explosion);
        }
    }

    private void PreloadClips(AudioClip[] clips)
    {
        if (clips == null) return;

        foreach (AudioClip clip in clips)
        {
            if (clip != null) clip.LoadAudioData();
        }
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
        mixer.SetFloat("SFXVol", vol + sfxVolumeBoostDb);
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

    private SpellSFXElementEntry GetSpellSFXEntry(Elements element)
    {
        return spellSFX.Find(entry => entry != null && entry.Element == element);
    }

    public int GetRandomSpellSFXIndex(Elements element, SpellTypes type)
    {
        SpellSFXElementEntry entry = GetSpellSFXEntry(element);

        if (entry == null) return -1;

        AudioClip[] clips = entry.GetClips(type);

        if (clips == null || clips.Length == 0) return -1;

        List<int> validIndexes = new List<int>();

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null) validIndexes.Add(i);
        }

        if (validIndexes.Count == 0) return -1;

        return validIndexes[UnityEngine.Random.Range(0, validIndexes.Count)];
    }

    public void PlaySpellSFX(AudioSource source, Elements element, SpellTypes type, int clipIndex)
    {
        if (source == null || clipIndex < 0) return;

        SpellSFXElementEntry entry = GetSpellSFXEntry(element);

        if (entry == null) return;

        AudioClip[] clips = entry.GetClips(type);

        if (clips == null || clipIndex >= clips.Length) return;

        AudioClip clip = clips[clipIndex];

        if (clip == null) return;

        source.Stop();
        source.clip = clip;
        source.loop = type == SpellTypes.Projectile || type == SpellTypes.Area;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = sfxFullVolumeDistance3D;
        source.dopplerLevel = 0f;

        if (sfxSource != null && sfxSource.outputAudioMixerGroup != null)
        {
            source.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }

        source.Play();
    }
}
