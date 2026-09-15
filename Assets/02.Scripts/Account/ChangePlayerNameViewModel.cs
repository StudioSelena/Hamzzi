using System;

public class ChangePlayerNameViewModel : ViewModelBase
{
    private ChangePlayerNameService _service;

    public event Action OnCompleteChangeName;
    public event Action OnFailChangeName;

    private string _inputName = "";
    public string InputName
    {
        get { return _inputName; }
        set
        {
            if (_inputName != value)
            {
                _inputName = value;
                OnPropertyChanged(nameof(InputName));
            }
        }
    }

    public void SetService(ChangePlayerNameService service)
    {
        _service = service;
    }

    public async void RequestChangePlayerName()
    {
        if (_service == null) return;

        LoginService loginService = ServiceManager.Instance.LoginService;
        if (loginService == null) return;

        long myUid = loginService.GetViewModel().UserUID;
        if (myUid == 0) return;

        bool isSuccess = await _service.TryChangePlayerNameAsync(myUid, _inputName);

        if (isSuccess)
        {
            UserService userService = ServiceManager.Instance.UserService;
            if (userService != null)
            {
                UserViewModel userVm = userService.GetUserViewModel();
                if (userVm != null)
                {
                    userVm.UserName = _inputName;
                }
            }

            InvokeCompleteChangeName();
        }
        else
        {
            InvokeFailChangeName();
        }
    }

    private void InvokeCompleteChangeName()
    {
        if (OnCompleteChangeName != null)
        {
            OnCompleteChangeName.Invoke();
        }
    }

    private void InvokeFailChangeName()
    {
        if (OnFailChangeName != null)
        {
            OnFailChangeName.Invoke();
        }
    }
}