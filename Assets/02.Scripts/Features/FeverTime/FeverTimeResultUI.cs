using TMPro;
using UnityEngine;

public class FeverTimeResultUI : ViewBase
{
    [SerializeField] private TMP_Text Text_RewardSeedCount;
    [SerializeField] private UIButton Button_ClaimReward;

    private void OnEnable()
    {
        Button_ClaimReward.BindOnClickButtonEvent(OnClick_ClaimReward);

        int rewardSeedCount = FeverTimeManager.Instance.GetRewardSeedCount();
        Text_RewardSeedCount.text = rewardSeedCount.ToString();
    }

    private void OnClick_ClaimReward()
    {
        FeverTimeManager.Instance.ClaimReward();
        UIManager.Instance.CloseFeverTimeResultUI();
    }
}
