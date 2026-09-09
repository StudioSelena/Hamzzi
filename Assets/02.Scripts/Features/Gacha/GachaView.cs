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

    private HamsterSave DrawHamster()
    {
        HamsterSave hamsterSave = ServiceManager.Instance.GachaService.DrawGacha();

        if(CheckCollectedHamster(hamsterSave) != true)
        {
            _collectionViewModel.AddCollectedHamsterList(hamsterSave);
        }
        Debug.Log($"{hamsterSave.HamsterId}, {hamsterSave.FaceId} ");

        return hamsterSave;
    }

    private bool CheckCollectedHamster(HamsterSave hamsterSave)
    {
        string hamsterId = hamsterSave.HamsterId;
        string faceId = hamsterSave.FaceId;

        if (_collectionViewModel.CollectedFaceByHamsterList.TryGetValue(hamsterId, out var collectedFaceId) == true)
        {
            if(collectedFaceId.ContainsKey(faceId) == true)
            {
                return true;
            }
        }

        return false;
    }

    private void DrawOneHamster()
    {
        List<HamsterSave> drawHamsterList = new List<HamsterSave>();

        var hamsterSave = DrawHamster();
        drawHamsterList.Add(hamsterSave);

        GachaResultView.gameObject.SetActive(true);
        GachaResultView.ShowGachaResult(drawHamsterList);

        int price = GachaViewModel.GachaPrice;
        _userViewModel.TryUseSeed(price);
    }

    private void DrawTenHamster()
    {
        List<HamsterSave> drawHamsterList = new List<HamsterSave>();
        for (int i = 0; i < 10; i++)
        {
            var hamsterSave = DrawHamster();
            drawHamsterList.Add(hamsterSave);
        }

        GachaResultView.gameObject.SetActive(true);
        GachaResultView.ShowGachaResult(drawHamsterList);

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
