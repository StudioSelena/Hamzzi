using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;

public class FriendListView : UIBase
{
    [SerializeField] private Transform Transform_Content;
    [SerializeField] private GameObject Prefab_FriendSlot;
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_Search;

    private FriendListViewModel _vm;
    private List<GameObject> _spawnedSlots = new List<GameObject>();

    private void Awake()
    {
        FriendListService service = ServiceManager.Instance.FriendListService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }
    }

    private void OnEnable()
    {
        Button_Close.BindOnClickButtonEvent(OnClickClose);
        Button_Search.BindOnClickButtonEvent(OnClickSearch);

        if (_vm != null)
        {
            _vm.RequestLoadFriendList();
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
            _vm.OnCompleteLoadFriendList -= OnCompleteLoadFriendList_View;
        }
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
    }

    public void BindViewModel(FriendListViewModel vm)
    {
        _vm = vm;

        _vm.PropertyChanged += OnPropChanged_View;
        _vm.OnCompleteLoadFriendList += OnCompleteLoadFriendList_View;
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.FriendListUI);
    }

    private void OnClickSearch()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.AccountSearchUI);
    }

    private void OnCompleteLoadFriendList_View()
    {
        ClearSpawnedSlots();

        if (_vm != null)
        {
            int count = _vm.FriendList.Count;

            for (int i = 0; i < count; i++)
            {
                FriendInfoData friendData = _vm.FriendList[i];

                GameObject slotObj = Instantiate(Prefab_FriendSlot, Transform_Content);
                FriendSlotUI slotView = slotObj.GetComponent<FriendSlotUI>();

                if (slotView != null)
                {
                    slotView.SetFriendData(friendData);
                }

                _spawnedSlots.Add(slotObj);
            }
        }
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