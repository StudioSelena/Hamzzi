using System.Collections.Generic;
using UnityEngine;

public class HamsterViewModel : ViewModelBase
{
    private List<string> _allHamsterIdList = new List<string>();
    public List<string> AllHamsterIdList
    {
        get { return _allHamsterIdList; }
        set
        {
            if (_allHamsterIdList != value)
            {
                _allHamsterIdList = value;
                OnPropertyChanged(nameof(AllHamsterIdList));
            }
        }
    }

    private List<string> _allFaceIdList = new List<string>();
    public List<string> AllFaceIdList
    {
        get { return _allFaceIdList; }
        set
        {
            if (_allFaceIdList != value)
            {
                _allFaceIdList = value;
                OnPropertyChanged(nameof(AllFaceIdList));
            }
        }
    }
}