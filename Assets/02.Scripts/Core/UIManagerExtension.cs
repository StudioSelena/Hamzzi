using System;
using UnityEngine;

public enum UIRootType 
{
    None,
    BackgroundUI,
    MainUI,
    ContentUI,
    PopupUI,
    VeryFrontUI,
}

public enum UIType
{
    LoadingUI,
    TitleUI,
    TitleSettingsUI,
    InGameSettingsUI,
    InGameUI,
    ShopUI,
    BuildUI,
    LoginUI,
    AccountSearchUI,
    AccountInfoUI,
    SearchFailUI,
    SetPlayerNameUI,
    FriendListUI,
    CollectionUI,
    GachaUI,
    HousingUI,
    FeverTimeCutsceneUI,
    FeverTimeResultUI,
    WheelUI,
    DecorUI,
    IdleRewardPopupUI,
    FriendRequestListUI,
    CrossUI,
    ProfileSettingUI,
}

public static class UIManagerExtension
{
    public static string GetUIPath(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        string path = string.Empty;

        path = $"UI/{uiRootType}/{uiType}";
        return path;
    }

    public static void OpenLoadingUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }

    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }

    public static void OpenTitleUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.MainUI, UIType.TitleUI);
    }

    public static void CloseTitleUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.MainUI, UIType.TitleUI);
    }

    public static void OpenTitleSettingsUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.PopupUI, UIType.TitleSettingsUI);
    }

    public static void CloseTitleSettingsUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.TitleSettingsUI);
    }

    public static void OpenInGameUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.MainUI, UIType.InGameUI);
    }

    public static void CloseInGameUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.MainUI, UIType.InGameUI);
    }

    public static void OpenBuildUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.ContentUI, UIType.BuildUI);
    }

    public static void CloseBuildUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.BuildUI);
    }

    public static void OpenShopUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.ContentUI, UIType.ShopUI);
    }

    public static void CloseShopUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.ShopUI);
    }

    public static void OpenFeverTimeCutsceneUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.PopupUI, UIType.FeverTimeCutsceneUI);
    }

    public static void CloseFeverTimeCutsceneUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.FeverTimeCutsceneUI);
    }

    public static void OpenFeverTimeResultUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.PopupUI, UIType.FeverTimeResultUI);
    }

    public static void CloseFeverTimeResultUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.FeverTimeResultUI);
    }

    public static void OpenDecorUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.MainUI, UIType.DecorUI);
    }

    public static void CloseDecorUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.MainUI, UIType.DecorUI);
    }

    public static void OpenHousingUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.ContentUI, UIType.HousingUI);
    }

    public static void CloseHousingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.HousingUI);
    }

    public static void OpenLoginUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.PopupUI, UIType.LoginUI);
    }

    public static void CloseLoginUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.LoginUI);
    }

    public static void OpenWheelUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.PopupUI, UIType.WheelUI);
    }

    public static void CloseWheelUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.WheelUI);
    }

    public static void OpenSetNameUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.PopupUI, UIType.SetPlayerNameUI);
    }

    public static void CloseSetNameUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.SetPlayerNameUI);
    }

    public static void OpenIdleRewardPopupUI(this UIManager uiManager, int rewardAmount, float elapsedSeconds, float capSeconds)
    {
        UIBase openedUI = uiManager.OpenUI(UIRootType.PopupUI, UIType.IdleRewardPopupUI);
        IdleRewardPopupUI popupUI = openedUI as IdleRewardPopupUI;
        popupUI.SetRewardInfo(rewardAmount, elapsedSeconds, capSeconds);
    }

    public static void CloseIdleRewardPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.IdleRewardPopupUI);
    }
}