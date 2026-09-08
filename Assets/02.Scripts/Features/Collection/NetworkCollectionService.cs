using Cysharp.Threading.Tasks;
using MySqlConnector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCollectionService
{
    private CollectionViewModel _currentCollectionViewModel;
    private Dictionary<long, CollectionViewModel> _collectionViewModelList = new Dictionary<long, CollectionViewModel>();
    private HamsterViewModel _hamsterViewModel;

    public event Action OnHamsterDataLoaded;
    public event Action OnChangedCurrentCollectionViewModel;

    public CollectionViewModel GetCurrentCollectionViewModel()
    {
        if (_currentCollectionViewModel == null)
        {
            return null;
        }

        return _currentCollectionViewModel;
    }

    public void SetCurrentCollectionViewModel(long userUID)
    {
        var collectionViewModel = GetCollectionViewModel(userUID);
        _currentCollectionViewModel = collectionViewModel;

        OnChangedCurrentCollectionViewModel?.Invoke();
    }

    public CollectionViewModel GetCollectionViewModel(long userUID)
    {
        if(_collectionViewModelList.ContainsKey(userUID) == false)
        {
            _collectionViewModelList.Add(userUID, new CollectionViewModel());
        }

        return _collectionViewModelList[userUID];
    }

    public HamsterViewModel GetHamsterViewModel()
    {
        if (_hamsterViewModel == null)
        {
            var hamsterViewModel = new HamsterViewModel();
            SetHamsterViewModel(hamsterViewModel);
            _hamsterViewModel = hamsterViewModel;
        }

        return _hamsterViewModel;
    }

    private void SetHamsterViewModel(HamsterViewModel vm)
    {
        GameDataManager.Instance.LoadData<HamsterData>();
        GameDataManager.Instance.LoadData<FaceData>();

        var allHamsterIds = GameDataManager.Instance.GetAllDataId<HamsterData>();
        vm.AllHamsterIdList = allHamsterIds;

        var allFaceIds = GameDataManager.Instance.GetAllDataId<FaceData>();
        vm.AllFaceIdList = allFaceIds;
    }

    public async UniTask LoadHamsterCollectionData(long userUID)
    {
        if (_collectionViewModelList.ContainsKey(userUID) == true)
            return;

        _collectionViewModelList.Add(userUID, new CollectionViewModel());
        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT * FROM {DBConfig.HamsterTable} WHERE Owner_User_UID = @userUID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userUID", userUID);

                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            HamsterSave hamster = new HamsterSave
                            {
                                HamsterUID = reader.GetInt64("Hamster_UID"),
                                UserUID = reader.GetInt64("Owner_User_UID"),
                                HamsterId = reader.GetString("Hamster_Data_ID"),
                                FaceId = reader.GetString("Face_Data_ID")
                            };

                            _collectionViewModelList[userUID].AddCollectedHamsterList(hamster, true);

                            Debug.Log($"Hamster Data Load : {hamster.HamsterId}, {hamster.FaceId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        OnHamsterDataLoaded?.Invoke();
    }

    public async UniTask TrySaveHamsterData(HamsterSave hamsterSave)
    {
        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"INSERT INTO {DBConfig.HamsterTable} (Hamster_UID, Owner_User_UID, Hamster_Data_ID, Face_Data_ID)" +
                               $"VALUES (@hamsterUID, @userUID, @hamsterId, @faceId)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@hamsterUID", hamsterSave.HamsterUID);
                    cmd.Parameters.AddWithValue("@userUID", hamsterSave.UserUID);
                    cmd.Parameters.AddWithValue("@hamsterId", hamsterSave.HamsterId);
                    cmd.Parameters.AddWithValue("@faceId", hamsterSave.FaceId);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if(rowsAffected < 0)
                    {
                        Debug.Log("햄스터 데이터 저장 성공");
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }

    public async UniTask TryDelectedHamsterData(long hamsterUID)
    {
        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"DELETE FROM {DBConfig.HamsterTable} WHERE Hamster_UID = @hamsterUID";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("hamsterUID", hamsterUID);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        Debug.Log($"햄스터 삭제 성공 (삭제된 UID: {hamsterUID})");
                    }
                    else
                    {
                        Debug.LogWarning($"삭제할 햄스터를 찾지 못했습니다. (UID: {hamsterUID})");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }
}
