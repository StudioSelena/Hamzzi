using System;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class LoginService
{
    private LoginViewModel _viewModel;

    private string defaultIconAddress = "Hamster/HasterIcon/Hamster_00_Icon";
    private string defaultName = "기본이름";

    public LoginService()
    {
        _viewModel = new LoginViewModel();
        _viewModel.SetLoginService(this);
    }

    public LoginViewModel GetViewModel()
    {
        return _viewModel;
    }

    public async UniTask<long> TryLoginAsync(string userId, string password)
    {
        long resultUid = 0;

        if (userId == "" || password == "") return resultUid;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT User_UID FROM {DBConfig.UserAccountTable} WHERE User_Id = @userId AND User_Password = @password;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@password", password);

                    object result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        resultUid = Convert.ToInt64(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return resultUid;
    }

    public async UniTask<long> CreateAccountAsync(string userId, string password)
    {
        long newUserUid = 0;

        if (userId != "" && password != "")
        {
            using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string checkQuery = $"SELECT COUNT(*) FROM {DBConfig.UserAccountTable} WHERE User_Id = @userId;";
                    bool isExist = false;

                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@userId", userId);

                        object result = await checkCmd.ExecuteScalarAsync();
                        int count = Convert.ToInt32(result);

                        if (count > 0)
                        {
                            isExist = true;
                        }
                    }

                    if (isExist == false)
                    {
                        long generatedUid = GameUtil.GenerateUID();

                        string insertAccountQuery = $"INSERT INTO {DBConfig.UserAccountTable} (User_UID, User_Id, User_Password, Last_Login) VALUES (@uid, @userId, @password, @lastLogin);";

                        using (MySqlCommand insertAccountCmd = new MySqlCommand(insertAccountQuery, conn))
                        {
                            insertAccountCmd.Parameters.AddWithValue("@uid", generatedUid);
                            insertAccountCmd.Parameters.AddWithValue("@userId", userId);
                            insertAccountCmd.Parameters.AddWithValue("@password", password);
                            insertAccountCmd.Parameters.AddWithValue("@lastLogin", DateTime.UtcNow);

                            int accountRows = await insertAccountCmd.ExecuteNonQueryAsync();

                            if (accountRows > 0)
                            {
                                string insertGameDataQuery = $"INSERT INTO {DBConfig.UserGameTable} (User_UID, User_Name, User_Icon_Data_ID, Gold_Count, Gold_Per_Sec, Gold_Bonus) VALUES (@uid, @userName, @iconId, @gold, @goldPerSec, @goldBonus);";

                                using (MySqlCommand insertGameDataCmd = new MySqlCommand(insertGameDataQuery, conn))
                                {
                                    insertGameDataCmd.Parameters.AddWithValue("@uid", generatedUid);
                                    insertGameDataCmd.Parameters.AddWithValue("@userName", defaultName);
                                    insertGameDataCmd.Parameters.AddWithValue("@iconId", defaultIconAddress);
                                    insertGameDataCmd.Parameters.AddWithValue("@gold", 0);
                                    insertGameDataCmd.Parameters.AddWithValue("@goldPerSec", 0);
                                    insertGameDataCmd.Parameters.AddWithValue("@goldBonus", 0);

                                    int gameDataRows = await insertGameDataCmd.ExecuteNonQueryAsync();

                                    if (gameDataRows > 0)
                                    {
                                        newUserUid = generatedUid;
                                    }
                                }
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

        return newUserUid;
    }
    public async UniTask<DateTime> GetLastLoginTimeAsync(long uid)
    {
        DateTime lastLogin = DateTime.MinValue;

        if (uid == 0) return lastLogin;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT Last_Login FROM {DBConfig.UserAccountTable} WHERE User_UID = @uid;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", uid);

                    object result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        lastLogin = Convert.ToDateTime(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return lastLogin;
    }

    public async UniTask<bool> UpdateLastLoginAsync(long uid, DateTime loginTime)
    {
        bool isSuccess = false;

        if (uid == 0) return isSuccess;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string updateQuery = $"UPDATE {DBConfig.UserAccountTable} SET Last_Login = @lastLogin WHERE User_UID = @uid;";

                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@lastLogin", loginTime);
                    updateCmd.Parameters.AddWithValue("@uid", uid);

                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

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