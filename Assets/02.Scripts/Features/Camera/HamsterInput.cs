using UnityEngine;
using UnityEngine.EventSystems;

public class HamsterInput : MonoBehaviour
{
    private Camera _mainCamera;
    private CameraController _cameraController;

    private void Start()
    {
        _mainCamera = Camera.main;
        _cameraController = _mainCamera.GetComponent<CameraController>();
    }

    private void Update()
    {
        if (UIManager.Instance.IsOpenUI(UIType.HousingUI) || UIManager.Instance.IsOpenUI(UIType.BuildUI))
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            CheckTouch(Input.mousePosition);
        }

#elif UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            CheckTouch(Input.GetTouch(0).position);
        }
#endif
    }

    private void CheckTouch(Vector2 screenPos)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform)
            {
                _cameraController.StartFollowHamster(transform);

                if (UIManager.Instance.IsOpenUI(UIType.InGameUI))
                {
                    UIBase uiBase = UIManager.Instance.GetOpenUI(UIRootType.MainUI, UIType.InGameUI);

                    if (uiBase is InGameUI inGameUI)
                    {
                        inGameUI.UpdateButton();
                    }
                }
            }
        }
    }
}