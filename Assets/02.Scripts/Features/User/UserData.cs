using System;
using UnityEngine;

[System.Serializable]
public class UserData : GameDataBase
{
    public string UserName;
    public string UserIconId;
    public int GoldCount;
    public float GoldPerSec;
    public DateTime LastCrossTime;
}

[System.Serializable]
public class UserSaveData : GameDataBase
{
    public int GoldCount;
    public float GoldPerSec;
    public DateTime LastCrossTime;
}
