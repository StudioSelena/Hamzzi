using Cysharp.Threading.Tasks;
using UnityEngine;

public class HamsterModelViewModel : ViewModelBase
{
    private string _hamsterId;
    public string HamsterId
    {
        get { return _hamsterId; }
        set
        {
            if(_hamsterId != value)
            {
                _hamsterId = value;
                OnPropertyChanged(nameof(HamsterId));
            }
        }
    }

    private string _faceId;
    public string FaceId
    {
        get { return _faceId; }
        set
        {
            if (_faceId != value)
            {
                _faceId = value;
                OnPropertyChanged(nameof(FaceId));
            }
        }
    }
}