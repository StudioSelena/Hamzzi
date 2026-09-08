using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public enum HousingState
{
    SelectRoom,
    Placing,
    Editing
}

public enum HousingViewMode
{
    OverView,
    FocusRoom,
    Garden
}

public class HousingViewModel : ViewModelBase
{
    public List<FurnitureViewModel> GardenFurnitureList { get; private set; } = new List<FurnitureViewModel>();
    public bool IsInHousingMode { get; private set; } = false;

    public Vector2Int GardenGridSize { get; } = new Vector2Int(150, 60);
    private Dictionary<Vector2Int, FurnitureViewModel> _gardenFurnitureGrid = new Dictionary<Vector2Int, FurnitureViewModel>();

    public bool CanConfirm
    {
        get
        {
            return FurnitureVM != null && FurnitureVM.IsValid;
        }
    }

    private HousingViewMode _currentViewMode = HousingViewMode.OverView;
    public HousingViewMode CurrentViewMode
    {
        get => _currentViewMode;
        set
        {
            if (_currentViewMode != value)
            {
                _currentViewMode = value;
                OnPropertyChanged(nameof(CurrentViewMode));
            }
        }
    }

    private HousingState _currentState = HousingState.SelectRoom;
    public HousingState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                _currentState = value;
                OnPropertyChanged(nameof(CurrentState));
            }
        }
    }

    private Dictionary<long, FurnitureSlotViewModel> _itemList = new Dictionary<long, FurnitureSlotViewModel>();
    public Dictionary<long, FurnitureSlotViewModel> ItemList
    {
        get => _itemList;
        set
        {
            if (_itemList != value)
            {
                _itemList = value;
                OnPropertyChanged(nameof(ItemList));
            }
        }
    }

    private FurnitureViewModel _furnitureVM;
    public FurnitureViewModel FurnitureVM
    {
        get => _furnitureVM;
        set
        {
            if (_furnitureVM != value)
            {
                _furnitureVM = value;
                OnPropertyChanged(nameof(FurnitureVM));
                OnPropertyChanged(nameof(CanConfirm));
            }
        }
    }

    private RoomViewModel _targetRoom;
    public RoomViewModel TargetRoom
    {
        get => _targetRoom;
        set
        {
            if (_targetRoom != value)
            {
                _targetRoom = value;
                OnPropertyChanged(nameof(TargetRoom));

                if (_targetRoom != null)
                {
                    CurrentState = HousingState.Placing;
                }
                else
                {
                    CurrentState = HousingState.SelectRoom;
                    FurnitureVM = null;
                }
            }
        }
    }

    private FurnitureViewModel _selectedInstallFurniture;
    public FurnitureViewModel SelectedInstallFurniture
    {
        get => _selectedInstallFurniture;
        set
        {
            if (_selectedInstallFurniture != value)
            {
                _selectedInstallFurniture = value;
                OnPropertyChanged(nameof(SelectedInstallFurniture));
            }
        }
    }

    private FurnitureViewModel _destroyFurniture;
    public FurnitureViewModel DestroyFurniture
    {
        get => _destroyFurniture;
        set
        {
            if (_destroyFurniture != value)
            {
                _destroyFurniture = value;
                OnPropertyChanged(nameof(DestroyFurniture));
            }
        }
    }

    private FurnitureViewModel _confirmFurniture;
    public FurnitureViewModel ConfirmFurniture
    {
        get => _confirmFurniture;
        set
        {
            if (_confirmFurniture != value)
            {
                _confirmFurniture = value;
                OnPropertyChanged(nameof(ConfirmFurniture));
            }
        }
    }

    private FurnitureViewModel _requestAssignHamster;
    public FurnitureViewModel RequestAssignHamster
    {
        get => _requestAssignHamster;
        set
        {
            if (_requestAssignHamster != value)
            {
                _requestAssignHamster = value;
                OnPropertyChanged(nameof(RequestAssignHamster));
            }
        }
    }

    private HousingCategory _housingCategory;
    public HousingCategory HousingCategory
    {
        get => _housingCategory;
        set
        {
            if (_housingCategory != value)
            {
                _housingCategory = value;
                OnPropertyChanged(nameof(HousingCategory));
            }
        }
    }

    public bool CanAssignCurrentFurniture
    {
        get
        {
            return CurrentState == HousingState.Editing && FurnitureVM.CanAssignHamster;
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(CurrentViewMode));
        OnPropertyChanged(nameof(CurrentState));
        OnPropertyChanged(nameof(FurnitureVM));
        OnPropertyChanged(nameof(TargetRoom));
        OnPropertyChanged(nameof(ItemList));
        OnPropertyChanged(nameof(HousingCategory));
    }

    public Dictionary<long, FurnitureSlotViewModel> GetHousingCategory()
    {
        if (_housingCategory == HousingCategory.All)
        {
            return ItemList;
        }

        Dictionary<long, FurnitureSlotViewModel> categoryList = new Dictionary<long, FurnitureSlotViewModel>();

        foreach (var item in ItemList)
        {
            FurnitureSlotViewModel slotVM = item.Value;
            ItemData itemData = GameDataManager.Instance.GetData<ItemData>(slotVM.ItemDataId);

            if (itemData != null && itemData.Category.ToString() == _housingCategory.ToString())
            {
                categoryList.Add(item.Key, slotVM);
            }
        }

        return categoryList;
    }

    public void EnterHousingMode(RoomViewModel targetRoom = null)
    {
        IsInHousingMode = true;
        _furnitureVM = null;

        if (targetRoom != null)
        {
            _targetRoom = targetRoom;
            _currentState = HousingState.Placing;
        }
        else
        {
            _targetRoom = null;
            _currentState = (CurrentViewMode == HousingViewMode.Garden) ? HousingState.Placing : HousingState.SelectRoom;
        }

        OnPropertyChanged(nameof(TargetRoom));
        OnPropertyChanged(nameof(CurrentState));
    }

    public void SelectInstallFurniture(FurnitureViewModel furnitureVM)
    {
        CurrentState = HousingState.Editing;

        SelectedInstallFurniture = furnitureVM;
        FurnitureVM = furnitureVM;

        if (TargetRoom != null)
        {
            TargetRoom.RemoveFurniture(furnitureVM);
        }
        else if (CurrentViewMode == HousingViewMode.Garden)
        {
            RemoveGardenFurniture(furnitureVM);
        }

        CheckCurrentPos();
    }

    public async UniTask<bool> RemoveSelectedFurniture()
    {
        DestroyFurniture = FurnitureVM;
        string furnitureID = FurnitureVM.FurnitureID;

        if (!string.IsNullOrEmpty(FurnitureVM.AssignHamsterID))
        {
            FurnitureVM.AssignHamsterID = null;
        }

        ServiceManager.Instance.HousingService.RefreshFurnitureBuff();

        if (CurrentViewMode == HousingViewMode.Garden)
        {
            RemoveGardenFurniture(FurnitureVM);
        }
        else if (TargetRoom != null)
        {
            TargetRoom.RemoveFurniture(FurnitureVM);
        }

        string iconPath = GameDataManager.Instance.GetData<ItemData>(furnitureID).IconPath;
        Sprite icon = await ResourceManager.Instance.LoadAsset<Sprite>(iconPath);
        ServiceManager.Instance.HousingService.AddItem(furnitureID, icon);

        ResetPlacingState();

        ServiceManager.Instance.NetworkBuildService.RequestSaveHousingData();

        return true;
    }

    public void SelectFurniture(ItemData data, Vector2Int subSize, RoomViewModel targetRoom)
    {
        TargetRoom = targetRoom;

        Vector2Int initialPos = new Vector2Int(TargetRoom.SubGridSize.x / 2 - subSize.x / 2, TargetRoom.SubGridSize.y / 2 - subSize.y / 2);

        string newInstanceID = GameUtil.GenerateUID().ToString();
        FurnitureVM = new FurnitureViewModel(newInstanceID, data.Id, data.PrefabPath, initialPos, subSize);
        CheckCurrentPos();
    }

    public void SelectGardenFurniture(ItemData data, Vector2Int subSize)
    {
        Vector2Int initialPos = new Vector2Int(10, 10);

        string newInstanceID = GameUtil.GenerateUID().ToString();
        FurnitureVM = new FurnitureViewModel(newInstanceID, data.Id, data.PrefabPath, initialPos, subSize);
        CheckCurrentPos();
    }

    public void MovePos(Vector2Int pos)
    {
        FurnitureVM.LocalPos = pos;
        CheckCurrentPos();
    }

    public void RotatePos()
    {
        FurnitureVM.Rotate();
        CheckCurrentPos();

        OnPropertyChanged(nameof(FurnitureVM));
    }

    public void SetConfirmFurniture(FurnitureViewModel furnitureVM)
    {
        _confirmFurniture = furnitureVM;
        OnPropertyChanged(nameof(ConfirmFurniture));
    }

    public bool ConfirmPos()
    {
        if (FurnitureVM == null || !FurnitureVM.IsValid)
        {
            return false;
        }

        FurnitureViewModel furnitureVM = FurnitureVM;
        bool success = false;

        if (CurrentViewMode == HousingViewMode.Garden)
        {
            success = AddGardenFurniture(FurnitureVM);
        }
        else if (TargetRoom != null)
        {
            furnitureVM.RoomInstanceID = TargetRoom.InstanceID;
            success = TargetRoom.AddFurniture(FurnitureVM);
        }

        if (success)
        {
            SetConfirmFurniture(furnitureVM);

            if (CurrentState != HousingState.Editing && furnitureVM.CanAssignHamster)
            {
                RequestAssignHamster = furnitureVM;
            }

            DecreaseFurnitureStack(furnitureVM.FurnitureID);

            ConfirmFurniture = furnitureVM;

            if (CurrentState != HousingState.Editing)
            {
                ServiceManager.Instance.HousingService.RefreshFurnitureBuff();
            }


            ResetPlacingState();
            NavigationManager.Instance.BuildNav();

            ServiceManager.Instance.NetworkBuildService.RequestSaveHousingData();

            return true;
        }

        return false;
    }

    private void DecreaseFurnitureStack(string furnitureId)
    {
        foreach(var itemKv in ItemList)
        {
            var furnitureSlotVm = itemKv.Value;

            if(furnitureSlotVm.ItemDataId == furnitureId)
            {
                furnitureSlotVm.StackCount--;

                if(furnitureSlotVm.StackCount <= 0)
                {
                    ItemList.Remove(itemKv.Key);
                }

                OnPropertyChanged(nameof(ItemList));
                return;
            }
        }
    }

    public void CancelPos()
    {
        if (CurrentState == HousingState.Editing && SelectedInstallFurniture != null)
        {
            if (CurrentViewMode == HousingViewMode.Garden)
            {
                AddGardenFurniture(SelectedInstallFurniture);
            }
            else if (TargetRoom != null)
            {
                TargetRoom.AddFurniture(SelectedInstallFurniture);
            }

            SetConfirmFurniture(SelectedInstallFurniture);
        }

        ResetPlacingState();
    }

    public void ExitRoom()
    {
        TargetRoom = null;
    }

    private void CheckCurrentPos()
    {
        if (CurrentViewMode == HousingViewMode.Garden)
        {
            FurnitureVM.IsValid = IsValidGardenPlace(FurnitureVM.LocalPos, FurnitureVM.Size);
        }
        else if (TargetRoom != null)
        {
            FurnitureVM.IsValid = TargetRoom.IsValidPlace(FurnitureVM.LocalPos, FurnitureVM.Size);
        }

        OnPropertyChanged(nameof(CanConfirm));
    }

    public void EnterGardenMode()
    {
        TargetRoom = null;
        CurrentViewMode = HousingViewMode.Garden;
        CurrentState = HousingState.Placing;

        OnPropertyChanged(nameof(CurrentViewMode));
        OnPropertyChanged(nameof(TargetRoom));
    }

    public void EnterOverviewMode()
    {
        IsInHousingMode = false;
        TargetRoom = null;
        CurrentViewMode = HousingViewMode.OverView;
    }

    public bool IsValidGardenPlace(Vector2Int localPos, Vector2Int furnitureSize)
    {
        for (int x = 0; x < furnitureSize.x; x++)
        {
            for (int y = 0; y < furnitureSize.y; y++)
            {
                Vector2Int checkPos = localPos + new Vector2Int(x, y);

                if (checkPos.x < 0 || checkPos.x >= GardenGridSize.x || checkPos.y < 0 || checkPos.y >= GardenGridSize.y)
                {
                    return false;
                }

                if (_gardenFurnitureGrid.ContainsKey(checkPos))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void LoadGardenFurniture(FurnitureViewModel furnitureVM)
    {
        for (int x = 0; x < furnitureVM.Size.x; x++)
        {
            for (int y = 0; y < furnitureVM.Size.y; y++)
            {
                _gardenFurnitureGrid[furnitureVM.LocalPos + new Vector2Int(x, y)] = furnitureVM;
            }
        }

        if (!GardenFurnitureList.Contains(furnitureVM))
        {
            GardenFurnitureList.Add(furnitureVM);
        }

        OnPropertyChanged(nameof(GardenFurnitureList));
    }

    public bool AddGardenFurniture(FurnitureViewModel furnitureVM)
    {
        if (!IsValidGardenPlace(furnitureVM.LocalPos, furnitureVM.Size))
        {
            return false;
        }

        for (int x = 0; x < furnitureVM.Size.x; x++)
        {
            for (int y = 0; y < furnitureVM.Size.y; y++)
            {
                _gardenFurnitureGrid[furnitureVM.LocalPos + new Vector2Int(x, y)] = furnitureVM;
            }
        }

        GardenFurnitureList.Add(furnitureVM);
        OnPropertyChanged(nameof(GardenFurnitureList));

        return true;
    }

    public bool RemoveGardenFurniture(FurnitureViewModel furnitureVM)
    {
        List<Vector2Int> removeKeys = new List<Vector2Int>();

        foreach (var pair in _gardenFurnitureGrid)
        {
            if (pair.Value == furnitureVM || pair.Value.InstanceID == furnitureVM.InstanceID)
            {
                removeKeys.Add(pair.Key);
            }
        }

        foreach (var key in removeKeys)
        {
            _gardenFurnitureGrid.Remove(key);
        }

        GardenFurnitureList.Remove(furnitureVM);
        OnPropertyChanged(nameof(GardenFurnitureList));

        return true;
    }

    private void ResetPlacingState()
    {
        FurnitureVM = null;
        SelectedInstallFurniture = null;
        CurrentState = HousingState.Placing;
    }

    public void OpenAssignUI()
    {
        if (FurnitureVM.CanAssignHamster)
        {
            RequestAssignHamster = FurnitureVM;
        }
    }

    public void CloseAssignUI()
    {
        RequestAssignHamster = null;

        if (CurrentState == HousingState.Editing && FurnitureVM != null)
        {
            ConfirmPos();
        }
    }

    public Dictionary<long, FurnitureSlotViewModel> GetOwnedFurnitureList()
    {
        return ItemList;
    }

    public void AddItemSlotViewModel(FurnitureSlotViewModel furnitureSlotVm)
    {
        _itemList.Add(furnitureSlotVm.ItemUniqueId, furnitureSlotVm);
        OnPropertyChanged(nameof(ItemList));
    }
}