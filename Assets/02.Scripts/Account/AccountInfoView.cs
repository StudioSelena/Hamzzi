using System.ComponentModel;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class AccountInfoView : UIBase
{
    [SerializeField] private TextMeshProUGUI TextMesh_UserId;
    [SerializeField] private TextMeshProUGUI TextMesh_UserName;
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_AddFriend;
    [SerializeField] private UIButton Button_Visit; 
    [SerializeField] private Image Image_UserIcon;

    private AccountInfoViewModel _vm;

    private void Awake()
    {
        AccountInfoService service = ServiceManager.Instance.AccountInfoService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }
    }

    private void OnEnable()
    {
        Button_Close.BindOnClickButtonEvent(OnClickClose);
        Button_AddFriend.BindOnClickButtonEvent(OnClickAddFriend);
        Button_Visit.BindOnClickButtonEvent(OnClickVisit);

        if (_vm != null)
        {
            _vm.RequestLoadAccountInfo();
        }
    }

    private void OnDisable()
    {
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropChanged_View;
            _vm.OnCompleteLoadInfo -= OnCompleteLoadInfo_View;
            _vm.OnCompleteAddFriend -= OnCompleteAddFriend_View;
            _vm.OnFailAddFriend -= OnFailAddFriend_View;
        }
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(AccountInfoViewModel.DisplayUserId):
                TextMesh_UserId.text = _vm.DisplayUserId;
                break;

            case nameof(AccountInfoViewModel.DisplayUserName):
                TextMesh_UserName.text = _vm.DisplayUserName;
                break;

            case nameof(AccountInfoViewModel.DisplayUserIcon):
                if (Image_UserIcon != null && _vm.DisplayUserIcon != null)
                {
                    Image_UserIcon.sprite = _vm.DisplayUserIcon;
                }
                break;
        }
    }

    public void BindViewModel(AccountInfoViewModel vm)
    {
        _vm = vm;

        _vm.PropertyChanged += OnPropChanged_View;
        _vm.OnCompleteLoadInfo += OnCompleteLoadInfo_View;
        _vm.OnCompleteAddFriend += OnCompleteAddFriend_View;
        _vm.OnFailAddFriend += OnFailAddFriend_View;
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.AccountInfoUI);
    }

    private void OnClickAddFriend()
    {
        if (_vm != null)
        {
            _vm.RequestAddFriend();
        }
    }

    private void OnClickVisit()
    {
        var loginVm = ServiceManager.Instance.LoginService.GetViewModel();
        ServiceManager.Instance.UserService.SaveUserAsync(loginVm.UserUID).Forget();

        var visitedUserVm = ServiceManager.Instance.VisitedUserService.GetViewModel();

        if(visitedUserVm != null)
        {
            visitedUserVm.RequestLoadVisitedInfo();
        }

        UIManager.Instance.OpenLoadingUI();
        Debug.Log("방문하기");

        ServiceManager.Instance.CollectionService.LoadHamsterCollectionData(visitedUserVm.DisplayUid).Forget();
        ServiceManager.Instance.CollectionService.SetCurrentCollectionViewModel(visitedUserVm.DisplayUid);

        GameManager.Instance.ChangeMap(visitedUserVm.DisplayUid).Forget();
    }

    private void OnCompleteLoadInfo_View()
    {
        Debug.Log("계정 정보 로드 성공");
    }

    private void OnCompleteAddFriend_View()
    {
        Debug.Log("친구 요청 전송 완료");
    }

    private void OnFailAddFriend_View()
    {
        Debug.Log("친구 요청 실패. 이미 요청했거나 오류가 발생했습니다.");
    }
}