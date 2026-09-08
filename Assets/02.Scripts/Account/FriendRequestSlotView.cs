using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendRequestSlotView : MonoBehaviour
{
    [SerializeField] private Image Image_FriendIcon;
    [SerializeField] private TextMeshProUGUI TextMesh_FriendName;
    [SerializeField] private UIButton Button_Accept;
    [SerializeField] private UIButton Button_Reject;

    private FriendRequestViewModel _vm;
    private long _targetUid = 0;

    private void OnEnable()
    {
        Button_Accept.BindOnClickButtonEvent(OnClickAccept);
        Button_Reject.BindOnClickButtonEvent(OnClickReject);
    }

    public async void SetData(FriendRequestData data, FriendRequestViewModel vm)
    {
        if (data == null || vm == null) return;

        _vm = vm;
        _targetUid = data.FriendUid;
        TextMesh_FriendName.text = data.FriendName;

        if (data.FriendIconId != "")
        {
            Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(data.FriendIconId);
            if (Image_FriendIcon != null && loadedSprite != null)
            {
                Image_FriendIcon.sprite = loadedSprite;
            }
        }
    }

    private void OnClickAccept()
    {
        if (_vm != null && _targetUid != 0)
        {
            _vm.RequestAcceptFriend(_targetUid);
        }
    }

    private void OnClickReject()
    {
        if (_vm != null && _targetUid != 0)
        {
            _vm.RequestRejectFriend(_targetUid);
        }
    }
}