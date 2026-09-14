// 주기적으로 확률을 판정해 보너스 씨앗을 랜덤 위치에 스폰하는 매니저
using System.Collections.Generic;
using UnityEngine;

public class SeedClickBonusManager : SingletonBase<SeedClickBonusManager>
{
    private const string BonusSeedAddress = "BonusSeed";
    private const float SpawnCheckIntervalSec = 5f;
    private const float SpawnProbability = 0.5f;
    private const float SeedSpawnYOffset = 0.5f;
    private const float SeedOccupiedCheckRadius = 0.5f;
    private const string SeedLayerName = "Seed";

    [SerializeField] private Vector3 _spawnRangeMin;
    [SerializeField] private Vector3 _spawnRangeMax;

    private float _elapsedTime;
    private BuildViewModel _buildVM;
    private int _seedLayerMask;

    private void Start()
    {
        _buildVM = ServiceManager.Instance.BuildService.GetBuildViewModel();
        _seedLayerMask = LayerMask.GetMask(SeedLayerName);
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
        if (TryGetEmptyRoomSpawnPosition(out Vector3 spawnPosition))
        {
            GameObjectManager.Instance.CreateObject(BonusSeedAddress, BonusSeedAddress, spawnPosition);
        }
    }

    private bool TryGetEmptyRoomSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        List<Vector3> emptyRoomPositions = new List<Vector3>();

        foreach (var build in _buildVM.Builds)
        {
            RoomViewModel roomVM = build.Value;

            if (roomVM == null || roomVM.BuildType != BuildType.Room)
            {
                continue;
            }

            Vector3 center = GetRoomCenterWorldPosition(roomVM);
            Vector3 candidatePosition = new Vector3(center.x, center.y + SeedSpawnYOffset, center.z);

            if (IsSeedAlreadySpawned(candidatePosition))
            {
                continue;
            }

            emptyRoomPositions.Add(candidatePosition);
        }

        if (emptyRoomPositions.Count == 0)
        {
            return false;
        }

        int randomIndex = Random.Range(0, emptyRoomPositions.Count);
        spawnPosition = emptyRoomPositions[randomIndex];
        return true;
    }

    private bool IsSeedAlreadySpawned(Vector3 checkPosition)
    {
        return Physics.CheckSphere(checkPosition, SeedOccupiedCheckRadius, _seedLayerMask);
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