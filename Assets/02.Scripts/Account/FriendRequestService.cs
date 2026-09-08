using System;
using System.Collections.Generic;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class FriendRequestData
{
    public long FriendUid = 0;
    public string FriendName = "";
    public string FriendIconId = "";
}

public class FriendRequestService
{
    private FriendRequestViewModel _viewModel;

    public FriendRequestService()
    {
        _viewModel = new FriendRequestViewModel();
        _viewModel.SetService(this);
    }

    public FriendRequestViewModel GetViewModel()
    {
        return _viewModel;
    }

    public async UniTask<List<FriendRequestData>> GetFriendRequestsAsync(long myUid)
    {
        List<FriendRequestData> resultList = new List<FriendRequestData>();

        if (myUid == 0) return resultList;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT f.Friend_User_UID, u.User_Name, u.User_Icon_Data_ID FROM {DBConfig.FriendTable} f JOIN {DBConfig.UserGameTable} u ON f.Friend_User_UID = u.User_UID WHERE f.Owner_User_UID = @uid AND f.Is_Send = 0 AND f.Is_Accept = 0;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", myUid);

                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            FriendRequestData data = new FriendRequestData();
                            data.FriendUid = reader.GetInt64(0);
                            data.FriendName = reader.GetString(1);
                            data.FriendIconId = reader.GetString(2);

                            resultList.Add(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return resultList;
    }

    public async UniTask<bool> AcceptFriendRequestAsync(long myUid, long targetUid)
    {
        bool isSuccess = false;

        if (myUid == 0 || targetUid == 0) return isSuccess;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"UPDATE {DBConfig.FriendTable} SET Is_Accept = 1 WHERE (Owner_User_UID = @myUid AND Friend_User_UID = @targetUid) OR (Owner_User_UID = @targetUid AND Friend_User_UID = @myUid);";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@myUid", myUid);
                    cmd.Parameters.AddWithValue("@targetUid", targetUid);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 2)
                    {
                        isSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return isSuccess;
    }

    public async UniTask<bool> RejectFriendRequestAsync(long myUid, long targetUid)
    {
        bool isSuccess = false;

        if (myUid == 0 || targetUid == 0) return isSuccess;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"DELETE FROM {DBConfig.FriendTable} WHERE (Owner_User_UID = @myUid AND Friend_User_UID = @targetUid) OR (Owner_User_UID = @targetUid AND Friend_User_UID = @myUid);";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@myUid", myUid);
                    cmd.Parameters.AddWithValue("@targetUid", targetUid);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 2)
                    {
                        isSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return isSuccess;
    }
}