using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameSettingsUI : ViewBase
{
    [SerializeField] private UIButton Button_BackgroundClose;
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private Slider Slider_BGM;
    [SerializeField] private Slider Slider_SFX;

    [SerializeField] private UIButton Button_Logout;

    private float _bgmVolume;
    private float _sfxVolume;

    private void OnEnable()
    {
        Button_BackgroundClose.BindOnClickButtonEvent(OnClick_Close);
        Button_Close.BindOnClickButtonEvent(OnClick_Close);

        Slider_BGM.onValueChanged.AddListener(OnChangedBGMVolume);
        _bgmVolume = SoundManager.Instance.GetBGMVolume();
        Slider_BGM.value = _bgmVolume;

        Slider_SFX.onValueChanged.AddListener(OnChangedSFXVolume);
        _sfxVolume = SoundManager.Instance.GetSFXVolume();
        Slider_SFX.value = _sfxVolume;

        SoundManager.Instance.OnChangedBGMVolume += UpdateBGMVolume;
        SoundManager.Instance.OnChangedSFXVolume += UpdateSFXVolume;

        Button_Logout.BindOnClickButtonEvent(LogoutInGame);
    }

    private void OnDisable()
    {
        Button_BackgroundClose.UnBindOnClickButtonEvent(OnClick_Close);
        Button_Close.UnBindOnClickButtonEvent(OnClick_Close);

        Slider_BGM.onValueChanged.RemoveListener(OnChangedBGMVolume);
        Slider_SFX.onValueChanged.RemoveListener(OnChangedSFXVolume);

        SoundManager.Instance.OnChangedBGMVolume -= UpdateBGMVolume;
        SoundManager.Instance.OnChangedSFXVolume -= UpdateSFXVolume;

        Button_Logout.UnBindOnClickButtonEvent(LogoutInGame);
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.InGameSettingsUI);
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
        // Slider 값은 변경하지만 onValueChanged 이벤트는 발생 안 할 것
        Slider_BGM.SetValueWithoutNotify(volume);
    }

    private void UpdateSFXVolume(float volume)
    {
        Slider_SFX.SetValueWithoutNotify(volume);
    }

    private void LogoutInGame()
    {
        // 씨앗 저장
        var loginService = ServiceManager.Instance.LoginService;
        var userService = ServiceManager.Instance.UserService;

        // DB 저장
        long userUID = loginService.GetViewModel().UserUID;
        userService.SaveUserAsync(userUID).Forget();

        if (loginService != null)
        {
            LoginViewModel loginVm = loginService.GetViewModel();

            if (loginVm != null)
            {
                loginVm.RequestLogout();
            }
        }

        // 씬 리로드
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
