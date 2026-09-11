using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class StorageSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image HamsterIconImage;
    [SerializeField] private Image FaceIconImage;

    private long _hamsterUID;

    public event Action OnClickStorageSlot;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            OnClickStorageSlot?.Invoke();
        }
    }

    public void SetStorageSlot(HamsterSave hamsterSave)
    {
        _hamsterUID = hamsterSave.HamsterUID;
        string hamsterId = hamsterSave.HamsterId;
        string faceId = hamsterSave.FaceId;

        UpdateHamsterIconImage(hamsterId).Forget();
        UpdateFaceIconImage(faceId).Forget();
    }

    private async UniTask UpdateHamsterIconImage(string hamsterId)
    {
        var hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);
        string path = hamsterData.IconPath;

        var hamsterIcon = await ResourceManager.Instance.LoadAsset<Sprite>(path);
        HamsterIconImage.sprite = hamsterIcon;
    }

    private async UniTask UpdateFaceIconImage(string faceId)
    {
        var faceData = GameDataManager.Instance.GetData<FaceData>(faceId);
        string path = faceData.IconPath;

        var faceIcon = await ResourceManager.Instance.LoadAsset<Sprite>(path);
        FaceIconImage.sprite = faceIcon;
    }
}