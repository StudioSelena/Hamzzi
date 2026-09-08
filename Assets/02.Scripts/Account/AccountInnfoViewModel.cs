using System;
using UnityEngine;

public class AccountInfoViewModel : ViewModelBase
{
    private AccountInfoService _service;

    public event Action OnCompleteLoadInfo;
    public event Action OnCompleteAddFriend;
    public event Action OnFailAddFriend;

    private string _displayUserId = "";
    public string DisplayUserId
    {
        get { return _displayUserId; }
        set
        {
            if (_displayUserId != value)
            {
                _displayUserId = value;
                OnPropertyChanged(nameof(DisplayUserId));
            }
        }
    }

    private string _displayUserName = "";
    public string DisplayUserName
    {
        get { return _displayUserName; }
        set
        {
            if (_displayUserName != value)
            {
                _displayUserName = value;
                OnPropertyChanged(nameof(DisplayUserName));
            }
        }
    }

    private Sprite _displayUserIcon;
    public Sprite DisplayUserIcon
    {
        get { return _displayUserIcon; }
        set
        {
            if (_displayUserIcon != value)
            {
                _displayUserIcon = value;
                OnPropertyChanged(nameof(DisplayUserIcon));
            }
        }
    }

    public void SetService(AccountInfoService service)
    {
        _service = service;
    }

    public async void RequestLoadAccountInfo()
    {
        if (_service == null) return;

        AccountSearchService searchService = ServiceManager.Instance.AccountSearchService;
        if (searchService == null) return;

        long targetUid = searchService.GetViewModel().TargetUserUid;
        if (targetUid == 0) return;

        AccountInfoData data = await _service.GetAccountInfoAsync(targetUid);

        if (data != null)
        {
            DisplayUserId = data.UserId;
            DisplayUserName = data.UserName;

            if (data.UserIconId != "")
            {
                Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(data.UserIconId);
                DisplayUserIcon = loadedSprite;
            }

            InvokeCompleteLoadInfo();
        }
    }

    public async void RequestAddFriend()
    {
        LoginService loginService = ServiceManager.Instance.LoginService;
        if (loginService == null) return;

        AccountSearchService searchService = ServiceManager.Instance.AccountSearchService;
        if (searchService == null) return;

        long myUid = loginService.GetViewModel().UserUID;
        long targetUid = searchService.GetViewModel().TargetUserUid;

        if (myUid == 0 || targetUid == 0) return;

        FriendService friendService = ServiceManager.Instance.FriendService;
        if (friendService == null) return;

        bool isSuccess = await friendService.TryAddFriendAsync(myUid, targetUid);

        if (isSuccess)
        {
            InvokeCompleteAddFriend();
        }
        else
        {
            InvokeFailAddFriend();
        }
    }

    private void InvokeCompleteLoadInfo()
    {
        if (OnCompleteLoadInfo != null)
        {
            OnCompleteLoadInfo.Invoke();
        }
    }

    private void InvokeCompleteAddFriend()
    {
        if (OnCompleteAddFriend != null)
        {
            OnCompleteAddFriend.Invoke();
        }
    }

    private void InvokeFailAddFriend()
    {
        if (OnFailAddFriend != null)
        {
            OnFailAddFriend.Invoke();
        }
    }
}