using System.ComponentModel;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class SetPlayerNameView : UIBase
{
    [SerializeField] private TMP_InputField InputField_Name;
    [SerializeField] private UIButton Button_Confirm;
    [SerializeField] private UIButton Button_Close;

    private LoginViewModel _loginVm;
    private SetPlayerNameViewModel _setNameVm;

    private void Start()
    {
        SetPlayerNameService service = ServiceManager.Instance.SetPlayerNameService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }

        _loginVm = ServiceManager.Instance?.LoginService.GetViewModel();
        _loginVm.OnCompleteLogin += UIClose;
    }

    public void BindViewModel(SetPlayerNameViewModel vm)
    {
        _setNameVm = vm;

        _setNameVm.PropertyChanged += OnPropChanged_View;
        _setNameVm.OnCompleteSetName += OnCompleteSetName_View;
        _setNameVm.OnFailSetName += OnFailSetName_View;
        _setNameVm.OnCompleteSetName += OnCompleteEnter;
    }

    private void OnEnable()
    {
        Button_Confirm.BindOnClickButtonEvent(OnClickConfirm);
        Button_Close.BindOnClickButtonEvent(UIClose);
        InputField_Name.onValueChanged.AddListener(OnChangeName);
    }

    private void OnDisable()
    {
        InputField_Name.onValueChanged.RemoveListener(OnChangeName);
    }

    private void OnDestroy()
    {
        if (_setNameVm != null)
        {
            _setNameVm.PropertyChanged -= OnPropChanged_View;
            _setNameVm.OnCompleteSetName -= OnCompleteSetName_View;
            _setNameVm.OnFailSetName -= OnFailSetName_View;

            _setNameVm.OnCompleteSetName -= OnCompleteEnter;
        }
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
    }

    private void OnChangeName(string text)
    {
        if (_setNameVm != null)
        {
            _setNameVm.InputName = text;
        }
    }

    private void OnClickConfirm()
    {
        if (_setNameVm != null)
        {
            _setNameVm.RequestSetPlayerName();
        }
    }

    private void UIClose()
    {
        UIManager.Instance.CloseSetNameUI();
    }

    private void OnCompleteSetName_View()
    {
        Debug.Log("닉네임 설정 성공");
        _loginVm.InvokeCompleteLogin();
    }

    private void OnFailSetName_View()
    {
        Debug.Log("닉네임 설정 실패");
    }

    private void OnCompleteEnter()
    {
        if (_loginVm != null && _loginVm.UserUID != 0)
        {
            GameManager.Instance.InitMap(_loginVm.UserUID).Forget();
        }
    }
}