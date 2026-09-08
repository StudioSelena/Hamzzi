using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;

public class FriendRequestListView : UIBase
{
    [SerializeField] private Transform Transform_Content;
    [SerializeField] private GameObject Prefab_RequestSlot;

    private FriendRequestViewModel _vm;
    private List<GameObject> _spawnedSlots = new List<GameObject>();

    private void Awake()
    {
        FriendRequestService service = ServiceManager.Instance.FriendRequestService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }
    }

    private void OnEnable()
    {

        if (_vm != null)
        {
            _vm.RequestLoadFriendRequests();
        }
    }

    private void OnDisable()
    {
        ClearSpawnedSlots();
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropChanged_View;
            _vm.OnCompleteLoadRequests -= OnCompleteLoadRequests_View;
            _vm.OnCompleteAccept -= OnCompleteAccept_View;
            _vm.OnCompleteReject -= OnCompleteReject_View;
        }
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
    }

    public void BindViewModel(FriendRequestViewModel vm)
    {
        _vm = vm;

        _vm.PropertyChanged += OnPropChanged_View;
        _vm.OnCompleteLoadRequests += OnCompleteLoadRequests_View;
        _vm.OnCompleteAccept += OnCompleteAccept_View;
        _vm.OnCompleteReject += OnCompleteReject_View;
    }

    private void OnCompleteLoadRequests_View()
    {
        ClearSpawnedSlots();

        if (_vm == null) return;

        int count = _vm.RequestList.Count;

        for (int i = 0; i < count; i++)
        {
            FriendRequestData requestData = _vm.RequestList[i];

            GameObject slotObj = Instantiate(Prefab_RequestSlot, Transform_Content);
            FriendRequestSlotView slotView = slotObj.GetComponent<FriendRequestSlotView>();

            if (slotView != null)
            {
                slotView.SetData(requestData, _vm);
            }

            _spawnedSlots.Add(slotObj);
        }
    }

    private void OnCompleteAccept_View(long targetUid)
    {
        Debug.Log($"[{targetUid}] 친구 요청 수락 성공");
    }

    private void OnCompleteReject_View(long targetUid)
    {
        Debug.Log($"[{targetUid}] 친구 요청 거절 성공");
    }

    private void ClearSpawnedSlots()
    {
        int count = _spawnedSlots.Count;

        for (int i = 0; i < count; i++)
        {
            if (_spawnedSlots[i] != null)
            {
                Destroy(_spawnedSlots[i]);
            }
        }

        _spawnedSlots.Clear();
    }
}