using System;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class AccountSearchService
{
    private AccountSearchViewModel _viewModel;

    public AccountSearchService()
    {
        _viewModel = new AccountSearchViewModel();
        _viewModel.SetService(this);
    }

    public AccountSearchViewModel GetViewModel()
    {
        return _viewModel;
    }

    public async UniTask<long> TrySearchAccountAsync(string userId)
    {
        long targetUid = 0;

        if (userId == "") return targetUid;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT User_UID FROM {DBConfig.UserAccountTable} WHERE User_Id = @userId;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    object result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        targetUid = Convert.ToInt64(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return targetUid;
    }
}