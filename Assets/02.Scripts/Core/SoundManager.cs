using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class SoundManager : SingletonBase<SoundManager>
{
    [SerializeField] private AudioSource AudioSource_BGM;
    [SerializeField] private AudioSource AudioSource_SFX;

    public event Action<float> OnChangedBGMVolume;
    public event Action<float> OnChangedSFXVolume;

    //[TODO] 나라: 일단 비지엠 시작 여기다 넣음
    private void Start()
    {
        PlayBGM("Bgm");
    }

    private async UniTaskVoid LoadAndPlayAudioClip(AudioSource audioSource, string path, bool isLoop = false, float volume = 1.0f)
    {
        AudioClip clip = await ResourceManager.Instance.LoadAsset<AudioClip>(path);

        if (isLoop)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private string GetPathSFX(string soundID)
    {
        return $"Audio/SFX/{soundID}";
    }

    private string GetPathBGM(string soundID)
    {
        return $"Audio/BGM/{soundID}";
    }

    public void PlaySFX(string soundID, float volume = 1.0f)
    {
        LoadAndPlayAudioClip(AudioSource_SFX, GetPathSFX(soundID), false, volume).Forget();
    }

    public void PlayBGM(string soundID, float volume = 1.0f)
    {
        LoadAndPlayAudioClip(AudioSource_BGM, GetPathBGM(soundID), true, volume).Forget();
    }

    public void StopSFX()
    {
        AudioSource_SFX.Stop();
    }

    public void StopBGM()
    {
        AudioSource_BGM.Stop();
    }

    public float GetBGMVolume()
    {
        return AudioSource_BGM.volume;
    }

    public void SetBGMVolume(float volume)
    {
        if(AudioSource_BGM == null)
        {
            return;
        }

        AudioSource_BGM.volume = volume;
        OnChangedBGMVolume?.Invoke(volume);
    }

    public float GetSFXVolume()
    {
        return AudioSource_SFX.volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioSource_SFX == null)
        {
            return;
        }

        AudioSource_SFX.volume = volume;
        OnChangedSFXVolume?.Invoke(volume);
    }
}
