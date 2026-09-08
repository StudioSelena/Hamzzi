using System;
using System.Collections.Generic;
using UnityEngine;

public class CrossHamsterSelectView : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private UIButton ExitButton;

    [Header("프리팹")]
    [SerializeField] private Transform ContentTransform;
    [SerializeField] private GameObject SlotPrefab;

    private HamsterOwnerType _onwerType;
    private List<HamsterSlot> _spawndSlotList = new List<HamsterSlot>();

    public event Action<string, HamsterOwnerType> OnSlotSelect;

    private void OnEnable()
    {
        ExitButton.BindOnClickButtonEvent(ExitSelectView);
    }

    private void OnDisable()
    {
        ExitButton.UnBindOnClickButtonEvent(ExitSelectView);
        ResetSelectView();
    }

    private void ExitSelectView()
    {
        gameObject.SetActive(false);
    }

    public void OpenSelectView(long userUID, HamsterOwnerType ownerType)
    {
        gameObject.SetActive(true);

        _onwerType = ownerType;

        // 보유 햄스터 불러오기
        var collectionViewModel = ServiceManager.Instance.CollectionService.GetCollectionViewModel(userUID);
        var collectionList = collectionViewModel.CollectedHamsterList;

        // Slot 생성 및 클릭 할당
        foreach(var kv in collectionList)
        {
            var hamster = kv.Value;
            if (hamster == null) 
                continue;

            var slotObject = Instantiate(SlotPrefab, ContentTransform);
            var slotComponent = slotObject.GetComponent<HamsterSlot>();

            var hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamster.HamsterId);

            slotComponent.InitSlot(hamsterData, true);
            slotComponent.OnSlotClicked += OnClickSlot;
            _spawndSlotList.Add(slotComponent);
        }
    }

    private void OnClickSlot(string hamsterId)
    {
        OnSlotSelect?.Invoke(hamsterId, _onwerType);

        gameObject.SetActive(false);
    }

    private void ResetSelectView()
    {
        foreach(var slot in _spawndSlotList)
        {
            Destroy(slot.gameObject);
        }
        _spawndSlotList.Clear();
    }
}