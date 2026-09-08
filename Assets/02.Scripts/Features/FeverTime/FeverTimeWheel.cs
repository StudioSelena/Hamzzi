// 쳇바퀴에 부착되어 일정 시간마다 피버타임 트리거를 발생시키는 컴포넌트
using UnityEngine;
using UnityEngine.EventSystems;

public class FeverTimeWheel : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_Effect;

    private float _elapsedTime; //쳇바퀴가 마지막으로 리셋된 뒤부터 지금까지 흐른 시간을 초단위로 저장
    private bool _isReady;
    private bool _isFeverInProgress;

    private HamsterData _currentHamsterData;
    private float _triggerIntervalSec;

    private void OnEnable()
    {
        FeverTimeManager.Instance.OnFeverTimeEnded += ResetTimerForNextFever;
        UpdateSparkleEffect();
    }

    private void OnDisable()
    {
        FeverTimeManager.Instance.OnFeverTimeEnded -= ResetTimerForNextFever;
    }

    private void Update()
    {
        if (_currentHamsterData == null)
        {
            return;
        }

        if (_isReady)
        {
            CheckTouchInput();
            return;
        }

        if (_isFeverInProgress)
            return;

        UpdateTimer();
    }

    private void UpdateTimer()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _triggerIntervalSec)
        {
            SetReadyState();
        }
    }

    private void SetReadyState()
    {
        _isReady = true;
        UpdateSparkleEffect();
    }

    private void CheckTouchInput()
    {
        if (!TryGetInputPosition(out Vector3 inputPosition))
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            OnWheelTouched();
        }
    }

    private bool TryGetInputPosition(out Vector3 inputPosition)
    {
        inputPosition = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            inputPosition = Input.mousePosition;
            return true;
        }
#else
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return false;
        }

        if (touch.phase == TouchPhase.Began)
        {
            inputPosition = touch.position;
            return true;
        }
    }
#endif

        return false;
    }

    private void OnWheelTouched()
    {
        _isReady = false;
        _isFeverInProgress = true;
        UpdateSparkleEffect();

        FeverTimeManager.Instance.StartFeverTime(_currentHamsterData);
    }

    private void ResetTimerForNextFever()
    {
        _isFeverInProgress = false;
        _elapsedTime = 0f;
        _isReady = false;
        UpdateSparkleEffect();
    }

    public void SetHamster(HamsterData hamsterData)
    {
        _currentHamsterData = hamsterData;
        _isReady = false;
        _elapsedTime = 0f;
        UpdateSparkleEffect();

        if (_currentHamsterData != null)
        {
            FeverTimeData feverData = GameDataManager.Instance.GetData<FeverTimeData>(_currentHamsterData.HamsterTier.ToString());
            _triggerIntervalSec = feverData.TriggerIntervalSec;
        }
    }

    private void UpdateSparkleEffect()
    {
        Prefab_Effect.SetActive(_isReady);
    }
}