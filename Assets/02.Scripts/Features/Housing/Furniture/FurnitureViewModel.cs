using UnityEngine;

public class FurnitureViewModel : ViewModelBase
{
    public string FurnitureID { get; private set; }
    public string PrefabPath { get; private set; }

    private string _instanceID;
    public string InstanceID
    {
        get => _instanceID;
        set
        {
            if (_instanceID != value)
            {
                _instanceID = value;
                OnPropertyChanged(nameof(InstanceID));
            }
        }
    }

    private string _roomInstanceID;
    public string RoomInstanceID
    {
        get => _roomInstanceID;
        set
        {
            if (_roomInstanceID != value)
            {
                _roomInstanceID = value;
                OnPropertyChanged(nameof(RoomInstanceID));
            }
        }
    }

    private Vector2Int _localPos;
    public Vector2Int LocalPos
    {
        get => _localPos;
        set
        {
            if (_localPos != value)
            {
                _localPos = value;
                OnPropertyChanged(nameof(LocalPos));
            }
        }
    }

    private Vector2Int _size;
    public Vector2Int Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }
    }

    private int _rotationAngle;
    public int RotationAngle
    {
        get => _rotationAngle;
        set
        {
            if (_rotationAngle != value)
            {
                _rotationAngle = value;
                OnPropertyChanged(nameof(RotationAngle));
            }
        }
    }

    private bool _isValid;
    public bool IsValid
    {
        get => _isValid;
        set
        {
            if (_isValid != value)
            {
                _isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    private string _assignHamsterID;
    public string AssignHamsterID
    {
        get => _assignHamsterID;
        set
        {
            if (_assignHamsterID != value)
            {
                _assignHamsterID = value;
                OnPropertyChanged(nameof(AssignHamsterID));
            }
        }
    }

    public bool CanAssignHamster
    {
        get
        {
            return !string.IsNullOrEmpty(FurnitureID) && FurnitureID.Contains("Wheel");
        }
    }

    public FurnitureViewModel (string instanceID, string furnitureID, string prefabPath, Vector2Int localPos, Vector2Int size)
    {
        InstanceID = instanceID;
        PrefabPath = prefabPath;
        FurnitureID = furnitureID;
        LocalPos = localPos;
        Size = size;
        RotationAngle = 0;
    }

    public void Rotate()
    {
        float currentCenterX = LocalPos.x + Size.x * 0.5f;
        float currentCenterY = LocalPos.y + Size.y * 0.5f;

        RotationAngle = (RotationAngle + 90) % 360;
        int temp = Size.x;
        Size = new Vector2Int(Size.y, temp);

        LocalPos = new Vector2Int(Mathf.RoundToInt(currentCenterX - Size.x * 0.5f), Mathf.RoundToInt(currentCenterY - Size.y * 0.5f));
    }
}
