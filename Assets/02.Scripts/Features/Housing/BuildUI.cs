using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildUI : ViewBase
{
    [SerializeField] private GameObject Panel_Vignette;
    [SerializeField] private GameObject Panel_InfoText;
    [SerializeField] private TextMeshProUGUI Text_Info;

    [SerializeField] private Button Button_Exit;
    [SerializeField] private Button Button_Confirm;
    [SerializeField] private Button Button_Destroy;
    [SerializeField] private Button Button_Connect;

    private BuildViewModel _buildVM;

    private void Awake()
    {
        Button_Exit.onClick.AddListener(OnClickClose);
        Button_Confirm.onClick.AddListener(OnClickConfirm);
        Button_Destroy.onClick.AddListener(OnClickDestroy);
        Button_Connect.onClick.AddListener(OnClickConnect);

        ResetUI();
    }

    private void OnEnable()
    {
        if (_buildVM == null)
        {
            BindViewModel(ServiceManager.Instance.BuildService.GetBuildViewModel());
        }

        Button_Exit.gameObject.SetActive(true);

        Panel_InfoText.SetActive(true);
        Text_Info.text = "땅을 터치해 새로운 굴을 만들거나 기존 굴을 터치해 관리하세요!";

        _buildVM.EnterBuildMode();
    }

    public void BindViewModel(BuildViewModel buildVM)
    {
        _buildVM = buildVM;
        _buildVM.PropertyChanged += OnPropertyChanged_View;
    }

    private void OnDestroy()
    {
        _buildVM.PropertyChanged -= OnPropertyChanged_View;
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_buildVM.SelectType):

                if (_buildVM.SelectType == BuildType.Room)
                {
                    OpenVignette();
                }
                else if (_buildVM.SelectType == BuildType.Aisle)
                {
                    EnterAisleMode();
                }

                break;

            case nameof(_buildVM.CanConfirm):
                Button_Confirm.gameObject.SetActive(_buildVM.CanConfirm);
                break;

            case nameof(_buildVM.SelectRoom):
                UpdateSelectionUI();
                break;
        }
    }

    private void OpenVignette()
    {
        Panel_Vignette.SetActive(true);
    }

    private void EnterAisleMode()
    {
        Text_Info.text = "건설한 굴과 연결할 다른 굴을 터치해주세요!";

        Button_Destroy.gameObject.SetActive(false);
        Button_Connect.gameObject.SetActive(false);
    }

    private void OnClickClose()
    {
        _buildVM.CancelBuildMode();
        ResetUI();

        UIManager.Instance.CloseBuildUI();
        UIManager.Instance.OpenDecorUI();
    }

    private void OnClickConfirm()
    {
        _buildVM.ConfirmBuild();
        ResetUI();

        UIManager.Instance.CloseBuildUI();
        UIManager.Instance.OpenDecorUI();
    }

    private void OnClickDestroy()
    {
        _buildVM.DestroyRoom();
    }

    private void UpdateSelectionUI()
    {
        bool hasSelectedRoom = _buildVM.SelectRoom != null;
        bool canDestroy = _buildVM.CanDestroy;
        bool canConnect = _buildVM.CanConnectAisle;

        Button_Destroy.gameObject.SetActive(canDestroy);
        Button_Connect.gameObject.SetActive(canConnect);

        if (hasSelectedRoom)
        {
            if (canDestroy)
            {
                Text_Info.text = "선택한 굴을 파괴하거나 다른 굴과 통로를 연결할 수 있습니다.";
            }
            else
            {
                Text_Info.text = "기본 굴은 파괴할 수 없습니다.";
            }
        }
        else if (_buildVM.SelectType == BuildType.Room)
        {
            Text_Info.text = "땅을 터치해 새로운 굴을 만들거나 기존 굴을 터치해 관리하세요!";
        }
    }

    private void OnClickConnect()
    {
        _buildVM.StartConnectingAisle();
    }

    private void ResetUI()
    {
        Button_Confirm.gameObject.SetActive(false);
        Button_Exit.gameObject.SetActive(false);
        Button_Destroy.gameObject.SetActive(false);
        Button_Connect.gameObject.SetActive(false);
        Panel_InfoText.gameObject.SetActive(false);
        Panel_Vignette.SetActive(false);
    }
}
