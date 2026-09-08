using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CollectionView : UIBase
{
    [Header("UI Base")]
    [SerializeField] private UIButton ExitButton;

    [Header("햄스터 리스트")]
    [SerializeField] private UIButton BodyListButton;
    [SerializeField] private UIButton EyeListButton;

    [SerializeField] private GameObject HamsterSlotPrefab;
    [SerializeField] private GameObject FaceSlotPrefab;
    [SerializeField] private Transform SlotContent;

    [Header("햄스터 정보")]
    [SerializeField] private HamsterModelRotate HamsterRotate;
    [SerializeField] private UIButton KickButton;
    [SerializeField] private KickUI KickUI;
    [SerializeField] private RawImage HamsterModelImage;
    [SerializeField] private TextMeshProUGUI HamsterCount;
    [SerializeField] private TextMeshProUGUI HamsterName;
    [SerializeField] private TextMeshProUGUI HamsterAbility;

    private Dictionary<string, HamsterSlot> _spawnedHamsterSlotList = new Dictionary<string, HamsterSlot>();
    private Dictionary<string, FaceSlot> _spawnedFaceSlotList = new Dictionary<string, FaceSlot>();

    private HamsterForm _modelForm;

    private CollectionViewModel _collectionViewModel;
    private HamsterViewModel _hamsterViewModel;
    private HamsterModelViewModel _hamsterModelViewModel;

    private void Awake()
    {
        long userUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        _collectionViewModel = ServiceManager.Instance.CollectionService.GetCollectionViewModel(userUID);
        _hamsterViewModel = ServiceManager.Instance.CollectionService.GetHamsterViewModel();
        _hamsterModelViewModel = ServiceManager.Instance.HamsterModelService.GetHamsterModelViewModel();

        var hamsterRender = ServiceManager.Instance.HamsterModelService.HamsterTexture;
        HamsterModelImage.texture = hamsterRender;
    }

    private void OnEnable()
    {
        ExitButton.BindOnClickButtonEvent(CloseCollectionUI);

        BodyListButton.BindOnClickButtonEvent(ShowHamsterList);
        EyeListButton.BindOnClickButtonEvent(ShowFaceList);

        KickButton.BindOnClickButtonEvent(KickHamster);

        // 수집 데이터들 View에 표시
        _collectionViewModel.PropertyChanged += OnPropertyChanged;
        _collectionViewModel.ContainerPropertyChanged += OnContainerPropChanged;

        // 슬롯이 없다면 초기화
        InitCollectionList();
        ShowHamsterList();

        // 슬롯 업데이트
        UpdateHamsterSlot();
        UpdateFaceSlot();

        _collectionViewModel.InitInvokePropertyChanged();
        ServiceManager.Instance.HamsterModelService.SetHamsterAnimator("IdleTrigger");

        KickUI.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ExitButton.UnBindOnClickButtonEvent(CloseCollectionUI);

        BodyListButton.UnBindOnClickButtonEvent(ShowHamsterList);
        EyeListButton.UnBindOnClickButtonEvent(ShowFaceList);

        KickButton.UnBindOnClickButtonEvent(KickHamster);

        _collectionViewModel.PropertyChanged -= OnPropertyChanged;
        _collectionViewModel.ContainerPropertyChanged -= OnContainerPropChanged;
    }

    private void CloseCollectionUI()
    {
        UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.CollectionUI);
    }

    private void ShowHamsterList()
    {
        ShowList(true);
    }

    private void ShowFaceList()
    {
        ShowList(false);
    }

    private void ShowList(bool isHamster)
    {
        foreach (var kv in _spawnedHamsterSlotList)
        {
            var slot = kv.Value;
            slot.gameObject.SetActive(isHamster);
        }

        foreach (var kv in _spawnedFaceSlotList)
        {
            var slot = kv.Value;
            slot.gameObject.SetActive(!isHamster);
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName) 
        {
            case nameof(CollectionViewModel.CollectedHamsterIdList):
                UpdateHamsterSlot();
                break;
            case nameof(CollectionViewModel.CurrentSelectHamsterId):
                UpdateHamsterInfo();
                UpdateFaceSlot();
                ChangedHamsterModel();
                break;
            case nameof(CollectionViewModel.CurrentSelectedHamsterFaceId):
                ChangedFaceModel();
                break;
        }
    }

    private void OnContainerPropChanged(string propertyName, ContainerEventType eventType, string hamsterId)
    {
        if (propertyName == nameof(_collectionViewModel.CollectedHamsterIdList) == true)
        {
            switch (eventType)
            {
                case ContainerEventType.Add:
                    UpdateHamsterSlot();
                    break;
                case ContainerEventType.Remove:
                    UpdateHamsterSlot();
                    ChangedFaceModel();
                    break;
                case ContainerEventType.Update:
                    break;
            }
        }

        if(propertyName == nameof(_collectionViewModel.CollectedFaceByHamsterList) == true)
        {
            switch (eventType)
            {
                case ContainerEventType.Add:
                    UpdateFaceSlot();
                    break;
                case ContainerEventType.Remove:
                    UpdateFaceSlot();
                    break;
                case ContainerEventType.Update:
                    break;
            }
        }
    }

    private void InitCollectionList()
    {
        InitHamsterList();
        InitFaceList();
    }

    private void InitHamsterList()
    {
        List<string> allHamsterList = _hamsterViewModel.AllHamsterIdList;

        if (_spawnedHamsterSlotList.Count > 0)
            return;

        foreach (var hamsterId in allHamsterList)
        {
            GameObject hamsterSlotObject = Instantiate(HamsterSlotPrefab, SlotContent);
            if (hamsterSlotObject == null)
                return;

            HamsterSlot hamsterSlot = hamsterSlotObject.GetComponent<HamsterSlot>();
            if (hamsterSlot == null)
                return;

            HamsterData hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);
            if (hamsterData == null)
                return;

            bool isCollected = _collectionViewModel.CollectedHamsterIdList.Contains(hamsterId);

            hamsterSlot.InitSlot(hamsterData, isCollected);
            hamsterSlot.OnSlotClicked += OnSelectedHamster;

            _spawnedHamsterSlotList.Add(hamsterId, hamsterSlot);
        }

        // 티어 기준으로 정렬
        SortSlotsByTier();
    }

    private void InitFaceList()
    {
        List<string> allFaceList = _hamsterViewModel.AllFaceIdList;
        Debug.Log("얼굴 초기화");

        if (_spawnedFaceSlotList.Count > 0)
            return;

        foreach (var faceId in allFaceList)
        {
            GameObject faceSlotObject = Instantiate(FaceSlotPrefab, SlotContent);
            if (faceSlotObject == null)
                return;

            FaceSlot faceSlot = faceSlotObject.GetComponent<FaceSlot>();
            if (faceSlot == null)
                return;

            FaceData faceData = GameDataManager.Instance.GetData<FaceData>(faceId);
            if (faceData == null)
                return;

            bool isCollected = CheckUnlockFace(faceId);

            faceSlot.InitSlot(faceData, isCollected);
            faceSlot.OnSlotClicked += OnSelectedFace;

            _spawnedFaceSlotList.Add(faceId, faceSlot);
        }
    }

    private bool CheckUnlockFace(string faceId)
    {
        string currentHamsterId = _collectionViewModel.CurrentSelectHamsterId;

        var faceList = _collectionViewModel.CollectedFaceByHamsterList;
        if (faceList.ContainsKey(currentHamsterId) == false)
        {
            return false;
        }
        return faceList[currentHamsterId].ContainsKey(faceId);
    }

    private void SortSlotsByTier()
    {
        if (_spawnedHamsterSlotList.Count() <= 0)
            return;

        var slotList = _spawnedHamsterSlotList.Keys.ToList();
        slotList.Sort(CompareSlots);

        int index = 0;
        foreach(string hamsterId in slotList)
        {
            var slotComponent = _spawnedHamsterSlotList[hamsterId];
            if (slotComponent == null)
                continue;

            slotComponent.transform.SetSiblingIndex(index);
            index++;
        }
    }

    private int CompareSlots(string aHamsterId, string bHamsterId)
    {
        int aHamsterTier = GetHamsterTier(aHamsterId);
        int bHamsterTier = GetHamsterTier(bHamsterId);

        int tierComparison = aHamsterTier.CompareTo(bHamsterTier);

        if(tierComparison == 0)
        {
            return aHamsterId.CompareTo(bHamsterId);
        }
        return tierComparison;
    }

    private int GetHamsterTier(string hamsterId)
    {
        var hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);

        return (int)hamsterData.HamsterTier;
    }

    private void UpdateHamsterSlot()
    {
        HashSet<string> collectedHamsterList = _collectionViewModel.CollectedHamsterIdList;
        foreach(var kv in _spawnedHamsterSlotList)
        {
            string hamsterId = kv.Key;
            HamsterSlot slot = kv.Value;

            bool isCollected = collectedHamsterList.Contains(hamsterId);
            slot.UpdateLockImage(isCollected);

            Debug.Log($"슬롯 업데이트 {hamsterId}");
        }
    }

    private void UpdateFaceSlot()
    {
        string currentHamsterId = _collectionViewModel.CurrentSelectHamsterId;

        foreach(var kv in _spawnedFaceSlotList)
        {
            var slot = kv.Value;
            slot.UpdateLockImage(false);
        }

        var collectedFaceList = _collectionViewModel.CollectedFaceByHamsterList;
        if (collectedFaceList.ContainsKey(currentHamsterId) == false)
        {
            return;
        }

        foreach (var kv in collectedFaceList[currentHamsterId])
        {
            string faceId = kv.Key;
            FaceSlot slot = _spawnedFaceSlotList[faceId];
            slot.UpdateLockImage(true);

            Debug.Log($"슬롯 업데이트 {faceId}");
        }
    }

    private void OnSelectedHamster(string hamsterId)
    {
        _collectionViewModel.RequestSelectedHamsterId(hamsterId);
    }

    private void OnSelectedFace(string faceId)
    {
        _collectionViewModel.RequestSelectedFaceId(faceId);
    }

    private void ChangedHamsterModel()
    {
        string hamsterId = _collectionViewModel.CurrentSelectHamsterId;
        _hamsterModelViewModel.HamsterId = hamsterId;
    }

    private void ChangedFaceModel()
    {
        string faceId = _collectionViewModel.CurrentSelectedHamsterFaceId;
        string hamsterId = _collectionViewModel.CurrentSelectHamsterId;

        _hamsterModelViewModel.FaceId = faceId;

        int count = 0;
        if (_collectionViewModel.CollectedFaceByHamsterList.TryGetValue(hamsterId, out var faceDict))
        {
            faceDict.TryGetValue(faceId, out count);
        }

        HamsterCount.text = $"보유 : {count:D2}";
    }

    private void KickHamster()
    {
        KickUI.gameObject.SetActive(true);
    }

    private void UpdateHamsterInfo()
    {
        string hamsterId = _collectionViewModel.CurrentSelectHamsterId;
        HamsterData hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);
        if (hamsterData == null)
            return;

        // 햄스터 이름
        HamsterName.text = hamsterData.Name;

        // 햄스터 디테일 정보
        HamsterAbility.text = $"{hamsterData.CollectSpeed}";
    }
}