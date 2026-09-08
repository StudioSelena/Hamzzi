using Cysharp.Threading.Tasks;
using UnityEngine;

public class ServiceManager : SingletonBase<ServiceManager>
{
    public ShopService ShopService { get; private set; }
    public BuildService BuildService { get; private set; }
    public HousingService HousingService { get; private set; }
    public UserService UserService { get; private set; }
    public NetworkCollectionService CollectionService { get; private set; }
    public NetworkGachaService GachaService { get; private set; }
    public LoginService LoginService { get; private set; }
    public FriendListService FriendListService { get; private set; }
    public AccountSearchService AccountSearchService { get; private set; }
    public FriendService FriendService { get; private set; }
    public AccountInfoService AccountInfoService { get; private set; }
    public SetPlayerNameService SetPlayerNameService { get; private set; }
    public NetworkBuildService NetworkBuildService { get; private set; }
    public FriendRequestService FriendRequestService { get; private set; }
    public VisitedUserService VisitedUserService { get; private set; }
    public ProfileSettingService ProfileSettingService { get; private set; }
    public HamsterModelService HamsterModelService { get; private set; }

    public void Start()
    {
        Debug.Log($"[ServiceManager Start] {GetHashCode()}");

        InitShopService();
        ShopService.InitShop().Forget();

        InitBuildService();
        InitHousingService();
        InitCollectionService();
        InitGachaService();
        InitLoginService();
        InitFriendListService();
        InitUserService();

        LoginService.GetViewModel().OnCompleteLogin += LoadDataFromDB;
        LoginService.GetViewModel().OnCompleteLogin += LoadInventory;

        InitAccountSearchService();
        InitFriendService();
        InitAccountInfoService();
        InitSetPlayerNameService();
        InitNetworkBuildService();
        InitFriendRequestService();
        InitVisitedUserService();
        InitProfileSettingService();
        InitModelViewrService();
    }

    private void InitShopService()
    {
        ShopService = new ShopService();
    }

    private void InitBuildService()
    {
        BuildService = new BuildService();
    }

    private void InitNetworkBuildService()
    {
        NetworkBuildService = new NetworkBuildService();
    }

    private void InitHousingService()
    {
        HousingService = new HousingService();
    }

    private void InitCollectionService()
    {
        CollectionService = new NetworkCollectionService();
        //CollectionService.GetCollectionViewModel();
        CollectionService.GetHamsterViewModel();
    }

    private void InitGachaService()
    {
        GachaService = new NetworkGachaService();
        GachaService.GetGachaViewModel();
    }

    private void InitUserService()
    {
        UserService = new UserService();
    }

    private void InitLoginService()
    {
        LoginService = new LoginService();
    }

    private void InitFriendListService()
    {
        FriendListService = new FriendListService();
    }

    private void InitAccountSearchService()
    {
        AccountSearchService = new AccountSearchService();
    }

    private void InitFriendService()
    {
        FriendService = new FriendService();
    }

    private void InitAccountInfoService()
    {
        AccountInfoService = new AccountInfoService();
    }

    private void InitSetPlayerNameService()
    {
        SetPlayerNameService = new SetPlayerNameService();
    }

    private void InitFriendRequestService()
    {
        FriendRequestService = new FriendRequestService();
    }

    private void InitVisitedUserService()
    {
        VisitedUserService = new VisitedUserService();
    }

    private void InitProfileSettingService()
    {
        ProfileSettingService = new ProfileSettingService();
    }

    private void InitModelViewrService()
    {
        HamsterModelService = new HamsterModelService();
        HamsterModelService.GetHamsterModelViewModel();
    }

    public void LoadDataFromDB()
    {
        var loginVM = LoginService.GetViewModel();
        long userUID = loginVM.UserUID;

        Debug.Log($"User UID : {userUID}");
        CollectionService.LoadHamsterCollectionData(userUID).Forget();  
        HamsterModelService.LoadHamsterModel().Forget();
        HamsterManager.Instance.Init();
        UserService.InitUser(userUID).Forget();

        CollectionService.SetCurrentCollectionViewModel(userUID);
    }

    public void LoadInventory()
    {
        var loginVM = LoginService.GetViewModel();
        long userUID = loginVM.UserUID;

        HousingService.LoadInventory(userUID).Forget();
    }
}