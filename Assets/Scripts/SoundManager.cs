using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    private AudioClip[] sfxs;
    private AudioClip[] osts;
    private AudioSource sfxSource;
    private AudioSource ostSource;
    private AudioSource footstepsSource;
    [SerializeField] private AudioMixer mixer;

    public struct SFX
    {
        public const int BUTTON_PRESSED = 0;
        public const int COUNTDOWN = 1;
        public const int FOOTSTEPS = 2;
        public const int RACE_END = 3;
        public const int SLIDE = 4;
        public const int STARBIT = 5;
        public const int START  = 6;
    }

    public struct OST
    {
        public const int LELVEL_1 = 0;
        public const int MAIN_MENU = 1;
    }

    void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
        sfxs = Resources.LoadAll<AudioClip>("Sound/SFX");
        osts = Resources.LoadAll<AudioClip>("Sound/Music");
        AudioSource[] sources = gameObject.GetComponents<AudioSource>();
        ostSource = sources[0];
        sfxSource = sources[1];
        footstepsSource = sources[2];
        StartCoroutine(SFXCleaner());
    }

    void Update()
    {
        transform.position = Camera.main.transform.position;
    }

    public void PlaySFXByName(string name)
    {
        foreach (AudioClip sfx in sfxs)
        {
            if (sfx.name == name)
            {
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                newSource.clip = sfx;
                newSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
                newSource.Play();
                break;
            }
        }
    }

    public void PlayMusicByName(string name)
    {
        foreach (AudioClip ost in osts)
        {
            if (ost.name == name)
            {
                ostSource.Stop();
                ostSource.clip = ost;
                ostSource.Play();
                break;
            }
        }
    }

    public void PlaySFXByIndex(int index, bool loop = false)
    {
        StopSFX(index);
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = sfxs[index];
        newSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        newSource.loop = loop;
        newSource.Play();
    }

    public void PlayMusicByIndex(int index)
    {
        ostSource.Stop();
        ostSource.clip = osts[index];
        ostSource.Play();
    }

    public void StopMusic()
    {
        ostSource.Stop();
    }

    public void PauseMusic()
    {
        ostSource.Pause();
    }

    public void ResumeMusic()
    {
        ostSource.UnPause();
    }

    public void StopSFX(int index)
    {
        AudioSource[] sources = gameObject.GetComponents<AudioSource>();
        foreach (AudioSource source in sources)
        {
            if (source.clip == sfxs[index])
            {
                source.Stop();
                Destroy(source);
                break;
            }
        }
    }

    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("VolumeSFX",  Mathf.Log10(volume / 100) * 20f);
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("VolumeMusic", Mathf.Log10(volume / 100) * 20f);
    }

    public float GetSFXVolume()
    {
        float volume;
        mixer.GetFloat("VolumeSFX", out volume);
        return Mathf.Pow(10f, volume / 20f) * 100f;
    }

    public float GetMusicVolume()
    {
        float volume;
        mixer.GetFloat("VolumeMusic", out volume);
        return Mathf.Pow(10f, volume / 20f) * 100f;
    }

    public void PlayFootsteps(bool Walking)
    {
        footstepsSource.enabled = Walking;
    }

    public void ChangeFootsteps(int index)
    {
        footstepsSource.clip = sfxs[index];
    }

    public void StopAllSFX()
    {
        AudioSource[] sources = gameObject.GetComponents<AudioSource>();
        foreach (AudioSource source in sources)
        {
            if (source != sfxSource && source != ostSource && source != footstepsSource) source.Stop();
            else if (source == footstepsSource) footstepsSource.enabled = false;
        }
    }

    public IEnumerator SFXCleaner()
    {
        yield return new WaitForSeconds(2);
        AudioSource[] sources = gameObject.GetComponents<AudioSource>();
        foreach (AudioSource source in sources)
        {
            if (!source.isPlaying && source != sfxSource && source != ostSource && source != footstepsSource) Destroy(source);
        }
        StartCoroutine(SFXCleaner());
    }
}
