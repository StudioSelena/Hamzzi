using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginView : UIBase
{
    [SerializeField] private TMP_InputField InputField_Id;
    [SerializeField] private TMP_InputField InputField_Password;

    [SerializeField] private UIButton Button_Login;
    [SerializeField] private UIButton Button_CreateAccount;
    [SerializeField] private UIButton Button_BackgroundClose;
    [SerializeField] private TextMeshProUGUI TextMesh_Feedback;
    [SerializeField] private Toggle Toggle_AutoLogin;

    private LoginViewModel _vm;

    private void Awake()
    {
        LoginService service = ServiceManager.Instance.LoginService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }

        if (_vm != null)
        {
            _vm.CheckAutoLogin();
        }
    }

    private void OnEnable()
    {
        Button_Login.BindOnClickButtonEvent(OnClickLogin);
        Button_CreateAccount.BindOnClickButtonEvent(OnClickCreateAccount);

        Button_BackgroundClose.BindOnClickButtonEvent(OnClick_Close);

        InputField_Id.onValueChanged.AddListener(OnChangeId);
        InputField_Password.onValueChanged.AddListener(OnChangePassword);

        Toggle_AutoLogin.onValueChanged.AddListener(OnChangeAutoLoginToggle);
    }

    private void OnDisable()
    {
        _vm.FeedbackMessage = "";

        Button_Login.UnBindOnClickButtonEvent(OnClickLogin);
        Button_CreateAccount.UnBindOnClickButtonEvent(OnClickCreateAccount);

        InputField_Id.onValueChanged.RemoveListener(OnChangeId);
        InputField_Password.onValueChanged.RemoveListener(OnChangePassword);

        Toggle_AutoLogin.onValueChanged.RemoveListener(OnChangeAutoLoginToggle);
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropChanged_View;
            _vm.OnCompleteLogin -= OnCompleteLogin_View;
            _vm.OnFailLogin -= OnFailLogin_View;
            _vm.OnCompleteCreateAccount -= OnCompleteCreateAccount_View;
            _vm.OnFailCreateAccount -= OnFailCreateAccount_View;
        }
    }


    public void BindViewModel(LoginViewModel vm)
    {
        _vm = vm;

        _vm.PropertyChanged += OnPropChanged_View;
        _vm.OnCompleteLogin += OnCompleteLogin_View;
        _vm.OnFailLogin += OnFailLogin_View;
        _vm.OnCompleteCreateAccount += OnCompleteCreateAccount_View;
        _vm.OnFailCreateAccount += OnFailCreateAccount_View;

        TextMesh_Feedback.text = "";
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(LoginViewModel.InputId):
                InputField_Id.text = _vm.InputId;
                break;

            case nameof(LoginViewModel.InputPassword):
                InputField_Password.text = _vm.InputPassword;
                break;

            case nameof(LoginViewModel.IsAutoLogin):
                Toggle_AutoLogin.isOn = _vm.IsAutoLogin;
                break;

            case nameof(LoginViewModel.FeedbackMessage):
                TextMesh_Feedback.text = _vm.FeedbackMessage;
                break;
        }
    }

    private void OnChangeId(string text)
    {
        if (_vm != null)
        {
            _vm.InputId = text;
        }
    }

    private void OnChangePassword(string text)
    {
        if (_vm != null)
        {
            _vm.InputPassword = text;
        }
    }
    private void OnChangeAutoLoginToggle(bool isOn)
    {
        if (_vm != null)
        {
            _vm.IsAutoLogin = isOn;
        }
    }

    private void OnClickLogin()
    {
        if (_vm != null)
        {
            _vm.RequestLogin();
        }
    }

    private void OnClickCreateAccount()
    {
        if (_vm != null)
        {
            _vm.RequestCreateAccount();
        }
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseLoginUI();
    }

    private void OnCompleteLogin_View()
    {
        UIManager.Instance.CloseLoginUI();
        UIManager.Instance.CloseTitleUI();
        UIManager.Instance.OpenLoadingUI();
        UIManager.Instance.OpenInGameUI();
        Debug.Log("로그인 성공");
    }

    private void OnFailLogin_View()
    {
        Debug.Log("로그인 실패");
    }

    private void OnCompleteCreateAccount_View()
    {
        UIManager.Instance.CloseLoginUI();
        UIManager.Instance.OpenSetNameUI();
        Debug.Log("계정 생성 완료");
    }

    private void OnFailCreateAccount_View()
    {
        Debug.Log("계정 생성 실패");
    }
}