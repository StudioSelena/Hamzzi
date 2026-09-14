using System.ComponentModel;
using UnityEngine;
using TMPro;

public class ChangePlayerNameView : UIBase
{
    [SerializeField] private TMP_InputField InputField_Name;
    [SerializeField] private UIButton Button_Confirm;
    [SerializeField] private UIButton Button_Close;

    [SerializeField] private UIButton Button_BackgroundBlocker;

    private ChangePlayerNameViewModel _vm;

    private void Awake()
    {
        ChangePlayerNameService service = ServiceManager.Instance.ChangePlayerNameService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }
    }

    public void BindViewModel(ChangePlayerNameViewModel vm)
    {
        _vm = vm;

        _vm.PropertyChanged += OnPropChanged_View;
        _vm.OnCompleteChangeName += OnCompleteChangeName_View;
        _vm.OnFailChangeName += OnFailChangeName_View;
    }

    private void OnEnable()
    {
        Button_Confirm.BindOnClickButtonEvent(OnClickConfirm);
        Button_Close.BindOnClickButtonEvent(OnClickClose);

        if (Button_BackgroundBlocker != null)
        {
            Button_BackgroundBlocker.BindOnClickButtonEvent(OnClickClose);
        }

        InputField_Name.onValueChanged.AddListener(OnChangeName);
    }

    private void OnDisable()
    {
        InputField_Name.onValueChanged.RemoveListener(OnChangeName);
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropChanged_View;
            _vm.OnCompleteChangeName -= OnCompleteChangeName_View;
            _vm.OnFailChangeName -= OnFailChangeName_View;
        }
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
    }

    private void OnChangeName(string text)
    {
        if (_vm != null)
        {
            _vm.InputName = text;
        }
    }

    private void OnClickConfirm()
    {
        if (_vm != null)
        {
            _vm.RequestChangePlayerName();
        }
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.ChangePlayerNameUI);
    }

    private void OnCompleteChangeName_View()
    {
        Debug.Log("인게임 닉네임 변경 성공!");
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.ChangePlayerNameUI);
    }

    private void OnFailChangeName_View()
    {
        Debug.Log("인게임 닉네임 변경 실패.");
    }
}