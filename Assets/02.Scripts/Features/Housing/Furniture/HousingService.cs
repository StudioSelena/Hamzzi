using Cysharp.Threading.Tasks;
using MySqlConnector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HousingService
{
    private Dictionary<string, GameObject> _spawnFurniture = new Dictionary<string, GameObject>();

    private HousingViewModel _housingVM;

    public HousingViewModel GetHousingViewModel()
    {
        if (_housingVM == null)
        {
            CreateHousingViewModel();
        }

        return _housingVM;
    }

    public HousingViewModel CreateHousingViewModel()
    {
        HousingViewModel housingVm = new HousingViewModel();
        _housingVM = housingVm;

        return housingVm;
    }

    public List<FurnitureViewModel> GetAllPlacedFurniture()
    {
        List<FurnitureViewModel> allList = new List<FurnitureViewModel>();

        BuildViewModel buildVM = ServiceManager.Instance.BuildService.GetBuildViewModel();

        if (buildVM?.Builds != null)
        {
            var uniqueRooms = new HashSet<RoomViewModel>(buildVM.Builds.Values);

            foreach (var room in uniqueRooms)
            {
                if (room.FurnitureList != null)
                {
                    allList.AddRange(room.FurnitureList);
                }
            }
        }

        if (_housingVM?.GardenFurnitureList != null)
        {
            allList.AddRange(_housingVM.GardenFurnitureList);
        }

        return allList;
    }

    public void RegisterSpawnFurniture(string instanceID, GameObject obj)
    {
        _spawnFurniture[instanceID] = obj;
    }

    public GameObject GetSpawnFurniture(string instanceID)
    {
        _spawnFurniture.TryGetValue(instanceID, out GameObject obj);
        return obj;
    }

    public void RemoveSpawnFurniture(string instanceID)
    {
        if (_spawnFurniture.TryGetValue(instanceID, out GameObject obj))
        {
            GameObjectManager.Instance.RequestDestroyObject(obj);
            _spawnFurniture.Remove(instanceID);
        }
    }

    public void ClearAllFurniture()
    {
        foreach (var pair in _spawnFurniture)
        {
            if (pair.Value != null)
            {
                GameObjectManager.Instance.RequestDestroyObject(pair.Value);
            }
        }

        _spawnFurniture.Clear();
    }

    // DB에서 저장된 인벤토리 데이터를 조회해 리스트로 반환
    public async UniTask<List<InventoryData>> LoadInventoryData(long userUid)
    {
        List<InventoryData> resultList = new List<InventoryData>();

        if (userUid != 0)
        {
            using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string query = $"SELECT Furniture_Data_ID, Count FROM Inventory_Data WHERE User_UID = @userUid";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userUid", userUid);

                        using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var inventoryData = new InventoryData();

                                inventoryData.ItemDataId = reader.GetString(0);
                                inventoryData.StackCount = reader.GetInt32(1);

                                resultList.Add(inventoryData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        return resultList;

    }

    // 불러온 인벤토리 데이터를 가구 슬롯으로 생성해서 인벤토리 리스트에 추가
    public async UniTask LoadInventory(long userUid)
    {
        List<InventoryData> inventoryList = await LoadInventoryData(userUid);

        var housingVm = ServiceManager.Instance.HousingService.GetHousingViewModel();
        foreach(var inventoryData in inventoryList)
        {
            var itemData = GameDataManager.Instance.GetData<ItemData>(inventoryData.ItemDataId);
            if(itemData == null)
            {
                continue;
            }

            var iconSprite = await ResourceManager.Instance.LoadAsset<Sprite>(itemData.IconPath);

            var slotVm = new FurnitureSlotViewModel();

            slotVm.ItemUniqueId = TestGameUtil.GenerateUniqueId();
            slotVm.ItemDataId = inventoryData.ItemDataId;
            slotVm.IconSprite = iconSprite;
            slotVm.StackCount = inventoryData.StackCount;

            housingVm.AddItemSlotViewModel(slotVm);
            Debug.Log($"[인벤토리 로드] UID : {userUid} / Item : {slotVm.ItemDataId} / Count : {slotVm.StackCount}");
        }
    }

    // 기존 인벤토리 데이터 삭제하고 현재 인벤토리 데이터를 DB에 저장
    public async UniTask SaveAllInventoryData(long userUid)
    {
        if (userUid == 0)
        {
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    string deleteQuery = $"DELETE FROM Inventory_Data WHERE User_UID = @userUid";

                    using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, conn, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@userUid", userUid);

                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    foreach(var itemKv in _housingVM.ItemList)
                    {
                        var slotVm = itemKv.Value;

                        if(slotVm.StackCount <= 0)
                        {
                            continue;
                        }

                        long inventoryUid = TestGameUtil.GenerateUniqueId();

                        string insertQuery = $"INSERT INTO Inventory_Data (InventoryUID, User_UID, Furniture_Data_ID, Count) VALUES (@inventoryUid, @userUid, @furnitureDataId, @count)";

                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn, transaction))
                        {
                            insertCmd.Parameters.AddWithValue("@inventoryUid", inventoryUid);
                            insertCmd.Parameters.AddWithValue("@userUid", userUid);
                            insertCmd.Parameters.AddWithValue("@furnitureDataId", slotVm.ItemDataId);
                            insertCmd.Parameters.AddWithValue("@count", slotVm.StackCount);

                            await insertCmd.ExecuteNonQueryAsync();
                            Debug.Log($"[인벤토리 저장] UID : {userUid} / Item : {slotVm.ItemDataId} / Count : {slotVm.StackCount}");
                        }
                    }

                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }

    private void AddInventoryItem(string itemDataId, Sprite iconSprite)
    {
        HousingViewModel housingVm = GetHousingViewModel();
        FurnitureSlotViewModel targetSlotVm = null;

        foreach (var itemKv in housingVm.ItemList)
        {
            var slotVm = itemKv.Value;

            if (slotVm.ItemDataId == itemDataId)
            {
                slotVm.StackCount++;
                targetSlotVm = slotVm;
                break;
            }
        }

        if(targetSlotVm == null)
        {
            targetSlotVm = new FurnitureSlotViewModel();
            targetSlotVm.ItemUniqueId = TestGameUtil.GenerateUniqueId();
            targetSlotVm.ItemDataId = itemDataId;
            targetSlotVm.IconSprite = iconSprite;
            targetSlotVm.StackCount = 1;

            housingVm.AddItemSlotViewModel(targetSlotVm);
        }

        var loginVm = ServiceManager.Instance.LoginService.GetViewModel();

        InventoryData inventoryData = new InventoryData
        {
            ItemDataId = targetSlotVm.ItemDataId,
            StackCount = targetSlotVm.StackCount
        };
    }

    public void AddItem(ShopSlotViewModel shopSlotVm)
    {
        if (shopSlotVm == null)
        {
            return;
        }

        AddInventoryItem(shopSlotVm.ItemDataId, shopSlotVm.IconSprite);
    }

    public void AddItem(string itemDataId, Sprite iconSprite)
    {
        AddInventoryItem(itemDataId, iconSprite);
    }

    public void RefreshFurnitureBuff()
    {
        float totalBuffRate = 0f;

        List<FurnitureViewModel> placedFurnitureList = GetAllPlacedFurniture();

        foreach(var furnitureVm in placedFurnitureList)
        {
            var itemData = GameDataManager.Instance.GetData<ItemData>(furnitureVm.FurnitureID);
            if (itemData == null)
            {
                return;
            }

            var subCategoryEffectData = GameDataManager.Instance.GetData<SubCategoryEffectData>(itemData.SubCategory);
            if (subCategoryEffectData != null)
            {
                float itemEffect = subCategoryEffectData.SeedCollectionBonus;
                totalBuffRate += itemEffect;
                
            }
        }

        var userVm = ServiceManager.Instance.UserService.GetUserViewModel();
        if (userVm != null)
        {
            userVm.SetFurnitureBuff(totalBuffRate);
        }
    }
}
