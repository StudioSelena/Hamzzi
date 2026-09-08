using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleSettingsUI : ViewBase
{
    [SerializeField] private UIButton Button_BackgroundClose;
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private Slider Slider_BGM;
    [SerializeField] private Slider Slider_SFX;

    private float _bgmVolume;
    private float _sfxVolume;

    private void OnEnable()
    {
        Button_BackgroundClose.BindOnClickButtonEvent(OnClick_Close);
        Button_Close.BindOnClickButtonEvent(OnClick_Close);

        _bgmVolume = SoundManager.Instance.GetBGMVolume();
        Slider_BGM.value = _bgmVolume;

        _sfxVolume = SoundManager.Instance.GetSFXVolume();
        Slider_SFX.value = _sfxVolume;

        Slider_BGM.onValueChanged.AddListener(OnChangedBGMVolume);
        Slider_SFX.onValueChanged.AddListener(OnChangedSFXVolume);

        SoundManager.Instance.OnChangedBGMVolume += UpdateBGMVolume;
        SoundManager.Instance.OnChangedSFXVolume += UpdateSFXVolume;
    }

    private void OnDisable()
    {
        Slider_BGM.onValueChanged.RemoveListener(OnChangedBGMVolume);
        Slider_SFX.onValueChanged.RemoveListener(OnChangedSFXVolume);

        SoundManager.Instance.OnChangedBGMVolume -= UpdateBGMVolume;
        SoundManager.Instance.OnChangedSFXVolume -= UpdateSFXVolume;
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseTitleSettingsUI();
    }

    private void OnChangedBGMVolume(float volume)
    {
        _bgmVolume = volume;
        SoundManager.Instance.SetBGMVolume(_bgmVolume);
    }

    private void OnChangedSFXVolume(float volume)
    {
        _sfxVolume = volume;
        SoundManager.Instance.SetSFXVolume(_sfxVolume);
    }

    private void UpdateBGMVolume(float volume)
    {
        Slider_BGM.SetValueWithoutNotify(volume);
    }

    private void UpdateSFXVolume(float volume)
    {
        Slider_SFX.SetValueWithoutNotify(volume);
    }
}

