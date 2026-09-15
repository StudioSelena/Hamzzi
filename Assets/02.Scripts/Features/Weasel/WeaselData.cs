//방해요소 족제비 밸런싱 데이터
using UnityEngine;

[System.Serializable]
public class WeaselData : GameDataBase
{
    public int OnlineAppearIntervalSec;
    public int TapDurationSec;
    public int RequiredTapCount;
    public int StealSeedMinAmount;
    public int StealSeedMaxAmount;
    public int MinSeedToAppear;
    public int OfflineMaxOccurrence;
    public int OfflineMaxOccurrenceHours;
    public int OfflineDefenseSuccessPercent;
}
