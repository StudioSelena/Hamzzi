using System.Collections.Generic;
using UnityEngine;

public enum BuildType
{
    None,
    Room,
    Aisle
}

public struct DoorInfo
{
    public Vector2Int InsidePos;
    public Vector2Int OutsidePos;
    public int DirectionIndex;
}

public struct DoorData
{
    public Vector2Int Offset;
    public int DirectionIndex;
}

public class RoomViewModel : ViewModelBase
{
    private const int ROOM_WIDTH = 10;
    private const int ROOM_HEIGHT = 6;
    private const int AISLE_SIZE = 2;
    private const float DEPTH_Z = 9.0f;
    private const int DOOR_Y = 2;

    public bool IsDefault { get; set; } = false;
    public bool IsReady { get; set; } = false;
    public List<DoorData> DoorDataList { get; private set; } = new List<DoorData>();
    public List<FurnitureViewModel> FurnitureList { get; private set; } = new List<FurnitureViewModel>();

    private Dictionary<Vector2Int, FurnitureViewModel> _furnitureGrid = new Dictionary<Vector2Int, FurnitureViewModel>();

    public Vector2Int SubGridSize
    {
        get
        {
            return new Vector2Int(Size.x * _gridFactor, Size.y * _gridFactor);
        }
    }

    private int _gridFactor = 4;
    public int GridFactor
    {
        get
        {
            return _gridFactor;
        }
    }

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

    private BuildType _buildType;
    public BuildType BuildType
    {
        get => _buildType;
        set
        {
            if (_buildType != value)
            {
                _buildType = value;
                OnPropertyChanged(nameof(BuildType));
            }
        }
    }

    private Vector2Int _originPos;
    public Vector2Int OriginPos
    {
        get => _originPos;
        set
        {
            if (_originPos != value)
            {
                _originPos = value;
                OnPropertyChanged(nameof(OriginPos));
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

    private AisleConnection _aisleConnection;
    public AisleConnection AisleConnection
    {
        get => _aisleConnection;
        set
        {
            if (_aisleConnection.Up != value.Up || _aisleConnection.Down != value.Down || _aisleConnection.Left != value.Left || _aisleConnection.Right != value.Right)
            {
                _aisleConnection = value;
                OnPropertyChanged(nameof(AisleConnection));
            }
        }
    }

    public RoomViewModel(string instanceID, BuildType type, Vector2Int pos)
    {
        InstanceID = instanceID;
        BuildType = type;
        OriginPos = pos;
        Size = (type == BuildType.Room) ? new Vector2Int(ROOM_WIDTH, ROOM_HEIGHT) : new Vector2Int(AISLE_SIZE, AISLE_SIZE);

        if (type == BuildType.Aisle)
        {
            DoorDataList.Add(new DoorData { Offset = Vector2Int.zero, DirectionIndex = 0 });
        }
        else if (type == BuildType.Room)
        {
            InitDefaultDoor();
        }

        IsReady = true;
    }

    // 건설 관련
    private void InitDefaultDoor()
    { 
        List<DoorData> defaultDoors = new List<DoorData>
        {
            new DoorData { Offset = new Vector2Int(0, DOOR_Y), DirectionIndex = 2 },
            new DoorData { Offset = new Vector2Int(Size.x - 1, DOOR_Y), DirectionIndex = 3 }
        };

        SetDoorData(defaultDoors);
    }

    public void SetDoorData(List<DoorData> doorDataList)
    {
        DoorDataList = doorDataList;
        OnPropertyChanged(nameof(DoorDataList));
    }

    public Vector2Int GetNearDoor(Vector2Int targetPos)
    {
        if (DoorDataList == null || DoorDataList.Count == 0)
        {
            InitDefaultDoor();
        }

        Vector2Int nearDoor = OriginPos + DoorDataList[0].Offset;
        int minDist = int.MaxValue;

        foreach (DoorData doorData in DoorDataList)
        {
            DoorInfo info = GetDoorInfo(doorData.Offset);

            int dist = Mathf.Abs(info.OutsidePos.x - targetPos.x) + Mathf.Abs(info.OutsidePos.y - targetPos.y);

            if (dist < minDist)
            {
                minDist = dist;
                nearDoor = OriginPos + doorData.Offset;
            }
        }

        return nearDoor;
    }

    public void SetWallActive(int dir, bool isConnected)
    {
        AisleConnection current = AisleConnection;

        switch (dir)
        {
            case 0:
                current.Up = isConnected;
                break;

            case 1:
                current.Down = isConnected;
                break;

            case 2:
                current.Left = isConnected;
                break;

            case 3:
                current.Right = isConnected;
                break;
        }

        AisleConnection = current;
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(AisleConnection));
    }

    public DoorInfo GetDoorInfo(Vector2Int doorOffset)
    {
        Vector2Int inside = OriginPos + doorOffset;
        int dirIndex = (doorOffset.x == 0) ? 2 : 3;

        if (DoorDataList != null)
        {
            foreach (DoorData data in DoorDataList)
            {
                if (data.Offset == doorOffset)
                {
                    dirIndex = data.DirectionIndex;
                    break;
                }
            }
        }

        Vector2Int outside = inside;

        switch (dirIndex)
        {
            case 0:
                outside = inside + new Vector2Int(0, 1);
                break;

            case 1:
                outside = inside + new Vector2Int(0, -AISLE_SIZE);
                break;

            case 2:
                outside = inside + new Vector2Int(-AISLE_SIZE, 0);
                break;

            case 3:
                outside = inside + new Vector2Int(1, 0);
                break;
        }

        return new DoorInfo { InsidePos = inside, OutsidePos = outside, DirectionIndex = dirIndex };
    }

    // 하우징 관련
    public Vector2Int ChangeLocalGrid(Vector3 worldPos, Vector2Int furnitureSize, float cellSize = 1.0f)
    {
        float subCellSize = cellSize / _gridFactor;

        float localX = worldPos.x - (OriginPos.x * cellSize);
        float localZ = DEPTH_Z - worldPos.z;

        int gridX = Mathf.FloorToInt(localX / subCellSize);
        int gridY = Mathf.FloorToInt(localZ / subCellSize);

        int maxX = Mathf.Max(0, SubGridSize.x - furnitureSize.x);
        int maxY = Mathf.Max(0, SubGridSize.y - furnitureSize.y);

        gridX = Mathf.Clamp(gridX, 0, maxX);
        gridY = Mathf.Clamp(gridY, 0, maxY);

        return new Vector2Int(gridX, gridY);
    }

    public bool IsValidPlace(Vector2Int localPos, Vector2Int furnitureSize)
    {
        for (int x = 0; x < furnitureSize.x; x++)
        {
            for (int y = 0; y < furnitureSize.y; y++)
            {
                Vector2Int checkPos = localPos + new Vector2Int(x, y);

                if (checkPos.x < 0 || checkPos.x >= SubGridSize.x || checkPos.y < 0 || checkPos.y >= SubGridSize.y)
                {
                    return false;
                }

                if (_furnitureGrid.ContainsKey(checkPos) || IsDoorPos(checkPos))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsDoorPos(Vector2Int checkPos)
    {
        foreach (DoorData door in DoorDataList)
        {
            int minX = door.Offset.x * _gridFactor;
            int maxX = minX + _gridFactor;
            int minY = door.Offset.y * _gridFactor * 2;
            int maxY = minY + _gridFactor * 2;

            if (checkPos.x >= minX && checkPos.x < maxX && checkPos.y >= minY && checkPos.y < maxY)
            {
                return true;
            }
        }

        return false;
    }

    public bool RemoveFurniture(FurnitureViewModel furnitureVM)
    {
        List<Vector2Int> removeKeys = new List<Vector2Int>();

        foreach (var pair in _furnitureGrid)
        {
            if (pair.Value == furnitureVM || pair.Value.InstanceID == furnitureVM.InstanceID)
            {
                removeKeys.Add(pair.Key);
            }
        }

        foreach (var key in removeKeys)
        {
            _furnitureGrid.Remove(key);
        }

        FurnitureList.Remove(furnitureVM);
        OnPropertyChanged(nameof(FurnitureList));

        return true;
    }

    public bool AddFurniture(FurnitureViewModel furnitureVM)
    {
        if (!IsValidPlace(furnitureVM.LocalPos, furnitureVM.Size))
        {
            return false;
        }

        for (int x = 0; x < furnitureVM.Size.x; x++)
        {
            for (int y = 0; y < furnitureVM.Size.y; y++)
            {
                _furnitureGrid[furnitureVM.LocalPos + new Vector2Int(x, y)] = furnitureVM;
            }
        }

        FurnitureList.Add(furnitureVM);
        OnPropertyChanged(nameof(FurnitureList));

        return true;
    }
}