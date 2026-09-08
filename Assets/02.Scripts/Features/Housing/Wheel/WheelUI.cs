using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelUI : ViewBase
{
    [SerializeField] private Button Button_Close;
    [SerializeField] private Button Button_Confirm;
    [SerializeField] private Button Button_Skip;

    [SerializeField] private Image Image_PrevHamster;
    [SerializeField] private Image Image_NextHamster;
    [SerializeField] private TextMeshProUGUI Text_PrevInfo;
    [SerializeField] private TextMeshProUGUI Text_NextInfo;
    [SerializeField] private TextMeshProUGUI Text_PrevDescription;
    [SerializeField] private TextMeshProUGUI Text_NextDescription;

    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private Transform Parent_Hamsters;
    [SerializeField] private GameObject Text_InfoText;

    private Dictionary<int, HamsterSlot> _spawnSlotList = new Dictionary<int, HamsterSlot>();
    private WheelViewModel _wheelVM;
    private string _selectHamsterID;

    private Dictionary<HamsterSlot, string> _slotUID = new Dictionary<HamsterSlot, string>();

    private void Start()
    {
        Button_Close.onClick.AddListener(OnClickClose);
        Button_Confirm.onClick.AddListener(OnClickConfirm);
        Button_Skip.onClick.AddListener(OnClickSkip);
    }

    private void OnEnable()
    {
        GameDataManager.Instance.LoadData<HamsterData>();

        HousingViewModel housingVM = ServiceManager.Instance.HousingService.GetHousingViewModel();
        BindViewModel(new WheelViewModel(housingVM.FurnitureVM));
    }

    private void OnDisable()
    {
        if (_wheelVM != null)
        {
            _wheelVM.PropertyChanged -= OnPropertyChanged_VM;
        }
    }

    public void BindViewModel(WheelViewModel wheelVM)
    {
        _wheelVM = wheelVM;
        _wheelVM.PropertyChanged += OnPropertyChanged_VM;

        _selectHamsterID = null;

        InitWheelSlotList();
        RefreshInfoUI();
    }

    private void OnPropertyChanged_VM(object sender, PropertyChangedEventArgs e)
    {
        InitWheelSlotList();
        RefreshInfoUI();
    }

    private void InitWheelSlotList()
    {
        if (Parent_Hamsters.childCount > 0)
        {
            foreach (Transform child in Parent_Hamsters)
            {
                Destroy(child.gameObject);
            }

            _spawnSlotList.Clear();
            _slotUID.Clear();
        }

        int index = 0;
        foreach (WheelSlotData slotData in _wheelVM.Hamsters)
        {
            GameObject prefab = Instantiate(Prefab_Slot, Parent_Hamsters);

            if (prefab.TryGetComponent<HamsterSlot>(out var hamsterSlot))
            {
                hamsterSlot.InitSlot(slotData.HamsterData, true);

                string realUIDStr = slotData.HamsterSaveData.HamsterUID.ToString();
                _slotUID[hamsterSlot] = realUIDStr;

                hamsterSlot.OnSlotClicked += OnSlotClicked;

                _spawnSlotList.Add(index, hamsterSlot);
                index++;
            }
        }

        if (_spawnSlotList.Count == 0)
        {
            Text_InfoText.SetActive(true);
        }
        else
        {
            Text_InfoText.SetActive(false);
        }
    }

    private void RefreshInfoUI()
    {
        UpdatePrevHamsterInfo();
        UpdateNextHamsterInfo();

        bool hasHamster = !string.IsNullOrEmpty(_wheelVM.CurrentHamsterID);
        Button_Skip.interactable = hasHamster;
    }

    private void UpdatePrevHamsterInfo()
    {
        string currentID = _wheelVM.CurrentHamsterID;

        if (!string.IsNullOrEmpty(currentID))
        {
            string targetDataID = currentID;

            if (long.TryParse(currentID, out long uid))
            {
                long userUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
                var collectionList = ServiceManager.Instance.CollectionService.GetCollectionViewModel(userUID).CollectedHamsterList;

                if (collectionList.TryGetValue(uid, out var save))
                {
                    targetDataID = save.HamsterId; ;
                }
            }

            HamsterData data = GameDataManager.Instance.GetData<HamsterData>(targetDataID);

            if (data != null)
            {
                Text_PrevInfo.gameObject.SetActive(false);
                Text_PrevDescription.gameObject.SetActive(true);
                Text_PrevDescription.text = $"이름: {data.Name}\n해씨 수집율 {data.CollectSpeed * 100}%";

                Image_PrevHamster.gameObject.SetActive(true);
                LoadIcon(Image_PrevHamster, data.IconPath).Forget();
                return;
            }
        }

        Text_PrevInfo.gameObject.SetActive(true);
        Text_PrevDescription.gameObject.SetActive(false);
        Image_PrevHamster.gameObject.SetActive(false);
    }

    private void UpdateNextHamsterInfo()
    {
        if (!string.IsNullOrEmpty(_selectHamsterID))
        {
            string targetDataID = _selectHamsterID;

            if (long.TryParse(_selectHamsterID, out long uid))
            {
                foreach (WheelSlotData slot in _wheelVM.Hamsters)
                {
                    if (slot.HamsterSaveData != null && slot.HamsterSaveData.HamsterUID == uid)
                    {
                        targetDataID = slot.HamsterSaveData.HamsterId;
                        break;
                    }
                }
            }

            HamsterData data = GameDataManager.Instance.GetData<HamsterData>(targetDataID);

            Text_NextInfo.gameObject.SetActive(false);
            Text_NextDescription.gameObject.SetActive(true);
            Text_NextDescription.text = $"이름: {data.Name}\n해씨 수집율 {data.CollectSpeed * 100}%";

            Image_NextHamster.gameObject.SetActive(true);
            LoadIcon(Image_NextHamster, data.IconPath).Forget();
        }
        else
        {
            Text_InfoText.SetActive(false);
            Text_NextInfo.gameObject.SetActive(true);
            Text_NextDescription.gameObject.SetActive(false);
            Image_NextHamster.gameObject.SetActive(false);
        }
    }

    private async UniTask LoadIcon(Image icon, string path)
    {
        Sprite sprite = await ResourceManager.Instance.LoadAsset<Sprite>(path);
        icon.sprite = sprite;
    }

    private void OnClickClose()
    {
        HousingViewModel housingVM = ServiceManager.Instance.HousingService.GetHousingViewModel();
        housingVM.CloseAssignUI();

        UIManager.Instance.CloseWheelUI();
    }

    private void OnClickConfirm()
    {
        if (!string.IsNullOrEmpty(_selectHamsterID))
        {
            _wheelVM.AssignHamster(_selectHamsterID);
        }

        OnClickClose();
    }

    private void OnClickSkip()
    {
        _wheelVM.UnassignHamster();

        OnClickClose();
    }

    private void OnSlotClicked(string dataID)
    {
        GameObject currentSelectedObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (currentSelectedObj != null)
        {
            HamsterSlot clickedSlot = currentSelectedObj.GetComponentInParent<HamsterSlot>();

            if (clickedSlot != null && _slotUID.TryGetValue(clickedSlot, out string realUID))
            {
                _selectHamsterID = realUID;
            }
        }

        UpdateNextHamsterInfo();
    }
}