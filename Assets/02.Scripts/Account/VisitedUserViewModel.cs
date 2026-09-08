using System;
using UnityEngine;

public class VisitedUserViewModel : ViewModelBase
{
    private VisitedUserService _service;

    public event Action OnCompleteLoadInfo;

    private long _displayUid = 0;
    public long DisplayUid
    {
        get { return _displayUid; }
        set
        {
            if (_displayUid != value)
            {
                _displayUid = value;
                OnPropertyChanged(nameof(DisplayUid));
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

    public void SetService(VisitedUserService service)
    {
        _service = service;
    }

    public async void RequestLoadVisitedInfo()
    {
        if (_service == null) return;

        VisitedUserInfoData data = await _service.GetVisitedUserInfoAsync();

        if (data != null)
        {
            DisplayUid = data.UserUid;
            DisplayUserName = data.UserName;

            if (data.UserIconId != "")
            {
                Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(data.UserIconId);
                DisplayUserIcon = loadedSprite;
            }

            InvokeCompleteLoadInfo();
        }
    }

    private void InvokeCompleteLoadInfo()
    {
        if (OnCompleteLoadInfo != null)
        {
            OnCompleteLoadInfo.Invoke();
        }
    }
}