using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public struct AisleConnection
{
    public bool Up;
    public bool Down;
    public bool Left;
    public bool Right;
}

public class BuildService
{
    private static readonly Vector2Int[] _directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private static readonly Vector2Int[] _diagonalDirs = { new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(1, 1) };

    private const int MAX_SEARCH = 500;

    private BuildViewModel _buildVM;
    private List<AisleNavMeshLink> _aisleLinks = new List<AisleNavMeshLink>();

    public BuildViewModel GetBuildViewModel()
    {
        if (_buildVM == null)
        {
            CreateBuildViewModel();
        }

        return _buildVM;
    }

    public BuildViewModel CreateBuildViewModel()
    {
        _buildVM = new BuildViewModel();

        return _buildVM;
    }

    public List<Vector2Int> SearchBestPath(RoomViewModel startRoom, RoomViewModel endRoom, Dictionary<Vector2Int, RoomViewModel> room)
    {
        List<DoorData> startDoors = startRoom.DoorDataList;
        List<DoorData> endDoors = endRoom.DoorDataList;
        List<Vector2Int> bestPath = null;

        int minPathLength = int.MaxValue;

        foreach (DoorData startData in startDoors)
        {
            DoorInfo startInfo = startRoom.GetDoorInfo(startData.Offset);

            if (IsAreaRoom(startInfo.OutsidePos, new Vector2Int(2, 2), room))
            {
                continue;
            }

            foreach (DoorData endData in endDoors)
            {
                DoorInfo endInfo = endRoom.GetDoorInfo(endData.Offset);

                if (IsAreaRoom(endInfo.OutsidePos, new Vector2Int(2, 2), room))
                {
                    continue;
                }

                List<Vector2Int> path = GetAislePath(startInfo.OutsidePos, endInfo.OutsidePos, room);

                if (path != null && path.Count > 0)
                {
                    int currentPath = CalculatePath(path, room);

                    if (currentPath < minPathLength)
                    {
                        minPathLength = currentPath;
                        bestPath = path;
                    }
                }
            }
        }

        return bestPath;
    }

    public List<Vector2Int> GetAislePath(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, RoomViewModel> room)
    {
        if (IsAreaRoom(start, new Vector2Int(2, 2), room) || IsAreaRoom(end, new Vector2Int(2, 2), room))
        {
            return null;
        }

        if (start == end)
        {
            return new List<Vector2Int> { start };
        }

        Dictionary<Vector2Int, int> dist = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        List<Vector2Int> open = new List<Vector2Int>();

        dist[start] = 0;
        open.Add(start);

        bool isFind = false;
        int searchCount = 0;

        while (open.Count > 0 && searchCount < MAX_SEARCH)
        {
            searchCount++;

            int minIdx = 0;

            int minScore = dist[open[0]] + CalculateDistance(open[0], end);

            for (int i = 1; i < open.Count; i++)
            {
                int score = dist[open[i]] + CalculateDistance(open[i], end);

                if (score < minScore)
                {
                    minScore = score;
                    minIdx = i;
                }
            }

            Vector2Int current = open[minIdx];

            open.RemoveAt(minIdx);

            if (current == end)
            {
                isFind = true;
                break;
            }

            foreach (Vector2Int dir in _directions)
            {
                Vector2Int next = current + dir;

                if (IsAreaRoom(next, new Vector2Int(2, 2), room) || IsNearRoomCorner(next, room))
                {
                    continue;
                }

                int move = 100;

                if (room.TryGetValue(next, out RoomViewModel roomVM) && roomVM.BuildType == BuildType.Aisle)
                {
                    move = 1;
                }

                int newDis = dist[current] + move;

                if (!dist.ContainsKey(next) || newDis < dist[next])
                {
                    dist[next] = newDis;
                    parent[next] = current;

                    if (!open.Contains(next))
                    {
                        open.Add(next);
                    }
                }
            }
        }

        if (!isFind)
        {
            return null;
        }

        List<Vector2Int> path = new List<Vector2Int>();

        Vector2Int curr = end;

        int count = 0;
        int maxCount = 1000;

        while (curr != start && count < maxCount)
        {
            path.Add(curr);

            if (!parent.TryGetValue(curr, out curr))
            {
                return null;
            }

            count++;
        }

        if (count >= maxCount)
        {
            return null;
        }

        path.Add(start);

        return path;
    }

    private int CalculatePath(List<Vector2Int> path, Dictionary<Vector2Int, RoomViewModel> room)
    {
        int total = 0;

        foreach (Vector2Int pos in path)
        {
            if (room.TryGetValue(pos, out RoomViewModel vm) && vm.BuildType == BuildType.Aisle)
            {
                total += 1;
            }
            else
            {
                total += 100;
            }
        }

        return total;
    }

    private bool IsAreaRoom(Vector2Int pos, Vector2Int size, Dictionary<Vector2Int, RoomViewModel> roomMap)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (roomMap.TryGetValue(pos + new Vector2Int(x, y), out RoomViewModel vm) && vm.BuildType == BuildType.Room)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsNearRoomCorner(Vector2Int pos, Dictionary<Vector2Int, RoomViewModel> room)
    {
        foreach (Vector2Int dir in _diagonalDirs)
        {
            Vector2Int checkPos = pos + dir;

            if (IsAreaRoom(checkPos, Vector2Int.one, room))
            {
                bool nearRoomX = IsAreaRoom(pos + new Vector2Int(dir.x, 0), Vector2Int.one, room);
                bool nearRoomY = IsAreaRoom(pos + new Vector2Int(0, dir.y), Vector2Int.one, room);

                if (!nearRoomX && !nearRoomY)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private int CalculateDistance(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 100;
    }

    public void RefreshAisleNavMesh(Dictionary<Vector2Int, RoomViewModel> room)
    {
        List<RoomViewModel> aisleList = new List<RoomViewModel>();

        foreach (var pair in room)
        {
            if (pair.Value.BuildType == BuildType.Aisle && !aisleList.Contains(pair.Value))
            {
                aisleList.Add(pair.Value);
            }
        }

        ClearAisleLink();

        if (aisleList.Count == 0)
        {
            return;
        }

        List<float> aisles = new List<float>();

        foreach (RoomViewModel aisle in aisleList)
        {
            if (!aisles.Contains(aisle.OriginPos.x))
            {
                aisles.Add(aisle.OriginPos.x);
            }
        }

        float endOffsetY = 0.2f;
        float offsetZ = -0.5f;
        int linkIndex = 0;

        foreach (float x in aisles)
        {
            List<RoomViewModel> sortedGroup = new List<RoomViewModel>();

            foreach (RoomViewModel aisle in aisleList)
            {
                if (Mathf.Abs(aisle.OriginPos.x - x) < 0.01f)
                {
                    sortedGroup.Add(aisle);
                }
            }

            sortedGroup.Sort((a, b) => a.OriginPos.y.CompareTo(b.OriginPos.y));

            if (sortedGroup.Count == 0)
            {
                continue;
            }

            for (int i = 0; i < sortedGroup.Count - 1; i++)
            {
                RoomViewModel current = sortedGroup[i];
                RoomViewModel next = sortedGroup[i + 1];

                float currentTop = current.OriginPos.y + current.Size.y;

                if (Mathf.Abs(currentTop - next.OriginPos.y) > 0.01f)
                {
                    continue;
                }

                float startX = current.OriginPos.x + (current.Size.x * 0.5f);
                float startY = current.OriginPos.y + (current.Size.y * 0.5f);

                Vector3 startWorldPos = new Vector3(startX, startY, 9f + offsetZ);

                float endX = next.OriginPos.x + (next.Size.x * 0.5f);
                float endY = next.OriginPos.y + (next.Size.y * 0.5f) + endOffsetY;

                Vector3 endWorldPos = new Vector3(endX, endY, 9f + offsetZ);

                CreateLink(startWorldPos, endWorldPos, linkIndex++).Forget();
            }
        }

        NavigationManager.Instance.BuildNav();
    }

    private void ClearAisleLink()
    {
        foreach (AisleNavMeshLink link in _aisleLinks)
        {
            GameObjectManager.Instance.RequestDestroyObject(link.gameObject);
        }

        _aisleLinks.Clear();
    }

    private async UniTask CreateLink(Vector3 startWorldPos, Vector3 endWorldPos, int index)
    {
        GameObject prefab = await GameObjectManager.Instance.CreateObjectAsync($"AisleLink{index}", "Prefabs/Housing/AisleLink", Vector3.zero);
        AisleNavMeshLink aisleNavLink = prefab.GetComponent<AisleNavMeshLink>();

        Vector3 startPos = new Vector3(startWorldPos.x, startWorldPos.y - 1.0f, startWorldPos.z);
        prefab.transform.position = startPos;
        Vector3 endPos = new Vector3(endWorldPos.x, startPos.y + 2.0f, endWorldPos.z);

        aisleNavLink.SetPosition(startPos, endPos);

        _aisleLinks.Add(aisleNavLink);
    }
}