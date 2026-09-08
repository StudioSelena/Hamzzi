// 클릭 시 보상을 지급하고 일정 시간 미클릭 시 자동 소멸하는 보너스 씨앗
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BonusSeed : MonoBehaviour
{
    private const int RewardSeedAmount = 100;

    [SerializeField] private float _despawnDelaySec = 3f;

    private Camera _mainCamera;
    private float _elapsedTime;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _elapsedTime = 0f;
    }

    private void Update()
    {
        if (TryGetInputPosition(out Vector3 inputPosition) && TryCollectSeed(inputPosition))
        {
            return;
        }

        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _despawnDelaySec)
        {
            Despawn();
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

    private bool TryCollectSeed(Vector3 inputPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            //ServiceManager.Instance.UserService.AddSeed(RewardSeedAmount);
            var userVm = ServiceManager.Instance.UserService.GetUserViewModel();
            if(userVm != null)
            {
                userVm.AddSeed(RewardSeedAmount);
            }

            Despawn();
            return true;
        }

        return false;
    }

    private void Despawn()
    {
        GameObjectManager.Instance.RequestDestroyObject(gameObject);
    }
}