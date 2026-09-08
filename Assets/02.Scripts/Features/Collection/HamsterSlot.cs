using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class HamsterSlot : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private Button SlotButton;
    [SerializeField] private GameObject LockImage;
    [SerializeField] private Image HamsterIcon;
    [SerializeField] private Image TierImage;

    [Header("Tier Color")]
    [SerializeField] private Color SSTierColor;
    [SerializeField] private Color STierColor;
    [SerializeField] private Color ATierColor;

    private string _hamsterId;

    public event Action<string> OnSlotClicked;

    private void OnEnable()
    {
        SlotButton.onClick.AddListener(OnClickSlot);
    }

    private void OnDisable()
    {
        SlotButton.onClick.RemoveListener(OnClickSlot);
    }

    public void InitSlot(HamsterData hamsterData, bool isCollected)
    {
        _hamsterId = hamsterData.Id;

        LoadHamsterIcon(hamsterData.IconPath).Forget();
        UpdateLockImage(isCollected);
        SetTierColor(hamsterData.HamsterTier);
    }

    public void UpdateLockImage(bool isCollected)
    {
        LockImage.SetActive(!isCollected);
    }

    private async UniTask LoadHamsterIcon(string path)
    {
        Sprite hamsterIcon = await ResourceManager.Instance.LoadAsset<Sprite>(path);
        if (hamsterIcon == null)
            return;

        HamsterIcon.sprite = hamsterIcon;
    }


    private void SetTierColor(HamsterTier tier)
    {
        switch (tier)
        {
            case HamsterTier.SS:
                TierImage.color = SSTierColor;
                break;
            case HamsterTier.S:
                TierImage.color = STierColor;
                break;
            case HamsterTier.A:
                TierImage.color = ATierColor;
                break;
        }
    }

    private void OnClickSlot()
    {
        OnSlotClicked?.Invoke(_hamsterId);
    }
}