// 방치 보상 수령 팝업 UI. 오프라인 경과 시간과 보상 수량을 표시한다
using TMPro;
using UnityEngine;

public class IdleRewardPopupUI : UIBase
{
    private const int SecondsPerHour = 3600;
    private const int SecondsPerMinute = 60;
    private const int PercentScale = 100;
    private const string BuffCheckingText = "로딩 중";

    [SerializeField] private TMP_Text Text_OfflineTime;
    [SerializeField] private TMP_Text Text_RewardCount;
    [SerializeField] private UIButton Button_GetReward;
    [SerializeField] private TMP_Text Text_Effect;

    private void OnEnable()
    {
        Button_GetReward.BindOnClickButtonEvent(OnClickGetReward);
        ServiceManager.Instance.NetworkBuildService.OnBuildAndFurnitureDataLoaded += HandleBuildAndFurnitureDataLoaded;
    }

    private void OnDisable()
    {
        ServiceManager.Instance.NetworkBuildService.OnBuildAndFurnitureDataLoaded -= HandleBuildAndFurnitureDataLoaded;
    }

    public void SetRewardInfo(int rewardAmount, float elapsedSeconds, float capSeconds)
    {
        Text_RewardCount.text = rewardAmount.ToString();

        int capHours = Mathf.FloorToInt(capSeconds / SecondsPerHour);
        Text_OfflineTime.text = $"{BuildElapsedTimeText(elapsedSeconds)} (최대 {capHours}시간)";

        RefreshBuffText();
    }

    private void HandleBuildAndFurnitureDataLoaded()
    {
        RefreshBuffText();
    }

    private void RefreshBuffText()
    {
        if (ServiceManager.Instance.NetworkBuildService.IsBuildAndFurnitureDataLoaded == false)
        {
            Text_Effect.text = BuffCheckingText;
            return;
        }

        UserViewModel userVm = ServiceManager.Instance.UserService.GetUserViewModel();
        SetBuffText(userVm.GetSeedBuffRate());
    }

    private void SetBuffText(float buffRate)
    {
        if (buffRate <= 0f)
        {
            Text_Effect.text = "없음";
            return;
        }

        int buffPercent = Mathf.RoundToInt(buffRate * PercentScale);
        Text_Effect.text = $"가구 버프 +{buffPercent}%";
    }

    private string BuildElapsedTimeText(float elapsedSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(elapsedSeconds);
        int hours = totalSeconds / SecondsPerHour;
        int minutes = (totalSeconds % SecondsPerHour) / SecondsPerMinute;
        int seconds = totalSeconds % SecondsPerMinute;

        return $"{hours}시간 {minutes}분 {seconds}초";
    }

    private void OnClickGetReward()
    {
        InGameManager.Instance.ClaimIdleReward();
    }
}