using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultView : MonoBehaviour
{
    [SerializeField] private Button ExitButton;

    [SerializeField] private GameObject ResultSlotPrefab;
    [SerializeField] private Transform SlotContent;

    private List<GachaResultSlot> _createdSlotList = new List<GachaResultSlot>();
    private const int _slotCount = 10;
    private float duration = 0.3f;
    private float delayIcon = 0.15f;

    private void OnEnable()
    {
        ExitButton.onClick.AddListener(ExitResultUI);
    }

    private void OnDisable()
    {
        ExitButton.onClick.RemoveListener(ExitResultUI);
    }

    private void ExitResultUI()
    {
        gameObject.SetActive(false);
    }

    public void ShowGachaResult(List<HamsterSave> hamsterList)
    {
        if (_createdSlotList.Count <= 0)
        {
            CreateResultSlot();
        }
        InitSlot();

        Sequence gachaSequence = DOTween.Sequence();

        int resultCount = hamsterList.Count;
        for(int i = 0; i < resultCount; i++)
        {
            string hamsterId = hamsterList[i].HamsterId;
            string faceId = hamsterList[i].FaceId;
            GachaResultSlot slot = _createdSlotList[i];
            slot.UpdateSlot(hamsterId, faceId);

            var rect = slot.GetComponent<RectTransform>();
            gachaSequence.AppendCallback(() => PlaySingleStamp(rect));

            if(i < resultCount - 1)
            {
                gachaSequence.AppendInterval(delayIcon);
            }
        }
    }

    private void InitSlot()
    {
        foreach(var slot in _createdSlotList)
        {
            slot.gameObject.SetActive(false);
            var rect = slot.GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;
        }
    }

    private void CreateResultSlot()
    {
        for(int i = 0; i < _slotCount; i++)
        {
            GameObject slot = Instantiate<GameObject>(ResultSlotPrefab, SlotContent);
            GachaResultSlot component = slot.GetComponent<GachaResultSlot>();

            _createdSlotList.Add(component);
        }
    }

    private void PlaySingleStamp(RectTransform iconRect)
    {
        iconRect.gameObject.SetActive(true);

        iconRect.localScale = Vector3.one * 1.5f;

        Sequence singleSeq = DOTween.Sequence();
        singleSeq.Join(iconRect.DOScale(Vector3.one, duration).SetEase(OutBackCustom()));
    }

    private Ease OutBackCustom()
    {
        return Ease.OutBack;
    }
}