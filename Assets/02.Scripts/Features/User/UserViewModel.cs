using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class UserViewModel : ViewModelBase
{
    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(UserName));
        OnPropertyChanged(nameof(UserIconId));
        OnPropertyChanged(nameof(SeedCount));
    }

    private string _userName;
    public string UserName
    {
        get => _userName;
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
    }

    private string _userIconId;
    public string UserIconId
    {
        get => _userIconId;
        set
        {
            if (_userIconId != value)
            {
                _userIconId = value;
                OnPropertyChanged(nameof(UserIconId));
            }
        }
    }

    private int _seedCount;
    public int SeedCount
    {
        get => _seedCount;
        set
        {
            if (_seedCount != value)
            {
                _seedCount = value;
                OnPropertyChanged(nameof(SeedCount));
            }
        }
    }

    public float GoldPerSec { get; set; }

    private DateTime _lastCrossTime;
    public DateTime LastCrossTime
    {
        get { return _lastCrossTime; }
        set
        {
            if (_lastCrossTime != value)
            {
                _lastCrossTime = value;
                OnPropertyChanged(nameof(LastCrossTime));
            }
        }
    }
}

public static class UserViewModelExtension
{
    private static float _seedBuffRate;
    private static float _seedBonusRemain;

    public static void AddSeed(this UserViewModel userVm, int amount)
    {
        userVm.SeedCount += amount;

        float addedBonus = amount * _seedBuffRate;
        _seedBonusRemain += addedBonus;

        int bonusAmount = Mathf.FloorToInt(_seedBonusRemain);

        if(bonusAmount > 0)
        {
            userVm.SeedCount += bonusAmount;
            _seedBonusRemain -= bonusAmount;

            Debug.Log($"[보너스 지급] +" + $"{bonusAmount}");
        }
    }

    //방치보상 계산 전용 - 버프 중복 적용 막기 위해
    public static void AddSeedWithoutBuff(this UserViewModel userVm, int amount)
    {
        userVm.SeedCount += amount;
    }

    public static void SetFurnitureBuff(this UserViewModel userVm, float amount)
    {
        _seedBuffRate = amount;
    }

    public static float GetSeedBuffRate(this UserViewModel userVm)
    {
        return _seedBuffRate;
    }

    public static int PredictSeedGain(this UserViewModel userVm, int amount)
    {
        float addedBonus = amount * _seedBuffRate;
        int bonusAmount = Mathf.FloorToInt(_seedBonusRemain + addedBonus);

        return amount + bonusAmount;
    }

    public static bool TryUseSeed(this UserViewModel userVm, int amount)
    {
        if(userVm.SeedCount < amount)
        {
            return false;
        }

        userVm.SeedCount -= amount;

        return true;
    }

    public static void SetLastCrossTime(this UserViewModel userVm, DateTime lastCrossTime)
    {
        userVm.LastCrossTime = lastCrossTime;

        long userUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        ServiceManager.Instance.UserService.SaveUserAsync(userUID).Forget();
    }
}
