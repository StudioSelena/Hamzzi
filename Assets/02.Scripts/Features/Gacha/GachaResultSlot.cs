using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultSlot : MonoBehaviour
{
    [SerializeField] private Image HamsterIcon;
    [SerializeField] private Image TierImage;
    [SerializeField] private Image TierIcon;

    [Header("Tier Color")]
    [SerializeField] private Color SSTierColor;
    [SerializeField] private Color STierColor;
    [SerializeField] private Color ATierColor;

    [Header("Tier Icon")]
    [SerializeField] private Sprite SSTierIcon;
    [SerializeField] private Sprite STierIcon;
    [SerializeField] private Sprite ATierIcon;

    public void UpdateSlot(string hamsterId)
    {
        HamsterData hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);

        LoadHamsterIcon(hamsterData.IconPath).Forget();
        SetTier(hamsterData.HamsterTier);
    }

    private async UniTask LoadHamsterIcon(string iconPath)
    {
        Sprite hamsterIcon = await ResourceManager.Instance.LoadAsset<Sprite>(iconPath);
        HamsterIcon.sprite = hamsterIcon;
    }

    private void SetTier(HamsterTier tier)
    {
        switch (tier)
        {
            case HamsterTier.SS:
                TierImage.color = SSTierColor;
                TierIcon.sprite = SSTierIcon;
                break;
            case HamsterTier.S:
                TierImage.color = STierColor;
                TierIcon.sprite = STierIcon;
                break;
            case HamsterTier.A:
                TierImage.color = ATierColor;
                TierIcon.sprite = ATierIcon;
                break;
        }
    }
}