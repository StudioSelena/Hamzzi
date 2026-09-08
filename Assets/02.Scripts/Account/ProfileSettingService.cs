using System;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class ProfileSettingService
{
    private ProfileSettingViewModel _viewModel;

    public ProfileSettingService()
    {
        _viewModel = new ProfileSettingViewModel();
        _viewModel.SetService(this);
    }

    public ProfileSettingViewModel GetViewModel()
    {
        return _viewModel;
    }

    public async UniTask<bool> TrySetUserIconAsync(long uid, string iconPath)
    {
        bool isSuccess = false;

        if (uid == 0 || iconPath == "") return isSuccess;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"UPDATE {DBConfig.UserGameTable} SET User_Icon_Data_ID = @iconPath WHERE User_UID = @uid;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@iconPath", iconPath);
                    cmd.Parameters.AddWithValue("@uid", uid);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
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