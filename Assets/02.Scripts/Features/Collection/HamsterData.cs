using System;
using UnityEngine;

[System.Serializable]
public class HamsterData : GameDataBase
{
    public string Name;
    public string Description;
    public HamsterTier HamsterTier;
    public float CollectSpeed;
    public string IconPath;
    public string MaterialPath;
    public string PrefabPath;
}

[System.Serializable]
public class FaceData : GameDataBase
{
    public string Name;
    public string Description;
    public string IconPath;
    public string MaterialPath;
}

[Serializable]
public class ProfileIconData : GameDataBase
{
    public string IconPath;
}

[System.Serializable]
public class HamsterSave
{
    public long HamsterUID;
    public long UserUID;
    public string HamsterId;
    public string FaceId;
}