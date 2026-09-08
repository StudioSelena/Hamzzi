using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaView : ViewBase
{
    [Header("UI Base")]
    [SerializeField] private Button ExitButton;

    [Header("가챠 버튼")]
    [SerializeField] private Button DrawOneButton;
    [SerializeField] private TextMeshProUGUI DrawOnePriceText;
    [SerializeField] private Button DrawTenButton;
    [SerializeField] private TextMeshProUGUI DrawTenPriceText;

    [Header("가챠 결과창 UI")]
    [SerializeField] private GachaResultView GachaResultView;

    private CollectionViewModel _collectionViewModel;
    private UserViewModel _userViewModel;

    private void Start()
    {
        long userUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        _collectionViewModel = ServiceManager.Instance.CollectionService.GetCollectionViewModel(userUID);
        _userViewModel = ServiceManager.Instance.UserService.GetUserViewModel();

        _userViewModel.PropertyChanged += OnPropertyChanged;

        SetGachaPriceText();
    }

    private void OnEnable()
    {
        ExitButton.onClick.AddListener(CloseCollectionUI);

        DrawOneButton.onClick.AddListener(DrawOneHamster);
        DrawTenButton.onClick.AddListener(DrawTenHamster);
    }

    private void OnDisable()
    {
        ExitButton.onClick.RemoveListener(CloseCollectionUI);

        DrawOneButton.onClick.RemoveListener(DrawOneHamster);
        DrawTenButton.onClick.RemoveListener(DrawTenHamster);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(UserViewModel.SeedCount):
                CheckDrawable();
                break;
        }
    }

    private void CloseCollectionUI()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.GachaUI);
    }

    private void SetGachaPriceText()
    {
        int price = GachaViewModel.GachaPrice;
        DrawOnePriceText.text = $"{price}";
        DrawTenPriceText.text = $"{price * 10}";
    }

    private string DrawHamster()
    {
        HamsterSave hamsterSave = ServiceManager.Instance.GachaService.DrawGacha();
        _collectionViewModel.AddCollectedHamsterList(hamsterSave);
        Debug.Log($"{hamsterSave.HamsterId}, {hamsterSave.FaceId} ");

        return hamsterSave.HamsterId;
    }

    private void DrawOneHamster()
    {
        List<string> drawHamsterIdList = new List<string>();

        string hamsterId = DrawHamster();
        drawHamsterIdList.Add(hamsterId);

        GachaResultView.gameObject.SetActive(true);
        GachaResultView.ShowGachaResult(drawHamsterIdList);

        int price = GachaViewModel.GachaPrice;
        _userViewModel.TryUseSeed(price);
    }

    private void DrawTenHamster()
    {
        List<string> drawHamsterIdList = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            string hamsterId = DrawHamster();
            drawHamsterIdList.Add(hamsterId);
        }

        GachaResultView.gameObject.SetActive(true);
        GachaResultView.ShowGachaResult(drawHamsterIdList);

        int price = GachaViewModel.GachaPrice;
        _userViewModel.TryUseSeed(price * 10);
    }

    private void CheckDrawable()
    {
        int seedCount = _userViewModel.SeedCount;
        int price = GachaViewModel.GachaPrice;

        DrawOneButton.interactable = seedCount >= price;
        DrawTenButton.interactable = seedCount >= price * 10;
    }
}
