using System.ComponentModel;
using UnityEngine;
using TMPro;

public class AccountSearchView : UIBase
{
    [SerializeField] private TMP_InputField InputField_Id;
    [SerializeField] private UIButton Button_Search;
    [SerializeField] private UIButton Button_Close;

    private AccountSearchViewModel _vm;

    private void Start()
    {
        AccountSearchService service = ServiceManager.Instance.AccountSearchService;

        if (service != null)
        {
            BindViewModel(service.GetViewModel());
        }
    }

    private void OnEnable()
    {
        Button_Search.BindOnClickButtonEvent(OnClickSearch);
        InputField_Id.onValueChanged.AddListener(OnChangeId);
        Button_Close.BindOnClickButtonEvent(OnClickClose);
    }

    private void OnDisable()
    {
        InputField_Id.onValueChanged.RemoveListener(OnChangeId);
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropChanged_View;
            _vm.OnCompleteSearch -= OnCompleteSearch_View;
            _vm.OnFailSearch -= OnFailSearch_View;
        }
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
    }

    public void BindViewModel(AccountSearchViewModel vm)
    {
        _vm = vm;

        _vm.PropertyChanged += OnPropChanged_View;
        _vm.OnCompleteSearch += OnCompleteSearch_View;
        _vm.OnFailSearch += OnFailSearch_View;
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.AccountSearchUI);
    }

    private void OnChangeId(string text)
    {
        if (_vm != null)
        {
            _vm.InputId = text;
        }
    }

    private void OnClickSearch()
    {
        if (_vm != null)
        {
            _vm.RequestSearch();
        }
    }

    private void OnCompleteSearch_View()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.AccountInfoUI);
    }
        
    private void OnFailSearch_View()
    {
        UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.SearchFailUI);
    }
}