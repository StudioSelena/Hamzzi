using System;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class VisitedUserInfoData
{
    public long UserUid = 0;
    public string UserName = "";
    public string UserIconId = "";
}

public class VisitedUserService
{
    private VisitedUserViewModel _viewModel;

    public long CurrentVisitedUid { get; set; }

    public event Action<bool> OnVisitStateChange;

    public VisitedUserService()
    {
        _viewModel = new VisitedUserViewModel();
        _viewModel.SetService(this);
    }

    public VisitedUserViewModel GetViewModel()
    {
        return _viewModel;
    }

    public async UniTask<VisitedUserInfoData> GetVisitedUserInfoAsync()
    {
        VisitedUserInfoData resultData = null;

        if (CurrentVisitedUid == 0) return resultData;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT User_Name, User_Icon_Data_ID FROM {DBConfig.UserGameTable} WHERE User_UID = @uid;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", CurrentVisitedUid);

                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            resultData = new VisitedUserInfoData();
                            resultData.UserUid = CurrentVisitedUid;
                            resultData.UserName = reader.GetString(0);
                            resultData.UserIconId = reader.GetString(1);
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