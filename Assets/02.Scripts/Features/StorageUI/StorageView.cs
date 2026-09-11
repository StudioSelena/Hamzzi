using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StorageView : ViewBase
{
    [Header("햄스터 슬롯 관련")]
    [SerializeField] private GameObject StorageSlotPrefab;
    [SerializeField] private Transform PlacedHamsterContent;
    [SerializeField] private Transform StorageHamsterContent;

    [Header("배치 햄스터 관련")]
    private List<StorageSlot> _spawnedPlacedSlotList = new List<StorageSlot>();

    [Header("창고 햄스터 관련")]
    private List<StorageSlot> _spawnedStorageSlotList = new List<StorageSlot>();

    private CollectionViewModel _collectionViewModel;
    private int _maxStorageSlotCount = 100;

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

        // 햄스터 슬롯 생성
        for(int i = spawndPlacedSlotCount; i < maxPlacedHamster; i++)
        {
            GameObject slotObject = Instantiate(StorageSlotPrefab, PlacedHamsterContent);
            var slotComponent = slotObject.GetComponent<StorageSlot>();
            _spawnedPlacedSlotList.Add(slotComponent);
        }

        // 배치된 햄스터 설정
    }

    private void UpdateStorageSlot()
    {
        var storageHamsters = _collectionViewModel.CollectedHamsterList
            .Where(kvp => !_collectionViewModel.PlacedHamsterUIDList.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        int allHamsterCount = _collectionViewModel.CollectedHamsterList.Count;
        int placedHamsterCount = _collectionViewModel.PlacedHamsterUIDList.Count;

        int spawndStorageSlotCount = allHamsterCount - placedHamsterCount;
        int currentSpawnedSlotCount = _spawnedPlacedSlotList.Count;

        // 창고에 있는 햄스터 슬롯 업데이트
        for (int i = currentSpawnedSlotCount; i < spawndStorageSlotCount; i++)
        {
            GameObject slotObject = Instantiate(StorageSlotPrefab, StorageHamsterContent);
            var slotComponent = slotObject.GetComponent<StorageSlot>();
            _spawnedStorageSlotList.Add(slotComponent);
        }

        for(int i = 0; i < _spawnedStorageSlotList.Count; i++)
        {
            if(i < storageHamsters.Count)
            {
                HamsterSave hamsterSave = storageHamsters[i];
                _spawnedStorageSlotList[i].gameObject.SetActive(true);

                _spawnedStorageSlotList[i].SetStorageSlot(hamsterSave);
            }
        }
    }
}
