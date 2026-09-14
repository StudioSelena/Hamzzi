using TMPro;
using UnityEngine;

public class StealSeedResultUI : ViewBase
{
    [Header("버튼")]
    [SerializeField] private UIButton Button_Confirm;

    [Header("씨앗 개수")]
    [SerializeField] private TMP_Text Text_StealSeedCount;

    private void OnEnable()
    {
        Button_Confirm.BindOnClickButtonEvent(OnClick_Confirm);
    }

    private void OnClick_Confirm()
    {
        UIManager.Instance.CloseStealSeedResultUI();
        UIManager.Instance.CloseStealSeedUI();
    }

    public void SetStealSeedCount(int stealSeedCount)
    {
        Text_StealSeedCount.text = stealSeedCount.ToString();
    }
}
