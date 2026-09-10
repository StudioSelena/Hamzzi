using System.Collections.Generic;
using UnityEngine;

public class StorageView : ViewBase
{
    [Header("햄스터 슬롯 관련")]
    [SerializeField] private GameObject StorageSlotPrefab;
    [SerializeField] private Transform PlacedHamsterContent;
    [SerializeField] private Transform StorageHamsterContent;

    [Header("배치 햄스터 관련")]

    [Header("창고 햄스터 관련")]

    private CollectionViewModel _collectionViewModel;
    private int _maxStorageSlotCount = 100;

    private List<StorageSlot> _spawnedPlacedSlotList = new List<StorageSlot>();
    private List<StorageSlot> _spawnedStorageSlotList = new List<StorageSlot>();

    private void Awake()
    {
        long userUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;

        _collectionViewModel = ServiceManager.Instance.CollectionService.GetCollectionViewModel(userUID);
    }

    private void OnEnable()
    {
        UpdatePlacedSlot();
        UpdateStorageSlot();
    }

    // TODO : 굴 제거했을 경우도 생객해서 다시 짜야함
    private void UpdatePlacedSlot()
    {
        int maxPlacedHamster = _collectionViewModel.MaxPlacedHamster;
        int spawndPlacedSlotCount = _spawnedPlacedSlotList.Count;

        for(int i = spawndPlacedSlotCount; i < maxPlacedHamster; i++)
        {
            GameObject slotObject = Instantiate(StorageSlotPrefab, PlacedHamsterContent);
            var slotComponent = slotObject.GetComponent<StorageSlot>();
            _spawnedPlacedSlotList.Add(slotComponent);
        }
    }

    private void UpdateStorageSlot()
    {
        int spawndStorageSlotCount = _spawnedStorageSlotList.Count;

        for(int i = spawndStorageSlotCount; i < _maxStorageSlotCount; i++)
        {
            GameObject slotObject = Instantiate(StorageSlotPrefab, StorageHamsterContent);
            var slotComponent = slotObject.GetComponent<StorageSlot>();
            _spawnedStorageSlotList.Add(slotComponent);
        }
    }
}
