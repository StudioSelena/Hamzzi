using System;

public class SetPlayerNameViewModel : ViewModelBase
{
    private SetPlayerNameService _service;

    public event Action OnCompleteSetName;
    public event Action OnFailSetName;

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

    public void SetService(SetPlayerNameService service)
    {
        _service = service;
    }

    public async void RequestSetPlayerName()
    {
        if (_service == null) return;

        LoginService loginService = ServiceManager.Instance.LoginService;
        if (loginService == null) return;

        long myUid = loginService.GetViewModel().UserUID;
        if (myUid == 0) return;

        bool isSuccess = await _service.TrySetPlayerNameAsync(myUid, _inputName);

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

            InvokeCompleteSetName();
        }
        else
        {
            InvokeFailSetName();
        }
    }

    private void InvokeCompleteSetName()
    {
        if (OnCompleteSetName != null)
        {
            OnCompleteSetName.Invoke();
        }
    }

    private void InvokeFailSetName()
    {
        if (OnFailSetName != null)
        {
            OnFailSetName.Invoke();
        }
    }
}