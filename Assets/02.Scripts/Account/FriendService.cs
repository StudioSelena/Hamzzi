using System;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

public class FriendService
{
    public async UniTask<bool> TryAddFriendAsync(long myUid, long targetUid)
    {
        bool isSuccess = false;

        if (myUid == 0 || targetUid == 0 || myUid == targetUid) return isSuccess;

        using (MySqlConnection conn = new MySqlConnection(DBConfig.ConnectionString))
        {
            try
            {
                await conn.OpenAsync();

                string checkQuery = $"SELECT COUNT(*) FROM {DBConfig.FriendTable} WHERE Owner_User_UID = @myUid AND Friend_User_UID = @targetUid;";
                bool isExist = false;

                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@myUid", myUid);
                    checkCmd.Parameters.AddWithValue("@targetUid", targetUid);

                    object result = await checkCmd.ExecuteScalarAsync();
                    int count = Convert.ToInt32(result);

                    if (count > 0)
                    {
                        isExist = true;
                    }
                }

                if (isExist == false)
                {
                    long myDataUid = GameUtil.GenerateUID();
                    long targetDataUid = GameUtil.GenerateUID();

                    string insertQuery = $"INSERT INTO {DBConfig.FriendTable} (Friend_Data_UID, Owner_User_UID, Friend_User_UID, Is_Send, Is_Accept) VALUES (@uid1, @owner1, @friend1, 1, 0), (@uid2, @owner2, @friend2, 0, 0);";

                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@uid1", myDataUid);
                        insertCmd.Parameters.AddWithValue("@owner1", myUid);
                        insertCmd.Parameters.AddWithValue("@friend1", targetUid);

                        insertCmd.Parameters.AddWithValue("@uid2", targetDataUid);
                        insertCmd.Parameters.AddWithValue("@owner2", targetUid);
                        insertCmd.Parameters.AddWithValue("@friend2", myUid);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 2)
                        {
                            isSuccess = true;
                        }
                    }
                }
                else
                {
                    Debug.Log("이미 친구 요청을 보냈거나 친구인 상태입니다.");
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