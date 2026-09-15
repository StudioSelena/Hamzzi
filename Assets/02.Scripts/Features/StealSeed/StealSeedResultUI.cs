using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StealSeedResultUI : ViewBase
{
    [Header("버튼")]
    [SerializeField] private UIButton Button_Confirm;

    [Header("씨앗 개수")]
    [SerializeField] private TMP_Text Text_StealSeedCount;

    private int _stealSeedCount;

    private void OnEnable()
    {
        Button_Confirm.BindOnClickButtonEvent(OnClick_Confirm);
    }

    private void OnClick_Confirm()
    {
        AddSeedCount();
        UIManager.Instance.CloseStealSeedResultUI();
        UIManager.Instance.CloseStealSeedUI();
    }

    private void AddSeedCount()
    {
        var userVm = ServiceManager.Instance.UserService.GetUserViewModel();
        if(userVm != null)
        {
            userVm.AddSeedWithoutBuff(_stealSeedCount);

            var userUid = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
            ServiceManager.Instance.UserService.SaveUserAsync(userUid).Forget();
        }
    }

    public void SetStealSeedCount(int stealSeedCount)
    {
        _stealSeedCount = stealSeedCount;
        Text_StealSeedCount.text = stealSeedCount.ToString();
    }
}
