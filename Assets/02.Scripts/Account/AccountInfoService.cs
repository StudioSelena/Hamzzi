using System;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class AccountInfoData
{
    public long UserUid = 0;
    public string UserId = "";
    public string UserName = "";
    public string UserIconId = "";
}

public class AccountInfoService
{
    private AccountInfoViewModel _viewModel;

    public AccountInfoService()
    {
        _viewModel = new AccountInfoViewModel();
        _viewModel.SetService(this);
    }

    public AccountInfoViewModel GetViewModel()
    {
        return _viewModel;
    }

    public async UniTask<AccountInfoData> GetAccountInfoAsync(long targetUid)
    {
        AccountInfoData resultData = null;

        if (targetUid == 0) return resultData;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT a.User_Id, g.User_Name, g.User_Icon_Data_ID FROM {DBConfig.UserAccountTable} a JOIN {DBConfig.UserGameTable} g ON a.User_UID = g.User_UID WHERE a.User_UID = @uid;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", targetUid);

                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            resultData = new AccountInfoData();
                            resultData.UserUid = targetUid;
                            resultData.UserId = reader.GetString(0);
                            resultData.UserName = reader.GetString(1); 
                            resultData.UserIconId = reader.GetString(2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return resultData;
    }
}