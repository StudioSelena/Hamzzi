using Cysharp.Threading.Tasks;
using MySqlConnector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBuildService
{
    private BuildViewModel _buildVM;
    private bool _isSaving = false;
    private bool _pendingSave = false;

    public event Action OnBuildAndFurnitureDataLoaded;

    public bool IsBuildAndFurnitureDataLoaded { get; private set; }

    private const int GARDEN_ROOM_INDEX = 99;

    public BuildViewModel GetBuildViewModel()
    {
        if (_buildVM == null)
        {
            _buildVM = ServiceManager.Instance.BuildService.GetBuildViewModel();
        }

        return _buildVM;
    }

    public async UniTask LoadBuildAndFurnitureData(long userUID)
    {
        BuildViewModel buildVM = GetBuildViewModel();
        HousingViewModel housingVM = ServiceManager.Instance.HousingService.GetHousingViewModel();

        buildVM.Builds.Clear();
        housingVM.GardenFurnitureList.Clear();

        long userGardenRoomUID = 0;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string roomQuery = $"SELECT Room_UID, Room_Index, Position_X, Position_Y FROM {DBConfig.RoomTable} WHERE Owner_User_UID = @userUID GROUP BY Room_UID";
                using (MySqlCommand cmd = new MySqlCommand(roomQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userUID", userUID);
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            long roomUID = reader.GetInt64("Room_UID");
                            int roomIndex = reader.GetInt32("Room_Index");
                            int posX = reader.GetInt32("Position_X");
                            int posY = reader.GetInt32("Position_Y");

                            if (roomIndex == GARDEN_ROOM_INDEX)
                            {
                                userGardenRoomUID = roomUID;
                                continue;
                            }

                            BuildType buildType = (roomIndex == 2 || roomIndex == 3) ? BuildType.Aisle : BuildType.Room;
                            bool isDefault = (roomIndex == 0 || roomIndex == 3);

                            Vector2Int pos = new Vector2Int(posX, posY);

                            RoomViewModel roomVM = null;
                            foreach (var existing in buildVM.Builds.Values)
                            {
                                if (existing.InstanceID == roomUID.ToString())
                                {
                                    roomVM = existing;
                                    break;
                                }
                            }

                            if (roomVM == null)
                            {
                                roomVM = new RoomViewModel(roomUID.ToString(), buildType, pos)
                                {
                                    InstanceID = roomUID.ToString(),
                                    IsReady = true,
                                    IsDefault = isDefault
                                };
                            }

                            int sizeX = (buildType == BuildType.Room) ? 10 : 2;
                            int sizeY = (buildType == BuildType.Room) ? 6 : 2;

                            for (int x = 0; x < sizeX; x++)
                            {
                                for (int y = 0; y < sizeY; y++)
                                {
                                    buildVM.Builds[pos + new Vector2Int(x, y)] = roomVM;
                                }
                            }
                        }
                    }
                }

                string furnitureQuery = $@"SELECT furniture.* FROM {DBConfig.FurnitureTable} furniture JOIN {DBConfig.RoomTable} room ON furniture.Room_UID = room.Room_UID WHERE room.Owner_User_UID = @userUID";

                using (MySqlCommand cmd = new MySqlCommand(furnitureQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userUID", userUID);

                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            long roomUID = reader.GetInt64("Room_UID");
                            long furnitureUID = reader.GetInt64("Furniture_UID");
                            string furnitureDataId = reader.GetString("Furniture_Data_ID");
                            int posX = reader.GetInt32("Position_X");
                            int posY = reader.GetInt32("Position_Y");
                            int rotateState = reader.GetInt32("Rotate_State");
                            long? hamsterUID = reader.IsDBNull(reader.GetOrdinal("Useing_Hamster_UID")) ? (long?)null : reader.GetInt64("Useing_Hamster_UID");
                            Debug.Log($"가구 ID: {furnitureDataId} / 할당된 햄스터 UID: {hamsterUID}");

                            var itemData = GameDataManager.Instance.GetData<ItemData>(furnitureDataId);

                            if (itemData == null)
                            {
                                continue;
                            }

                            FurnitureViewModel furnitureVM = new FurnitureViewModel(furnitureUID.ToString(), itemData.Id, itemData.PrefabPath, new Vector2Int(posX, posY), Vector2Int.one)
                            {
                                RotationAngle = rotateState,
                                AssignHamsterID = hamsterUID?.ToString()
                            };

                            if (userGardenRoomUID != 0 && roomUID == userGardenRoomUID)
                            {
                                if (housingVM != null)
                                {
                                    await SpawnLoadGardenFurniture(housingVM, furnitureVM);
                                }
                            }
                            else
                            {
                                foreach (var room in buildVM.Builds.Values)
                                {
                                    if (room.InstanceID == roomUID.ToString())
                                    {
                                        await SpawnLoadFurniture(room, furnitureVM);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var pair in buildVM.Builds)
                {
                    Vector2Int pos = pair.Key;
                    RoomViewModel vm = pair.Value;

                    if (vm.BuildType == BuildType.Room)
                    {
                        buildVM.UpdateRoomConnection(vm);
                    }
                    else
                    {
                        buildVM.UpdateConnection(pos);
                    }
                }

                HashSet<RoomViewModel> uniqueAisles = new HashSet<RoomViewModel>();
                int defaultAisleCount = 0;
                foreach (var vm in buildVM.Builds.Values)
                {
                    if (vm.BuildType == BuildType.Aisle && uniqueAisles.Add(vm))
                    {
                        if (vm.IsDefault)
                        {
                            defaultAisleCount++;
                        }
                    }
                }

                ServiceManager.Instance.BuildService.RefreshAisleNavMesh(buildVM.Builds);
            }
            catch (Exception ex)
            {
                Debug.LogError($"건설 및 가구 데이터 로드 오류 : {ex.Message}");
            }
        }

        NavigationManager.Instance.BuildNav();
        ServiceManager.Instance.HousingService.RefreshFurnitureBuff();

        IsBuildAndFurnitureDataLoaded = true;
        OnBuildAndFurnitureDataLoaded?.Invoke();

        Debug.Log("건설 및 가구 데이터 로드 완료");
    }

    public async UniTask SaveAllBuildAndFurnitureData(long userUID)
    {
        if (userUID == 0)
        {
            return;
        }

        var buildVM = GetBuildViewModel();
        var housingVM = ServiceManager.Instance.HousingService.GetHousingViewModel();

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            await conn.OpenAsync();

            using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
            {
                try
                {
                    string deleteFurniture = $@"DELETE furniture FROM {DBConfig.FurnitureTable} furniture JOIN {DBConfig.RoomTable} room ON furniture.Room_UID = room.Room_UID WHERE room.Owner_User_UID = @userUID";
                    using (MySqlCommand cmd = new MySqlCommand(deleteFurniture, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@userUID", userUID);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    string deleteRoom = $"DELETE FROM {DBConfig.RoomTable} WHERE Owner_User_UID = @userUID AND Room_Index != {GARDEN_ROOM_INDEX}";
                    using (MySqlCommand cmd = new MySqlCommand(deleteRoom, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@userUID", userUID);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    HashSet<RoomViewModel> uniqueBuilds = new HashSet<RoomViewModel>(buildVM.Builds.Values);
                    foreach (var build in uniqueBuilds)
                    {
                        long uid = 0;
                        long.TryParse(build.InstanceID, out uid);
                        if (uid == 0)
                        {
                            uid = GameUtil.GenerateUID();
                            build.InstanceID = uid.ToString();
                        }

                        int roomIndexValue;
                        if (build.BuildType == BuildType.Aisle)
                        {
                            roomIndexValue = build.IsDefault ? 3 : 2;
                        }
                        else
                        {
                            roomIndexValue = build.IsDefault ? 0 : 1;
                        }

                        string insertQuery = $@"INSERT INTO {DBConfig.RoomTable} (Room_UID, Owner_User_UID, Room_Index, Position_X, Position_Y) VALUES (@roomUID, @userUID, @roomIndex, @roomPosX, @roomPosY)";

                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@roomUID", uid);
                            cmd.Parameters.AddWithValue("@userUID", userUID);
                            cmd.Parameters.AddWithValue("@roomIndex", roomIndexValue);
                            cmd.Parameters.AddWithValue("@roomPosX", build.OriginPos.x);
                            cmd.Parameters.AddWithValue("@roomPosY", build.OriginPos.y);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        if (build.FurnitureList != null && build.FurnitureList.Count > 0)
                        {
                            foreach (var furnitureVM in build.FurnitureList)
                            {
                                await InsertFurnitureData(conn, transaction, uid, furnitureVM);
                            }
                        }
                    }

                    long gardenRoomUID = 0;
                    string gardenQuery = $"SELECT Room_UID FROM {DBConfig.RoomTable} WHERE Owner_User_UID = @userUID AND Room_Index = @gardenIndex LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(gardenQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@userUID", userUID);
                        cmd.Parameters.AddWithValue("@gardenIndex", GARDEN_ROOM_INDEX);
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            gardenRoomUID = Convert.ToInt64(result);
                        }
                    }

                    if (gardenRoomUID == 0)
                    {
                        gardenRoomUID = GameUtil.GenerateUID();
                        string insertGardenRoom = $@"INSERT INTO {DBConfig.RoomTable} (Room_UID, Owner_User_UID, Room_Index, Position_X, Position_Y) VALUES (@roomUID, @userUID, @roomIndex, @roomPosX, @roomPosY)";
                        using (MySqlCommand cmd = new MySqlCommand(insertGardenRoom, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@roomUID", gardenRoomUID);
                            cmd.Parameters.AddWithValue("@userUID", userUID);
                            cmd.Parameters.AddWithValue("@roomIndex", GARDEN_ROOM_INDEX);
                            cmd.Parameters.AddWithValue("@roomPosX", 0);
                            cmd.Parameters.AddWithValue("@roomPosY", 0);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    if (housingVM != null && housingVM.GardenFurnitureList != null)
                    {
                        foreach (var furnitureVM in housingVM.GardenFurnitureList)
                        {
                            await InsertFurnitureData(conn, transaction, gardenRoomUID, furnitureVM);
                        }
                    }

                    await transaction.CommitAsync();
                    Debug.Log("건설 및 가구(정원 포함) 저장 성공");
                }
                catch (Exception ex)
                {
                    try { await transaction.RollbackAsync(); } catch { }
                    Debug.LogError($"저장 오류 : {ex.Message}");
                }
            }
        }
    }

    private async UniTask InsertFurnitureData(MySqlConnection conn, MySqlTransaction transaction, long roomUID, FurnitureViewModel furnitureVM)
    {
        long furnitureUID = 0;

        if (string.IsNullOrEmpty(furnitureVM.InstanceID) || !long.TryParse(furnitureVM.InstanceID, out furnitureUID) || furnitureUID == 0)
        {
            furnitureUID = GameUtil.GenerateUID();
            furnitureVM.InstanceID = furnitureUID.ToString();
        }

        string insertFurniture = $@"INSERT INTO {DBConfig.FurnitureTable} 
        (Furniture_UID, Room_UID, Furniture_Data_ID, Position_X, Position_Y, Rotate_State, Useing_Hamster_UID)
        VALUES (@furnitureUID, @roomUID, @furnitureDataId, @posX, @posY, @rotate, @hamsterUID)";

        using (MySqlCommand fCmd = new MySqlCommand(insertFurniture, conn, transaction))
        {
            fCmd.Parameters.AddWithValue("@furnitureUID", furnitureUID);
            fCmd.Parameters.AddWithValue("@roomUID", roomUID);
            fCmd.Parameters.AddWithValue("@furnitureDataId", furnitureVM.FurnitureID);
            fCmd.Parameters.AddWithValue("@posX", furnitureVM.LocalPos.x);
            fCmd.Parameters.AddWithValue("@posY", furnitureVM.LocalPos.y);
            fCmd.Parameters.AddWithValue("@rotate", furnitureVM.RotationAngle);

            long parsedHamsterUID = 0;
            object hamsterVal = DBNull.Value;

            if (!string.IsNullOrEmpty(furnitureVM.AssignHamsterID) && long.TryParse(furnitureVM.AssignHamsterID, out parsedHamsterUID))
            {
                hamsterVal = parsedHamsterUID;
            }

            fCmd.Parameters.AddWithValue("@hamsterUID", hamsterVal);

            await fCmd.ExecuteNonQueryAsync();
        }
    }

    private async UniTask SpawnLoadFurniture(RoomViewModel roomVM, FurnitureViewModel furnitureVM)
    {
        GameObject prefab = await GameObjectManager.Instance.CreateObjectAsync(furnitureVM.InstanceID.ToString(), furnitureVM.PrefabPath, Vector3.zero);
        prefab.transform.rotation = Quaternion.identity;

        float subCellSize = 1.0f / roomVM.GridFactor;

        if (prefab.TryGetComponent(out FurnitureView furnitureView))
        {
            Vector2Int rawSize = furnitureView.GetFurnitureSize(subCellSize);

            bool rotatedFlag = (furnitureVM.RotationAngle / 90) % 2 != 0;
            furnitureVM.Size = rotatedFlag ? new Vector2Int(rawSize.y, rawSize.x) : rawSize;
        }

        roomVM.AddFurniture(furnitureVM);

        int sizeX = furnitureVM.Size.x;
        int sizeY = furnitureVM.Size.y;

        float localX = (furnitureVM.LocalPos.x + sizeX * 0.5f) * subCellSize;
        float localZ = (furnitureVM.LocalPos.y + sizeY * 0.5f) * subCellSize;

        Vector3 spawnPos = new Vector3((roomVM.OriginPos.x * 1.0f) + localX, (roomVM.OriginPos.y + 2.0f) * 1.0f + 0.2f, 9f - localZ - 0.5f);
        Quaternion spawnRot = Quaternion.Euler(0f, furnitureVM.RotationAngle, 0f);

        spawnPos.y += furnitureView.Offset;

        prefab.transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (furnitureView != null)
        {
            furnitureView.ResetMaterial();
            furnitureView.Bind(furnitureVM);
        }

        ServiceManager.Instance.HousingService.RegisterSpawnFurniture(furnitureVM.InstanceID, prefab);
    }

    private async UniTask SpawnLoadGardenFurniture(HousingViewModel housingVM, FurnitureViewModel furnitureVM)
    {
        GameObject prefab = await GameObjectManager.Instance.CreateObjectAsync(furnitureVM.InstanceID.ToString(), furnitureVM.PrefabPath, Vector3.zero);
        prefab.transform.rotation = Quaternion.identity;

        float subCellSize = 1.0f;

        if (prefab.TryGetComponent(out FurnitureView furnitureView))
        {
            Vector2Int rawSize = furnitureView.GetFurnitureSize(subCellSize);

            bool rotatedFlag = (furnitureVM.RotationAngle / 90) % 2 != 0;
            furnitureVM.Size = rotatedFlag ? new Vector2Int(rawSize.y, rawSize.x) : rawSize;
        }

        housingVM.LoadGardenFurniture(furnitureVM);

        int sizeX = furnitureVM.Size.x;
        int sizeY = furnitureVM.Size.y;

        float localX = (furnitureVM.LocalPos.x + sizeX * 0.5f) * subCellSize;
        float localZ = (furnitureVM.LocalPos.y + sizeY * 0.5f) * subCellSize;

        Vector3 gardenOrigin = new Vector3(-40f, 12f, 12f);
        Vector3 spawnPos = new Vector3(gardenOrigin.x + localX, gardenOrigin.y, gardenOrigin.z + localZ);
        Quaternion spawnRot = Quaternion.Euler(0f, furnitureVM.RotationAngle, 0f);

        spawnPos.y += furnitureView.Offset;

        prefab.transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (furnitureView != null)
        {
            furnitureView.ResetMaterial();
            furnitureView.Bind(furnitureVM);
        }

        ServiceManager.Instance.HousingService.RegisterSpawnFurniture(furnitureVM.InstanceID, prefab);
    }

    public async UniTask<bool> HasUserRoomData(long userUID)
    {
        if (userUID == 0)
        {
            return false;
        }

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();
                string query = $"SELECT COUNT(*) FROM {DBConfig.RoomTable} WHERE Owner_User_UID = @userUID";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userUID", userUID);
                    long count = (long)(await cmd.ExecuteScalarAsync());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"저장 데이터 확인 오류 : {ex.Message}");
                return false;
            }
        }
    }

    public void RequestSaveHousingData()
    {
        long userUID = 0;
        var loginVm = ServiceManager.Instance.LoginService?.GetViewModel();
        if (loginVm != null)
        {
            userUID = loginVm.UserUID;
        }

        if (userUID == 0)
        {
            return;
        }

        if (_isSaving)
        {
            _pendingSave = true;
            return;
        }

        SaveLoop(userUID).Forget();
    }

    private async UniTask SaveLoop(long userUID)
    {
        _isSaving = true;

        do
        {
            _pendingSave = false;
            await SaveAllBuildAndFurnitureData(userUID);

            Debug.Log("건설/가구/인벤토리 데이터 저장 요청 완료");
        }
        while (_pendingSave);

        _isSaving = false;
    }
}