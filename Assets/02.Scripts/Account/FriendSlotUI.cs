using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendSlotUI : UIBase
{
    [SerializeField] private Image Image_FriendIcon;
    [SerializeField] private TextMeshProUGUI TextMesh_FriendName;
    [SerializeField] private TextMeshProUGUI TextMesh_FriendId; 
    [SerializeField] private UIButton Button_Visit;

    private long _friendUid = 0;
    public async void SetFriendData(FriendInfoData data)
    {
        if (data != null)
        {
            TextMesh_FriendName.text = data.FriendName;
            TextMesh_FriendId.text = data.FriendId.ToString();
            _friendUid = data.FriendUid;

            if (data.FriendIconId != "")
            {
                Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(data.FriendIconId);

                if (Image_FriendIcon != null && loadedSprite != null)
                {
                    Image_FriendIcon.sprite = loadedSprite;
                }
            }
        }
    }

    private void OnEnable()
    {
        Button_Visit.BindOnClickButtonEvent(OnClickVisit);
    }

    private void OnDisable()
    {
    }

    private void OnClickVisit()
    {
        var loginVm = ServiceManager.Instance.LoginService.GetViewModel();
        ServiceManager.Instance.UserService.SaveUserAsync(loginVm.UserUID).Forget();

        UIManager.Instance.OpenLoadingUI();

        ServiceManager.Instance.VisitedUserService.CurrentVisitedUid = _friendUid;
        Debug.Log($"친구 방문. 대상 UID: {_friendUid}");

        var visitedUserVm = ServiceManager.Instance.VisitedUserService.GetViewModel();
        if (visitedUserVm == null)
        {
            return;
        }

        visitedUserVm.RequestLoadVisitedInfo();

        ServiceManager.Instance.CollectionService.LoadHamsterCollectionData(_friendUid).Forget();
        ServiceManager.Instance.CollectionService.SetCurrentCollectionViewModel(_friendUid);

        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.FriendListUI);

        GameManager.Instance.ChangeMap(_friendUid).Forget();
    }
}