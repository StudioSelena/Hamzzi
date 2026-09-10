using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectionViewModel : ViewModelBase, IContainerPropertyChanged<string>
{
    public event Action<string, ContainerEventType, string> ContainerPropertyChanged;

    private int _maxPlacedHamster = 20;
    public int MaxPlacedHamster => _maxPlacedHamster;

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

    // 맵에 배치된 햄스터 UID
    private HashSet<long> _placedHamsterUIDList = new HashSet<long>();
    public HashSet<long> PlacedHamsterUIDList
    {
        get { return _placedHamsterUIDList; }
        set
        {
            if (_placedHamsterUIDList != value)
            {
                _placedHamsterUIDList = value;
                OnPropertyChanged(nameof(PlacedHamsterUIDList));
            }
        }
    }

    // 햄스터 종류에 따라 보유한 얼굴ID 저장
    private Dictionary<string, HashSet<string>> _collectedFaceByHamsterList = new Dictionary<string, HashSet<string>>();
    public Dictionary<string, HashSet<string>> CollectedFaceByHamsterList
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

    // 현재 도감에서 선택된 햄스터 아이디
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

    // 현재 도감에서 선택된 얼굴 아이디
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
            collectionViewModel.CollectedFaceByHamsterList.Add(hamsterSave.HamsterId, new HashSet<string>());
        }

        var faceList = collectionViewModel.CollectedFaceByHamsterList[hamsterSave.HamsterId];
        faceList.Add(hamsterSave.FaceId);

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
        collectionViewModel.CollectedFaceByHamsterList[hamsterId].Remove(faceId);

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