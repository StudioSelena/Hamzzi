using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : ViewBase
{
    [Header("버튼")]
    [SerializeField] private UIButton Button_ShopUI;
    [SerializeField] private UIButton Button_DecorUI;
    [SerializeField] private UIButton Button_FriendUI;
    [SerializeField] private UIButton Button_CollectionUI;
    [SerializeField] private UIButton Button_Gacha;
    [SerializeField] private UIButton Button_Setting;
    [SerializeField] private UIButton Button_Garden;
    [SerializeField] private UIButton Button_Exit;
    [SerializeField] private UIButton Button_GoHome;
    [SerializeField] private UIButton Button_ProfileSetting;
    [SerializeField] private UIButton Button_Cross;

    [Header("보유 씨앗")]
    [SerializeField] private GameObject Prefab_CurrencyUI;

    [Header("DB 연동")]
    [SerializeField] private Image Image_UserIcon;
    [SerializeField] private TMP_Text Text_UserName;


    private UserViewModel _userVm;
    private VisitedUserViewModel _visitedUserVm;

    private HousingViewModel _housingVM;
    private CameraController _cameraController;

    private void Awake()
    {
        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void OnEnable()
    {
        Button_ShopUI.BindOnClickButtonEvent(OnClick_OpenShop, true);
        Button_DecorUI.BindOnClickButtonEvent(OnClick_OpenDecor, true);
        Button_FriendUI.BindOnClickButtonEvent(OnClick_OpenFriend);
        Button_CollectionUI.BindOnClickButtonEvent(OnClick_OpenCollectionUI);
        Button_Gacha.BindOnClickButtonEvent(OnClick_OpenGachaUI, true);
        Button_Setting.BindOnClickButtonEvent(OnClick_OpenSetting);
        Button_Garden.BindOnClickButtonEvent(OnClick_Garden, true);
        Button_Exit.BindOnClickButtonEvent(OnClick_Exit, true);
        Button_GoHome.BindOnClickButtonEvent(OnClick_GoHome, true);
        Button_ProfileSetting.BindOnClickButtonEvent(OnClick_ProfileSetting);
        Button_Cross.BindOnClickButtonEvent(OnClick_Cross, true);

        if (_housingVM == null)
        {
            _housingVM = ServiceManager.Instance.HousingService?.GetHousingViewModel();
        }

        _housingVM.PropertyChanged += OnPropertyChanged_VM;

        FindUserViewModelAndBind();
        FindVisitedViewModelAndBind();

        UpdateButton();
    }

    private void OnDisable()
    {
        _userVm.PropertyChanged -= OnPropChanged_UserInfoView;
        _housingVM.PropertyChanged -= OnPropertyChanged_VM;
        _visitedUserVm.PropertyChanged -= OnPropChanged_VisitedUserView;
        _visitedUserVm.OnCompleteLoadInfo -= OnCompleteLoadVisitUserInfo;

        Button_ShopUI.UnBindOnClickButtonEvent(OnClick_OpenShop);
        Button_DecorUI.UnBindOnClickButtonEvent(OnClick_OpenDecor);
        Button_Gacha.UnBindOnClickButtonEvent(OnClick_OpenGachaUI);
        Button_Garden.UnBindOnClickButtonEvent(OnClick_Garden);
        Button_Exit.UnBindOnClickButtonEvent(OnClick_Exit);
        Button_GoHome.UnBindOnClickButtonEvent(OnClick_GoHome);
        Button_Cross.UnBindOnClickButtonEvent(OnClick_Cross);
    }

    private void FindUserViewModelAndBind()
    {
        var userVm = ServiceManager.Instance.UserService.GetUserViewModel();
        _userVm = userVm;

        _userVm.PropertyChanged += OnPropChanged_UserInfoView;
        _userVm.InvokeOnceOnInit();
    }

    private void FindVisitedViewModelAndBind()
    {
        var visitedVm = ServiceManager.Instance.VisitedUserService.GetViewModel();
        _visitedUserVm = visitedVm;

        _visitedUserVm.PropertyChanged += OnPropChanged_VisitedUserView;
        _visitedUserVm.OnCompleteLoadInfo += OnCompleteLoadVisitUserInfo;
    }

    private void OnPropChanged_UserInfoView(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(UserViewModel.UserName):
                UpdateUserInfo().Forget();
                break;
            case nameof(UserViewModel.UserIconId):
                UpdateUserInfo().Forget();
                break;
        }
    }

    private void OnPropChanged_VisitedUserView(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_visitedUserVm.DisplayUid):
                UpdateButton();
                UpdateUserInfo().Forget();
                break;
            case nameof(_visitedUserVm.DisplayUserName):
            case nameof(_visitedUserVm.DisplayUserIcon):
                UpdateUserInfo().Forget();
                break;
        }
    }

    private void OnPropertyChanged_VM(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_housingVM.CurrentViewMode) || e.PropertyName == nameof(_housingVM.TargetRoom))
        {
            UpdateButton();
        }
    }

    private void OnCompleteLoadVisitUserInfo()
    {
        UpdateUserInfo().Forget();
    }

    private async UniTask UpdateUserInfo()
    {
        if(_visitedUserVm == null)
        {
            return;
        }

        if(_visitedUserVm.DisplayUid == 0)
        {
            if (string.IsNullOrEmpty(_userVm.UserIconId) == false)
            {
                Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(_userVm.UserIconId);

                Image_UserIcon.sprite = loadedSprite;
            }

            if (string.IsNullOrEmpty(_userVm.UserName) == false)
            {
                Text_UserName.text = _userVm.UserName;
            }
        }
        else
        {
            if (_visitedUserVm.DisplayUserIcon != null)
            {
                Image_UserIcon.sprite = _visitedUserVm.DisplayUserIcon;
            }

            if (string.IsNullOrEmpty(_visitedUserVm.DisplayUserName) == false)
            {
                Text_UserName.text = _visitedUserVm.DisplayUserName;
            }
        }
    }

    private void OnClick_OpenShop()
    {
        UIManager.Instance.OpenShopUI();
    }

    private void OnClick_OpenDecor()
    {
        UIManager.Instance.OpenDecorUI();
        UIManager.Instance.CloseInGameUI();
    }

    private void OnClick_OpenFriend()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.FriendListUI);
    }

    private void OnClick_OpenCollectionUI()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.CollectionUI);
    }

    private void OnClick_OpenGachaUI()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.GachaUI);
    }

    private void OnClick_OpenSetting()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.InGameSettingsUI);
    }

    private void OnClick_Garden()
    {
        _housingVM.TargetRoom = null;
        _housingVM.CurrentViewMode = HousingViewMode.Garden;
        _housingVM.EnterGardenMode();
        UpdateButton();
    }

    private void OnClick_Exit()
    {
        if (_cameraController.IsFollowing)
        {
            bool isGarden = (_housingVM.CurrentViewMode == HousingViewMode.Garden);

            _cameraController.StopFollowHamster();

            if (isGarden)
            {
                _cameraController.ShowGardenView().Forget();
                _housingVM.CurrentViewMode = HousingViewMode.Garden;
            }
            else
            {
                _cameraController.ShowOverview().Forget();
                _housingVM.CurrentViewMode = HousingViewMode.OverView;
            }

            UpdateButton();

            return;
        }

        _housingVM.TargetRoom = null;
        _housingVM.CurrentViewMode = HousingViewMode.OverView;
        _housingVM.EnterOverviewMode();
        UpdateButton();
    }

    private void OnClick_ProfileSetting()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.ProfileSettingUI);
    }

    private void OnClick_GoHome()
    {
        var visitedService = ServiceManager.Instance.VisitedUserService;

        visitedService.CurrentVisitedUid = 0;
        _visitedUserVm.DisplayUid = 0;


        GameManager.Instance.ChangeMap(ServiceManager.Instance.LoginService.GetViewModel().UserUID).Forget();

        UIManager.Instance.OpenLoadingUI();
        ServiceManager.Instance.LoadDataFromDB();
        
    }

    private void OnClick_Cross()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.CrossUI);
    }

    public void UpdateButton()
    {
        if(_visitedUserVm == null)
        {
            return;
        }

        bool isVisiting = _visitedUserVm.DisplayUid != 0;
        SetVisitMode(isVisiting);

        bool isFollowing = (_cameraController != null && _cameraController.IsFollowing);
        bool isSubView = (_housingVM.CurrentViewMode == HousingViewMode.Garden) || (_housingVM.TargetRoom != null) || isFollowing;

        Button_Exit.gameObject.SetActive(isSubView);
        Button_Garden.gameObject.SetActive(!isSubView);
    }

    private void SetVisitMode(bool isVisiting)
    {
        Button_ShopUI.gameObject.SetActive(!isVisiting);
        Button_DecorUI.gameObject.SetActive(!isVisiting);
        Button_Gacha.gameObject.SetActive(!isVisiting);
        Prefab_CurrencyUI.SetActive(!isVisiting);

        Button_GoHome.gameObject.SetActive(isVisiting);
        Button_Cross.gameObject.SetActive(isVisiting);
    }
}
