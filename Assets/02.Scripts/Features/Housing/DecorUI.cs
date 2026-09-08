using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DecorUI : ViewBase
{
    [SerializeField] private Button Button_EnterBuild;
    [SerializeField] private Button Button_EnterHousing;
    [SerializeField] private UIButton Button_GoToInGame;

    private HousingViewModel _housingVM;
    private BuildViewModel _buildVM;
    private CameraController _cameraController;

    private void Awake()
    {
        Button_EnterBuild.onClick.AddListener(OnClickEnterBuild);
        Button_EnterHousing.onClick.AddListener(OnClickEnterHousing);

        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void OnEnable()
    {
        Button_GoToInGame.BindOnClickButtonEvent(OnClickGoToInGame);
    }

    private void Start()
    {
        _housingVM = ServiceManager.Instance.HousingService.GetHousingViewModel();
        _buildVM = ServiceManager.Instance.BuildService.GetBuildViewModel();
    }

    private void OnClickEnterBuild()
    {
        _cameraController.StopFollowHamster();
        _cameraController.ShowOverview(false).Forget();

        _housingVM.TargetRoom = null;
        _housingVM.EnterOverviewMode();
         
        _buildVM.EnterBuildMode();
        _buildVM.SelectType = BuildType.None;

        UIManager.Instance.OpenBuildUI();
        UIManager.Instance.CloseDecorUI();
    }

    private void OnClickEnterHousing()
    {
        _cameraController.StopFollowHamster();

        RoomViewModel currentRoom = _housingVM.TargetRoom;
        _housingVM.EnterHousingMode(currentRoom);

        UIManager.Instance.OpenHousingUI();
        UIManager.Instance.CloseDecorUI();
    }

    private void OnClickGoToInGame()
    {
        UIManager.Instance.CloseDecorUI();
        UIManager.Instance.OpenInGameUI();
    }
}
