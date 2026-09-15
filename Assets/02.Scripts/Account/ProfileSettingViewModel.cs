using System;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSettingViewModel : ViewModelBase
{
    private ProfileSettingService _service;

    public event Action OnCompleteLoadIcons;
    public event Action OnCompleteChangeIcon;

    private List<string> _iconPathList = new List<string>();
    public List<string> IconPathList
    {
        get { return _iconPathList; }
        set
        {
            if (_iconPathList != value)
            {
                _iconPathList = value;
                OnPropertyChanged(nameof(IconPathList));
            }
        }
    }

    private string _selectedIconPath = "";
    public string SelectedIconPath
    {
        get { return _selectedIconPath; }
        set
        {
            if (_selectedIconPath != value)
            {
                _selectedIconPath = value;
                OnPropertyChanged(nameof(SelectedIconPath));
            }
        }
    }

    public void SetService(ProfileSettingService service)
    {
        _service = service;
    }

    public void RequestLoadIcons()
    {
        List<ProfileIconData> iconDataList = GameDataManager.Instance.GetAllData<ProfileIconData>();
        List<string> paths = new List<string>();

        if (iconDataList != null)
        {
            int count = iconDataList.Count;
            Debug.Log($"[프로필 세팅] 불러온 아이콘 데이터 개수: {count}개");

            for (int i = 0; i < count; i++)
            {
                paths.Add(iconDataList[i].IconPath);
            }
        }
        else
        {
            Debug.LogError("[프로필 세팅] ProfileIconData 리스트가 null입니다! GameDataManager에서 로드되지 않았습니다.");
        }

        IconPathList = paths;

        if (ServiceManager.Instance.UserService != null)
        {
            UserViewModel userVm = ServiceManager.Instance.UserService.GetUserViewModel();
            if (userVm != null)
            {
                SelectedIconPath = userVm.UserIconId;
            }
        }

        InvokeCompleteLoadIcons();
    }

    public async void RequestChangeIcon(string targetIconPath)
    {
        if (_service == null || targetIconPath == "") return;

        LoginService loginService = ServiceManager.Instance.LoginService;
        if (loginService == null) return;

        long myUid = loginService.GetViewModel().UserUID;
        if (myUid == 0) return;

        bool isSuccess = await _service.TrySetUserIconAsync(myUid, targetIconPath);

        if (isSuccess)
        {
            SelectedIconPath = targetIconPath;

            if (ServiceManager.Instance.UserService != null)
            {
                UserViewModel userVm = ServiceManager.Instance.UserService.GetUserViewModel();
                if (userVm != null)
                {
                    userVm.UserIconId = targetIconPath;
                }
            }

            InvokeCompleteChangeIcon();
        }
    }

    private void InvokeCompleteLoadIcons()
    {
        if (OnCompleteLoadIcons != null)
        {
            OnCompleteLoadIcons.Invoke();
        }
    }

    private void InvokeCompleteChangeIcon()
    {
        if (OnCompleteChangeIcon != null)
        {
            OnCompleteChangeIcon.Invoke();
        }
    }
}