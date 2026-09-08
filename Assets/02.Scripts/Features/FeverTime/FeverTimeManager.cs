// 쳇바퀴 피버타임 미니게임 상태와 진행을 관리하는 매니저
using System;
using UnityEngine;

public enum FeverTimeState
{
    None = 0,
    Idle,
    Ready,
    CutscenePlaying,
    TapInput,
    Result
}

public class FeverTimeManager : SingletonBase<FeverTimeManager>
{
    public event Action OnFeverTimeEnded;

    private FeverTimeState _currentState;

    private int _tapCount;
    private float _tapInputElapsedTime;

    private HamsterData _currentHamsterData;
    private FeverTimeData _currentFeverTimeData;

    private int _rewardSeedCount;

    private void Start()
    {
        GameDataManager.Instance.LoadData<FeverTimeData>();
    }

    private void Update()
    {
        if (_currentState != FeverTimeState.TapInput || _currentFeverTimeData == null)
        {
            return;
        }

        _tapInputElapsedTime += Time.deltaTime;

        if (_tapInputElapsedTime >= _currentFeverTimeData.TapDurationSec)
        {
            SetState(FeverTimeState.Result);
        }
    }

    public void RegisterTap()
    {
        if (_currentState != FeverTimeState.TapInput)
        {
            return;
        }

        _tapCount++;

#if UNITY_EDITOR
        Debug.Log($"연타 카운트: {_tapCount}");
#endif
    }

    public void SetState(FeverTimeState state)
    {
        if (_currentState == state)
        {
            return;
        }

        _currentState = state;

        switch (_currentState)
        {
            case FeverTimeState.Idle:
                HandleIdleState();
                break;
            case FeverTimeState.Ready:
                HandleReadyState();
                break;
            case FeverTimeState.CutscenePlaying:
                HandleCutscenePlayingState();
                break;
            case FeverTimeState.TapInput:
                HandleTapInputState();
                break;
            case FeverTimeState.Result:
                HandleResultState();
                break;
        }
    }

    private void HandleIdleState()
    {
#if UNITY_EDITOR
        Debug.Log("FeverTimeState: Idle");
#endif
        UIManager.Instance.CloseFeverTimeCutsceneUI();

        _currentHamsterData = null;
        _currentFeverTimeData = null;

        OnFeverTimeEnded?.Invoke();
    }

    private void HandleReadyState()
    {
#if UNITY_EDITOR
        Debug.Log("FeverTimeState: Ready");
#endif
    }

    private void HandleCutscenePlayingState()
    {
#if UNITY_EDITOR
        Debug.Log("FeverTimeState: CutscenePlaying");
#endif
        UIManager.Instance.OpenFeverTimeCutsceneUI();

        // TODO: 실제 영상 에셋 연결되면 영상 재생이 끝나는 시점(VideoPlayer 재생 완료 콜백)에 아래 호출로 교체
        SetState(FeverTimeState.TapInput);
    }

    private void HandleTapInputState()
    {
#if UNITY_EDITOR
        Debug.Log("FeverTimeState: TapInput");
#endif
        _tapCount = 0;
        _tapInputElapsedTime = 0f;
    }

    private void HandleResultState()
    {
        _rewardSeedCount = _tapCount * _currentFeverTimeData.SeedPerTap;
        UIManager.Instance.OpenFeverTimeResultUI();

#if UNITY_EDITOR
        Debug.Log($"FeverTimeState: Result, 보상 {_rewardSeedCount} 지급 (연타 {_tapCount}회 x {_currentFeverTimeData.SeedPerTap})");
#endif

    }

    public void ClaimReward()
    {
        if(_rewardSeedCount <= 0)
        {
            return;
        }

        var userVm = ServiceManager.Instance.UserService.GetUserViewModel();
        if (userVm != null)
        {
            userVm.AddSeed(_rewardSeedCount);
        }

#if UNITY_EDITOR
        Debug.Log($"피버타임 보상 수령: +{_rewardSeedCount}");
#endif

        _rewardSeedCount = 0;
        SetState(FeverTimeState.Idle);
    }

    // 피버타임 시작 메서드
    public void StartFeverTime(HamsterData hamsterData = null)
    {
        if (hamsterData == null)
        {
#if UNITY_EDITOR
            Debug.Log($"{hamsterData} / 할당된 햄스터 없음 / 피버타임 불가");
#endif
            return;
        }

        _currentHamsterData = hamsterData;
        _currentFeverTimeData = GameDataManager.Instance.GetData<FeverTimeData>(hamsterData.HamsterTier.ToString());

        SetState(FeverTimeState.CutscenePlaying);

#if UNITY_EDITOR
        Debug.Log($"HamsterData: {hamsterData.Name} / {_currentFeverTimeData})");
#endif
    }

    public int GetRewardSeedCount()
    {
        return _rewardSeedCount;
    }

    public FeverTimeData GetCurrentFeverTimeData()
    {
        return _currentFeverTimeData;
    }

    public float GetTapInputElapsedTime()
    {
        return _tapInputElapsedTime;
    }
  
}