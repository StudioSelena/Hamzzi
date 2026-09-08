using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectionViewModel : ViewModelBase, IContainerPropertyChanged<string>
{
    public event Action<string, ContainerEventType, string> ContainerPropertyChanged;

    // 보유 중인 햄스터의 상세 데이터 저장
    private Dictionary<long, HamsterSave> _collectedHamsterList = new Dictionary<long, HamsterSave>();
    public Dictionary<long, HamsterSave> CollectedHamsterList
    {
        get { return _collectedHamsterList; }
        set
        {
            if (_collectedHamsterList != value)
            {
                _collectedHamsterList = value;
                OnPropertyChanged(nameof(CollectedHamsterList));
            }
        }
    }

    private Dictionary<string, Dictionary<string, int>> _collectedFaceByHamsterList = new Dictionary<string, Dictionary<string, int>>();
    public Dictionary<string, Dictionary<string, int>> CollectedFaceByHamsterList
    {
        get { return _collectedFaceByHamsterList; }
        set
        {
            if (_collectedFaceByHamsterList != value)
            {
                _collectedFaceByHamsterList = value;
                OnPropertyChanged(nameof(CollectedFaceByHamsterList));
            }
        }
    }

    // 보유 중인 햄스터ID만 저장
    private HashSet<string> _collectedHamsterIdList = new HashSet<string>();
    public HashSet<string> CollectedHamsterIdList
    {
        get { return _collectedHamsterIdList; }
        set
        {
            if(_collectedHamsterIdList != value)
            {
                _collectedHamsterIdList = value;
                OnPropertyChanged(nameof(CollectedHamsterIdList));
            }
        }
    }

    private string _currentSelectHamsterId = "Hamster_01";
    public string CurrentSelectHamsterId
    {
        get { return _currentSelectHamsterId; }
        set
        {
            if (_currentSelectHamsterId != value)
            {
                _currentSelectHamsterId = value;
                OnPropertyChanged(nameof(CurrentSelectHamsterId));
            }
        }
    }

    private string _currentSelectedHamsterFaceId = "Face_01";
    public string CurrentSelectedHamsterFaceId
    {
        get { return _currentSelectedHamsterFaceId; }
        set
        {
            if (_currentSelectedHamsterFaceId != value)
            {
                _currentSelectedHamsterFaceId = value;
                OnPropertyChanged(nameof(CurrentSelectedHamsterFaceId));
            }
        }
    }

    public void InitInvokePropertyChanged()
    {
        OnPropertyChanged(nameof(CurrentSelectHamsterId));
        OnPropertyChanged(nameof(CurrentSelectedHamsterFaceId));
    }

    public void InvokeContainerPropertyChanged(string containerName, ContainerEventType type, string id)
    {
        ContainerPropertyChanged?.Invoke(containerName, type, id);
    }
}

public static class HamsterViewModelExtention 
{
    public static void AddCollectedHamsterList(this CollectionViewModel collectionViewModel, HamsterSave hamsterSave, bool isLoading = false)
    {
        if (collectionViewModel.CollectedHamsterList.ContainsKey(hamsterSave.HamsterUID) == true)
            return;

        collectionViewModel.CollectedHamsterList.Add(hamsterSave.HamsterUID, hamsterSave);
        collectionViewModel.CollectedHamsterIdList.Add(hamsterSave.HamsterId);

        if(collectionViewModel.CollectedFaceByHamsterList.ContainsKey(hamsterSave.HamsterId) == false)
        {
            collectionViewModel.CollectedFaceByHamsterList.Add(hamsterSave.HamsterId, new Dictionary<string, int>());
        }

        var faceList = collectionViewModel.CollectedFaceByHamsterList[hamsterSave.HamsterId];
        if (faceList.TryGetValue(hamsterSave.FaceId, out int currentCount))
        {
            faceList[hamsterSave.FaceId] = currentCount + 1;
        }
        else
        {
            faceList[hamsterSave.FaceId] = 1;
        }

        collectionViewModel.InvokeContainerPropertyChanged(nameof(collectionViewModel.CollectedHamsterIdList), ContainerEventType.Add, hamsterSave.HamsterId);
        collectionViewModel.InvokeContainerPropertyChanged(nameof(collectionViewModel.CollectedFaceByHamsterList), ContainerEventType.Add, hamsterSave.FaceId);

        if(isLoading != true)
        {
            ServiceManager.Instance.CollectionService.TrySaveHamsterData(hamsterSave).Forget();
        }
    }

    public static void RemoveCollectedHamsterList(this CollectionViewModel collectionViewModel, string hamsterId, string faceId)
    {
        long targetUID = -1;

        foreach(var kv in collectionViewModel.CollectedHamsterList)
        {
            var hamsterSave = kv.Value;
            if(hamsterSave.HamsterId == hamsterId && hamsterSave.FaceId == faceId)
            {
                targetUID = kv.Key;
                break;
            }
        }

        if(targetUID == -1)
        {
            return;
        }

        collectionViewModel.CollectedHamsterList.Remove(targetUID);
        int count = collectionViewModel.CollectedFaceByHamsterList[hamsterId][faceId] - 1;
        collectionViewModel.CollectedFaceByHamsterList[hamsterId][faceId] = count;
        if (count < 1)
        {
            collectionViewModel.CollectedFaceByHamsterList[hamsterId].Remove(faceId);

            if(collectionViewModel.CollectedFaceByHamsterList[hamsterId].Count <= 0)
            {
                collectionViewModel.CollectedHamsterIdList.Remove(hamsterId);
            }
        }

        collectionViewModel.InvokeContainerPropertyChanged(nameof(collectionViewModel.CollectedHamsterIdList), ContainerEventType.Remove, hamsterId);
        collectionViewModel.InvokeContainerPropertyChanged(nameof(collectionViewModel.CollectedFaceByHamsterList), ContainerEventType.Remove, faceId);

        ServiceManager.Instance.CollectionService.TryDelectedHamsterData(targetUID).Forget();
    }

    public static void RequestSelectedHamsterId(this CollectionViewModel collectionViewModel, string selectedHamsterId)
    {
        collectionViewModel.CurrentSelectHamsterId = selectedHamsterId;
    }

    public static void RequestSelectedFaceId(this CollectionViewModel collectionViewModel, string selectedFaceId)
    {
        collectionViewModel.CurrentSelectedHamsterFaceId = selectedFaceId;
    }
}