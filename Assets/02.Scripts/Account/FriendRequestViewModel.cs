using System;
using System.Collections.Generic;

public class FriendRequestViewModel : ViewModelBase
{
    private FriendRequestService _service;

    public event Action OnCompleteLoadRequests;
    public event Action<long> OnCompleteAccept;
    public event Action<long> OnCompleteReject;

    private List<FriendRequestData> _requestList = new List<FriendRequestData>();
    public List<FriendRequestData> RequestList
    {
        get { return _requestList; }
        set
        {
            if (_requestList != value)
            {
                _requestList = value;
                OnPropertyChanged(nameof(RequestList));
            }
        }
    }

    public void SetService(FriendRequestService service)
    {
        _service = service;
    }

    public async void RequestLoadFriendRequests()
    {
        if (_service == null) return;

        LoginService loginService = ServiceManager.Instance.LoginService;
        if (loginService == null) return;

        long myUid = loginService.GetViewModel().UserUID;
        if (myUid == 0) return;

        List<FriendRequestData> dataList = await _service.GetFriendRequestsAsync(myUid);

        if (dataList != null)
        {
            RequestList = dataList;
            InvokeCompleteLoadRequests();
        }
    }

    public async void RequestAcceptFriend(long targetUid)
    {
        if (_service == null || targetUid == 0) return;

        long myUid = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        if (myUid == 0) return;

        bool isSuccess = await _service.AcceptFriendRequestAsync(myUid, targetUid);

        if (isSuccess)
        {
            InvokeCompleteAccept(targetUid);
            RequestLoadFriendRequests();
        }
    }

    public async void RequestRejectFriend(long targetUid)
    {
        if (_service == null || targetUid == 0) return;

        long myUid = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        if (myUid == 0) return;

        bool isSuccess = await _service.RejectFriendRequestAsync(myUid, targetUid);

        if (isSuccess)
        {
            InvokeCompleteReject(targetUid);
            RequestLoadFriendRequests();
        }
    }

    private void InvokeCompleteLoadRequests()
    {
        if (OnCompleteLoadRequests != null)
        {
            OnCompleteLoadRequests.Invoke();
        }
    }

    private void InvokeCompleteAccept(long targetUid)
    {
        if (OnCompleteAccept != null)
        {
            OnCompleteAccept.Invoke(targetUid);
        }
    }

    private void InvokeCompleteReject(long targetUid)
    {
        if (OnCompleteReject != null)
        {
            OnCompleteReject.Invoke(targetUid);
        }
    }
}