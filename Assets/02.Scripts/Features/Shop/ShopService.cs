using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class ShopService
{
    private ShopViewModel _shopViewModel;

    public ShopViewModel GetShopViewModel()
    {
        if(_shopViewModel == null)
        {
            CreateShopViewModel();
        }

        return _shopViewModel;
    }

    private ShopViewModel CreateShopViewModel()
    {
        var shopVm = new ShopViewModel();
        _shopViewModel = shopVm;
        return shopVm;
    }

    public async UniTask InitShop()
    {
        GameDataManager.Instance.LoadData<ShopData>();
        GameDataManager.Instance.LoadData<ItemData>();
        GameDataManager.Instance.LoadData<SubCategoryEffectData>();

        var shopDataList = GameDataManager.Instance.GetAllData<ShopData>();
        if(shopDataList == null)
        {
            Debug.LogWarning("상점 데이터가 없습니다.");
            return;
        }

        // 여러 비동기 작업들을 하나로 묶어서 관리하기 위한 리스트
        var tasks = new List<UniTask<ShopSlotViewModel>>();

        foreach (var shopData in shopDataList)
        {
            var itemData = GameDataManager.Instance.GetData<ItemData>(shopData.ItemId);
            if(itemData == null)
            {
                Debug.LogWarning("아이템 데이터가 없습니다.");
                return;
            }

            var effectDataList = GameDataManager.Instance.GetAllData<SubCategoryEffectData>();
            if(effectDataList == null)
            {
                Debug.LogWarning("버프 데이터가 없습니다.");
                return;
            }

            SubCategoryEffectData matchedEffectData = null;

            foreach(var effectData in effectDataList)
            {
                if(effectData.SubCategory == itemData.SubCategory)
                {
                    matchedEffectData = effectData;
                    break;
                }
            }

            if(matchedEffectData == null)
            {
                Debug.LogWarning($"{itemData.SubCategory} 버프 데이터를 찾을 수 없습니다.");
                continue;
            }

            tasks.Add(CreateItem(shopData, itemData, matchedEffectData));
        }

        // 모든 아이템의 리소스 로드가 끝날 때까지 병렬로 동시 대기 -> 리소스는 병렬로 로드
        var shopSlotVmList = await UniTask.WhenAll(tasks);
        var shopVm = GetShopViewModel();

        // 로드가 끝난 후 원래 tasks 순서대로 추가
        foreach(var shopSlotVm in shopSlotVmList)
        {
            shopVm.AddItemSlotViewModel(shopSlotVm);
        }

        // 로드가 완벽히 끝난 후 한번만 UI에 알림
        shopVm.NotifyItemListChanged();
    }

    private async UniTask<ShopSlotViewModel> CreateItem(ShopData shopData, ItemData itemData, SubCategoryEffectData effectData)
    {
        long uniqueId = TestGameUtil.GenerateUniqueId();

        Sprite loadedSprite = null;
        if(string.IsNullOrEmpty(itemData.IconPath) == false)
        {
            loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(itemData.IconPath);
        }
        
        var shopSlotVm = new ShopSlotViewModel();
        shopSlotVm.ItemUniqueId = uniqueId;
        shopSlotVm.ItemDataId = shopData.ItemId;
        shopSlotVm.Name = itemData.Name;
        shopSlotVm.Description = itemData.Description;
        shopSlotVm.Category = itemData.Category;
        shopSlotVm.SubCategory = itemData.SubCategory;
        shopSlotVm.ItemEffect = effectData.SeedCollectionBonus;
        shopSlotVm.CostAmount = shopData.CostAmount;
        shopSlotVm.IconSprite = loadedSprite;

        return shopSlotVm;
    }

    public void BuyItem()
    {
        var selectedSlot = GetShopViewModel().SelectedSlot;
        if(selectedSlot == null)
        {
            return;
        }

        var userVm = ServiceManager.Instance.UserService.GetUserViewModel();
        if(userVm != null)
        {
            bool Success = userVm.TryUseSeed(selectedSlot.CostAmount);

            if (Success == false)
            {
                Debug.Log("씨앗이 부족합니다.");
                return;
            }

            ServiceManager.Instance.HousingService.AddItem(selectedSlot);
            SaveInventoryDate().Forget();

            Debug.Log($"아이템을 구매했다!  Id: {GetShopViewModel().SelectedSlot.ItemDataId}   이름: {GetShopViewModel().SelectedSlot.Name}");
        }

    }

    private async UniTask SaveInventoryDate()
    {
        var loginVm = ServiceManager.Instance.LoginService.GetViewModel();
        if(loginVm != null)
        {
            await ServiceManager.Instance.HousingService.SaveAllInventoryData(loginVm.UserUID);
            Debug.Log($"아이템 저장 완료!");
        }
       
    }
}
