using Cysharp.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera Camera_Main;
    [SerializeField] private LayerMask _priorityLayerMask;

    [Header("기본 시야")]
    [SerializeField] private Vector3 Position_Overview = new Vector3(3f, 4f, -10);
    [SerializeField] private Vector3 Rotation_Overview = Vector3.zero;
    [SerializeField] private float Size_Ortho = 8f;

    [Header("정원 시야")]
    [SerializeField] private Vector3 Position_Garden = new Vector3(-5f, 10f, -10f);
    [SerializeField] private Vector3 Rotation_Garden = new Vector3(30f, -45f, 0f);
    [SerializeField] private float Garden_FOV = 50f;

    [Header("줌")]
    [SerializeField] private Vector3 Zoom_Angle = new Vector3(30f, -45f, 0f);
    [SerializeField] private float Zoom_Distance = 7f;
    [SerializeField] private float Zoom_FOV = 45f;

    [Header("조작감")]
    [SerializeField] private float Zoom_Sensitive = 0.01f;
    [SerializeField] private float Size_MinOrtho = 4f;
    [SerializeField] private float Size_MaxOrtho = 12f;
    [SerializeField] private float FOV_Min = 30f;
    [SerializeField] private float FOV_Max = 75f;
    [SerializeField] private float Duration = 0.8f;
    [SerializeField] private Vector2 Bound_Min = new Vector2(-10f, -5f);
    [SerializeField] private Vector2 Bound_Max = new Vector2(20f, 15f);

    private HousingViewModel _housingVM;
    private BuildViewModel _buildVM;
    private CancellationTokenSource _zoomCancel;
    private bool _isTransition = false;
    private bool _isViewRoom = false;

    private Transform _targetHamster;
    public bool IsFollowing { get; private set; } = false;

    private void Start()
    {
        HousingViewModel housingVM = ServiceManager.Instance.HousingService?.GetHousingViewModel();
        BuildViewModel buildVM = ServiceManager.Instance.BuildService?.GetBuildViewModel();

        BindViewModel(housingVM, buildVM);

        _housingVM.CurrentViewMode = HousingViewMode.Garden;
        ShowGardenView().Forget();
    }

    private void Update()
    {
        if (IsFollowing)
        {
            return;
        }

        if (_housingVM.CurrentViewMode == HousingViewMode.OverView && _buildVM.SelectType == BuildType.None)
        {
            CheckNormalRoom();
        }

        if (_buildVM.CanConfirm || _housingVM.TargetRoom != null || _housingVM.FurnitureVM != null)
        {
            return;
        }

        if (!_isTransition)
        {
            if (Input.touchCount > 0)
            {
                GetTouchInput();
            }
            else
            {
                GetMouseInput();
            }
        }
    }

    private void LateUpdate()
    {
        if (_buildVM.SelectType != BuildType.None)
        {
            StopFollowHamster();
        }

        if (IsFollowing && _targetHamster != null)
        {
            if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
            {
                Quaternion gardenRot = Quaternion.Euler(Rotation_Garden);
                Vector3 targetPos = _targetHamster.position - (gardenRot * Vector3.forward * 5f) + new Vector3(0f, 2f, -1.5f);

                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
                transform.rotation = Quaternion.Lerp(transform.rotation, gardenRot, Time.deltaTime * 5f);
            }
            else
            {
                Vector3 targetPos = new Vector3(_targetHamster.position.x, _targetHamster.position.y, Position_Overview.z);
                Quaternion overviewRot = Quaternion.Euler(Rotation_Overview);

                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
                transform.rotation = Quaternion.Lerp(transform.rotation, overviewRot, Time.deltaTime * 5f);

                if (Camera_Main.orthographic)
                {
                    Camera_Main.orthographicSize = Mathf.Lerp(Camera_Main.orthographicSize, 3.5f, Time.deltaTime * 5f);
                }
            }
        }
    }

    public void BindViewModel(HousingViewModel housingVM, BuildViewModel buildVM)
    {
        _housingVM = housingVM;
        _buildVM = buildVM;

        _housingVM.PropertyChanged += OnPropertyChanged_VM;
    }

    private void OnDestroy()
    {
        if(_housingVM != null)
        {
            _housingVM.PropertyChanged -= OnPropertyChanged_VM;
        }
    }

    private void OnPropertyChanged_VM(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_housingVM.TargetRoom):
                if (_housingVM.TargetRoom != null)
                {
                    _isViewRoom = true;
                    Vector3 roomCenterWorld = GetRoomCenterPos(_housingVM.TargetRoom);
                    FocusRoom(roomCenterWorld).Forget();
                }
                else
                {
                    _isViewRoom = false;

                    if (_housingVM.CurrentViewMode != HousingViewMode.Garden)
                    {
                        ShowOverview().Forget();
                    }
                }
                break;

            case nameof(_housingVM.CurrentViewMode):
                StopFollowHamster();

                if (_housingVM.CurrentViewMode == HousingViewMode.OverView)
                {
                    if (_housingVM.TargetRoom != null)
                    {
                        _housingVM.TargetRoom = null;
                        return;
                    }
                }

                if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
                {
                    ShowGardenView().Forget();
                }
                else if (_housingVM.CurrentViewMode == HousingViewMode.OverView)
                {
                    ShowOverview().Forget();
                }
                break;
        }
    }

    private void GetTouchInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (prevPos0 - prevPos1).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            ApplyZoom(difference * Zoom_Sensitive);
        }
        else if (Input.touchCount == 1)
        {
            if (_isViewRoom)
            {
                return;
            }

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
                {
                    delta.y = 0f;
                }

                float factor = Camera_Main.orthographic ? (Camera_Main.orthographicSize * 2f) / Screen.height : (Camera_Main.fieldOfView * 0.1f) / Screen.height;

                Vector3 targetPos;

                if (_housingVM != null && _housingVM.CurrentViewMode == HousingViewMode.Garden)
                {
                    Vector3 move = Vector3.right * (-delta.x * factor * 5f);
                    targetPos = Camera_Main.transform.position + move;
                    targetPos.y = Position_Garden.y;
                    targetPos.z = Position_Garden.z;
                }
                else
                {
                    Vector3 move = (-Camera_Main.transform.right * delta.x - Camera_Main.transform.up * delta.y) * factor;
                    targetPos = Camera_Main.transform.position + move;
                    targetPos.y = Mathf.Clamp(targetPos.y, Bound_Min.y, Bound_Max.y);
                }

                targetPos.x = Mathf.Clamp(targetPos.x, Bound_Min.x, Bound_Max.x);

                Camera_Main.transform.position = targetPos;
            }
        }
    }

    private void GetMouseInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.001f)
        {
            ApplyZoom(scroll * Zoom_Sensitive * 100f);
        }

        if (Input.GetMouseButton(1))
        {
            if (_isViewRoom)
            {
                return;
            }

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
            {
                mouseY = 0f;
            }

            float factor = Camera_Main.orthographic ? (Camera_Main.orthographicSize * 2f) / Screen.height : (Camera_Main.fieldOfView * 0.1f) / Screen.height;

            Vector3 targetPos;

            if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
            {
                Vector3 move = Vector3.right * (-mouseX * factor * 5f);
                targetPos = Camera_Main.transform.position + move;
                targetPos.y = Position_Garden.y;
                targetPos.z = Position_Garden.z;
            }
            else
            {
                Vector3 move = (-Camera_Main.transform.right * mouseX * factor * 150f - Camera_Main.transform.up * mouseY * factor * 150f);
                targetPos = Camera_Main.transform.position + move;
                targetPos.y = Mathf.Clamp(targetPos.y, Bound_Min.y, Bound_Max.y);
            }

            targetPos.x = Mathf.Clamp(targetPos.x, Bound_Min.x, Bound_Max.x);

            Camera_Main.transform.position = targetPos;
        }
    }

    private void ApplyZoom(float delta)
    {
        if (Camera_Main.orthographic)
        {
            Camera_Main.orthographicSize -= delta;
            Camera_Main.orthographicSize = Mathf.Clamp(Camera_Main.orthographicSize, Size_MinOrtho, Size_MaxOrtho);
        }
        else
        {
            Camera_Main.fieldOfView -= delta;
            Camera_Main.fieldOfView = Mathf.Clamp(Camera_Main.fieldOfView, FOV_Min, FOV_Max);
        }
    }

    public async UniTask ShowGardenView()
    {
        _isViewRoom = false;
        CancelZoom();
        _zoomCancel = new CancellationTokenSource();

        Vector3 targetPos = Position_Garden;
        Quaternion targetRotation = Quaternion.Euler(Rotation_Garden);

        Matrix4x4 startMatrix = Camera_Main.projectionMatrix;
        Matrix4x4 targetMatrix = Matrix4x4.Perspective(Garden_FOV, Camera_Main.aspect, Camera_Main.nearClipPlane, Camera_Main.farClipPlane);

        await TransitionCamera(targetPos, targetRotation, startMatrix, targetMatrix, false, Garden_FOV, _zoomCancel.Token);
    }

    public async UniTask ShowOverview(bool moveCamera = true)
    {
        _isViewRoom = false;
        _zoomCancel?.Cancel();
        _zoomCancel = new CancellationTokenSource();

        Vector3 targetPos = moveCamera ? Position_Overview : Camera_Main.transform.position;
        Quaternion targetRot = moveCamera ? Quaternion.Euler(Rotation_Overview) : Camera_Main.transform.rotation;

        Matrix4x4 startMatrix = Camera_Main.projectionMatrix;
        Matrix4x4 targetMatrix = Matrix4x4.Ortho(-Size_Ortho * Camera_Main.aspect, Size_Ortho * Camera_Main.aspect, -Size_Ortho, Size_Ortho, Camera_Main.nearClipPlane, Camera_Main.farClipPlane);

        float currentDuration = moveCamera ? Duration : 0f;

        await TransitionCamera(targetPos, targetRot, startMatrix, targetMatrix, true, 0f, _zoomCancel.Token);
    }

    private async UniTask FocusRoom(Vector3 roomCenter)
    {
        CancelZoom();
        _zoomCancel = new CancellationTokenSource();

        Quaternion targetRotation = Quaternion.Euler(Zoom_Angle);
        Vector3 targetPos = roomCenter - (targetRotation * Vector3.forward * Zoom_Distance);

        Matrix4x4 startMatrix = Camera_Main.projectionMatrix;
        Matrix4x4 targetMatrix = Matrix4x4.Perspective(Zoom_FOV, Camera_Main.aspect, Camera_Main.nearClipPlane, Camera_Main.farClipPlane);

        await TransitionCamera(targetPos, targetRotation, startMatrix, targetMatrix, false, Zoom_FOV, _zoomCancel.Token);
    }

    public void FocusRoomInGame(RoomViewModel roomVM)
    {
        _isViewRoom = true;
        _housingVM.TargetRoom = roomVM;

        Vector3 roomCenterWorld = GetRoomCenterPos(roomVM);
        FocusRoom(roomCenterWorld).Forget();
    }

    private void CheckNormalRoom()
    {
        if (_housingVM == null || _housingVM.IsInHousingMode)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 inputPosition = Vector2.zero;
        bool isTriggered = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
            isTriggered = true;
        }
#endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                inputPosition = touch.position;
                isTriggered = true;
            }
        }

        if (!isTriggered)
        {
            return;
        }

        Ray priorityRay = Camera_Main.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(priorityRay, out RaycastHit priorityHit, Mathf.Infinity, _priorityLayerMask))
        {
            return;
        }

        Ray ray = Camera_Main.ScreenPointToRay(inputPosition);
        Plane mapPlane = new Plane(Vector3.forward, new Vector3(0, 0, 9f));

        if (mapPlane.Raycast(ray, out float hit))
        {
            Vector3 hitPoint = ray.GetPoint(hit);
            int gridX = Mathf.FloorToInt(hitPoint.x / 1.0f);
            int gridY = Mathf.FloorToInt((hitPoint.y - 2.0f) / 1.0f);
            Vector2Int gridPos = new Vector2Int(gridX, gridY);

            if (_buildVM.Builds.TryGetValue(gridPos, out RoomViewModel roomVM))
            {
                if (roomVM.BuildType == BuildType.Room)
                {
                    FocusRoomInGame(roomVM);
                }
            }
        }
    }

    private async UniTask TransitionCamera(Vector3 targetPos, Quaternion targetRot, Matrix4x4 startMatrix, Matrix4x4 targetMatrix, bool endIsOrtho, float targetFOV, CancellationToken token)
    {
        _isTransition = true;

        Vector3 startPos = Camera_Main.transform.position;
        Quaternion startRot = Camera_Main.transform.rotation;

        float elapsedTime = 0f;

        Matrix4x4 currentMatrix = new Matrix4x4();

        try
        {
            while (elapsedTime < Duration)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                elapsedTime += Time.deltaTime;
                float time = Mathf.SmoothStep(0f, 1f, elapsedTime / Duration);

                Camera_Main.transform.position = Vector3.Lerp(startPos, targetPos, time);
                Camera_Main.transform.rotation = Quaternion.Lerp(startRot, targetRot, time);
                Camera_Main.projectionMatrix = MatrixLerp(startMatrix, targetMatrix, time, ref currentMatrix);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (!token.IsCancellationRequested)
            {
                Camera_Main.transform.position = targetPos;
                Camera_Main.transform.rotation = targetRot;
                Camera_Main.orthographic = endIsOrtho;
                Camera_Main.ResetProjectionMatrix();

                if (endIsOrtho)
                {
                    Camera_Main.orthographicSize = Size_Ortho;
                }
                else
                {
                    Camera_Main.fieldOfView = targetFOV;
                }
            }
        }
        catch (System.OperationCanceledException)
        {
        }
        finally
        {
            _isTransition = false;
        }
    }

    private Matrix4x4 MatrixLerp(Matrix4x4 start, Matrix4x4 end, float time, ref Matrix4x4 result)
    {
        result.SetRow(0, Vector4.Lerp(start.GetRow(0), end.GetRow(0), time));
        result.SetRow(1, Vector4.Lerp(start.GetRow(1), end.GetRow(1), time));
        result.SetRow(2, Vector4.Lerp(start.GetRow(2), end.GetRow(2), time));
        result.SetRow(3, Vector4.Lerp(start.GetRow(3), end.GetRow(3), time));

        return result;
    }

    private Vector3 GetRoomCenterPos(RoomViewModel roomVM)
    {
        float cellSize = 1.0f;
        float yOffset = 2.0f;
        float subCellSize = cellSize / roomVM.GridFactor;

        float roomX = (roomVM.OriginPos.x * cellSize) + (roomVM.SubGridSize.x * subCellSize * 0.5f);

        float floorY = (roomVM.OriginPos.y + yOffset) * cellSize;
        float roomY = floorY;

        float roomZ = 9.0f - (roomVM.SubGridSize.y * subCellSize * 0.5f) + 0.3f;

        return new Vector3(roomX, roomY, roomZ);
    }

    public void StartFollowHamster(Transform target)
    {
        _targetHamster = target;
        IsFollowing = true;
    }

    public void StopFollowHamster()
    {
        _targetHamster = null;
        IsFollowing = false;
    }

    private void CancelZoom()
    {
        if (_zoomCancel != null)
        {
            _zoomCancel.Cancel();
            _zoomCancel.Dispose();
            _zoomCancel = null;
        }
    }
}