// 도감에 등록된 햄스터를 맵(정원)에 동적으로 생성하는 매니저
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;

public class HamsterManager : SingletonBase<HamsterManager>
{
    private const string HamsterPrefabAddress = "Hamster/Hamster_00";
    private const float MinMeasureSeconds = 30f;
    private const float BaseIdleDurationSeconds = 3f;
    private const float BaseFarmDurationSeconds = 2f;
    private const string IdleDurationVariableName = "IdleDuration";
    private const string FarmDurationVariableName = "FarmDuration";

    [SerializeField] private Vector3 _gardenSpawnRangeMin;
    [SerializeField] private Vector3 _gardenSpawnRangeMax;

    private CollectionViewModel _collectionViewModel;
    private Dictionary<long, GameObject> _spawnedHamsterObjectDict = new Dictionary<long, GameObject>();
    private int _collectionGeneration = 0;

    public float TotalCollectSpeedPerSec { get; private set; }
    private int _collectCount = 0;
    private float _measureStartTime = 0f;

    public bool IsCurrentCollectionMine()
    {
        long myUserUid = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        CollectionViewModel myCollectionViewModel = ServiceManager.Instance.CollectionService.GetCollectionViewModel(myUserUid);

        if (myCollectionViewModel == null)
        {
            return false;
        }

        return myCollectionViewModel == ServiceManager.Instance.CollectionService.GetCurrentCollectionViewModel();
    }
    public void Init()
    {
        NetworkCollectionService collectionService = ServiceManager.Instance.CollectionService;

        collectionService.OnChangedCurrentCollectionViewModel -= ChangedCollectionViewModel;
        collectionService.OnChangedCurrentCollectionViewModel += ChangedCollectionViewModel;

        if (_collectionViewModel != null)
        {
            _collectionViewModel.ContainerPropertyChanged -= OnContainerPropertyChanged;
        }

        _collectionViewModel = collectionService.GetCurrentCollectionViewModel();

        if (_collectionViewModel == null)
        {
            return;
        }

        _collectionViewModel.ContainerPropertyChanged += OnContainerPropertyChanged;

        SyncCollectedHamsters();
    }

    private void ChangedCollectionViewModel()
    {
        RemoveAllSpawnedHamsters();
        _collectionGeneration++;

        if (_collectionViewModel != null)
        {
            _collectionViewModel.ContainerPropertyChanged -= OnContainerPropertyChanged;
        }

        _collectionViewModel = ServiceManager.Instance.CollectionService.GetCurrentCollectionViewModel();

        if (_collectionViewModel == null)
        {
            return;
        }

        _collectionViewModel.ContainerPropertyChanged += OnContainerPropertyChanged;

        SyncCollectedHamsters();
    }

    private void OnDestroy()
    {
        if (_collectionViewModel != null)
        {
            _collectionViewModel.ContainerPropertyChanged -= OnContainerPropertyChanged;
        }

        ServiceManager serviceManager = ServiceManager.Instance;

        if (serviceManager != null && serviceManager.CollectionService != null)
        {
            serviceManager.CollectionService.OnChangedCurrentCollectionViewModel -= ChangedCollectionViewModel;
        }
    }

    private void OnContainerPropertyChanged(string propertyName, ContainerEventType eventType, string id)
    {
        if (propertyName != nameof(_collectionViewModel.CollectedHamsterIdList))
        {
            return;
        }

        if (eventType != ContainerEventType.Add)
        {
            return;
        }

        SyncCollectedHamsters();
    }

    private void SyncCollectedHamsters()
    {
        foreach (HamsterSave hamsterSave in _collectionViewModel.CollectedHamsterList.Values)
        {
            if (_spawnedHamsterObjectDict.ContainsKey(hamsterSave.HamsterUID))
            {
                continue;
            }

            _spawnedHamsterObjectDict.Add(hamsterSave.HamsterUID, null);
            SpawnHamster(hamsterSave);
        }

        ResetCollectMeasure();
    }

    private void SpawnHamster(HamsterSave hamsterSave)
    {
        SpawnHamsterAsync(hamsterSave, _collectionGeneration).Forget();
    }

    private async UniTaskVoid SpawnHamsterAsync(HamsterSave hamsterSave, int requestedGeneration)
    {
        Vector3 spawnSpot = GetRandomGardenSpawnPosition();

        GameObject hamsterObject = await GameObjectManager.Instance.CreateObjectAsync(hamsterSave.HamsterUID.ToString(), HamsterPrefabAddress, spawnSpot);

        if (requestedGeneration != _collectionGeneration)
        {
            if (hamsterObject != null)
            {
                GameObjectManager.Instance.RequestDestroyObject(hamsterObject);
            }

            return;
        }

        if (hamsterObject == null)
        {
            _spawnedHamsterObjectDict.Remove(hamsterSave.HamsterUID);
            return;
        }

        NavMeshAgent agent = hamsterObject.GetComponent<NavMeshAgent>();
        agent.enabled = false;

        HamsterForm hamsterForm = hamsterObject.GetComponent<HamsterForm>();
        if (hamsterForm == null)
        {
            _spawnedHamsterObjectDict.Remove(hamsterSave.HamsterUID);
            return;
        }

        hamsterForm.SetBodyMesh(hamsterSave.HamsterId);
        hamsterForm.SetFaceMesh(hamsterSave.FaceId);

        agent.enabled = true;
        SetHamsterCollectCycle(hamsterObject, hamsterSave.HamsterId);

        _spawnedHamsterObjectDict[hamsterSave.HamsterUID] = hamsterObject;
    }

    // 햄스터의 씨앗 수집 능력에 맞춰 BT의 채집 사이클 시간을 설정한다
    private void SetHamsterCollectCycle(GameObject hamsterObject, string hamsterId)
    {
        HamsterData hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);

        if (hamsterData == null)
        {
            return;
        }

        BehaviorGraphAgent behaviorAgent = hamsterObject.GetComponent<BehaviorGraphAgent>();

        if (behaviorAgent == null)
        {
            return;
        }

        float idleDuration = GameUtil.CalculateCollectCycleSeconds(BaseIdleDurationSeconds, hamsterData.CollectSpeed);
        float farmDuration = GameUtil.CalculateCollectCycleSeconds(BaseFarmDurationSeconds, hamsterData.CollectSpeed);

        bool isIdleSet = behaviorAgent.SetVariableValue(IdleDurationVariableName, idleDuration);
        bool isFarmSet = behaviorAgent.SetVariableValue(FarmDurationVariableName, farmDuration);

#if UNITY_EDITOR
        if (isIdleSet == false || isFarmSet == false)
        {
            Debug.LogError($"[채집 주기] Blackboard 변수 이름을 찾지 못했습니다. Idle={isIdleSet} Farm={isFarmSet}");
        }
        else
        {
            Debug.Log($"[채집 주기] {hamsterData.Name} 배율 {hamsterData.CollectSpeed} → Idle {idleDuration:F2}초 / Farm {farmDuration:F2}초");
        }
#endif
    }

    private Vector3 GetRandomGardenSpawnPosition()
    {
        return new Vector3(
            Random.Range(_gardenSpawnRangeMin.x, _gardenSpawnRangeMax.x),
            Random.Range(_gardenSpawnRangeMin.y, _gardenSpawnRangeMax.y),
            Random.Range(_gardenSpawnRangeMin.z, _gardenSpawnRangeMax.z));
    }

    private void RemoveAllSpawnedHamsters()
    {
        foreach (GameObject hamsterObject in _spawnedHamsterObjectDict.Values)
        {
            if (hamsterObject == null)
            {
                continue;
            }

            GameObjectManager.Instance.RequestDestroyObject(hamsterObject);
        }

        _spawnedHamsterObjectDict.Clear();
    }

    // BT가 씨앗을 채집할 때마다 호출해 측정 구간의 채집 횟수를 누적한다
    public void AddCollectCount()
    {
        _collectCount++;
    }

    // 측정 구간의 실제 채집 횟수로 초당 채집량을 갱신하고 구간을 새로 연다
    // 구간이 너무 짧으면 갱신하지 않고 구간을 이어서 누적한다 (열자마자 게임을 종료하면 큰 값으로 갱신되는 것을 막는다)
    public void RefreshTotalCollectSpeedPerSec()
    {
        float elapsedSeconds = Time.time - _measureStartTime;

        if (elapsedSeconds < MinMeasureSeconds)
        {
#if UNITY_EDITOR
            Debug.Log($"[초당 채집량] 측정 구간 {elapsedSeconds}초로 부족. 갱신 스킵");
#endif
            return;
        }

        TotalCollectSpeedPerSec = _collectCount / elapsedSeconds;

#if UNITY_EDITOR
        Debug.Log($"[초당 채집량] 구간 {elapsedSeconds}초 / 채집 {_collectCount}회 → 초당 {TotalCollectSpeedPerSec}");
#endif

        ResetCollectMeasure();
    }

    // 측정 구간만 새로 연다. 컬렉션이 바뀌어 이전 구간을 쓸 수 없을 때 호출한다
    public void ResetCollectMeasure()
    {
        _collectCount = 0;
        _measureStartTime = Time.time;
    }
}