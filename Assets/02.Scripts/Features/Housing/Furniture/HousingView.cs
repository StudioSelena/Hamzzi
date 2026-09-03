using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class HousingView : ViewBase
{
    [SerializeField] private Material Material_Ghost;
    [SerializeField] private SpriteRenderer SpriteRenderer_Tile;

    [SerializeField] private Color Color_Valid = new Color(0f, 1f, 0f, 0.4f);
    [SerializeField] private Color Color_Invalid = new Color(1f, 0f, 0f, 0.4f);

    private Vector3 _gardenOrigin = new Vector3(-40f, 12f, 12f);
    private Vector2Int _gardenGridSize = new Vector2Int(80, 60);
    private float _gardenSubCellSize = 1f;

    private float _cellSize = 1.0f;
    private float _yOffset = 2.0f;

    private Camera _mainCamera;
    private Plane _mapPlane = new Plane(Vector3.forward, new Vector3(0, 0, 9f));

    private HousingService _housingService;
    private HousingViewModel _housingVM;
    private BuildViewModel _buildVM;
    private GameObject _ghostObject;
    private float _lastGhostAngle = 0f;

    private List<GameObject> _activeGridLines = new List<GameObject>();

    private void Awake()
    {
        _mainCamera = Camera.main;
        _housingService = ServiceManager.Instance.HousingService;

        SpriteRenderer_Tile.gameObject.SetActive(false);
    }

    private void Start()
    {
        BuildViewModel buildVM = ServiceManager.Instance.BuildService.GetBuildViewModel();
        HousingViewModel housingVM = _housingService.GetHousingViewModel();

        BindViewModel(housingVM, buildVM);
    }

    public void BindViewModel(HousingViewModel housingVM, BuildViewModel buildVM)
    {
        _housingVM = housingVM;
        _buildVM = buildVM;

        _housingVM.PropertyChanged += OnPropertyChanged_VM;

        if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
        {
            ShowGardenGrid().Forget();
        }
        else if (_housingVM.TargetRoom != null)
        {
            ShowRoomGrid(_housingVM.TargetRoom).Forget();
        }
    }

    private void OnDestroy()
    {
        if (_housingVM != null)
        {
            _housingVM.PropertyChanged -= OnPropertyChanged_VM;
        }
    }

    private void OnPropertyChanged_VM(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_housingVM.CurrentViewMode):
                if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
                {
                    ShowGardenGrid().Forget();
                }
                else if (_housingVM.CurrentViewMode == HousingViewMode.OverView)
                {
                    ClearRoomGrid();
                }
                break;

            case nameof(_housingVM.TargetRoom):
                if (_housingVM.TargetRoom != null)
                {
                    ShowRoomGrid(_housingVM.TargetRoom).Forget();
                }
                else if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
                {
                    ShowGardenGrid().Forget();
                }
                break;

            case nameof(_housingVM.FurnitureVM):
                if (_housingVM.FurnitureVM != null)
                {
                    if (_ghostObject != null)
                    {
                        UpdateGhostTransform();
                    }
                    else
                    {
                        SpawnGhostObject(_housingVM.FurnitureVM.FurnitureID, _housingVM.FurnitureVM.PrefabPath).Forget();
                    }
                }
                else
                {
                    ClearGhostObject();
                }
                break;

            case nameof(_housingVM.ConfirmFurniture):
                if (_housingVM.ConfirmFurniture != null)
                {
                    SpawnFurniture(_housingVM.ConfirmFurniture).Forget();
                    _housingService.SaveAllInventoryData(ServiceManager.Instance.LoginService.GetViewModel().UserUID).Forget();
                }
                break;

            case nameof(_housingVM.DestroyFurniture):
                if (_housingVM.DestroyFurniture != null)
                {
                    string id = _housingVM.DestroyFurniture.InstanceID;
                    _housingService.RemoveSpawnFurniture(id);
                    _housingService.SaveAllInventoryData(ServiceManager.Instance.LoginService.GetViewModel().UserUID).Forget();
                }
                break;
        }
    }

    private void Update()
    {
        if (_buildVM == null || _buildVM.SelectType != BuildType.None || _buildVM.CanConfirm)
        {
            return;
        }

        if (IsTouchUI() || !GetInputPosition(out Vector3 inputPosition))
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(inputPosition);

        if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
        {
            if (_housingVM.FurnitureVM == null)
            {
                SelectInstallFurniture(ray);
            }
            else
            {
                DragGarden(ray);
            }
            return;
        }

        if (_housingVM.CurrentState == HousingState.SelectRoom)
        {
            RoomSelect(ray);
        }
        else if (_housingVM.CurrentState == HousingState.Placing && _housingVM.FurnitureVM == null)
        {
            SelectInstallFurniture(ray);
        }
        else if (_housingVM.FurnitureVM != null)
        {
            RoomDrag(ray);
        }
    }

    private bool IsTouchUI()
    {
        if (EventSystem.current == null) return false;

#if UNITY_EDITOR || UNITY_STANDALONE
        return EventSystem.current.IsPointerOverGameObject();
#else
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return true;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = touch.position };

        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        return results.Count > 0;
    }
    return false;
#endif
    }

    private bool SelectInstallFurniture(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<FurnitureView>(out var furnitureView))
            {
                if (furnitureView.FurnitureVM != null)
                {
                    string instanceID = furnitureView.FurnitureVM.InstanceID;
                    _housingService.RemoveSpawnFurniture(instanceID);

                    SoundManager.Instance.PlaySFX("Select_Furniture");

                    _housingVM.SelectInstallFurniture(furnitureView.FurnitureVM);

                    return true;
                }
            }
        }

        return false;
    }

    private void DragGarden(Ray ray)
    {
        Plane gardenPlane = new Plane(Vector3.up, new Vector3(0f, _gardenOrigin.y, 0f));

        if (gardenPlane.Raycast(ray, out float hit))
        {
            Vector3 hitPoint = ray.GetPoint(hit);
            Vector2Int gridPos = WorldToGardenGrid(hitPoint, _housingVM.FurnitureVM.Size);

            _housingVM.MovePos(gridPos);
            UpdateGhostTransform();
        }
    }

    private void RoomSelect(Ray ray)
    {
        if (_mapPlane.Raycast(ray, out float hit))
        {
            Vector3 hitPoint = ray.GetPoint(hit);

            int gridX = Mathf.FloorToInt(hitPoint.x / _cellSize);
            int gridY = Mathf.FloorToInt((hitPoint.y - _yOffset) / _cellSize);
            Vector2Int gridPos = new Vector2Int(gridX, gridY);

            if (_buildVM.Builds.TryGetValue(gridPos, out RoomViewModel roomVM))
            {
                if (roomVM.BuildType == BuildType.Room)
                {
                    _housingVM.TargetRoom = roomVM;
                }
            }
        }
    }

    private void RoomDrag(Ray ray)
    {
        RoomViewModel roomVM = _housingVM.TargetRoom;

        float floorY = (roomVM.OriginPos.y + _yOffset) * _cellSize;
        Plane roomFloorPlane = new Plane(Vector3.up, new Vector3(0f, floorY, 0f));

        if (roomFloorPlane.Raycast(ray, out float hit))
        {
            Vector3 hitPoint = ray.GetPoint(hit);
            Vector2Int localPos = roomVM.ChangeLocalGrid(hitPoint, _housingVM.FurnitureVM.Size, _cellSize);
            _housingVM.MovePos(localPos);

            UpdateGhostTransform();
        }
    }

    private bool GetInputPosition(out Vector3 inputPosition)
    {
        inputPosition = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            inputPosition = Input.mousePosition;
            return true;
        }
#endif
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return false;
            }

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                inputPosition = touch.position;
                return true;
            }
        }

        return false;
    }

    private float GetCurrentSubCellSize()
    {
        return (_housingVM.TargetRoom != null) ? (_cellSize / _housingVM.TargetRoom.GridFactor) : _gardenSubCellSize;
    }

    private void GetFurnitureWorldTransform(FurnitureViewModel furnitureVM, out Vector3 pos, out Quaternion rot, out float tileYOffset)
    {
        float subCellSize = GetCurrentSubCellSize();
        rot = Quaternion.Euler(0f, furnitureVM.RotationAngle, 0f);

        float furnitureYOffset = 0f;
        if (_ghostObject != null && _ghostObject.TryGetComponent<FurnitureView>(out var fView))
        {
            furnitureYOffset = fView.Offset;
        }

        if (_housingVM.TargetRoom != null)
        {
            float localX = (furnitureVM.LocalPos.x + furnitureVM.Size.x * 0.5f) * subCellSize;
            float localZ = (furnitureVM.LocalPos.y + furnitureVM.Size.y * 0.5f) * subCellSize;

            float worldX = (_housingVM.TargetRoom.OriginPos.x * _cellSize) + localX;
            float worldY = (_housingVM.TargetRoom.OriginPos.y + _yOffset) * _cellSize + 0.2f;
            float worldZ = 9f - localZ - 0.5f;

            pos = new Vector3(worldX, worldY, worldZ);
            tileYOffset = pos.y + 0.01f;
        }
        else
        {
            float localX = (furnitureVM.LocalPos.x + furnitureVM.Size.x * 0.5f) * subCellSize;
            float localZ = (furnitureVM.LocalPos.y + furnitureVM.Size.y * 0.5f) * subCellSize;

            float worldX = _gardenOrigin.x + localX;
            float worldY = _gardenOrigin.y;
            float worldZ = _gardenOrigin.z + localZ;

            pos = new Vector3(worldX, worldY, worldZ);
            tileYOffset = _gardenOrigin.y + 0.02f;
        }
    }

    public void UpdateGhostTransform()
    {
        if (_housingVM.FurnitureVM == null)
        {
            return;
        }

        GetFurnitureWorldTransform(_housingVM.FurnitureVM, out Vector3 pos, out Quaternion rot, out float tileYOffset);
        float subCellSize = GetCurrentSubCellSize();

        float furnitureYOffset = 0f;
        if (_ghostObject.TryGetComponent<FurnitureView>(out var fView))
        {
            furnitureYOffset = fView.Offset;
        }

        _ghostObject.transform.position = new Vector3(pos.x, pos.y + furnitureYOffset, pos.z);

        float currentAngle = _housingVM.FurnitureVM.RotationAngle;

        if (!Mathf.Approximately(_lastGhostAngle, currentAngle))
        {
            _lastGhostAngle = currentAngle;

            if (_ghostObject.TryGetComponent<FurnitureView>(out var furnitureView))
            {
                furnitureView.PlayRotationAnimation(currentAngle);
            }
            else
            {
                _ghostObject.transform.rotation = rot;
            }
        }

        if (SpriteRenderer_Tile != null)
        {
            SpriteRenderer_Tile.transform.position = new Vector3(pos.x, tileYOffset, pos.z);
            SpriteRenderer_Tile.transform.rotation = Quaternion.Euler(90f, 0f, currentAngle);

            Vector2Int baseSize = _housingVM.FurnitureVM.Size;

            bool isRotated = Mathf.Approximately(currentAngle, 90f) || Mathf.Approximately(currentAngle, 270f) || ((int)currentAngle / 90) % 2 != 0;
            int sizeX = isRotated ? baseSize.y : baseSize.x;
            int sizeY = isRotated ? baseSize.x : baseSize.y;

            float tileWidth = sizeX * subCellSize;
            float tileHeight = sizeY * subCellSize;

            SpriteRenderer_Tile.transform.localScale = new Vector3(tileWidth, tileHeight, 1f);
            SpriteRenderer_Tile.color = _housingVM.FurnitureVM.IsValid ? Color_Valid : Color_Invalid;
        }
    }

    public async UniTask SpawnGhostObject(string furnitureID, string prefabPath)
    {
        ClearGhostObject();

        _ghostObject = await GameObjectManager.Instance.CreateObjectAsync(furnitureID, prefabPath, Vector3.zero);

        GetFurnitureWorldTransform(_housingVM.FurnitureVM, out Vector3 pos, out Quaternion rotation, out float tileYOffset);
        _ghostObject.transform.rotation = rotation;

        _lastGhostAngle = _housingVM.FurnitureVM.RotationAngle;

        SoundManager.Instance.PlaySFX("Select_Furniture", 0.6f);

        if (_ghostObject.TryGetComponent<FurnitureView>(out var furnitureView))
        {
            furnitureView.SetGhostMode(Material_Ghost);

            float subCellSize = GetCurrentSubCellSize();
            Vector2Int calculatedSize = _housingVM.FurnitureVM.Size;

            if (calculatedSize == Vector2Int.one && furnitureView.GetFurnitureSize(subCellSize) != Vector2Int.one)
            {
                calculatedSize = furnitureView.GetFurnitureSize(subCellSize);
                _housingVM.FurnitureVM.Size = calculatedSize;
            }

            if (_housingVM.CurrentState == HousingState.Editing)
            {
                UpdateGhostTransform();
                SpriteRenderer_Tile.gameObject.SetActive(true);
                return;
            }

            if (_housingVM.CurrentState == HousingState.Placing)
            {
                if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
                {
                    Vector2Int centerPos = GetGardenCenterPosition(calculatedSize);
                    _housingVM.MovePos(centerPos);
                }
                else if (_housingVM.TargetRoom != null)
                {
                    Vector2Int roomCenterPos = new Vector2Int(_housingVM.TargetRoom.SubGridSize.x / 2 - calculatedSize.x / 2, _housingVM.TargetRoom.SubGridSize.y / 2 - calculatedSize.y / 2);
                    _housingVM.MovePos(roomCenterPos);
                }
            }
        }

        UpdateGhostTransform();
        SpriteRenderer_Tile.gameObject.SetActive(true);
    }

    public void ClearGhostObject()
    {
        if (_ghostObject != null)
        {
            GameObjectManager.Instance.RequestDestroyObject(_ghostObject);
            _ghostObject = null;
        }

        SpriteRenderer_Tile.gameObject.SetActive(false);
    }

    public async UniTask SpawnFurniture(FurnitureViewModel furnitureVM)
    {
        Vector3 spawnPos;
        Quaternion spawnRot;

        if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
        {
            GetGardenFurniturePositionAndRotation(furnitureVM, out spawnPos, out spawnRot);
        }
        else
        {
            GetFurniturePositionAndRotation(_housingVM.TargetRoom, furnitureVM, out spawnPos, out spawnRot);
        }

        GameObject prefab = await GameObjectManager.Instance.CreateObjectAsync(furnitureVM.InstanceID, furnitureVM.PrefabPath, spawnPos);


        prefab.transform.rotation = spawnRot;

        FurnitureView furnitureView = prefab.GetComponent<FurnitureView>();

        if (furnitureView != null)
        {
            spawnPos.y += furnitureView.Offset;
        }

        prefab.transform.SetPositionAndRotation(spawnPos, spawnRot);

        furnitureView.ResetMaterial();
        furnitureView.Bind(furnitureVM);

        furnitureView.PlayPlaceAnimation();

        _housingService.RegisterSpawnFurniture(furnitureVM.InstanceID, prefab);
    }

    private void GetFurniturePositionAndRotation(RoomViewModel roomVM, FurnitureViewModel furnitureVM, out Vector3 pos, out Quaternion rot)
    {
        float subCellSize = _cellSize / roomVM.GridFactor;

        float localX = (furnitureVM.LocalPos.x + furnitureVM.Size.x * 0.5f) * subCellSize;
        float localZ = (furnitureVM.LocalPos.y + furnitureVM.Size.y * 0.5f) * subCellSize;

        float worldX = (roomVM.OriginPos.x * _cellSize) + localX;
        float worldY = (roomVM.OriginPos.y + _yOffset) * _cellSize + 0.2f;
        float worldZ = 9f - localZ - 0.5F;

        pos = new Vector3(worldX, worldY, worldZ);
        rot = Quaternion.Euler(0f, furnitureVM.RotationAngle, 0f);
    }

    private void GetGardenFurniturePositionAndRotation(FurnitureViewModel furnitureVM, out Vector3 pos, out Quaternion rot)
    {
        float localX = (furnitureVM.LocalPos.x + furnitureVM.Size.x * 0.5f) * _gardenSubCellSize;
        float localZ = (furnitureVM.LocalPos.y + furnitureVM.Size.y * 0.5f) * _gardenSubCellSize;

        float worldX = _gardenOrigin.x + localX;
        float worldY = _gardenOrigin.y;
        float worldZ = _gardenOrigin.z + localZ;

        pos = new Vector3(worldX, worldY, worldZ);
        rot = Quaternion.Euler(0f, furnitureVM.RotationAngle, 0f);
    }

    private async UniTask CreateGridLineAsync(Vector3 pos, Vector3 scale)
    {
        GameObject line = await GameObjectManager.Instance.CreateObjectAsync("GridLine", "Prefabs/UI/GridLine", pos);

        if (line != null)
        {
            line.transform.SetParent(transform);
            line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            line.transform.localScale = scale;
            _activeGridLines.Add(line);
        }
    }

    public async UniTask ShowRoomGrid(RoomViewModel roomVM)
    {
        ClearRoomGrid();

        float subCellSize = _cellSize / roomVM.GridFactor;
        float roomX = roomVM.OriginPos.x * _cellSize;
        float roomY = (roomVM.OriginPos.y + _yOffset) * _cellSize + 0.21f;

        int subX = roomVM.SubGridSize.x;
        int subY = roomVM.SubGridSize.y;

        float totalWidth = subX * subCellSize;
        float totalHeight = subY * subCellSize;

        for (int x = 0; x <= subX; x++)
        {
            float currentX = roomX + (x * subCellSize);
            Vector3 pos = new Vector3(currentX, roomY, 9f - (totalHeight * 0.5f));
            await CreateGridLineAsync(pos, new Vector3(0.02f, totalHeight, 1f));
        }

        for (int y = 0; y <= subY; y++)
        {
            float currentZ = 9f - (y * subCellSize);
            Vector3 pos = new Vector3(roomX + (totalWidth * 0.5f), roomY, currentZ);
            await CreateGridLineAsync(pos, new Vector3(totalWidth, 0.02f, 1f));
        }
    }

    public async UniTask ShowGardenGrid()
    {
        ClearRoomGrid();

        float totalWidth = _gardenGridSize.x * _gardenSubCellSize;
        float totalDepth = _gardenGridSize.y * _gardenSubCellSize;
        float gridY = _gardenOrigin.y + 0.015f;

        for (int y = 0; y <= _gardenGridSize.y; y++)
        {
            float currentZ = _gardenOrigin.z + (y * _gardenSubCellSize);
            Vector3 pos = new Vector3(_gardenOrigin.x + (totalWidth * 0.5f), gridY, currentZ);
            await CreateGridLineAsync(pos, new Vector3(totalWidth, 0.02f, 1f));
        }

        for (int x = 0; x <= _gardenGridSize.x; x++)
        {
            float currentX = _gardenOrigin.x + (x * _gardenSubCellSize);
            Vector3 pos = new Vector3(currentX, gridY, _gardenOrigin.z + (totalDepth * 0.5f));
            await CreateGridLineAsync(pos, new Vector3(0.02f, totalDepth, 1f));
        }
    }

    public void ClearRoomGrid()
    {
        for (int i = 0; i < _activeGridLines.Count; i++)
        {
            if (_activeGridLines[i] != null)
            {
                GameObjectManager.Instance.RequestDestroyObject(_activeGridLines[i]);
            }
        }

        _activeGridLines.Clear();
    }

    private Vector2Int WorldToGardenGrid(Vector3 hitPoint, Vector2Int furnitureSize)
    {
        float halfWidth = furnitureSize.x * 0.5f * _gardenSubCellSize;
        float halfDepth = furnitureSize.y * 0.5f * _gardenSubCellSize;

        float localX = (hitPoint.x - halfWidth) - _gardenOrigin.x;
        float localZ = (hitPoint.z - halfDepth) - _gardenOrigin.z;

        int gridX = Mathf.RoundToInt(localX / _gardenSubCellSize);
        int gridZ = Mathf.RoundToInt(localZ / _gardenSubCellSize);

        gridX = Mathf.Clamp(gridX, 0, _gardenGridSize.x - furnitureSize.x);
        gridZ = Mathf.Clamp(gridZ, 0, _gardenGridSize.y - furnitureSize.y);

        return new Vector2Int(gridX, gridZ);
    }

    private Vector2Int GetGardenCenterPosition(Vector2Int furnitureSize)
    {
        Ray centerRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Plane gardenPlane = new Plane(Vector3.up, new Vector3(0f, _gardenOrigin.y, 0f));

        if (gardenPlane.Raycast(centerRay, out float hitDistance))
        {
            return WorldToGardenGrid(centerRay.GetPoint(hitDistance), furnitureSize);
        }

        return new Vector2Int(10, 10);
    }
}