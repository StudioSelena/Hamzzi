using System.Collections.Generic;
using UnityEngine;

public class ProfileSettingView : ViewBase
{
    [SerializeField] private Transform Transform_Content;
    [SerializeField] private GameObject Prefab_IconSlot;
    [SerializeField] private UIButton Button_Close;

    private ProfileSettingViewModel _vm;
    private List<GameObject> _spawnedSlots = new List<GameObject>();

    private void Awake()
    {
        ProfileSettingService service = ServiceManager.Instance.ProfileSettingService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }
    }

    private void OnEnable()
    {
        Button_Close.BindOnClickButtonEvent(OnClickClose);

        if (_vm != null)
        {
            _vm.RequestLoadIcons();
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
            _vm.OnCompleteLoadIcons -= OnCompleteLoadIcons_View;
            _vm.OnCompleteChangeIcon -= OnCompleteChangeIcon_View;
        }
    }

    public void BindViewModel(ProfileSettingViewModel vm)
    {
        _vm = vm;

        _vm.OnCompleteLoadIcons += OnCompleteLoadIcons_View;
        _vm.OnCompleteChangeIcon += OnCompleteChangeIcon_View;
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.ProfileSettingUI);
    }

    private void OnCompleteLoadIcons_View()
    {
        ClearSpawnedSlots();

        if (_vm == null) return;

        int count = _vm.IconPathList.Count;

        for (int i = 0; i < count; i++)
        {
            string iconPath = _vm.IconPathList[i];

            GameObject slotObj = Instantiate(Prefab_IconSlot, Transform_Content);
            ProfileIconSettingSlotView slotView = slotObj.GetComponent<ProfileIconSettingSlotView>();

            if (slotView != null)
            {
                slotView.SetData(iconPath, _vm);
            }

            _spawnedSlots.Add(slotObj);
        }
    }

    private void OnCompleteChangeIcon_View()
    {
        Debug.Log("프로필 이미지가 성공적으로 변경되었습니다.");
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