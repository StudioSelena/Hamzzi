using UnityEngine;

public class StealSeedUI : ViewBase
{
    [SerializeField] private UIButton Button_CloseBack;
    [SerializeField] private UIButton Button_Close;

    private void OnEnable()
    {
        Button_CloseBack.BindOnClickButtonEvent(OnClick_Close);
        Button_Close.BindOnClickButtonEvent(OnClick_Close);
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseStealSeedUI();
    }
}


