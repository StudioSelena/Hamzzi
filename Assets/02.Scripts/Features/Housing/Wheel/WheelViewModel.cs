using System.Collections.Generic;
using UnityEngine;

public class WheelSlotData
{
    public HamsterSave HamsterSaveData;
    public HamsterData HamsterData;
    public bool IsAssigned;
}

public class WheelViewModel : ViewModelBase
{
    public FurnitureViewModel TargetFurniture { get; private set; }
    public List<WheelSlotData> Hamsters { get; private set; } = new List<WheelSlotData>();

    public string CurrentHamsterID
    {
        get => TargetFurniture.AssignHamsterID;
    }

    private string _assignHamsterID;
    public string AssignHamsterID
    {
        get => TargetFurniture != null ? TargetFurniture.AssignHamsterID : _assignHamsterID;
        set
        {
            if (TargetFurniture != null && TargetFurniture.AssignHamsterID != value)
            {
                TargetFurniture.AssignHamsterID = value;
                _assignHamsterID = value;

                OnPropertyChanged(nameof(AssignHamsterID));
                OnPropertyChanged(nameof(CurrentHamsterID));

                ServiceManager.Instance.NetworkBuildService.RequestSaveHousingData();
            }
        }
    }

    public WheelViewModel(FurnitureViewModel targetFurnitureVM)
    {
        TargetFurniture = targetFurnitureVM;
        RefreshHamsterList();
    }

    public void RefreshHamsterList()
    {
        Hamsters.Clear();

        Dictionary<long, HamsterSave> collectHamsters = GetCollectHamsterID();
        Dictionary<string, int> otherAssignCounts = GetAssignHamster();

        Dictionary<string, int> remainingAssigns = new Dictionary<string, int>(otherAssignCounts);
        string currentAssigned = TargetFurniture?.AssignHamsterID;

        foreach (var kv in collectHamsters)
        {
            long hamsterUID = kv.Key;
            HamsterSave hamsterSave = kv.Value;
            string hamsterID = hamsterSave.HamsterId;

            if (remainingAssigns.TryGetValue(hamsterID, out int count) && count > 0)
            {
                remainingAssigns[hamsterID] = count - 1;
                continue;
            }

            HamsterData data = GameDataManager.Instance.GetData<HamsterData>(hamsterID);

            bool isCurrent = (hamsterUID.ToString() == currentAssigned);

            Hamsters.Add(new WheelSlotData
            {
                HamsterSaveData = hamsterSave,
                HamsterData = data,
                IsAssigned = isCurrent
            });
        }

        OnPropertyChanged(nameof(TargetFurniture));
        OnPropertyChanged(nameof(CurrentHamsterID));
    }

    private Dictionary<string, int> GetAssignHamster()
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        List<FurnitureViewModel> allFurniture = ServiceManager.Instance.HousingService.GetAllPlacedFurniture();
        var collectHamsters = GetCollectHamsterID();

        foreach (var furniture in allFurniture)
        {
            if (furniture != TargetFurniture && !string.IsNullOrEmpty(furniture.AssignHamsterID))
            {
                string assignedVal = furniture.AssignHamsterID;
                string hamsterID = assignedVal;

                if (long.TryParse(assignedVal, out long assignedUID))
                {
                    if (collectHamsters.TryGetValue(assignedUID, out var save))
                    {
                        hamsterID = save.HamsterId;
                    }
                }

                if (!counts.ContainsKey(hamsterID))
                {
                    counts[hamsterID] = 0;
                }

                counts[hamsterID]++;
            }
        }

        return counts;
    }

    public void AssignHamster(string hamsterUIDStr)
    {
        AssignHamsterID = hamsterUIDStr;
        RefreshHamsterList();
    }

    public void UnassignHamster()
    {
        AssignHamsterID = null;
        RefreshHamsterList();
    }

    private Dictionary<long, HamsterSave> GetCollectHamsterID()
    {
        long userUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        return ServiceManager.Instance.CollectionService.GetCollectionViewModel(userUID).CollectedHamsterList;
    }
}