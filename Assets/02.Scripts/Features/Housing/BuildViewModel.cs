using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class BuildViewModel : ViewModelBase
{
    private static readonly Vector2Int[] _directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private const int AISLE_SIZE = 2;

    public Dictionary<Vector2Int, RoomViewModel> Builds { get; private set; } = new Dictionary<Vector2Int, RoomViewModel>();

    private bool _hasStartPos = false;
    private RoomViewModel _startRoom;

    private RoomViewModel _waitingRoom;
    private List<RoomViewModel> _waitingAisle = new List<RoomViewModel>();

    public bool IsLoading { get; set; } = false;

    public bool CanDestroy
    {
        get
        {
            return SelectRoom != null && SelectRoom.IsReady && SelectType == BuildType.Room && !SelectRoom.IsDefault;
        }
    }

    public bool CanConnectAisle
    {
        get
        {
            return SelectRoom != null && SelectRoom.IsReady && SelectType == BuildType.Room;
        }
    }

    private bool _canConfirm = false;
    public bool CanConfirm
    {
        get => _canConfirm;
        set
        {
            if (_canConfirm != value)
            {
                _canConfirm = value;
                OnPropertyChanged(nameof(CanConfirm));
            }
        }
    }

    private RoomViewModel _selectRoom;
    public RoomViewModel SelectRoom
    {
        get => _selectRoom;
        set
        {
            if (_selectRoom != value)
            {
                _selectRoom = value;
                OnPropertyChanged(nameof(SelectRoom));
                OnPropertyChanged(nameof(CanDestroy));
                OnPropertyChanged(nameof(CanConnectAisle));
            }
        }
    }

    private RoomViewModel _destroyBuild;
    public RoomViewModel DestroyBuild
    {
        get => _destroyBuild;
        set
        {
            if (_destroyBuild != value)
            {
                _destroyBuild = value;
                OnPropertyChanged(nameof(DestroyBuild));
            }
        }
    }

    private BuildType _selectType = BuildType.None;
    public BuildType SelectType
    {
        get => _selectType;
        set
        {
            if (_selectType != value)
            {
                _selectType = value;
                OnPropertyChanged(nameof(SelectType));
                OnPropertyChanged(nameof(CanDestroy));
                OnPropertyChanged(nameof(CanConnectAisle));
            }
        }
    }

    private RoomViewModel _lastBuild;
    public RoomViewModel LastBuild
    {
        get => _lastBuild;
        set
        {
            if (_lastBuild != value)
            {
                _lastBuild = value;
                OnPropertyChanged(nameof(LastBuild));
            }
        }
    }

    private List<string> _destroyedInstanceIDs = new List<string>();
    public List<string> DestroyedInstanceIDs
    {
        get => _destroyedInstanceIDs;
        set
        {
            _destroyedInstanceIDs = value;
            OnPropertyChanged(nameof(DestroyedInstanceIDs));
        }
    }

    public void ChooseRoom(RoomViewModel room)
    {
        if (room == null || room.BuildType != BuildType.Room)
        {
            return;
        }

        SelectRoom = room;
    }

    public void DeselectRoom()
    {
        SelectRoom = null;
    }

    public void StartConnectingAisle()
    {
        if (SelectRoom == null)
        {
            return;
        }

        _startRoom = SelectRoom;
        _hasStartPos = true;

        DeselectRoom();
        SelectType = BuildType.Aisle;
    }

    public void DestroyRoom()
    {
        if (SelectRoom == null)
        {
            return;
        }

        RoomViewModel target = SelectRoom;

        if (target.FurnitureList != null && target.FurnitureList.Count > 0)
        {
            var furnituresToReturn = new List<FurnitureViewModel>(target.FurnitureList);

            foreach (var furniture in furnituresToReturn)
            {
                if (!string.IsNullOrEmpty(furniture.AssignHamsterID))
                {
                    furniture.AssignHamsterID = null;
                }

                ServiceManager.Instance.HousingService.RefreshFurnitureBuff();

                ServiceManager.Instance.HousingService.GetHousingViewModel().DestroyFurniture = furniture;
                ReturnFurniture(furniture.FurnitureID).Forget();
            }

            target.FurnitureList.Clear();
        }

        List<Vector2Int> connectedAislePositions = new List<Vector2Int>();

        foreach (DoorData doorData in target.DoorDataList)
        {
            DoorInfo doorInfo = target.GetDoorInfo(doorData.Offset);

            if (Builds.TryGetValue(doorInfo.OutsidePos, out RoomViewModel aisleVM) && aisleVM.BuildType == BuildType.Aisle)
            {
                connectedAislePositions.Add(doorInfo.OutsidePos);
            }
        }

        DeselectRoom();
        RemoveBuild(target);
        ClearAisle(connectedAislePositions);

        ServiceManager.Instance.BuildService.RefreshAisleNavMesh(Builds);
        ServiceManager.Instance.NetworkBuildService.RequestSaveHousingData();
    }

    public void ClearAisle(List<Vector2Int> startPositions)
    {
        HashSet<RoomViewModel> removeAisles = new HashSet<RoomViewModel>();
        Queue<RoomViewModel> queue = new Queue<RoomViewModel>();

        foreach (Vector2Int pos in startPositions)
        {
            if (Builds.TryGetValue(pos, out RoomViewModel aisle))
            {
                if (aisle.BuildType == BuildType.Aisle && !aisle.IsDefault && removeAisles.Add(aisle))
                {
                    queue.Enqueue(aisle);
                }
            }
        }

        while (queue.Count > 0)
        {
            RoomViewModel aisle = queue.Dequeue();

            for (int i = 0; i < _directions.Length; i++)
            {
                List<Vector2Int> edgeTiles = GetEdgeTiles(aisle.OriginPos, aisle.Size, i);

                foreach (Vector2Int tile in edgeTiles)
                {
                    Vector2Int nextPos = tile + _directions[i];

                    if (!Builds.TryGetValue(nextPos, out RoomViewModel next))
                    {
                        continue;
                    }

                    if (next.BuildType == BuildType.Room)
                    {
                        continue;
                    }

                    if (next.BuildType != BuildType.Aisle || next.IsDefault)
                    {
                        continue;
                    }

                    if (removeAisles.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }
        }

        foreach (RoomViewModel aisle in removeAisles)
        {
            RemoveBuild(aisle);
        }
    }

    private async UniTaskVoid ReturnFurniture(string furnitureId)
    {
        var itemData = GameDataManager.Instance.GetData<ItemData>(furnitureId);

        Sprite icon = await ResourceManager.Instance.LoadAsset<Sprite>(itemData.IconPath);

        ServiceManager.Instance.HousingService.AddItem(furnitureId, icon);
    }

    
    public void EnterBuildMode()
    {
        CancelBuildMode();
        DeselectRoom();
        SelectType = BuildType.Room;
    }

    public void ConfirmBuild()
    {
        DeselectRoom();

        if (_waitingRoom != null)
        {
            _waitingRoom.IsReady = true;
            _waitingRoom = null;
        }

        foreach (RoomViewModel aisle in _waitingAisle)
        {
            aisle.IsReady = true;
        }

        _waitingAisle.Clear();
        CanConfirm = false;
        SelectType = BuildType.None;

        ServiceManager.Instance.BuildService.RefreshAisleNavMesh(Builds);

        ServiceManager.Instance.NetworkBuildService.RequestSaveHousingData();
    }

    public void CancelBuildMode()
    {
        DeselectRoom();

        if (_waitingRoom != null)
        {
            RemoveBuild(_waitingRoom);
            _waitingRoom = null;
        }

        ClearWaitingAisle();

        ResetAisle();
        CanConfirm = false;
        SelectType = BuildType.None;
    }

    public void InitDefaultRoom(List<Vector2Int> defaultRoom, List<Vector2Int> defaultAisle)
    {
        foreach (Vector2Int pos in defaultRoom)
        {
            BuildDefaultRoom(pos);
        }

        foreach (Vector2Int pos in defaultAisle)
        {
            BuildDefaultAisle(pos);
        }

        Vector2Int exitAislePos = defaultAisle[0];

        if (Builds.TryGetValue(exitAislePos, out RoomViewModel aisleVM) && aisleVM.BuildType == BuildType.Aisle)
        {
            aisleVM.SetWallActive(0, true);
            aisleVM.Refresh();
        }

        ServiceManager.Instance.BuildService.RefreshAisleNavMesh(Builds);
    }

    public bool TryBuildRoom(Vector2Int pos)
    {
        pos = SnapAisle(pos);

        string uid = GameUtil.GenerateUID().ToString();
        RoomViewModel newRoom = new RoomViewModel(uid,BuildType.Room, pos);

        if (!CanPlaceRoom(pos, newRoom.Size))
        {
            return false;
        }

        RegisterRoom(newRoom, pos);
        UpdateRoomConnection(newRoom);

        _waitingRoom = newRoom;
        LastBuild = newRoom;

        _startRoom = newRoom;
        _hasStartPos = true;
        SelectType = BuildType.Aisle;

        return true;
    }

    private void BuildDefaultRoom(Vector2Int pos)
    {
        pos = SnapAisle(pos);

        string uid = GameUtil.GenerateUID().ToString();
        RoomViewModel newRoom = new RoomViewModel(uid, BuildType.Room, pos);

        newRoom.IsReady = true;
        newRoom.IsDefault = true;

        RegisterRoom(newRoom, pos);
        UpdateRoomConnection(newRoom);

        LastBuild = newRoom;
    }

    public void BuildDefaultAisle(Vector2Int pos)
    {
        if (IsAreaOccupied(pos, new Vector2Int(AISLE_SIZE, AISLE_SIZE)))
        {
            return;
        }

        string uid = GameUtil.GenerateUID().ToString();
        RoomViewModel newAisle = new RoomViewModel(uid, BuildType.Aisle, pos);
        newAisle.IsDefault = true;
        RegisterAisle(newAisle, pos);

        UpdateConnection(pos);

        for (int x = 0; x < AISLE_SIZE; x++)
        {
            for (int y = 0; y < AISLE_SIZE; y++)
            {
                UpdateNearConnection(pos + new Vector2Int(x, y));
            }
        }

        LastBuild = newAisle;
    }

    public bool TryBuildAisle(Vector2Int pos)
    {
        if (!_hasStartPos || _startRoom == null)
        {
            return false;
        }

        if (!Builds.TryGetValue(pos, out RoomViewModel targetVM) || targetVM.BuildType != BuildType.Room || !targetVM.IsReady || _startRoom == targetVM)
        {
            return false;
        }

        List<Vector2Int> path = ServiceManager.Instance.BuildService.SearchBestPath(_startRoom, targetVM, Builds);

        if (path == null || path.Count == 0)
        {
            return false;
        }

        foreach (Vector2Int rawPos in path)
        {
            Vector2Int aislePos = SnapAisle(rawPos);

            if (Builds.TryGetValue(aislePos, out var existingVM) && existingVM.BuildType == BuildType.Aisle)
            {
                UpdateConnection(aislePos);
                continue;
            }

            if (IsAreaRoom(aislePos, new Vector2Int(AISLE_SIZE, AISLE_SIZE)))
            {
                continue;
            }

            if (!IsAreaOccupied(aislePos, new Vector2Int(AISLE_SIZE, AISLE_SIZE)))
            {
                string uid = GameUtil.GenerateUID().ToString();
                RoomViewModel newAisle = new RoomViewModel(uid, BuildType.Aisle, aislePos);
                RegisterAisle(newAisle, aislePos);

                _waitingAisle.Add(newAisle);
                LastBuild = newAisle;
            }

            UpdateConnection(aislePos);

            for (int x = 0; x < AISLE_SIZE; x++)
            {
                for (int y = 0; y < AISLE_SIZE; y++)
                {
                    UpdateNearConnection(aislePos + new Vector2Int(x, y));
                }
            }
        }

        SelectType = BuildType.None;

        CanConfirm = true;
        return true;
    }

    private Vector2Int SnapAisle(Vector2Int pos)
    {
        int x = Mathf.FloorToInt(pos.x / (float)AISLE_SIZE) * AISLE_SIZE;
        int y = Mathf.FloorToInt((pos.y + 1) / (float)AISLE_SIZE) * AISLE_SIZE;

        return new Vector2Int(x, y);
    }

    private bool IsAreaOccupied(Vector2Int pos, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (Builds.ContainsKey(pos + new Vector2Int(x, y)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAreaRoom(Vector2Int pos, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (Builds.TryGetValue(pos + new Vector2Int(x, y), out var vm) && vm.BuildType == BuildType.Room)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RegisterAisle(RoomViewModel aisle, Vector2Int pos)
    {
        for (int x = 0; x < aisle.Size.x; x++)
        {
            for (int y = 0; y < aisle.Size.y; y++)
            {
                Vector2Int tile = pos + new Vector2Int(x, y);
                Builds[tile] = aisle;
            }
        }
    }

    private List<Vector2Int> GetEdgeTiles(Vector2Int origin, Vector2Int size, int direction)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        switch (direction)
        {
            case 0:
                for (int x = 0; x < size.x; x++)
                {
                    tiles.Add(new Vector2Int(origin.x + x, origin.y + size.y - 1));
                }
                break;
            case 1:
                for (int x = 0; x < size.x; x++)
                {
                    tiles.Add(new Vector2Int(origin.x + x, origin.y));
                }
                break;
            case 2:
                for (int y = 0; y < size.y; y++)
                {
                    tiles.Add(new Vector2Int(origin.x, origin.y + y));
                }
                break;
            case 3:
                for (int y = 0; y < size.y; y++)
                {
                    tiles.Add(new Vector2Int(origin.x + size.x - 1, origin.y + y));
                }
                break;
        }

        return tiles;
    }

    private void ResetAisle()
    {
        _hasStartPos = false;
        _startRoom = null;
    }

    private void ClearWaitingAisle()
    {
        foreach (RoomViewModel aisle in _waitingAisle)
        {
            RemoveBuild(aisle);
        }

        _waitingAisle.Clear();
        CanConfirm = false;
    }

    private void RemoveBuild(RoomViewModel target)
    {
        for (int x = 0; x < target.Size.x; x++)
        {
            for (int y = 0; y < target.Size.y; y++)
            {
                Vector2Int tilePos = target.OriginPos + new Vector2Int(x, y);

                if (Builds.TryGetValue(tilePos, out RoomViewModel existVM) && existVM == target)
                {
                    Builds.Remove(tilePos);
                }

                UpdateNearConnection(tilePos);
            }
        }

        DestroyBuild = target;
    }

    public void UpdateRoomConnection(RoomViewModel room)
    {
        if (room == null || room.BuildType != BuildType.Room)
        {
            return;
        }

        if (room.DoorDataList == null || room.DoorDataList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            room.SetWallActive(i, false);
        }

        foreach (DoorData doorData in room.DoorDataList)
        {
            DoorInfo doorInfo = room.GetDoorInfo(doorData.Offset);

            if (Builds.TryGetValue(doorInfo.OutsidePos, out RoomViewModel targetVM))
            {
                if (targetVM == room)
                {
                    continue;
                }

                bool shouldConnect = false;

                if (targetVM.BuildType == BuildType.Aisle)
                {
                    shouldConnect = true;
                }
                else if (targetVM.BuildType == BuildType.Room)
                {
                    Vector2Int neighborDoorPos = targetVM.GetNearDoor(doorInfo.InsidePos);
                    shouldConnect = (doorInfo.OutsidePos == neighborDoorPos);
                }

                if (shouldConnect)
                {
                    room.SetWallActive(doorInfo.DirectionIndex, true);
                    targetVM.SetWallActive(GetOppositeDirection(doorInfo.DirectionIndex), true);
                    targetVM.Refresh();
                }
            }
        }

        room.Refresh();
    }

    public void UpdateConnection(Vector2Int current)
    {
        if (!Builds.TryGetValue(current, out RoomViewModel currentVM))
        {
            return;
        }

        if (currentVM.BuildType == BuildType.Room)
        {
            UpdateRoomConnection(currentVM);
            return;
        }

        for (int i = 0; i < _directions.Length; i++)
        {
            int oppDir = GetOppositeDirection(i);
            bool isConnected = false;

            List<Vector2Int> edgeTiles = GetEdgeTiles(currentVM.OriginPos, currentVM.Size, i);

            foreach (Vector2Int tile in edgeTiles)
            {
                Vector2Int targetPos = tile + _directions[i];

                if (Builds.TryGetValue(targetPos, out RoomViewModel targetVM) && targetVM != currentVM)
                {
                    bool shouldConnect = targetVM.BuildType == BuildType.Aisle || (targetVM.BuildType == BuildType.Room && targetVM.GetNearDoor(tile) == targetPos);

                    if (shouldConnect)
                    {
                        isConnected = true;
                        targetVM.SetWallActive(oppDir, true);
                        targetVM.Refresh();
                    }
                }
            }

            currentVM.SetWallActive(i, isConnected);
        }

        currentVM.Refresh();
    }

    public void UpdateNearConnection(Vector2Int pos)
    {
        foreach (Vector2Int dir in _directions)
        {
            Vector2Int target = pos + dir;

            if (Builds.TryGetValue(target, out RoomViewModel vm))
            {
                if (vm.BuildType == BuildType.Room)
                {
                    UpdateRoomConnection(vm);
                }
                else
                {
                    UpdateConnection(target);
                }
            }
        }
    }

    private int GetOppositeDirection(int direction)
    {
        switch (direction)
        {
            case 0: return 1;
            case 1: return 0;
            case 2: return 3;
            case 3: return 2;
            default: return 0;
        }
    }

    private bool CanPlaceRoom(Vector2Int pos, Vector2Int size)
    {
        if (pos.y + size.y > 10 || pos.y < -40 || pos.x < -60 || pos.x + size.x > 60)
        {
            return false;
        }

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkPos = pos + new Vector2Int(x, y);

                if (Builds.ContainsKey(checkPos))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void RegisterRoom(RoomViewModel room, Vector2Int pos)
    {
        for (int x = 0; x < room.Size.x; x++)
        {
            for (int y = 0; y < room.Size.y; y++)
            {
                Builds[pos + new Vector2Int(x, y)] = room;
            }
        }
    }
}