using System;

public class LoginViewModel : ViewModelBase
{
    private LoginService _loginService;

    public event Action OnCompleteLogin;
    public event Action OnFailLogin;
    public event Action OnCompleteCreateAccount;
    public event Action OnFailCreateAccount; 
    public event Action OnCompleteUpdateLastLogin; 
    public event Action OnCompleteLogout;

    private string _inputId = "";
    public string InputId
    {
        get
        {
            return _inputId;
        }
        set
        {
            if (_inputId != value)
            {
                _inputId = value;
                OnPropertyChanged(nameof(InputId));
            }
        }
    }

    private string _inputPassword = "";
    public string InputPassword
    {
        get
        {
            return _inputPassword;
        }
        set
        {
            if (_inputPassword != value)
            {
                _inputPassword = value;
                OnPropertyChanged(nameof(InputPassword));
            }
        }
    }

    private bool _isAutoLogin = false;
    public bool IsAutoLogin
    {
        get { return _isAutoLogin; }
        set
        {
            if (_isAutoLogin != value)
            {
                _isAutoLogin = value;
                OnPropertyChanged(nameof(IsAutoLogin));
            }
        }
    }

    private DateTime _lastLoginTime;
    public DateTime LastLoginTime
    {
        get
        {
            return _lastLoginTime;
        }
        set
        {
            if (_lastLoginTime != value)
            {
                _lastLoginTime = value;
                OnPropertyChanged(nameof(LastLoginTime));
            }
        }
    }

    private string _feedbackMessage = "";
    public string FeedbackMessage
    {
        get
        {
            return _feedbackMessage;
        }
        set
        {
            if (_feedbackMessage != value)
            {
                _feedbackMessage = value;
                OnPropertyChanged(nameof(FeedbackMessage));
            }
        }
    }

    private long _userUID = 0;
    public long UserUID
    {
        get
        {
            return _userUID;
        }
        set
        {
            if (_userUID != value)
            {
                _userUID = value;
                OnPropertyChanged(nameof(UserUID));
            }
        }
    }

    private string _loggedUserId = "";
    public string LoggedUserId
    {
        get
        {
            return _loggedUserId;
        }
        set
        {
            if (_loggedUserId != value)
            {
                _loggedUserId = value;
                OnPropertyChanged(nameof(LoggedUserId));
            }
        }
    }

    public void SetLoginService(LoginService service)
    {
        _loginService = service;
    }

    public void CheckAutoLogin()
    {
        AutoLoginData savedData = GameDataManager.Instance.LoadLocalData<AutoLoginData>("AutoLoginSettings");

        if (savedData != null)
        {
            IsAutoLogin = savedData.IsAutoLogin;

            if (IsAutoLogin == true)
            {
                InputId = savedData.UserId;
                InputPassword = savedData.UserPassword;

                if (InputId != "" && InputPassword != "")
                {
                    RequestLogin();
                }
            }
        }
    }

    public async void RequestLogin()
    {
        if (_loginService != null)
        {
            long resultUid = await _loginService.TryLoginAsync(_inputId, _inputPassword);

            if (resultUid != 0)
            {
                UserUID = resultUid;
                SaveAutoLoginData();
                LoggedUserId = _inputId;
                LastLoginTime = await _loginService.GetLastLoginTimeAsync(UserUID);

                FeedbackMessage = "로그인 성공!";
                InvokeCompleteLogin();
            }
            else
            {
                FeedbackMessage = "로그인 실패. 아이디나 비밀번호를 확인하세요.";
                InvokeFailLogin();
            }
        }
    }

    private void SaveAutoLoginData()
    {
        AutoLoginData data = new AutoLoginData();
        data.IsAutoLogin = _isAutoLogin;

        if (_isAutoLogin == true)
        {
            data.UserId = _inputId;
            data.UserPassword = _inputPassword;
        }
        else
        {
            data.UserId = "";
            data.UserPassword = "";
        }

        GameDataManager.Instance.SaveLocalData(data, "AutoLoginSettings");
    }

    public async void RequestCreateAccount()
    {
        if (_loginService != null)
        {
            long resultUid = await _loginService.CreateAccountAsync(_inputId, _inputPassword);

            if (resultUid != 0)
            {
                UserUID = resultUid;
                LoggedUserId = _inputId;
                LastLoginTime = DateTime.UtcNow;

                FeedbackMessage = "계정 생성 성공!";
                InvokeCompleteCreateAccount();
            }
            else
            {
                FeedbackMessage = "계정 생성 실패. 이미 존재하는 아이디거나 오류가 발생했습니다.";
                InvokeFailCreateAccount();
            }
        }
    }

    public async void RequestUpdateLastLogin()
    {
        if (_loginService == null || UserUID == 0) return;

        DateTime currentTime = DateTime.UtcNow;
        bool isSuccess = await _loginService.UpdateLastLoginAsync(UserUID, currentTime);

        if (isSuccess)
        {
            LastLoginTime = currentTime;
            InvokeCompleteUpdateLastLogin();
        }
    }
    public void RequestLogout()
    {
        IsAutoLogin = false;
        InputId = "";
        InputPassword = "";

        SaveAutoLoginData();

        UserUID = 0;
        LoggedUserId = "";
        LastLoginTime = DateTime.MinValue;

        InvokeCompleteLogout();
    }

    public void InvokeCompleteLogin()
    {
        if (OnCompleteLogin != null)
        {
            OnCompleteLogin.Invoke();
        }
    }

    private void InvokeFailLogin()
    {
        if (OnFailLogin != null)
        {
            OnFailLogin.Invoke();
        }
    }

    private void InvokeCompleteCreateAccount()
    {
        if (OnCompleteCreateAccount != null)
        {
            OnCompleteCreateAccount.Invoke();
        }
    }

    private void InvokeFailCreateAccount()
    {
        if (OnFailCreateAccount != null)
        {
            OnFailCreateAccount.Invoke();
        }
    }

    private void InvokeCompleteUpdateLastLogin()
    {
        if(OnCompleteUpdateLastLogin != null)
        {
            OnCompleteUpdateLastLogin.Invoke();
        }
    }
    private void InvokeCompleteLogout()
    {
        if (OnCompleteLogout != null)
        {
            OnCompleteLogout.Invoke();
        }
    }
}