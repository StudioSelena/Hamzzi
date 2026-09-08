// 주기적으로 확률을 판정해 보너스 씨앗을 랜덤 위치에 스폰하는 매니저
using System.Collections.Generic;
using UnityEngine;

public class SeedClickBonusManager : SingletonBase<SeedClickBonusManager>
{
    private const string BonusSeedAddress = "BonusSeed";
    private const float SpawnCheckIntervalSec = 5f;
    private const float SpawnProbability = 0.5f;

    [SerializeField] private Vector3 _spawnRangeMin;
    [SerializeField] private Vector3 _spawnRangeMax;

    private float _elapsedTime;
    private BuildViewModel _buildVM;

    private void Start()
    {
        _buildVM = ServiceManager.Instance.BuildService.GetBuildViewModel();
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime < SpawnCheckIntervalSec)
        {
            return;
        }

        _elapsedTime = 0f;

        if (Random.value < SpawnProbability)
        {
            SpawnBonusSeed();
        }
    }

    private void SpawnBonusSeed()
    {
        if (TryGetRandomRoomCenter(out Vector3 spawnPosition))
        {
            GameObjectManager.Instance.CreateObject(BonusSeedAddress, BonusSeedAddress, spawnPosition);
        }
    }

    private bool TryGetRandomRoomCenter(out Vector3 roomCenterPos)
    {
        roomCenterPos = Vector3.zero;

        List<RoomViewModel> validRooms = new List<RoomViewModel>();

        foreach (var build in _buildVM.Builds)
        {
            RoomViewModel roomVM = build.Value;

            if (roomVM != null && roomVM.BuildType == BuildType.Room)
            {
                validRooms.Add(roomVM);
            }
        }

        if (validRooms.Count > 0)
        {
            int randomIndex = Random.Range(0, validRooms.Count);
            RoomViewModel selectedRoom = validRooms[randomIndex];

            Vector3 center = GetRoomCenterWorldPosition(selectedRoom);
            roomCenterPos = new Vector3(center.x, center.y + 0.5f, center.z);
            return true;
        }

        return false;
    }

    private Vector3 GetRoomCenterWorldPosition(RoomViewModel roomVM)
    {
        float cellSize = 1.0f;
        float yOffset = 3.5f;
        float subCellSize = cellSize / roomVM.GridFactor;

        float roomX = (roomVM.OriginPos.x * cellSize) + (roomVM.SubGridSize.x * subCellSize * 0.5f);
        float floorY = (roomVM.OriginPos.y + yOffset) * cellSize;
        float roomZ = 9.0f - (roomVM.SubGridSize.y * subCellSize * 0.5f) + 0.3f;

        return new Vector3(roomX, floorY, roomZ);
    }
}