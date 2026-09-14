using System;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class ChangePlayerNameService
{
    private ChangePlayerNameViewModel _viewModel;

    public ChangePlayerNameService()
    {
        _viewModel = new ChangePlayerNameViewModel();
        _viewModel.SetService(this);
    }

    public ChangePlayerNameViewModel GetViewModel()
    {
        return _viewModel;
    }

    public async UniTask<bool> TryChangePlayerNameAsync(long uid, string newName)
    {
        bool isSuccess = false;

        if (uid == 0 || newName == "") return isSuccess;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"UPDATE {DBConfig.UserGameTable} SET User_Name = @newName WHERE User_UID = @uid;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@newName", newName);
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